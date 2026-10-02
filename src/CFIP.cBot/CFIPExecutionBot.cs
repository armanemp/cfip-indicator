using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;
using CFIP.cBot.Binding;
using CFIP.cBot.Execution;
using CFIP.cBot.Shadow;

namespace CFIP.cBot
{
#pragma warning disable CS0612
    [Robot(
        "CFIP Smart Execution Bot",
        DefaultTimeFrame = "M15",
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
#pragma warning restore CS0612
        private const string ManagedLabel = "CFIP-SMART";
        private const string StartupState = "DEMO";

        [Parameter(
            "Enable Demo Market Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoMarketExecution { get; set; }

        [Parameter(
            "Enable Demo Pending Stop Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoPendingStopExecution { get; set; }

        [Parameter(
            "Enable Demo Aggressive Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoAggressiveExecution { get; set; }

        [Parameter(
            "Max Demo Executions Per Session",
            Group = "Safety",
            DefaultValue = 1,
            MinValue = 1,
            MaxValue = 10)]
        public int MaxDemoMarketExecutionsPerSession { get; set; }

        [Parameter(
            "Max Execution Margin Usage %",
            Group = "Safety",
            DefaultValue = 80,
            MinValue = 10,
            MaxValue = 100)]
        public double MaxExecutionMarginUsagePercent { get; set; }

        [Parameter(
            "Execution Margin Buffer %",
            Group = "Safety",
            DefaultValue = 10,
            MinValue = 0,
            MaxValue = 40)]
        public double ExecutionMarginBufferPercent { get; set; }

        [Parameter(
            "Provider Stale After Seconds",
            Group = "Safety",
            DefaultValue = 15,
            MinValue = 1,
            MaxValue = 60)]
        public int ProviderStaleAfterSeconds { get; set; }

        private readonly DemoMarketExecutionCoordinator _market =
            new DemoMarketExecutionCoordinator();

        private readonly DemoPendingOrderExecutionCoordinator _pending =
            new DemoPendingOrderExecutionCoordinator();

        private readonly ShadowHostCoordinator _shadow =
            new ShadowHostCoordinator();

        private ShadowHostState _state =
            ShadowHostState.Waiting;

        private long _lastLoggedRevision = -1;
        private string _lastLoggedReason = "";
        private long _tickCount;
        private int _sessionExecutions;
        private DateTime _startedUtc;
        private DateTime _nextBindingCheckUtc = DateTime.MinValue;
        private DateTime _nextSignalReloadUtc = DateTime.MinValue;
        private string _boundIndicatorInstanceId = "";

        protected override void OnStart()
        {
            _startedUtc = Server.TimeInUtc;
            _tickCount = 0;
            _sessionExecutions = 0;

            if (Account.IsLive)
            {
                Print(
                    "CFIP DEMO cBot BLOCKED | live account detected | " +
                    "this build is demo-only");
                Stop();
                return;
            }

            // Chart timeframe is host-only. CFIP execution is driven by the
            // Indicator's internal M15 analysis clock and does not use Bars.TimeFrame.
            Print(
                "CFIP cBot START | hostTimeframe={0} | execTimeframe=M15 | state={1} | " +
                "marketExecution={2} | pendingStopExecution={3} | aggressiveExecution={4} | " +
                "maxSessionExecutions={5} | staleAfter={6}s | contractVersion={7}",
                Bars == null ? "UNKNOWN" : Bars.TimeFrame.ToString(),
                StartupState,
                EnableDemoMarketExecution ? "ARMED" : "DISARMED",
                EnableDemoPendingStopExecution ? "ARMED" : "DISARMED",
                EnableDemoAggressiveExecution ? "ARMED" : "DISARMED",
                MaxDemoMarketExecutionsPerSession,
                ProviderStaleAfterSeconds,
                ContractVersion.Current);

            RefreshIndicatorBinding(true);
            ReloadSignalStore(true);
        }

        protected override void OnTick()
        {
            _tickCount++;

            if (!RefreshIndicatorBinding(false))
            {
                LogBlockedState("INDICATOR BINDING BLOCKED");
                return;
            }

            ReloadSignalStore(false);

            if (!CfipDeviceSignalTransport.TryRead(
                    this,
                    _boundIndicatorInstanceId,
                    out SignalEnvelope envelope,
                    out string transportReason))
            {
                LogBlockedState(transportReason);
                return;
            }

            DateTime nowUtc = Server.TimeInUtc;

            if (envelope.Identity == null)
            {
                LogBlockedState("SIGNAL IDENTITY UNAVAILABLE");
                return;
            }

            if (envelope.ObservedUtc > nowUtc)
            {
                LogBlockedState("SIGNAL OBSERVED TIME IS IN THE FUTURE");
                return;
            }

            double ageSeconds =
                (nowUtc - envelope.ObservedUtc).TotalSeconds;

            if (ageSeconds >
                Math.Max(1, ProviderStaleAfterSeconds))
            {
                LogBlockedState(
                    "SIGNAL ENVELOPE STALE • AGE " +
                    Math.Round(ageSeconds, 1) + "S");
                return;
            }

            if (envelope.Identity.Symbol != SymbolName)
            {
                LogBlockedState("SIGNAL SYMBOL SCOPE MISMATCH");
                return;
            }

            ShadowHostResult shadowResult =
                _shadow.Observe(
                    envelope,
                    ReadBrokerSnapshot(),
                    ContractVersion.Current,
                    envelope.Identity.Revision,
                    nowUtc);

            LogStateIfChanged(shadowResult);

            bool executionEnabled =
                EnableDemoMarketExecution ||
                EnableDemoPendingStopExecution ||
                EnableDemoAggressiveExecution;

            bool actionEnabled =
                envelope.Intent != null &&
                (envelope.Intent.Action == ExecutionAction.Aggressive
                    ? EnableDemoAggressiveExecution
                    : envelope.Intent.Action == ExecutionAction.PendingStop
                        ? EnableDemoPendingStopExecution
                        : envelope.Intent.Action == ExecutionAction.Market &&
                          EnableDemoMarketExecution);

            if (!executionEnabled ||
                !actionEnabled ||
                shadowResult == null ||
                shadowResult.State != ShadowHostState.Ready)
                return;

            if (_sessionExecutions >=
                Math.Max(1, MaxDemoMarketExecutionsPerSession))
            {
                LogBlockedState("DEMO SESSION EXECUTION CAP REACHED");
                return;
            }

            if (envelope.Intent.Action == ExecutionAction.PendingStop)
            {
                if (_pending.TryExecuteStop(
                        this,
                        envelope,
                        ManagedLabel,
                        nowUtc,
                        MaxExecutionMarginUsagePercent,
                        ExecutionMarginBufferPercent,
                        out BrokerExecutionReport pendingReport,
                        out string pendingReason))
                {
                    _sessionExecutions++;
                }

                if (pendingReport != null)
                {
                    Print(
                        "CFIP DEMO PENDING STOP | status={0} | action={1} | " +
                        "revision={2} | pending={3} | reason={4}",
                        pendingReport.Status,
                        pendingReport.Action,
                        pendingReport.AttemptRevision,
                        pendingReport.BrokerPendingOrderId.HasValue
                            ? pendingReport.BrokerPendingOrderId.Value.ToString()
                            : "",
                        pendingReason);
                }

                return;
            }

            if (_market.TryExecute(
                    this,
                    envelope,
                    ManagedLabel,
                    nowUtc,
                    MaxExecutionMarginUsagePercent,
                    ExecutionMarginBufferPercent,
                    out BrokerExecutionReport report,
                    out string executionReason))
            {
                _sessionExecutions++;
            }

            if (report != null)
            {
                Print(
                    "CFIP DEMO MARKET | status={0} | action={1} | " +
                    "revision={2} | position={3} | reason={4}",
                    report.Status,
                    report.Action,
                    report.AttemptRevision,
                    report.BrokerPositionId.HasValue
                        ? report.BrokerPositionId.Value.ToString()
                        : "",
                    executionReason);
            }
        }

        private bool RefreshIndicatorBinding(bool force)
        {
            DateTime now = Server.TimeInUtc;

            if (!force &&
                now < _nextBindingCheckUtc)
                return !string.IsNullOrWhiteSpace(_boundIndicatorInstanceId);

            _nextBindingCheckUtc = now.AddMilliseconds(500);

            if (!CfipIndicatorChartBinding.TryFind(
                    this,
                    out ChartIndicator indicator,
                    out string reason))
            {
                _boundIndicatorInstanceId = "";
                _state = ShadowHostState.Blocked;
                LogBlockedState(reason);
                return false;
            }

            string instanceId = indicator.InstanceId ?? "";

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                _boundIndicatorInstanceId = "";
                LogBlockedState("CFIP INDICATOR INSTANCE ID UNAVAILABLE");
                return false;
            }

            if (!force &&
                string.Equals(
                    _boundIndicatorInstanceId,
                    instanceId,
                    StringComparison.Ordinal))
                return true;

            _boundIndicatorInstanceId = instanceId;

            Print(
                "CFIP ANALYSIS BIND | state=READY | name={0} | instance={1}",
                CfipIndicatorChartBinding.DisplayName,
                _boundIndicatorInstanceId);

            return true;
        }

        private void ReloadSignalStore(bool force)
        {
            DateTime now = Server.TimeInUtc;

            if (!force &&
                now < _nextSignalReloadUtc)
                return;

            _nextSignalReloadUtc =
                now.AddMilliseconds(500);

            try
            {
                CfipDeviceSignalTransport.Reload(this);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SIGNAL STORE RELOAD FAILED | {0}",
                    ex.Message);
            }
        }

        private ShadowBrokerSnapshot ReadBrokerSnapshot()
        {
            int managedPositions = 0;
            foreach (Position position in Positions)
            {
                if (position != null &&
                    string.Equals(
                        position.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        position.Label,
                        ManagedLabel,
                        StringComparison.Ordinal))
                    managedPositions++;
            }

            int managedPendingOrders = 0;
            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        order.Label,
                        ManagedLabel + "-PENDING",
                        StringComparison.Ordinal))
                    managedPendingOrders++;
            }

            return new ShadowBrokerSnapshot(
                managedPositions,
                managedPendingOrders,
                SymbolName ?? "",
                Symbol.Bid,
                Symbol.Ask,
                Symbol.PipSize);
        }

        private void LogBlockedState(string reason)
        {
            reason = reason ?? "UNKNOWN";

            if (_state == ShadowHostState.Blocked &&
                string.Equals(
                    _lastLoggedReason,
                    reason,
                    StringComparison.Ordinal))
                return;

            _state = ShadowHostState.Blocked;
            _lastLoggedReason = reason;

            Print(
                "CFIP cBot STATE | state=BLOCKED | reason={0} | " +
                "indicator={1} | tickCount={2}",
                reason,
                string.IsNullOrWhiteSpace(_boundIndicatorInstanceId)
                    ? "NONE"
                    : _boundIndicatorInstanceId,
                _tickCount);
        }

        private void LogStateIfChanged(ShadowHostResult result)
        {
            if (result == null)
                return;

            if (result.State == _state &&
                result.Revision == _lastLoggedRevision &&
                string.Equals(
                    result.Reason,
                    _lastLoggedReason,
                    StringComparison.Ordinal))
                return;

            _state = result.State;
            _lastLoggedRevision = result.Revision;
            _lastLoggedReason = result.Reason ?? "";

            Print(
                "CFIP DEMO SHADOW | state={0} | reason={1} | revision={2} | " +
                "signal={3} | scenario={4} | plan={5} | key={6}",
                result.State,
                result.Reason,
                result.Revision,
                result.SignalId ?? "",
                result.ScenarioId ?? "",
                result.PlanId ?? "",
                result.IdempotencyKey ?? "");
        }

        protected override void OnStop()
        {
            Print(
                "CFIP DEMO cBot STOP | state={0} | executions={1} | " +
                "sessionMs={2}",
                _state,
                _sessionExecutions,
                (Server.TimeInUtc - _startedUtc).TotalMilliseconds);
        }
    }
}