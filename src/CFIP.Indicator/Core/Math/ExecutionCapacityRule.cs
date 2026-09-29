namespace cAlgo
{
    internal static class ExecutionCapacityRule
    {
        internal const int SupportedMaximumOpenPositions = 1;

        public static bool IsSupportedSinglePlanCapacity(
            int maximumOpenPositions)
        {
            return maximumOpenPositions ==
                   SupportedMaximumOpenPositions;
        }

        public static bool AllowsNewSinglePlan(
            int maximumOpenPositions,
            bool hasActivePlan,
            int managedPositionCount,
            int managedPendingOrderCount)
        {
            return
                IsSupportedSinglePlanCapacity(
                    maximumOpenPositions) &&
                !hasActivePlan &&
                managedPositionCount <
                    SupportedMaximumOpenPositions &&
                managedPendingOrderCount == 0;
        }

        public static bool AllowsNewSingleExecution(
            int maximumOpenPositions,
            int managedPositionCount,
            int managedPendingOrderCount)
        {
            return
                IsSupportedSinglePlanCapacity(
                    maximumOpenPositions) &&
                managedPositionCount <
                    SupportedMaximumOpenPositions &&
                managedPendingOrderCount == 0;
        }
    }
}
