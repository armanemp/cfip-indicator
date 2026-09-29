using System;

namespace cAlgo
{
    internal static class ZoneConfluenceRule
    {
        internal static bool HasOverlap(
            double leftLow,
            double leftHigh,
            double rightLow,
            double rightHigh,
            double tolerance)
        {
            if (leftLow >= leftHigh ||
                rightLow >= rightHigh)
                return false;

            double t =
                Math.Max(
                    0,
                    tolerance);

            return
                leftLow < rightHigh + t &&
                rightLow < leftHigh + t;
        }

        internal static bool IsDirectionalMatch(
            int expectedDirection,
            int actualDirection)
        {
            return
                (expectedDirection == 1 ||
                 expectedDirection == -1) &&
                actualDirection == expectedDirection;
        }
    }
}