using System;
using cAlgo.API;

namespace cAlgo.Robots
{
    // Test-only, NO-TRADE capability gate.
    // Required references in cTrader: CFIPIndicator and CFIPPreflightProbeIndicator.
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CFIPPreflightBot : Robot
    {
        private CFIPIndicator _cfip;
        private CFIPPreflightProbeIndicator _probe;
        private bool _probeCalculated;
        private bool _cfipCreated;
        private string _result = "NOT RUN";

        protected override void OnStart()
        {
            Print("CFIP PREFLIGHT START | NO TRADE");

            try
            {
                _probe = Indicators.GetIndicator<CFIPPreflightProbeIndicator>();
                // Access Output first. cTrader documents that consuming an Output
                // causes the referenced custom indicator to calculate.
                double probeValue = _probe.Probe.LastValue;
                _probeCalculated =
                    _probe.Revision > 0 &&
                    !double.IsNaN(probeValue) &&
                    !double.IsInfinity(probeValue) &&
                    _probe.Scope == SymbolName + "|" + TimeFrame;

                Print(
                    "PROBE | calculated={0} | revision={1} | value={2} | scope={3}",
                    _probeCalculated,
                    _probe.Revision,
                    probeValue,
                    _probe.Scope);
            }
            catch (Exception ex)
            {
                Print("PROBE FAIL | {0}", ex.Message);
            }

            try
            {
                // Current CFIP is intentionally instantiated with automatic execution
                // disabled in this capability test. This does not arm broker execution.
                _cfip = Indicators.GetIndicator<CFIPIndicator>(
                    new
                    {
                        EnableAutoTrading = false,
                        EnableAutomaticOrders = false,
                        EnableAggressiveAutoEntry = false,
                        AutoProtectBrokerPositions = false
                    });

                _cfipCreated = _cfip != null;
                Print("CFIP INSTANCE | created={0}", _cfipCreated);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP INSTANCE FAIL | {0}",
                    ex.Message);
            }

            _result =
                _probeCalculated && _cfipCreated
                    ? "HOST+INSTANCE PASS"
                    : "BLOCKED";

            Print(
                "CFIP PREFLIGHT RESULT | {0} | NO BROKER MUTATION EXECUTED",
                _result);
        }

        protected override void OnTick()
        {
            // Read-only liveness only. No order/position mutation APIs exist in
            // this test cBot by design.
            if (_probe == null)
                return;

            double value = _probe.Probe.LastValue;

            Print(
                "PREFLIGHT HEARTBEAT | result={0} | probeRevision={1} | probe={2}",
                _result,
                _probe.Revision,
                value);
        }

        protected override void OnStop()
        {
            Print(
                "CFIP PREFLIGHT STOP | result={0} | cfipCreated={1} | probeCalculated={2}",
                _result,
                _cfipCreated,
                _probeCalculated);
        }
    }
}