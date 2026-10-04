using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // The ChartText RIGHT anchor is the visible end of the label.
        // Keep exactly one full chart-bar interval between that end point
        // and the canonical left endpoint of the signal line.
        private const int CompactPlanLabelGapBars = 1;

        private int GetCompactPlanLabelAnchorBar()
        {
            return GetCompactPlanLabelAnchorBar(
                GetPlanLineLeftBar());
        }

        private int GetCompactPlanLabelAnchorBar(
            int lineLeftBar)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int canonicalLineLeftBar =
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        lineLeftBar));

            return Math.Max(
                0,
                canonicalLineLeftBar -
                CompactPlanLabelGapBars);
        }
    }
}
