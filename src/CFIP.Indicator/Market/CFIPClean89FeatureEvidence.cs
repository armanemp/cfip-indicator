// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89FeatureEvidence
        {
            public CFIPClean89MarketFeature Feature { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public double Value { get; private set; }
            public int Weight { get; private set; }
            public bool Triggered { get; private set; }
            public bool CountsAsEvidence { get; private set; }
            public bool CountsAsGate { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89FeatureEvidence(
                CFIPClean89MarketFeature feature,
                CFIPClean89Direction direction,
                double value,
                int weight,
                bool triggered,
                bool countsAsEvidence,
                bool countsAsGate,
                CFIPClean89Provenance provenance)
            {
                Feature = feature;
                Direction = direction;
                Value = Clamp01(value);
                Weight = Math.Max(0, weight);
                Triggered = triggered;
                CountsAsEvidence = countsAsEvidence;
                CountsAsGate = countsAsGate;
                Provenance =
                    provenance ??
                    CFIPClean89Provenance.Direct(
                        "MARKET_MODEL",
                        feature.ToString());
            }
    
            private static double Clamp01(double value)
            {
                if (double.IsNaN(value) ||
                    double.IsInfinity(value))
                    return 0;
    
                return
                    Math.Max(
                        0,
                        Math.Min(
                            1,
                            value));
            }
        }
}
