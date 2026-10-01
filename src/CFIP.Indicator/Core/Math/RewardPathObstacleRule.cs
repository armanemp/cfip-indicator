namespace cAlgo
{
    internal static class RewardPathObstacleRule
    {
        public static int OpposingDirection(int direction)
        {
            if (direction == 1)
                return -1;

            if (direction == -1)
                return 1;

            return 0;
        }

        public static bool IsOpposingZone(
            int tradeDirection,
            int zoneDirection)
        {
            return OpposingDirection(tradeDirection) ==
                   zoneDirection &&
                   zoneDirection != 0;
        }
    }
}
