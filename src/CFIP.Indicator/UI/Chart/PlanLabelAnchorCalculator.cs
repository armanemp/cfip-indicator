using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // The label's visible right edge is anchored exactly one chart bar
        // before the canonical signal-line start. Both line and label use
        // the same DateTime/OpenTime X coordinate system.
        private const int CompactPlanLabelGapBars = 1;

        private DateTime GetCompactPlanLabelAnchorTime()
        {
            return GetCompactPlanLabelAnchorTime(
                GetPlanLineLeftBar());
        }

        private DateTime GetCompactPlanLabelAnchorTime(
            int lineLeftBar)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return DateTime.MinValue;

            int canonicalLineLeftBar =
                Math.Max(
                    1,
                    Math.Min(
                        Bars.Count - 1,
                        lineLeftBar));

            int anchorBar =
                Math.Max(
                    0,
                    canonicalLineLeftBar -
                    CompactPlanLabelGapBars);

            return Bars.OpenTimes[anchorBar];
        }
    }
}
