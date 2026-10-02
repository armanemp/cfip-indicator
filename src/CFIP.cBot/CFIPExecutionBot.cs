using System;
using cAlgo.API;
using CFIP.Contracts;
using cAlgo;
using CFIP.cBot.Shadow;

namespace CFIP.cBot
{
    [Robot(
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
        private const string StartupState = "SHADOW";

        private CFIPIndicator _indicator;
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
                "CFIP cBot START | state={0} | broker mutation=DISARMED | " +
                "shadow host=ACTIVE | contractVersion={1}",
                StartupState,
                ContractVersion.Current);

            try
            {
                _indicator =
                    Indicators.GetIndicator<CFIPIndicator>(
                        new
                        {
                            EnableAutoTrading = false,
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
            bool tradingPermissionAllowed;

            try
            {
                tradingPermissionAllowed =
                    this.Permissions.TradingPermission.IsAllowed;
            }
            catch
            {
                tradingPermissionAllowed = false;
            }

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
                        ShadowHostValidator.ManagedLabel,
                        StringComparison.Ordinal))
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
                    string.Equals(
                        order.Label,
                        ShadowHostValidator.PendingManagedLabel,
                        StringComparison.Ordinal))
                {
                    managedPendingOrders++;
                }
            }

            return new ShadowBrokerSnapshot(
                tradingPermissionAllowed,
                managedPositions,
                managedPendingOrders,
                SymbolName ?? "",
                Symbol.Bid,
                Symbol.Ask,
                Symbol.PipSize);
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
                "CFIP cBot STOP | state={0} | broker mutation=DISARMED | " +
                "lastRevision={1} | sessionMs={2}",
                _state,
                _shadow.LastAcceptedRevision,
                (Server.TimeInUtc - _startedUtc).TotalMilliseconds);
        }
    }
}
