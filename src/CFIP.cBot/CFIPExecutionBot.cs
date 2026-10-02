using System;
using cAlgo.API;
using CFIP.Contracts;
using CFIP.cBot.Binding;
using CFIP.cBot.Execution;
using CFIP.cBot.Shadow;

namespace CFIP.cBot
{
    [Robot(
        Name = "CFIP Execution cBot",
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
        private const string StartupState = "HANDOFF";

        [Parameter(
            "Enable Market Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableMarketExecution { get; set; }

        [Parameter(
            "Use Market Range",
            Group = "Execution",
            DefaultValue = true)]
        public bool UseMarketRange { get; set; }

        [Parameter(
            "Provider Stale After Seconds",
            Group = "Safety",
            DefaultValue = 15,
            MinValue = 1,
            MaxValue = 60)]
        public int ProviderStaleAfterSeconds { get; set; }

        private readonly MarketExecutionCoordinator _market =
            new MarketExecutionCoordinator();

        private readonly ShadowHostCoordinator _shadow =
            new ShadowHostCoordinator();

        private ShadowHostState _state =
            ShadowHostState.Waiting;

        private long _lastLoggedRevision = -1;

        private string _lastLoggedReason = "";

        private long _tickCount;

        private DateTime _startedUtc;

        private DateTime _nextIndicatorBindingCheckUtc =
            DateTime.MinValue;

        private DateTime _nextSignalReloadUtc =
            DateTime.MinValue;

        private string _boundIndicatorInstanceId = "";

        private string _boundManagedExecutionLabel =
            "CFIP-SMART";

        private string _indicatorBindingReason =
            "NOT BOUND";

        protected override void OnStart()
        {
            _startedUtc =
                Server.TimeInUtc;

            _tickCount = 0;

            Print(
                "CFIP Execution cBot START | state={0} | " +
                "marketExecution={1} | staleAfter={2}s | " +
                "contractVersion={3}",
                StartupState,
                EnableMarketExecution
                    ? "ARMED"
                    : "DISARMED",
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
                LogBlockedState(
                    "INDICATOR BINDING BLOCKED");
                return;
            }

            ReloadSignalStore(false);

            SignalEnvelope snapshot;
            string transportReason;

            if (!CfipDeviceSignalTransport.TryRead(
                    this,
                    _boundIndicatorInstanceId,
                    out snapshot,
                    out transportReason))
            {
                LogBlockedState(transportReason);
                return;
            }

            DateTime nowUtc =
                Server.TimeInUtc;

            if (snapshot.ObservedUtc > nowUtc)
            {
                LogBlockedState(
                    "SIGNAL OBSERVED TIME IS IN THE FUTURE");
                return;
            }

            double ageSeconds =
                (nowUtc -
                 snapshot.ObservedUtc)
                .TotalSeconds;

            if (ageSeconds >
                Math.Max(
                    1,
                    ProviderStaleAfterSeconds))
            {
                LogBlockedState(
                    "SIGNAL ENVELOPE STALE • AGE " +
                    Math.Round(
                        ageSeconds,
                        1) +
                    "S");
                return;
            }

            ShadowBrokerSnapshot broker =
                ReadBrokerSnapshot();

            ShadowHostResult result =
                _shadow.Observe(
                    snapshot,
                    broker,
                    ContractVersion.Current,
                    nowUtc);

            LogStateIfChanged(result);

            if (!EnableMarketExecution ||
                result == null ||
                result.State != ShadowHostState.Ready)
                return;

            BrokerExecutionReport report;
            string executionReason;

            _market.TryExecute(
                this,
                snapshot,
                UseMarketRange,
                NormalizeManagedExecutionLabel(),
                nowUtc,
                out report,
                out executionReason);

            if (report != null)
            {
                Print(
                    "CFIP MARKET EXECUTION | status={0} | " +
                    "action={1} | revision={2} | position={3} | " +
                    "reason={4}",
                    report.Status,
                    report.Action,
                    report.AttemptRevision,
                    report.BrokerPositionId.HasValue
                        ? report.BrokerPositionId.Value.ToString()
                        : "",
                    executionReason);
            }
        }

        private bool RefreshIndicatorBinding(
            bool force)
        {
            DateTime now =
                Server.TimeInUtc;

            if (!force &&
                now <
                _nextIndicatorBindingCheckUtc)
                return
                    !string.IsNullOrWhiteSpace(
                        _boundIndicatorInstanceId);

            _nextIndicatorBindingCheckUtc =
                now.AddMilliseconds(500);

            ChartIndicator chartIndicator;
            string reason;

            if (!CfipIndicatorChartBinding.TryFind(
                    this,
                    out chartIndicator,
                    out reason))
            {
                _boundIndicatorInstanceId = "";
                _boundManagedExecutionLabel =
                    "CFIP-SMART";
                _indicatorBindingReason = reason;
                _state =
                    ShadowHostState.Blocked;

                Print(
                    "CFIP ANALYSIS BIND | state=BLOCKED | " +
                    "reason={0}",
                    reason);

                return false;
            }

            string instanceId =
                chartIndicator.InstanceId ?? "";

            if (!force &&
                string.Equals(
                    _boundIndicatorInstanceId,
                    instanceId,
                    StringComparison.Ordinal))
                return true;

            string managedExecutionLabel;
            if (!CfipIndicatorChartBinding.TryGetManagedExecutionLabel(
                    chartIndicator,
                    out managedExecutionLabel,
                    out reason))
            {
                _boundIndicatorInstanceId = "";
                _boundManagedExecutionLabel =
                    "CFIP-SMART";
                _indicatorBindingReason = reason;
                _state =
                    ShadowHostState.Blocked;

                Print(
                    "CFIP ANALYSIS BIND | state=BLOCKED | " +
                    "reason={0}",
                    reason);

                return false;
            }

            _boundIndicatorInstanceId =
                instanceId;

            _boundManagedExecutionLabel =
                string.IsNullOrWhiteSpace(
                    managedExecutionLabel)
                    ? "CFIP-SMART"
                    : managedExecutionLabel.Trim();

            _indicatorBindingReason =
                "BOUND TO " +
                CfipIndicatorChartBinding.DisplayName;

            Print(
                "CFIP ANALYSIS BIND | state=READY | " +
                "name={0} | instance={1} | executionLabel={2}",
                CfipIndicatorChartBinding.DisplayName,
                _boundIndicatorInstanceId,
                _boundManagedExecutionLabel);

            return true;
        }

        private void ReloadSignalStore(
            bool force)
        {
            DateTime now =
                Server.TimeInUtc;

            if (!force &&
                now < _nextSignalReloadUtc)
                return;

            _nextSignalReloadUtc =
                now.AddMilliseconds(750);

            try
            {
                CfipDeviceSignalTransport.Reload(
                    this);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SIGNAL STORE RELOAD FAILED | {0}",
                    ex.Message);
            }
        }

        private bool IsFresh(
            SignalEnvelope envelope)
        {
            if (envelope == null)
                return false;

            double ageSeconds =
                (Server.TimeInUtc -
                 envelope.ObservedUtc)
                .TotalSeconds;

            return
                ageSeconds >= 0 &&
                ageSeconds <=
                    Math.Max(
                        1,
                        ProviderStaleAfterSeconds);
        }

        private void LogBlockedState(
            string reason)
        {
            if (string.Equals(
                    _lastLoggedReason,
                    reason,
                    StringComparison.Ordinal) &&
                _state ==
                    ShadowHostState.Blocked)
                return;

            _state =
                ShadowHostState.Blocked;

            _lastLoggedReason =
                reason ?? "";

            Print(
                "CFIP cBot STATE | state=BLOCKED | " +
                "reason={0} | indicator={1} | tickCount={2}",
                _lastLoggedReason,
                string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId)
                    ? "NONE"
                    : _boundIndicatorInstanceId,
                _tickCount);
        }

        private ShadowBrokerSnapshot ReadBrokerSnapshot()
        {
            string executionLabel =
                NormalizeManagedExecutionLabel();

            int managedPositions = 0;

            foreach (Position position in Positions)
            {
                if (position != null &&
                    string.Equals(
                        position.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    ManagedExecutionLabelRule.Matches(
                        position.Label,
                        executionLabel))
                {
                    managedPositions++;
                }
            }

            int managedPendingOrders = 0;

            string pendingLabel =
                executionLabel +
                "-PENDING";

            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    ManagedExecutionLabelRule.Matches(
                        order.Label,
                        pendingLabel))
                {
                    managedPendingOrders++;
                }
            }

            return new ShadowBrokerSnapshot(
                managedPositions,
                managedPendingOrders,
                SymbolName ?? "",
                Symbol.Bid,
                Symbol.Ask,
                Symbol.PipSize);
        }

        private string NormalizeManagedExecutionLabel()
        {
            return
                string.IsNullOrWhiteSpace(
                    _boundManagedExecutionLabel)
                    ? "CFIP-SMART"
                    : _boundManagedExecutionLabel.Trim();
        }

        private void LogStateIfChanged(
            ShadowHostResult result)
        {
            if (result == null)
                return;

            if (result.State == _state &&
                result.Revision ==
                    _lastLoggedRevision &&
                string.Equals(
                    result.Reason,
                    _lastLoggedReason,
                    StringComparison.Ordinal))
                return;

            _state =
                result.State;

            _lastLoggedRevision =
                result.Revision;

            _lastLoggedReason =
                result.Reason ?? "";

            Print(
                "CFIP SHADOW STATE | state={0} | reason={1} | " +
                "revision={2} | signal={3} | scenario={4} | plan={5} | " +
                "key={6} | indicator={7} | tickCount={8}",
                result.State,
                result.Reason,
                result.Revision,
                result.SignalId ?? "",
                result.ScenarioId ?? "",
                result.PlanId ?? "",
                result.IdempotencyKey ?? "",
                _boundIndicatorInstanceId,
                _tickCount);
        }

        protected override void OnStop()
        {
            Print(
                "CFIP Execution cBot STOP | state={0} | " +
                "marketExecution={1} | indicator={2} | " +
                "lastRevision={3} | sessionMs={4}",
                _state,
                EnableMarketExecution
                    ? "ARMED"
                    : "DISARMED",
                _boundIndicatorInstanceId,
                _shadow.LastAcceptedRevision,
                (Server.TimeInUtc -
                 _startedUtc)
                    .TotalMilliseconds);
        }
    }
}
