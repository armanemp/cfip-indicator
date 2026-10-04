using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int CompactPlanLabelMinimumGapBars = 1;

        private int GetCompactPlanLabelAnchorBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            // One canonical horizontal label-gap owner. Respect the public
            // offset setting but never allow less than the deterministic
            // minimum gap from the line start.
            int offset =
                Math.Max(
                    CompactPlanLabelMinimumGapBars,
                    LabelLeftOffsetBars);

            int lineLeft =
                GetPlanLineLeftBar();

            return Math.Max(
                0,
                lineLeft - offset);
        }
    }
}