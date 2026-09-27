namespace cAlgo
{
    internal static class TargetProgressionRule
    {
        public static bool IsValid(
            int direction,
            double previous,
            double next)
        {
            if (previous <= 0 || next <= 0)
                return false;

            if (direction == 1)
                return next > previous;

            if (direction == -1)
                return next < previous;

            return false;
        }
    }
}
