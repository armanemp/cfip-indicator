using System;

namespace cAlgo
{
    internal static class MacdBiasRule
    {
        public static bool IsMacdDirectional(
            int direction,
            double macdLine,
            double previousMacdLine)
        {
            if (direction != 1 &&
                direction != -1)
                return false;

            if (!IsMacdFinite(macdLine) ||
                !IsMacdFinite(previousMacdLine))
                return false;

            return direction == 1
                ? macdLine > 0 &&
                  macdLine >= previousMacdLine
                : macdLine < 0 &&
                  macdLine <= previousMacdLine;
        }

        private static bool IsMacdFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
