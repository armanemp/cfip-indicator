// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89LiquidityRecord
        {
            public string Id { get; private set; }
            public CFIPClean89LiquidityKind Kind { get; private set; }
            public CFIPClean89LiquiditySide Side { get; private set; }
            public CFIPClean89Direction SweepDirection { get; private set; }
            public string Timeframe { get; private set; }
            public int BarIndex { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public double Price { get; private set; }
            public double Tolerance { get; private set; }
            public double Distance { get; private set; }
            public double Penetration { get; private set; }
            public bool Swept { get; private set; }
            public bool ForecastCandidate { get; private set; }
            public int Quality { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89LiquidityRecord(
                string id, CFIPClean89LiquidityKind kind,
                CFIPClean89LiquiditySide side, CFIPClean89Direction sweepDirection,
                string timeframe, int barIndex, DateTime timeUtc,
                double price, double tolerance, double distance, double penetration,
                bool swept, bool forecastCandidate, int quality,
                CFIPClean89Provenance provenance)
            {
                Id = id ?? string.Empty; Kind = kind; Side = side; SweepDirection = sweepDirection;
                Timeframe = timeframe ?? string.Empty; BarIndex = barIndex; TimeUtc = timeUtc;
                Price = price; Tolerance = Math.Max(0, tolerance); Distance = Math.Max(0, distance);
                Penetration = Math.Max(0, penetration); Swept = swept;
                ForecastCandidate = forecastCandidate;
                Quality = Math.Max(0, Math.Min(100, quality));
                Provenance = provenance ?? CFIPClean89Provenance.Direct("LIQUIDITY", kind.ToString());
            }
        }
}
