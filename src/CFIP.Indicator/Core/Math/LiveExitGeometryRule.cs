using System;

namespace cAlgo
{
    internal readonly struct LiveExitGeometryResult
    {
        public bool Allowed { get; }
        public double Distance { get; }
        public double RiskReward { get; }
        public string Reason { get; }

        public LiveExitGeometryResult(
            bool allowed,
            double distance,
            double riskReward,
            string reason)
        {
            Allowed = allowed;
            Distance = distance;
            RiskReward = riskReward;
            Reason = reason ?? "";
        }
    }

    internal static class LiveExitGeometryRule
    {
        public static LiveExitGeometryResult ValidateTarget(
            int direction,
            double entry,
            double market,
            double target,
            double risk,
            double minimumForwardDistance)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(target) ||
                !IsFinitePositive(risk) ||
                !IsFiniteNonNegative(minimumForwardDistance))
                return new LiveExitGeometryResult(
                    false,
                    0,
                    0,
                    "TARGET GEOMETRY INVALID");

            double entryDistance =
                direction == 1
                    ? target - entry
                    : entry - target;

            if (entryDistance <= 0)
                return new LiveExitGeometryResult(
                    false,
                    entryDistance,
                    0,
                    "TARGET WRONG SIDE");

            double marketDistance =
                direction == 1
                    ? target - market
                    : market - target;

            if (marketDistance <=
                Math.Max(
                    0,
                    minimumForwardDistance))
                return new LiveExitGeometryResult(
                    false,
                    marketDistance,
                    entryDistance / risk,
                    "TARGET BEHIND MARKET");

            return new LiveExitGeometryResult(
                true,
                marketDistance,
                entryDistance / risk,
                "OK");
        }

        public static bool ShouldAdvanceTarget(
            int direction,
            double current,
            double desired,
            double market,
            double minimumForwardDistance)
        {
            if (!IsFinitePositive(desired) ||
                !IsFinitePositive(market) ||
                !IsFiniteNonNegative(minimumForwardDistance))
                return false;

            bool forward =
                direction == 1
                    ? desired >
                      market +
                      minimumForwardDistance
                    : direction == -1 &&
                      desired <
                      market -
                      minimumForwardDistance;

            if (!forward)
                return false;

            if (!IsFinitePositive(current))
                return true;

            return ProtectionProgressionRule.ShouldAdvanceTarget(
                direction,
                current,
                desired,
                true);
        }

        public static bool IsProgressiveTargetLadder(
            int direction,
            double entry,
            double tp1,
            double tp2,
            double tp3,
            double tp4)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(tp1))
                return false;

            double previous = entry;

            foreach (double target in new[]
                     {
                         tp1,
                         tp2,
                         tp3,
                         tp4
                     })
            {
                if (!IsFinitePositive(target))
                    continue;

                if (!TargetProgressionRule.IsValid(
                        direction,
                        previous,
                        target))
                    return false;

                previous = target;
            }

            return true;
        }

        public static bool IsProtectiveStop(
            int direction,
            double entry,
            double market,
            double stop,
            double minimumDistance)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(stop) ||
                !IsFiniteNonNegative(minimumDistance))
                return false;

            return direction == 1
                ? stop < market - minimumDistance
                : stop > market + minimumDistance;
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
