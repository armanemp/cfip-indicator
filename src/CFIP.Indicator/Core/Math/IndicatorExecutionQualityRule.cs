using System;

namespace cAlgo
{
    internal enum IndicatorQualityGateStage
    {
        AutomaticMarket,
        PendingSubmission,
        PendingContinuation,
        PendingReversal
    }

    internal readonly struct IndicatorQualityGateResult
    {
        public bool Allowed { get; }
        public string Reason { get; }
        public int MinimumQuality { get; }
        public int MaximumConflict { get; }

        public IndicatorQualityGateResult(
            bool allowed,
            string reason,
            int minimumQuality,
            int maximumConflict)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
            MinimumQuality = minimumQuality;
            MaximumConflict = maximumConflict;
        }
    }

    internal static class IndicatorExecutionQualityRule
    {
        internal const int AutomaticMarketQualityMinimum = 60;
        internal const int AutomaticMarketConflictMaximum = 52;

        internal const int PendingSubmissionQualityMinimum = 58;
        internal const int PendingSubmissionConflictMaximum = 55;

        internal const int PendingSetupQualityMinimum = 62;
        internal const int PendingContinuationConflictMaximum = 48;
        internal const int PendingReversalConflictMaximum = 50;

        public static IndicatorQualityGateResult Evaluate(
            IndicatorQualityGateStage stage,
            int quality,
            int conflict)
        {
            int minimumQuality;
            int maximumConflict;

            ResolveThresholds(
                stage,
                out minimumQuality,
                out maximumConflict);

            if (quality < minimumQuality)
            {
                return new IndicatorQualityGateResult(
                    false,
                    "INDICATOR Q " +
                    quality +
                    " < " +
                    minimumQuality +
                    " • " +
                    StageText(stage),
                    minimumQuality,
                    maximumConflict);
            }

            if (conflict > maximumConflict)
            {
                return new IndicatorQualityGateResult(
                    false,
                    "INDICATOR CONFLICT " +
                    conflict +
                    " > " +
                    maximumConflict +
                    " • " +
                    StageText(stage),
                    minimumQuality,
                    maximumConflict);
            }

            return new IndicatorQualityGateResult(
                true,
                string.Empty,
                minimumQuality,
                maximumConflict);
        }

        private static void ResolveThresholds(
            IndicatorQualityGateStage stage,
            out int minimumQuality,
            out int maximumConflict)
        {
            switch (stage)
            {
                case IndicatorQualityGateStage.AutomaticMarket:
                    minimumQuality =
                        AutomaticMarketQualityMinimum;
                    maximumConflict =
                        AutomaticMarketConflictMaximum;
                    return;

                case IndicatorQualityGateStage.PendingSubmission:
                    minimumQuality =
                        PendingSubmissionQualityMinimum;
                    maximumConflict =
                        PendingSubmissionConflictMaximum;
                    return;

                case IndicatorQualityGateStage.PendingContinuation:
                    minimumQuality =
                        PendingSetupQualityMinimum;
                    maximumConflict =
                        PendingContinuationConflictMaximum;
                    return;

                case IndicatorQualityGateStage.PendingReversal:
                    minimumQuality =
                        PendingSetupQualityMinimum;
                    maximumConflict =
                        PendingReversalConflictMaximum;
                    return;

                default:
                    minimumQuality =
                        100;
                    maximumConflict =
                        0;
                    return;
            }
        }

        private static string StageText(
            IndicatorQualityGateStage stage)
        {
            switch (stage)
            {
                case IndicatorQualityGateStage.AutomaticMarket:
                    return "MARKET";

                case IndicatorQualityGateStage.PendingSubmission:
                    return "PENDING SUBMISSION";

                case IndicatorQualityGateStage.PendingContinuation:
                    return "PENDING CONTINUATION";

                case IndicatorQualityGateStage.PendingReversal:
                    return "PENDING REVERSAL";

                default:
                    return "UNKNOWN";
            }
        }
    }
}
