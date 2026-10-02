using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TrySelectExecutionZoneCandidate(
            int closedM5,
            int direction,
            double atr,
            double market,
            out double low,
            out double high,
            out string source,
            out int quality)
        {
            return TrySelectExecutionZoneCandidateCore(
                closedM5,
                direction,
                atr,
                market,
                out low,
                out high,
                out source,
                out quality);
        }

        // Architectural invariant: canonical M5 Order Block geometry must remain
        // sourced from the detector's Low/High values, never synthetic bounds.
        private bool IsCanonicalM5OrderBlockGeometry(Zone m5Ob)
        {
            if (m5Ob == null)
                return false;

            double canonicalLow = m5Ob.Low;
            double canonicalHigh = m5Ob.High;

            return
                IsFinitePositive(canonicalLow) &&
                IsFinitePositive(canonicalHigh) &&
                canonicalHigh > canonicalLow;
        }

















    }
}
