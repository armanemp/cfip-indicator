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

            // Canonical plan labels share the exact left endpoint of the
            // canonical plan-level line geometry.
            return GetPlanLineLeftBar();
        }
    }
}