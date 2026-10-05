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
                    ExecutionCapacityRule.SupportedMaximumOpenPositions,
                    _plan != null,
                    ManagedPositionCount(),
                    ManagedPendingOrderCount()))
            {
                reason =
                    ResolveCapacityBlockReason(
                        includeActivePlan: true);
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private bool ValidateSingleExecutionCapacity(
            out string reason)
        {
            if (!ExecutionCapacityRule.AllowsNewSingleExecution(
                    ExecutionCapacityRule.SupportedMaximumOpenPositions,
                    ManagedPositionCount(),
                    ManagedPendingOrderCount()))
            {
                reason =
                    ResolveCapacityBlockReason(
                        includeActivePlan: false);
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private string ResolveCapacityBlockReason(
            bool includeActivePlan)
        {
            if (!ExecutionCapacityRule.IsSupportedSinglePlanCapacity(
                    ExecutionCapacityRule.SupportedMaximumOpenPositions))
                return
                    "UNSUPPORTED CAPACITY • SINGLE PLAN ONLY";

            if (includeActivePlan &&
                _plan != null)
                return
                    "SINGLE ACTIVE PLAN EXISTS";

            if (ManagedPositionCount() >=
                ExecutionCapacityRule.SupportedMaximumOpenPositions)
                return
                    "MANAGED POSITION CAPACITY REACHED";

            if (ManagedPendingOrderCount() > 0)
                return
                    "MANAGED PENDING CAPACITY REACHED";

            return
                "EXECUTION CAPACITY BLOCKED";
        }
    }
}
