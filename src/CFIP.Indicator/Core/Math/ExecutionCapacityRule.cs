namespace cAlgo
{
    internal static class ExecutionCapacityRule
    {
        public static bool AllowsNewSinglePlan(
            bool hasManagedOpenPosition)
        {
            return !hasManagedOpenPosition;
        }
    }
}
