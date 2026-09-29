// CFIP Indicator — ExecutionCapacityGuard.cs
// Single-responsibility guard for the current single-plan execution model.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidateSinglePlanCapacity(
            out string reason)
        {
            if (!ExecutionCapacityRule.AllowsNewSinglePlan(
                    HasManagedOpenPosition()))
            {
                reason =
                    "SINGLE ACTIVE PLAN EXISTS";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}
