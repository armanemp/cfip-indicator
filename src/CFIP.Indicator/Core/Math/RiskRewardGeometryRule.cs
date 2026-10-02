using System;

namespace cAlgo
{
    internal readonly struct RiskRewardGeometryResult
    {
        public bool Valid { get; }
        public string Reason { get; }
        public double Risk { get; }
        public double Reward { get; }
        public double EffectiveRisk { get; }
        public double NominalRR { get; }
        public double EffectiveRR { get; }

        public RiskRewardGeometryResult(
            bool valid,
            string reason,
            double risk,
            double reward,
            double effectiveRisk,
            double nominalRR,
            double effectiveRR)
        {
            Valid = valid;
            Reason = reason ?? string.Empty;
            Risk = Math.Max(0, risk);
            Reward = Math.Max(0, reward);
            EffectiveRisk = Math.Max(0, effectiveRisk);
            NominalRR = IsFiniteNonNegative(nominalRR) ? nominalRR : 0;
            EffectiveRR = IsFiniteNonNegative(effectiveRR) ? effectiveRR : 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }

    internal static class RiskRewardGeometryRule
    {
        private const double PriceFloor = 1e-12;

        public static RiskRewardGeometryResult Evaluate(
            int direction,
            double entry,
            double stop,
            double target,
            double spread)
        {
            if (!IsFiniteNonNegative(spread) ||
                (direction != 1 && direction != -1) ||
                !IsPositiveFinite(entry) ||
                !IsPositiveFinite(stop) ||
                !IsPositiveFinite(target))
                return Block("LEVEL GEOMETRY INVALID");

            bool protectiveStop =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            if (!protectiveStop)
                return Block("STOP SIDE INVALID");

            bool progressiveTarget =
                direction == 1
                    ? target > entry
                    : target < entry;

            if (!progressiveTarget)
                return Block("TARGET SIDE INVALID");

            return CalculateFromDistances(
                Math.Abs(entry - stop),
                Math.Abs(target - entry),
                spread);
        }

        public static double CalculateNominalRR(
            double entry,
            double target,
            double risk)
        {
            if (!IsPositiveFinite(entry) ||
                !IsPositiveFinite(target) ||
                !IsPositiveFinite(risk))
                return 0;

            return CalculateNominalRRFromDistances(
                Math.Abs(target - entry),
                risk);
        }

        public static double CalculateNominalRRFromDistances(
            double reward,
            double risk)
        {
            if (!IsPositiveFinite(reward) ||
                !IsPositiveFinite(risk))
                return 0;

            double rr =
                reward /
                Math.Max(
                    PriceFloor,
                    risk);

            return IsPositiveFinite(rr)
                ? rr
                : 0;
        }

        public static double CalculateEffectiveRR(
            double reward,
            double risk,
            double spread)
        {
            if (!IsPositiveFinite(reward) ||
                !IsPositiveFinite(risk))
                return 0;

            double safeSpread =
                IsFiniteNonNegative(spread)
                    ? Math.Max(0, spread)
                    : 0;

            double effectiveRisk =
                risk +
                safeSpread;

            if (!IsPositiveFinite(effectiveRisk))
                return 0;

            double rr =
                reward /
                Math.Max(
                    PriceFloor,
                    effectiveRisk);

            return IsPositiveFinite(rr)
                ? rr
                : 0;
        }

        private static RiskRewardGeometryResult CalculateFromDistances(
            double risk,
            double reward,
            double spread)
        {
            if (!IsPositiveFinite(risk) ||
                !IsPositiveFinite(reward))
                return Block("EMPTY REWARD/RISK");

            double safeSpread =
                IsFiniteNonNegative(spread)
                    ? Math.Max(0, spread)
                    : 0;

            double effectiveRisk =
                risk +
                safeSpread;

            double nominalRR =
                CalculateNominalRRFromDistances(
                    reward,
                    risk);

            double effectiveRR =
                CalculateEffectiveRR(
                    reward,
                    risk,
                    safeSpread);

            if (!IsPositiveFinite(effectiveRisk) ||
                !IsPositiveFinite(nominalRR) ||
                !IsPositiveFinite(effectiveRR))
                return Block("RR INVALID");

            return new RiskRewardGeometryResult(
                true,
                "OK",
                risk,
                reward,
                effectiveRisk,
                nominalRR,
                effectiveRR);
        }

        private static RiskRewardGeometryResult Block(
            string reason)
        {
            return new RiskRewardGeometryResult(
                false,
                reason,
                0,
                0,
                0,
                0,
                0);
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return value >= 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
