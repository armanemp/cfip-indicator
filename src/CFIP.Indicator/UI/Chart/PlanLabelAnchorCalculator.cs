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

            // Anchor input delegates to the canonical line geometry.
            // PlanLabelRenderer owns the actual left-of-line gap and width.
            return GetPlanLineLeftBar();
        }
    }
}
