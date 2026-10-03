using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int GetCompactPlanLabelAnchorBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            // Native-style level labels are anchored to the exact endpoint
            // of their canonical plan line. The label box itself is attached
            // to that endpoint by PlanLabelRenderer.
            return GetPlanLineRightBar();
        }
    }
}
