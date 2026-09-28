namespace cAlgo
{
    internal static class LivePlanRecoveryRule
    {
        public static bool ShouldClearStaleLivePlan(
            bool planIsLivePosition,
            bool boundBrokerPositionExists)
        {
            return planIsLivePosition &&
                   !boundBrokerPositionExists;
        }
    }
}
