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
            NominalRR = IsCanonicalFiniteNonNegative(nominalRR) ? nominalRR : 0;
            EffectiveRR = IsCanonicalFiniteNonNegative(effectiveRR) ? effectiveRR : 0;
        }

        private static bool IsCanonicalFiniteNonNegative(double value)
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
            if (!IsCanonicalFiniteNonNegative(spread) ||
                (direction != 1 && direction != -1) ||
                !IsCanonicalPositiveFinite(entry) ||
                !IsCanonicalPositiveFinite(stop) ||
                !IsCanonicalPositiveFinite(target))
                return BuildBlockedGeometry("LEVEL GEOMETRY INVALID");

            bool protectiveStop =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            if (!protectiveStop)
                return BuildBlockedGeometry("STOP SIDE INVALID");

            bool progressiveTarget =
                direction == 1
                    ? target > entry
                    : target < entry;

            if (!progressiveTarget)
                return BuildBlockedGeometry("TARGET SIDE INVALID");

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
            if (!IsCanonicalPositiveFinite(entry) ||
                !IsCanonicalPositiveFinite(target) ||
                !IsCanonicalPositiveFinite(risk))
                return 0;

            return CalculateNominalRRFromDistances(
                Math.Abs(target - entry),
                risk);
        }

        public static double CalculateNominalRRFromDistances(
            double reward,
            double risk)
        {
            if (!IsCanonicalPositiveFinite(reward) ||
                !IsCanonicalPositiveFinite(risk))
                return 0;

            double rr =
                reward /
                Math.Max(
                    PriceFloor,
                    risk);

            return IsCanonicalPositiveFinite(rr)
                ? rr
                : 0;
        }

        public static double CalculateEffectiveRR(
            double reward,
            double risk,
            double spread)
        {
            if (!IsCanonicalPositiveFinite(reward) ||
                !IsCanonicalPositiveFinite(risk))
                return 0;

            double safeSpread =
                IsCanonicalFiniteNonNegative(spread)
                    ? Math.Max(0, spread)
                    : 0;

            double effectiveRisk =
                risk +
                safeSpread;

            if (!IsCanonicalPositiveFinite(effectiveRisk))
                return 0;

            double rr =
                reward /
                Math.Max(
                    PriceFloor,
                    effectiveRisk);

            return IsCanonicalPositiveFinite(rr)
                ? rr
                : 0;
        }

        private static RiskRewardGeometryResult CalculateFromDistances(
            double risk,
            double reward,
            double spread)
        {
            if (!IsCanonicalPositiveFinite(risk) ||
                !IsCanonicalPositiveFinite(reward))
                return BuildBlockedGeometry("EMPTY REWARD/RISK");

            double safeSpread =
                IsCanonicalFiniteNonNegative(spread)
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

            if (!IsCanonicalPositiveFinite(effectiveRisk) ||
                !IsCanonicalPositiveFinite(nominalRR) ||
                !IsCanonicalPositiveFinite(effectiveRR))
                return BuildBlockedGeometry("RR INVALID");

            return new RiskRewardGeometryResult(
                true,
                "OK",
                risk,
                reward,
                effectiveRisk,
                nominalRR,
                effectiveRR);
        }

        private static RiskRewardGeometryResult BuildBlockedGeometry(
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

        private static bool IsCanonicalPositiveFinite(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsCanonicalFiniteNonNegative(double value)
        {
            return value >= 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
