namespace cAlgo
{
    internal static class ExecutionCapacityRule
    {
        public static bool IsSupportedSinglePlanCapacity(
            int maximumOpenPositions)
        {
            return maximumOpenPositions == 1;
        }
    }
}
