namespace cAlgo
{
    internal static class OrphanManagedProtectionRule
    {
        public static bool CanReportSuccess(
            int direction,
            bool stopCandidateValid,
            bool brokerProtectionConfirmed)
        {
            if (direction != 1 && direction != -1)
                return false;

            return stopCandidateValid &&
                   brokerProtectionConfirmed;
        }
    }
}
