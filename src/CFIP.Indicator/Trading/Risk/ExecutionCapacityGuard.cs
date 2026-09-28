// CFIP Indicator — ExecutionCapacityGuard.cs
// Single-responsibility guard for the current single-plan execution model.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidateConfiguredPositionCapacity(out string reason)
        {
            if (!ExecutionCapacityRule.IsSupportedSinglePlanCapacity(
                    MaximumOpenPositions))
            {
                reason =
                    "MULTI-POSITION DISABLED • ACTIVE PLAN IS SINGLE-POSITION";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}
