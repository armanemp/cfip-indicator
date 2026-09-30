namespace cAlgo
{
    internal enum LiveReversalAction
    {
        None = 0,
        DetectedRetain = 1,
        ExitRequested = 2,
        AwaitBrokerConfirmation = 3,
        ReconcileBrokerState = 4
    }

    internal static class LiveReversalDecisionRule
    {
        public static int OppositeDirection(int positionDirection)
        {
            if (positionDirection == 1)
                return -1;

            if (positionDirection == -1)
                return 1;

            return 0;
        }

        public static bool IsOppositeEvidenceDirection(
            int positionDirection,
            int evidenceDirection)
        {
            int opposite =
                OppositeDirection(positionDirection);

            return opposite != 0 &&
                   evidenceDirection == opposite;
        }

        public static int ResolveDirectionalConfidence(
            int positionDirection,
            int reactionDirection,
            int reactionConfidence,
            int frameDirection,
            int frameQuality)
        {
            int opposite =
                OppositeDirection(positionDirection);

            if (opposite == 0)
                return 0;

            int confidence = 0;

            if (reactionDirection == opposite &&
                reactionConfidence > confidence)
            {
                confidence = reactionConfidence;
            }

            if (frameDirection == opposite &&
                frameQuality > confidence)
            {
                confidence = frameQuality;
            }

            return NumericGuards.ClampInt(
                confidence,
                0,
                100);
        }

        public static LiveReversalAction ResolveAction(
            bool hasManagedPosition,
            bool closeEnabled,
            bool profitable,
            bool exitAlreadyRequested)
        {
            if (!hasManagedPosition)
                return LiveReversalAction.ReconcileBrokerState;

            if (exitAlreadyRequested)
                return LiveReversalAction.AwaitBrokerConfirmation;

            if (!closeEnabled || !profitable)
                return LiveReversalAction.DetectedRetain;

            return LiveReversalAction.ExitRequested;
        }
    }
}
