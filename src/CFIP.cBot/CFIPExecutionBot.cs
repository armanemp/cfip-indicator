using System;
using cAlgo.API;
using CFIP.Contracts;
using cAlgo;
using CFIP.cBot.Shadow;
using CFIP.cBot.Execution;

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

        private CFIPIndicator _indicator;

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

        protected override void OnStart()
        {
            _startedUtc = Server.TimeInUtc;
            _tickCount = 0;

            Print(
                "CFIP cBot START | state={0} | marketExecution={1} | " +
                "shadow host=ACTIVE | contractVersion={2}",
                StartupState,
                EnableMarketExecution ? "ARMED" : "DISARMED",
                ContractVersion.Current);

            try
            {
                _indicator =
                    Indicators.GetIndicator<CFIPIndicator>(
                        new
                        {
                            EnableAutoTrading = true,
                            EnableAutomaticOrders = false,
                            EnableAggressiveAutoEntry = false,
                            AutoProtectBrokerPositions = false,
                            EnableLiveExitManagement = false
                        });

                double heartbeat =
                    _indicator.ProviderHeartbeat.LastValue;

                Print(
                    "CFIP PROVIDER HOST | created={0} | heartbeat={1} | " +
                    "providerRevision={2} | state={3}",
                    _indicator != null,
                    heartbeat,
                    _indicator == null
                        ? -1
                        : _indicator.ProviderRevision,
                    _indicator == null
                        ? "UNAVAILABLE"
                        : _indicator.ProviderState);
            }
            catch (Exception ex)
            {
                _state = ShadowHostState.Blocked;

                Print(
                    "CFIP PROVIDER HOST FAIL | state=BLOCKED | reason={0}",
                    ex.Message);
            }
        }

        protected override void OnTick()
        {
            _tickCount++;

            if (_indicator == null)
            {
                LogStateIfChanged(
                    new ShadowHostResult(
                        ShadowHostState.Blocked,
                        "INDICATOR UNAVAILABLE",
                        false,
                        -1,
                        "",
                        "",
                        "",
                        ""));
                return;
            }

            try
            {
                double heartbeat =
                    _indicator.ProviderHeartbeat.LastValue;

                SignalEnvelope snapshot =
                    _indicator.LatestSignalEnvelope;

                ShadowBrokerSnapshot broker =
                    ReadBrokerSnapshot();

                ShadowHostResult result =
                    _shadow.Observe(
                        snapshot,
                        broker,
                        ContractVersion.Current,
                        _indicator.ProviderRevision,
                        Server.TimeInUtc);

                LogStateIfChanged(result);

                if (EnableMarketExecution &&
                    result != null &&
                    result.State == ShadowHostState.Ready)
                {
                    BrokerExecutionReport report;
                    string executionReason;

                    _market.TryExecute(
                            this,
                            snapshot,
                            UseMarketRange,
                            Server.TimeInUtc,
                            out report,
                            out executionReason);

                    if (report != null)
                    {
                        Print(
                            "CFIP MARKET EXECUTION | status={0} | action={1} | " +
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
            }
            catch (Exception ex)
            {
                _state = ShadowHostState.Blocked;

                Print(
                    "CFIP SHADOW HOST FAULT | state=BLOCKED | reason={0}",
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
                    IsManagedLabel(
                        position.Label,
                        ShadowHostValidator.ManagedLabel))
                {
                    managedPositions++;
                }
            }

            int managedPendingOrders = 0;
            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    IsManagedLabel(
                        order.Label,
                        ShadowHostValidator.PendingManagedLabel))
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

        private static bool IsManagedLabel(
            string label,
            string baseLabel)
        {
            if (string.IsNullOrWhiteSpace(label))
                return false;

            if (string.Equals(
                    label,
                    baseLabel,
                    StringComparison.Ordinal))
                return true;

            return label.StartsWith(
                baseLabel + "|CFIP-I:",
                StringComparison.Ordinal);
        }

        private void LogStateIfChanged(
            ShadowHostResult result)
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

            _state =
                result.State;

            _lastLoggedRevision =
                result.Revision;

            _lastLoggedReason =
                result.Reason ?? "";

            Print(
                "CFIP SHADOW STATE | state={0} | reason={1} | " +
                "revision={2} | signal={3} | scenario={4} | plan={5} | " +
                "key={6} | tickCount={7}",
                result.State,
                result.Reason,
                result.Revision,
                result.SignalId ?? "",
                result.ScenarioId ?? "",
                result.PlanId ?? "",
                result.IdempotencyKey ?? "",
                _tickCount);
        }

        protected override void OnStop()
        {
            Print(
                "CFIP cBot STOP | state={0} | marketExecution={1} | " +
                "lastRevision={2} | sessionMs={3}",
                _state,
                EnableMarketExecution ? "ARMED" : "DISARMED",
                _shadow.LastAcceptedRevision,
                (Server.TimeInUtc - _startedUtc).TotalMilliseconds);
        }
    }
}
