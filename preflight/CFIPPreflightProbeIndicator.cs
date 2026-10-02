using System;
using cAlgo.API;

namespace cAlgo
{
    // Test-only fixture. Never used for trading.
    [Indicator(
        IsOverlay = false,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public class CFIPPreflightProbeIndicator : Indicator
    {
        [Output("Probe")]
        public IndicatorDataSeries Probe { get; set; }

        public int Revision { get; private set; }

        public string Scope =>
            SymbolName + "|" + TimeFrame;

        public DateTime LastCalculatedUtc { get; private set; }

        public DateTime FirstCalculatedUtc { get; private set; }

        protected override void Initialize()
        {
            Revision = 0;
            LastCalculatedUtc = DateTime.MinValue;
            FirstCalculatedUtc = DateTime.MinValue;
        }

        public override void Calculate(int index)
        {
            Probe[index] = index;
            Revision++;
            LastCalculatedUtc = Server.TimeInUtc;
            if (FirstCalculatedUtc == DateTime.MinValue)
                FirstCalculatedUtc = LastCalculatedUtc;
        }
    }
}