using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ZoneBlocksRewardPath(
            double low,
            double high,
            double entry,
            double target,
            double clearance)
        {
            if (low >= high)
                return false;

            double pathLow =
                Math.Min(
                    entry,
                    target);

            double pathHigh =
                Math.Max(
                    entry,
                    target);

            if (high <=
                pathLow +
                clearance ||
                low >=
                pathHigh -
                clearance)
                return false;

            bool containsTarget =
                target >=
                    low - clearance &&
                target <=
                    high + clearance;

            if (containsTarget)
                return false;

            bool containsEntry =
                entry >=
                    low - clearance &&
                entry <=
                    high + clearance;

            if (containsEntry)
                return false;

            return
                high >
                pathLow + clearance &&
                low <
                pathHigh - clearance;
        }
    }
}
