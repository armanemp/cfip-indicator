using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // The canonical line and label share the same X coordinate system:
        // integer chart bar indices. The visible right edge of the native
        // right-aligned ChartText is anchored exactly one chart bar before
        // the canonical line start.
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
                    1,
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
