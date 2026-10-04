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
        private long _tickCount;
        private DateTime _startedUtc;
        private DateTime _firstTickUtc;
        private DateTime _lastHeartbeatUtc;
        private int _lastObservedProbeRevision;
        private string _result = "NOT RUN";
        private const double MaxProbeAgeSeconds = 15;

        protected override void OnStart()
        {
            _startedUtc = Server.TimeInUtc;
            _lastHeartbeatUtc = _startedUtc;
            _tickCount = 0;
            _firstTickUtc = DateTime.MinValue;
            _lastObservedProbeRevision = 0;

            Print("CFIP PREFLIGHT START | NO TRADE | startedUtc={0}", _startedUtc.ToString("O"));

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
                    "PROBE | calculated={0} | revision={1} | value={2} | scope={3} | firstCalcUtc={4} | lastCalcUtc={5}",
                    _probeCalculated,
                    _probe.Revision,
                    probeValue,
                    _probe.Scope,
                    _probe.FirstCalculatedUtc.ToString("O"),
                    _probe.LastCalculatedUtc.ToString("O"));

                _lastObservedProbeRevision = _probe.Revision;
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
            // Read-only liveness and real-terminal timing only. No order/position
            // mutation APIs exist in this test cBot by design.
            _tickCount++;
            DateTime now = Server.TimeInUtc;

            if (_firstTickUtc == DateTime.MinValue)
            {
                _firstTickUtc = now;
                Print(
                    "FIRST TICK | utc={0} | startupToFirstTickMs={1} | symbol={2} | timeframe={3}",
                    now.ToString("O"),
                    (now - _startedUtc).TotalMilliseconds,
                    SymbolName,
                    TimeFrame);
            }

            if (_probe == null)
                return;

            double value = _probe.Probe.LastValue;
            bool finiteQuote =
                !double.IsNaN(Symbol.Bid) &&
                !double.IsInfinity(Symbol.Bid) &&
                !double.IsNaN(Symbol.Ask) &&
                !double.IsInfinity(Symbol.Ask) &&
                Symbol.Ask > 0 &&
                Symbol.Bid > 0;
            double probeAgeSeconds =
                _probe.LastCalculatedUtc == DateTime.MinValue
                    ? double.PositiveInfinity
                    : Math.Max(0, (now - _probe.LastCalculatedUtc).TotalSeconds);
            bool probeHealthy =
                _probe.Revision > 0 &&
                _probe.Scope == SymbolName + "|" + TimeFrame &&
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                probeAgeSeconds <= MaxProbeAgeSeconds &&
                finiteQuote;

            if (!probeHealthy)
                _result = "BLOCKED";
            double spreadPips =
                Symbol.PipSize > 0 && finiteQuote
                    ? Math.Abs(Symbol.Ask - Symbol.Bid) / Symbol.PipSize
                    : 0;

            if (_probe.Revision != _lastObservedProbeRevision ||
                (now - _lastHeartbeatUtc).TotalSeconds >= 5)
            {
                double calcAgeMs =
                    _probe.LastCalculatedUtc == DateTime.MinValue
                        ? -1
                        : (now - _probe.LastCalculatedUtc).TotalMilliseconds;

                Print(
                    "PREFLIGHT HEARTBEAT | result={0} | tickCount={1} | probeRevision={2} | probe={3} | probeCalcAgeMs={4} | probeHealthy={5} | bid={6} | ask={7} | spreadPips={8} | bars={9} | latestBarOpenUtc={10}",
                    _result,
                    _tickCount,
                    _probe.Revision,
                    value,
                    calcAgeMs,
                    probeHealthy,
                    Symbol.Bid,
                    Symbol.Ask,
                    spreadPips,
                    Bars.Count,
                    Bars.Count > 0 ? Bars.OpenTimes[Bars.Count - 1].ToString("O") : "NONE");

                _lastObservedProbeRevision = _probe.Revision;
                _lastHeartbeatUtc = now;
            }
        }

        protected override void OnStop()
        {
            DateTime stoppedUtc = Server.TimeInUtc;
            Print(
                "CFIP PREFLIGHT STOP | result={0} | cfipCreated={1} | probeCalculated={2} | tickCount={3} | startupToFirstTickMs={4} | sessionMs={5}",
                _result,
                _cfipCreated,
                _probeCalculated,
                _tickCount,
                _firstTickUtc == DateTime.MinValue ? -1 : (_firstTickUtc - _startedUtc).TotalMilliseconds,
                (stoppedUtc - _startedUtc).TotalMilliseconds);
        }
    }
}