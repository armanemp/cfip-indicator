using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int GetCompactPlanLabelAnchorBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            // Labels are anchored to the LEFT endpoint of the exact same
            // line geometry. The label then sits immediately before the line,
            // which keeps the annotation visually integrated with the level
            // instead of floating on the chart's right edge.
            return GetPlanLineLeftBar();
        }
    }
}