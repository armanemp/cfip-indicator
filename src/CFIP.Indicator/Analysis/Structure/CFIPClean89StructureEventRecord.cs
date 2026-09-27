// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89StructureEventRecord
        {
            public string Id { get; private set; }
            public CFIPClean89StructureEventKind Kind { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public string Timeframe { get; private set; }
            public int BarIndex { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public double Price { get; private set; }
            public double StrengthAtr { get; private set; }
            public int Quality { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89StructureEventRecord(
                string id, CFIPClean89StructureEventKind kind,
                CFIPClean89Direction direction, string timeframe,
                int barIndex, DateTime timeUtc, double price,
                double strengthAtr, int quality,
                CFIPClean89Provenance provenance)
            {
                Id = id ?? string.Empty;
                Kind = kind; Direction = direction; Timeframe = timeframe ?? string.Empty;
                BarIndex = barIndex; TimeUtc = timeUtc; Price = price;
                StrengthAtr = Math.Max(0, strengthAtr);
                Quality = Math.Max(0, Math.Min(100, quality));
                Provenance = provenance ?? CFIPClean89Provenance.Direct("STRUCTURE", kind.ToString());
            }
        }
}
