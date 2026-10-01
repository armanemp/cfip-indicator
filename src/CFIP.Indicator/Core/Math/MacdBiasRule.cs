using System;

namespace cAlgo
{
    internal static class MacdBiasRule
    {
        public static bool IsDirectional(
            int direction,
            double macdLine,
            double previousMacdLine)
        {
            if (direction != 1 &&
                direction != -1)
                return false;

            if (!IsFinite(macdLine) ||
                !IsFinite(previousMacdLine))
                return false;

            return direction == 1
                ? macdLine > 0 &&
                  macdLine >= previousMacdLine
                : macdLine < 0 &&
                  macdLine <= previousMacdLine;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
