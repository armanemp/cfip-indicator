using System;

namespace cAlgo
{
    internal enum PendingArbiterChoice
    {
        None = 0,
        ContinuationStop = 1,
        ReversalLimit = 2
    }

    internal readonly struct PendingArbiterResult
    {
        public PendingArbiterChoice Choice { get; }
        public int Direction { get; }
        public int ContinuationScore { get; }
        public int ReversalScore { get; }
        public string Reason { get; }

        public bool HasChoice
        {
            get { return Choice != PendingArbiterChoice.None; }
        }

        public PendingArbiterResult(
            PendingArbiterChoice choice,
            int direction,
            int continuationScore,
            int reversalScore,
            string reason)
        {
            Choice = choice;
            Direction = direction;
            ContinuationScore =
                Math.Max(0, continuationScore);
            ReversalScore =
                Math.Max(0, reversalScore);
            Reason = reason ?? string.Empty;
        }
    }

    internal static class PendingDecisionArbiterRule
    {
        public static int ScoreCandidate(
            int confidence,
            int smartQuality,
            int alignment,
            int evidence,
            int structuralConfirmations)
        {
            int normalizedConfidence =
                Math.Max(0, Math.Min(100, confidence));
            int normalizedSmart =
                Math.Max(0, Math.Min(100, smartQuality));
            int normalizedAlignment =
                Math.Max(0, Math.Min(100, alignment));
            int normalizedEvidence =
                Math.Max(0, Math.Min(8, evidence));
            int normalizedStructure =
                Math.Max(0, Math.Min(8, structuralConfirmations));

            double score =
                normalizedConfidence * 0.35 +
                normalizedSmart * 0.30 +
                normalizedAlignment * 0.15 +
                normalizedEvidence * 7.5 +
                normalizedStructure * 5.0;

            return Math.Max(
                0,
                Math.Min(
                    100,
                    (int)Math.Round(score)));
        }

        public static PendingArbiterResult SelectWinner(
            bool continuationEligible,
            int continuationDirection,
            int continuationScore,
            bool reversalEligible,
            int reversalDirection,
            int reversalScore,
            PendingOrderMode mode)
        {
            bool continuationAllowed =
                continuationEligible &&
                AllowsContinuation(mode);

            bool reversalAllowed =
                reversalEligible &&
                AllowsReversal(mode);

            if (!continuationAllowed &&
                !reversalAllowed)
            {
                return new PendingArbiterResult(
                    PendingArbiterChoice.None,
                    0,
                    continuationScore,
                    reversalScore,
                    "NO ELIGIBLE PENDING SETUP");
            }

            if (continuationAllowed &&
                !reversalAllowed)
            {
                return new PendingArbiterResult(
                    PendingArbiterChoice.ContinuationStop,
                    continuationDirection,
                    continuationScore,
                    reversalScore,
                    "CONTINUATION STOP SELECTED");
            }

            if (reversalAllowed &&
                !continuationAllowed)
            {
                return new PendingArbiterResult(
                    PendingArbiterChoice.ReversalLimit,
                    reversalDirection,
                    continuationScore,
                    reversalScore,
                    "REVERSAL LIMIT SELECTED");
            }

            if (reversalScore > continuationScore)
            {
                return new PendingArbiterResult(
                    PendingArbiterChoice.ReversalLimit,
                    reversalDirection,
                    continuationScore,
                    reversalScore,
                    "REVERSAL LIMIT WINS BY QUALITY");
            }

            return new PendingArbiterResult(
                PendingArbiterChoice.ContinuationStop,
                continuationDirection,
                continuationScore,
                reversalScore,
                continuationScore == reversalScore
                    ? "CONTINUATION STOP WINS TIE"
                    : "CONTINUATION STOP WINS BY QUALITY");
        }

        public static bool IsSameChoice(
            PendingArbiterChoice choice,
            bool existingOrderIsStop)
        {
            if (choice == PendingArbiterChoice.ContinuationStop)
                return existingOrderIsStop;

            if (choice == PendingArbiterChoice.ReversalLimit)
                return !existingOrderIsStop;

            return false;
        }

        public static bool ShouldCancelAfterHysteresis(
            bool invalidated,
            int invalidationStreak,
            int requiredConsecutiveBars)
        {
            if (!invalidated)
                return false;

            return invalidationStreak >=
                   Math.Max(
                       2,
                       requiredConsecutiveBars);
        }

        private static bool AllowsContinuation(
            PendingOrderMode mode)
        {
            return mode ==
                       PendingOrderMode.Adaptive ||
                   mode ==
                       PendingOrderMode.ContinuationStop ||
                   mode ==
                       PendingOrderMode.Both;
        }

        private static bool AllowsReversal(
            PendingOrderMode mode)
        {
            return mode ==
                       PendingOrderMode.Adaptive ||
                   mode ==
                       PendingOrderMode.ReversalLimit ||
                   mode ==
                       PendingOrderMode.Both;
        }
    }
}
