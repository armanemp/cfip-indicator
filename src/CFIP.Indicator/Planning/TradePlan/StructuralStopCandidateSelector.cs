using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double SelectStructuralStopCandidate(
            List<Level> candidates,
            int closedM5,
            int direction,
            double entry,
            double atr,
            out string source,
            out int quality)
        {
            source = "NONE";
            quality = 0;

            if (candidates == null ||
                candidates.Count == 0)
                return 0;

            if (!TrySelectBestStructuralStopCandidate(
                    candidates,
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out Level best,
                    out double selectedStop,
                    out source,
                    out quality))
                return 0;

            return selectedStop;
        }
    }
}
