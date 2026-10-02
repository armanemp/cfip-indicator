using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical price-distance and reward/risk mathematics.
    ///
    /// This class owns only geometry arithmetic and bound normalization.
    /// Policy callers decide which minimum/maximum limits apply to a stage.
    /// </summary>
    internal readonly struct RiskRewardMathResult
    {
        public bool Valid { get; }
        public string Reason { get; }
        public double Risk { get; }
        public double Reward { get; }
        public double EffectiveRisk { get; }
        public double NominalRR { get; }
        public double EffectiveRR { get; }
        public double MinimumRR { get; }
        public double MaximumRR { get; }

        public RiskRewardMathResult(
            bool valid,
            string reason,
            double risk,
            double reward,
            double effectiveRisk,
            double nominalRR,
            double effectiveRR,
            double minimumRR,
            double maximumRR)
        {
            Valid = valid;
            Reason = reason ?? string.Empty;
            Risk = NonNegativeFinite(risk);
            Reward = NonNegativeFinite(reward);
            EffectiveRisk = NonNegativeFinite(effectiveRisk);
            NominalRR = NonNegativeFinite(nominalRR);
            EffectiveRR = NonNegativeFinite(effectiveRR);
            MinimumRR = NonNegativeFinite(minimumRR);
            MaximumRR =
                double.IsInfinity(maximumRR)
                    ? double.PositiveInfinity
                    : NonNegativeFinite(maximumRR);
        }

        private static double NonNegativeFinite(double value)
        {
            return
                double.IsNaN(value) ||
                value < 0
                    ? 0
                    : value;
        }
    }

    internal static class RiskRewardMathRule
    {
        internal const double DistanceFloor = 1e-12;

        public static RiskRewardMathResult Evaluate(
            int direction,
            double entry,
            double stop,
            double target,
            double spread,
            double minimumRR,
            double maximumRR,
            double riskFloor)
        {
            if (direction != 1 && direction != -1)
                return CreateInvalidResult("DIRECTION INVALID");

            if (!IsPositiveFinite(entry) ||
                !IsPositiveFinite(stop) ||
                !IsPositiveFinite(target))
                return CreateInvalidResult("LEVEL GEOMETRY INVALID");

            bool protectiveStop =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            if (!protectiveStop)
                return CreateInvalidResult("STOP WRONG SIDE");

            bool progressiveTarget =
                direction == 1
                    ? target > entry
                    : target < entry;

            if (!progressiveTarget)
                return CreateInvalidResult("TARGET WRONG SIDE");

            double floor =
                Math.Max(
                    DistanceFloor,
                    IsFiniteNonNegativeDistance(riskFloor)
                        ? riskFloor
                        : 0);

            double risk =
                Math.Max(
                    floor,
                    Math.Abs(entry - stop));

            double reward =
                Math.Abs(target - entry);

            if (!IsPositiveFinite(risk) ||
                !IsPositiveFinite(reward))
                return CreateInvalidResult("EMPTY REWARD/RISK");

            double safeSpread =
                Math.Max(
                    0,
                    IsFiniteNonNegativeDistance(spread)
                        ? spread
                        : 0);

            double effectiveRisk =
                risk + safeSpread;

            double nominalRR =
                reward /
                Math.Max(
                    DistanceFloor,
                    risk);

            double effectiveRR =
                reward /
                Math.Max(
                    DistanceFloor,
                    effectiveRisk);

            double minimum =
                Math.Max(
                    0,
                    IsFiniteNonNegativeDistance(minimumRR)
                        ? minimumRR
                        : 0);

            double maximum =
                NormalizeMaximumRR(
                    maximumRR,
                    minimum);

            if (!IsPositiveFinite(nominalRR) ||
                !IsPositiveFinite(effectiveRR))
                return new RiskRewardMathResult(
                    false,
                    "RR INVALID",
                    risk,
                    reward,
                    effectiveRisk,
                    nominalRR,
                    effectiveRR,
                    minimum,
                    maximum);

            if (nominalRR < minimum)
                return new RiskRewardMathResult(
                    false,
                    "RR BELOW MINIMUM",
                    risk,
                    reward,
                    effectiveRisk,
                    nominalRR,
                    effectiveRR,
                    minimum,
                    maximum);

            if (nominalRR > maximum)
                return new RiskRewardMathResult(
                    false,
                    "RR ABOVE MAXIMUM",
                    risk,
                    reward,
                    effectiveRisk,
                    nominalRR,
                    effectiveRR,
                    minimum,
                    maximum);

            return new RiskRewardMathResult(
                true,
                "OK",
                risk,
                reward,
                effectiveRisk,
                nominalRR,
                effectiveRR,
                minimum,
                maximum);
        }

        public static double RiskFromLevels(
            double entry,
            double stop,
            double riskFloor)
        {
            if (!IsPositiveFinite(entry) ||
                !IsPositiveFinite(stop))
                return 0;

            double floor =
                Math.Max(
                    DistanceFloor,
                    IsFiniteNonNegativeDistance(riskFloor)
                        ? riskFloor
                        : 0);

            double risk =
                Math.Abs(
                    entry -
                    stop);

            return
                IsPositiveFinite(risk)
                    ? Math.Max(floor, risk)
                    : 0;
        }

        public static RiskRewardMathResult EvaluateFromRisk(
            int direction,
            double entry,
            double risk,
            double target,
            double spread,
            double minimumRR,
            double maximumRR,
            double riskFloor)
        {
            if (!IsPositiveFinite(risk))
                return CreateInvalidResult("RISK INVALID");

            double stop =
                direction == 1
                    ? entry - risk
                    : entry + risk;

            return Evaluate(
                direction,
                entry,
                stop,
                target,
                spread,
                minimumRR,
                maximumRR,
                riskFloor);
        }

        public static double NormalizeMaximumRR(
            double maximumRR,
            double minimumRR)
        {
            if (double.IsNaN(maximumRR) ||
                maximumRR < 0)
                return Math.Max(
                    0,
                    minimumRR);

            if (double.IsInfinity(maximumRR))
                return double.PositiveInfinity;

            return Math.Max(
                maximumRR,
                Math.Max(0, minimumRR));
        }

        public static double NominalRRFromDistance(
            double rewardDistance,
            double risk,
            double riskFloor)
        {
            if (!IsPositiveFinite(rewardDistance) ||
                !IsPositiveFinite(risk))
                return 0;

            double normalizedRisk =
                Math.Max(
                    DistanceFloor,
                    IsFiniteNonNegativeDistance(riskFloor)
                        ? riskFloor
                        : 0);

            return
                rewardDistance /
                Math.Max(
                    normalizedRisk,
                    risk);
        }

        public static double DirectionalProgressRR(
            int direction,
            double entry,
            double market,
            double risk,
            double riskFloor)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositiveFinite(entry) ||
                !IsPositiveFinite(market) ||
                !IsPositiveFinite(risk))
                return 0;

            double reward =
                direction == 1
                    ? market - entry
                    : entry - market;

            if (reward <= 0 ||
                double.IsNaN(reward) ||
                double.IsInfinity(reward))
                return reward == 0 ? 0 : -Math.Abs(reward) /
                    Math.Max(
                        DistanceFloor,
                        Math.Max(
                            risk,
                            IsFiniteNonNegativeDistance(riskFloor)
                                ? riskFloor
                                : 0));

            return
                NominalRRFromDistance(
                    reward,
                    risk,
                    riskFloor);
        }

        public static double TargetFromRR(
            int direction,
            double entry,
            double risk,
            double rewardRisk)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositiveFinite(entry) ||
                !IsPositiveFinite(risk) ||
                !IsPositiveFinite(rewardRisk))
                return 0;

            double normalizedRisk =
                Math.Max(
                    DistanceFloor,
                    risk);

            return direction == 1
                ? entry + normalizedRisk * rewardRisk
                : entry - normalizedRisk * rewardRisk;
        }

        private static RiskRewardMathResult CreateInvalidResult(
            string reason)
        {
            return new RiskRewardMathResult(
                false,
                reason,
                0,
                0,
                0,
                0,
                0,
                0,
                0);
        }

        private static bool IsPositiveFinite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegativeDistance(double value)
        {
            return
                !double.IsNaN(value) &&
                value >= 0;
        }
    }
}
