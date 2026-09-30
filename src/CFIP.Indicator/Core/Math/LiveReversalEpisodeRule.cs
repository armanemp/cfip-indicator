namespace cAlgo
{
    internal static class LiveReversalEpisodeRule
    {
        public static bool IsSameEpisode(
            long positionId,
            int oppositeDirection,
            long episodePositionId,
            int episodeDirection)
        {
            return positionId > 0 &&
                   oppositeDirection != 0 &&
                   episodePositionId == positionId &&
                   episodeDirection == oppositeDirection;
        }

        public static bool ShouldEmitDetectionAlert(
            bool alreadyEmitted)
        {
            return !alreadyEmitted;
        }
    }
}
