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
        public static LiveExitGeometryResult ValidateLiveTarget(
            int direction,
            double entry,
            double market,
            double target,
            double risk,
            double minimumForwardDistance)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFiniteLivePrice(entry) ||
                !IsFiniteLivePrice(market) ||
                !IsFiniteLivePrice(target) ||
                !IsFiniteLivePrice(risk) ||
                !IsFiniteLiveDistance(minimumForwardDistance))
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

            double rewardRisk =
                RiskRewardMathRule.EvaluateFromRisk(
                    direction,
                    entry,
                    risk,
                    target,
                    0,
                    0,
                    double.PositiveInfinity,
                    0).NominalRR;

            if (marketDistance <=
                Math.Max(
                    0,
                    minimumForwardDistance))
                return new LiveExitGeometryResult(
                    false,
                    marketDistance,
                    rewardRisk,
                    "TARGET BEHIND MARKET");

            return new LiveExitGeometryResult(
                true,
                marketDistance,
                rewardRisk,
                "OK");
        }

        public static bool ShouldAdvanceLiveTarget(
            int direction,
            double current,
            double desired,
            double market,
            double minimumForwardDistance)
        {
            if (!IsFiniteLivePrice(desired) ||
                !IsFiniteLivePrice(market) ||
                !IsFiniteLiveDistance(minimumForwardDistance))
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

            if (!IsFiniteLivePrice(current))
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
                !IsFiniteLivePrice(entry) ||
                !IsFiniteLivePrice(tp1))
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
                if (double.IsNaN(target) ||
                    double.IsInfinity(target) ||
                    target < 0)
                    return false;

                if (target == 0)
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
                !IsFiniteLivePrice(entry) ||
                !IsFiniteLivePrice(market) ||
                !IsFiniteLivePrice(stop) ||
                !IsFiniteLiveDistance(minimumDistance))
                return false;

            return direction == 1
                ? stop < market - minimumDistance
                : stop > market + minimumDistance;
        }

        private static bool IsFiniteLivePrice(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteLiveDistance(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
