// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ZoneRecord
        {
            public string Id { get; private set; }
            public CFIPClean89ZoneKind Kind { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public string Timeframe { get; private set; }
            public int CreatedIndex { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public double OriginalLower { get; private set; }
            public double OriginalUpper { get; private set; }
            public double CurrentLower { get; private set; }
            public double CurrentUpper { get; private set; }
            public int AgeBars { get; private set; }
            public bool Retested { get; private set; }
            public bool Mitigated { get; private set; }
            public bool Invalidated { get; private set; }
            public bool Consumed { get; private set; }
            public bool ExecutionEligible { get; private set; }
            public int Quality { get; private set; }
            public double DisplacementAtr { get; private set; }
            public bool LiquidityConfluence { get; private set; }
            public bool FvgConfluence { get; private set; }
            public CFIPClean89ZoneLifecycle Lifecycle { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89ZoneRecord(
                string id, CFIPClean89ZoneKind kind,
                CFIPClean89Direction direction, string timeframe,
                int createdIndex, DateTime createdUtc,
                double originalLower, double originalUpper,
                double currentLower, double currentUpper,
                int ageBars, bool retested, bool mitigated,
                bool invalidated, bool consumed, bool executionEligible,
                int quality, double displacementAtr,
                bool liquidityConfluence, bool fvgConfluence,
                CFIPClean89ZoneLifecycle lifecycle,
                CFIPClean89Provenance provenance)
            {
                Id = id ?? string.Empty; Kind = kind; Direction = direction;
                Timeframe = timeframe ?? string.Empty; CreatedIndex = createdIndex;
                CreatedUtc = createdUtc; OriginalLower = originalLower; OriginalUpper = originalUpper;
                CurrentLower = currentLower; CurrentUpper = currentUpper;
                AgeBars = Math.Max(0, ageBars); Retested = retested; Mitigated = mitigated;
                Invalidated = invalidated; Consumed = consumed; ExecutionEligible = executionEligible;
                Quality = Math.Max(0, Math.Min(100, quality));
                DisplacementAtr = Math.Max(0, displacementAtr);
                LiquidityConfluence = liquidityConfluence; FvgConfluence = fvgConfluence;
                Lifecycle = lifecycle;
                Provenance = provenance ?? CFIPClean89Provenance.Direct("ZONE", kind.ToString());
            }
    
            public bool Contains(double price)
            {
                return price >= CurrentLower && price <= CurrentUpper;
            }
        }
}
