namespace cAlgo
{
    internal static class WatchReactionAlertRule
    {
        // These values preserve the established early-WATCH threshold semantics:
        // a minimum floor of 60 and a 4-point allowance below MinimumConfidence.
        internal const int EarlyWatchConfidenceFloor = 60;
        internal const int EarlyWatchConfidenceGap = 4;

        internal static int ResolveEarlyWatchMinimumConfidence(
            int minimumConfidence)
        {
            if (minimumConfidence < 0)
                return EarlyWatchConfidenceFloor;

            return System.Math.Max(
                EarlyWatchConfidenceFloor,
                minimumConfidence - EarlyWatchConfidenceGap);
        }

        internal static bool IsStrongWatch(
            int direction,
            int confidence,
            int smartQuality,
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int minimumConfidence,
            int minimumSmartQuality,
            int smartQualityThreshold,
            int minimumTimeframeAgreement,
            int smartMinimumTimeframeAgreement,
            int minimumIndependentEvidence,
            int minimumStructuralConfirmations)
        {
            if ((direction != 1 &&
                 direction != -1) ||
                confidence < 0 ||
                smartQuality < 0 ||
                timeframeAgreement < 0 ||
                independentEvidence < 0 ||
                structuralConfirmations < 0)
                return false;

            int minimumWatchConfidence =
                ResolveEarlyWatchMinimumConfidence(
                    minimumConfidence);

            int minimumWatchSmartQuality =
                System.Math.Max(
                    0,
                    System.Math.Max(
                        minimumSmartQuality,
                        smartQualityThreshold));

            int minimumWatchTimeframeAgreement =
                System.Math.Max(
                    0,
                    System.Math.Max(
                        minimumTimeframeAgreement,
                        smartMinimumTimeframeAgreement));

            int minimumWatchIndependentEvidence =
                System.Math.Max(
                    0,
                    minimumIndependentEvidence);

            int minimumWatchStructuralConfirmations =
                System.Math.Max(
                    0,
                    minimumStructuralConfirmations);

            return
                confidence >= minimumWatchConfidence &&
                smartQuality >= minimumWatchSmartQuality &&
                timeframeAgreement >= minimumWatchTimeframeAgreement &&
                independentEvidence >= minimumWatchIndependentEvidence &&
                structuralConfirmations >= minimumWatchStructuralConfirmations;
        }

        internal static bool IsWatchAlertEligible(
            bool alertEnabled,
            bool entryAllowed,
            bool actionableNow,
            bool hasPlan,
            bool hasPendingOrder,
            bool hasLivePosition,
            int direction,
            int confidence,
            int smartQuality,
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int minimumConfidence,
            int minimumSmartQuality,
            int smartQualityThreshold,
            int minimumTimeframeAgreement,
            int smartMinimumTimeframeAgreement,
            int minimumIndependentEvidence,
            int minimumStructuralConfirmations)
        {
            if (!alertEnabled ||
                !entryAllowed ||
                actionableNow ||
                hasPlan ||
                hasPendingOrder ||
                hasLivePosition)
                return false;

            if ((direction != 1 &&
                 direction != -1) ||
                minimumConfidence < 0)
                return false;

            int minimumWatchConfidence =
                ResolveEarlyWatchMinimumConfidence(
                    minimumConfidence);

            return
                confidence >= minimumWatchConfidence &&
                confidence < minimumConfidence &&
                IsStrongWatch(
                    direction,
                    confidence,
                    smartQuality,
                    timeframeAgreement,
                    independentEvidence,
                    structuralConfirmations,
                    minimumConfidence,
                    minimumSmartQuality,
                    smartQualityThreshold,
                    minimumTimeframeAgreement,
                    smartMinimumTimeframeAgreement,
                    minimumIndependentEvidence,
                    minimumStructuralConfirmations);
        }

        internal static bool IsReactionAlertEligible(
            bool alertEnabled,
            bool liveReactionEnabled,
            bool entryAllowed,
            bool hasPlan,
            bool hasPendingOrder,
            bool hasLivePosition,
            bool rangeAllowed,
            int direction,
            int confidence,
            int independentEvidence,
            int minimumConfidence,
            int minimumEvidence)
        {
            if (!alertEnabled ||
                !liveReactionEnabled ||
                !entryAllowed ||
                hasPlan ||
                hasPendingOrder ||
                hasLivePosition ||
                !rangeAllowed)
                return false;

            if ((direction != 1 &&
                 direction != -1) ||
                confidence < 0 ||
                independentEvidence < 0 ||
                minimumConfidence < 0 ||
                minimumEvidence < 0)
                return false;

            return
                confidence >= minimumConfidence &&
                independentEvidence >=
                    System.Math.Max(
                        2,
                        minimumEvidence);
        }

        internal static string BuildWatchAlertKey(
            int closedM5,
            int direction)
        {
            return
                "WATCH|" +
                closedM5 +
                "|" +
                direction;
        }

        internal static string BuildReactionAlertKey(
            int reactionM5,
            int direction)
        {
            return
                "REACTION|" +
                reactionM5 +
                "|" +
                direction;
        }
    }
}
