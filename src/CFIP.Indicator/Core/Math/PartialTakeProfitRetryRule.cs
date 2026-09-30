namespace cAlgo
{
    internal static class PartialTakeProfitRetryRule
    {
        // One mutation attempt per logical stage and canonical closed M5 bar.
        // A later bar opens the next retry window; the same bar cannot spam
        // the broker with duplicate close requests.
        public static bool ShouldAttempt(
            int closedM5,
            int lastAttemptM5)
        {
            return
                closedM5 >= 0 &&
                closedM5 != lastAttemptM5;
        }

        public static bool ShouldAttemptStage(
            int closedM5,
            int lastAttemptM5,
            string lastStage,
            string stage)
        {
            if (closedM5 < 0 ||
                string.IsNullOrWhiteSpace(stage))
                return false;

            return
                closedM5 != lastAttemptM5 ||
                !string.Equals(
                    lastStage,
                    stage,
                    System.StringComparison.Ordinal);
        }
    }
}
