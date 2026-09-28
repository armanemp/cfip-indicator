namespace cAlgo
{
    internal static class ProtectionProgressionRule
    {
        public static bool ShouldAdvanceStop(
            int direction,
            double current,
            double desired)
        {
            if (direction == 1)
                return desired > current;

            if (direction == -1)
                return desired < current;

            return false;
        }

        public static bool ShouldAdvanceTarget(
            int direction,
            double current,
            double desired,
            bool preventBackward)
        {
            if (!preventBackward ||
                current <= 0)
                return true;

            if (direction == 1)
                return desired >= current;

            if (direction == -1)
                return desired <= current;

            return false;
        }
    }
}
