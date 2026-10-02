using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot
{
    [Robot(
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
        private const string StartupState = "READ_ONLY_SHADOW";

        private CFIPIndicator _indicator;
        private long _lastObservedProviderRevision = -1;
        private string _lastObservedSignalId = "";

        protected override void OnStart()
        {
            Print(
                "CFIP cBot START | state={0} | broker mutation=DISARMED | " +
                "provider=READ-ONLY | contractVersion={1}",
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

                // cTrader may lazily evaluate a referenced custom indicator until
                // an Output is consumed. Heartbeat is intentionally invisible and
                // exists to make provider evaluation deterministic.
                double heartbeat =
                    _indicator.ProviderHeartbeat.LastValue;

                Print(
                    "CFIP PROVIDER | created={0} | heartbeat={1} | revision={2} | state={3}",
                    _indicator != null,
                    heartbeat,
                    _indicator.ProviderRevision,
                    _indicator.ProviderState);

                ObserveProviderSnapshot();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP PROVIDER FAIL | state=BLOCKED | reason={0}",
                    ex.Message);
            }
        }

        protected override void OnTick()
        {
            if (_indicator == null)
                return;

            // Reading the Output first is the supported liveness trigger for a
            // referenced custom indicator; no chart scraping/reflection is used.
            _indicator.ProviderHeartbeat.LastValue;

            ObserveProviderSnapshot();
        }

        private void ObserveProviderSnapshot()
        {
            SignalEnvelope snapshot =
                _indicator.LatestSignalEnvelope;

            long revision =
                _indicator.ProviderRevision;

            if (snapshot == null ||
                revision == _lastObservedProviderRevision)
                return;

            _lastObservedProviderRevision =
                revision;

            _lastObservedSignalId =
                snapshot.Identity == null
                    ? ""
                    : snapshot.Identity.SignalId ?? "";

            Print(
                "CFIP PROVIDER UPDATE | revision={0} | state={1} | stage={2} | " +
                "signal={3} | scenario={4} | plan={5} | action={6} | " +
                "direction={7} | sourceTf={8} | createdM5={9}",
                revision,
                _indicator.ProviderState,
                snapshot.Stage,
                _lastObservedSignalId,
                snapshot.Identity == null ? "" : snapshot.Identity.ScenarioId,
                snapshot.Identity == null ? "" : snapshot.Identity.PlanId,
                snapshot.Intent == null
                    ? "NONE"
                    : snapshot.Intent.Action.ToString(),
                snapshot.Identity == null
                    ? TradeDirection.None.ToString()
                    : snapshot.Identity.Direction.ToString(),
                snapshot.Identity == null
                    ? ""
                    : snapshot.Identity.SourceTimeframe,
                snapshot.Identity == null
                    ? -1
                    : snapshot.Identity.CreatedClosedM5);
        }

        protected override void OnStop()
        {
            Print(
                "CFIP cBot STOP | state={0} | broker mutation=DISARMED | " +
                "lastProviderRevision={1}",
                StartupState,
                _lastObservedProviderRevision);
        }
    }
}
