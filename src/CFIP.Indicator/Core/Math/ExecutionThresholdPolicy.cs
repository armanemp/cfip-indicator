namespace cAlgo
{
    /// <summary>
    /// Central owner for execution-path safety thresholds and defensive
    /// parameter boundaries. Values preserve the current production
    /// defaults and public Parameter MinValue/MaxValue declarations.
    /// </summary>
    internal static class ExecutionThresholdPolicy
    {
        internal const int AutomaticMarketIndicatorConfluenceMinimum = 60;
        internal const int AutomaticMarketIndicatorConflictMaximum = 52;
        internal const int PendingSubmissionIndicatorConfluenceMinimum = 58;
        internal const int PendingSubmissionIndicatorConflictMaximum = 55;
        internal const int PendingContinuationIndicatorConfluenceMinimum = 62;
        internal const int PendingContinuationIndicatorConflictMaximum = 48;
        internal const int PendingReversalIndicatorConfluenceMinimum = 62;
        internal const int PendingReversalIndicatorConflictMaximum = 50;

        internal static int NormalizeDirectionShare(int value)
        {
            return ClampInt(value, 50, 95);
        }

        internal static int NormalizeReversalEvidence(int value)
        {
            return ClampInt(value, 2, 10);
        }

        internal static int NormalizeReversalMtf(int value)
        {
            return ClampInt(value, 50, 100);
        }

        internal static int NormalizeEndOfDayAlertMinutesBefore(int value)
        {
            return ClampInt(value, 5, 180);
        }

        internal static double NormalizeMaximumSpreadToStopRiskRatio(double value)
        {
            return ClampDouble(value, 0.02, 0.50);
        }

        private static int ClampInt(int value, int minimum, int maximum)
        {
            if (value < minimum)
                return minimum;
            if (value > maximum)
                return maximum;
            return value;
        }

        private static double ClampDouble(double value, double minimum, double maximum)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return minimum;
            if (value < minimum)
                return minimum;
            if (value > maximum)
                return maximum;
            return value;
        }
    }
}
