using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int CompactPlanLabelGapBars = 2;
        private const int CompactPlanLabelWidthBars = 7;

        private int GetCompactPlanLabelAnchorBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int lineLeft =
                GetPlanLineLeftBar();

            return Math.Max(
                0,
                lineLeft -
                CompactPlanLabelGapBars -
                (CompactPlanLabelWidthBars / 2));
        }
    }
}
