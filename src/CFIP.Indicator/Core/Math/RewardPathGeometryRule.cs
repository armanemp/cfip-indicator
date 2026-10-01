using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic geometry owner for reward-path obstacle checks.
    /// This rule is platform-neutral and does not inspect market data.
    /// </summary>
    internal static class RewardPathGeometryRule
    {
        public static bool IsOpposingZoneDirection(
            int tradeDirection,
            int zoneDirection)
        {
            return
                (tradeDirection == 1 ||
                 tradeDirection == -1) &&
                (zoneDirection == 1 ||
                 zoneDirection == -1) &&
                zoneDirection == -tradeDirection;
        }
        public static bool BlocksRewardPath(
            double low,
            double high,
            double entry,
            double target,
            double clearance)
        {
            if (!Finite(low) ||
                !Finite(high) ||
                !FinitePositive(entry) ||
                !FinitePositive(target) ||
                !FiniteNonNegative(clearance) ||
                low >= high)
                return false;

            double pathLow =
                Math.Min(
                    entry,
                    target);

            double pathHigh =
                Math.Max(
                    entry,
                    target);

            if (high <=
                pathLow +
                clearance ||
                low >=
                pathHigh -
                clearance)
                return false;

            if (target >= low - clearance &&
                target <= high + clearance)
                return false;

            if (entry >= low - clearance &&
                entry <= high + clearance)
                return false;

            return
                high >
                pathLow + clearance &&
                low <
                pathHigh - clearance;
        }

        private static bool Finite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }

        private static bool FinitePositive(double value)
        {
            return
                Finite(value) &&
                value > 0;
        }

        private static bool FiniteNonNegative(double value)
        {
            return
                Finite(value) &&
                value >= 0;
        }
    }
}
