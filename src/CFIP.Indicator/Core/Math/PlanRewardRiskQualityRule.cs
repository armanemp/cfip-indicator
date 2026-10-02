using System;

namespace cAlgo
{
    internal readonly struct PlanRewardRiskQualityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }
        public double Risk { get; }
        public double Reward { get; }
        public double EffectiveRisk { get; }
        public double RiskAtr { get; }
        public double NominalRR { get; }
        public double EffectiveRR { get; }
        public double RequiredRR { get; }
        public double MaximumRR { get; }

        public PlanRewardRiskQualityResult(
            bool allowed,
            string reason,
            double risk,
            double reward,
            double effectiveRisk,
            double riskAtr,
            double nominalRR,
            double effectiveRR,
            double requiredRR,
            double maximumRR)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
            Risk = Math.Max(0, risk);
            Reward = Math.Max(0, reward);
            EffectiveRisk = Math.Max(0, effectiveRisk);
            RiskAtr = Math.Max(0, riskAtr);
            NominalRR = Math.Max(0, nominalRR);
            EffectiveRR = Math.Max(0, effectiveRR);
            RequiredRR = Math.Max(0, requiredRR);
            MaximumRR =
                double.IsInfinity(maximumRR)
                    ? double.PositiveInfinity
                    : Math.Max(0, maximumRR);
        }
    }

    internal static class PlanRewardRiskQualityRule
    {
        // Named internal policy constants: fixed safety floors/margins only.
        // Their values are preserved from the pre-E8 implementation.
        internal const double BaseMinimumRrFloor = 0.50;
        internal const double PreferredStopRiskAtrFloor = 0.25;
        internal const double MaximumStopRiskAtrFloor = 0.50;
        internal const double AdaptiveStopExcessRrCap = 0.50;
        internal const double AdaptiveStopExcessRrMultiplier = 0.25;
        internal const double EffectiveRrBaseFactor = 0.90;
        internal const double EffectiveRrAbsoluteReduction = 0.15;

        public static PlanRewardRiskQualityResult Evaluate(
            int direction,
            double entry,
            double stop,
            double tp1,
            double atr,
            double spread,
            double baseMinimumRR,
            double preferredStopRiskAtr,
            double maximumStopRiskAtr,
            double maximumRewardRR,
            double riskFloor)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(stop) ||
                !IsFinitePositive(tp1) ||
                !IsFinitePositive(atr))
            {
                return CreateRewardRiskBlocked(
                    "INVALID REWARD/RISK GEOMETRY");
            }

            bool validStop =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            bool validTarget =
                direction == 1
                    ? tp1 > entry
                    : tp1 < entry;

            if (!validStop)
                return CreateRewardRiskBlocked("STOP SIDE INVALID");

            if (!validTarget)
                return CreateRewardRiskBlocked("TP1 SIDE INVALID");

            double boundedBase =
                Math.Max(
                    BaseMinimumRrFloor,
                    baseMinimumRR);

            double preferred =
                Math.Max(
                    PreferredStopRiskAtrFloor,
                    preferredStopRiskAtr);

            double maximumStopRisk =
                Math.Max(
                    Math.Max(
                        preferred,
                        MaximumStopRiskAtrFloor),
                    maximumStopRiskAtr);

            double rawRisk =
                Math.Abs(entry - stop);

            if (!IsFinitePositiveRewardRisk(rawRisk))
                return CreateRewardRiskBlocked("EMPTY RISK");

            double riskAtr =
                rawRisk /
                Math.Max(
                    SymbolTickFloor(),
                    atr);

            if (!IsFinitePositiveRewardRisk(riskAtr))
                return CreateRewardRiskBlocked(
                    "RISK ATR INVALID");

            if (riskAtr > maximumStopRisk)
            {
                return new PlanRewardRiskQualityResult(
                    false,
                    "STOP RISK TOO HIGH • " +
                    riskAtr.ToString("F2") +
                    " ATR > " +
                    maximumStopRisk.ToString("F2"),
                    0,
                    0,
                    0,
                    riskAtr,
                    0,
                    0,
                    boundedBase,
                    RiskRewardMathRule.NormalizeMaximumRR(
                        maximumRewardRR,
                        boundedBase));
            }

            double stopExcess =
                Math.Max(
                    0,
                    riskAtr - preferred);

            double adaptiveRequired =
                boundedBase +
                Math.Min(
                    AdaptiveStopExcessRrCap,
                    stopExcess *
                    AdaptiveStopExcessRrMultiplier);

            RiskRewardMathResult geometry =
                RiskRewardMathRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    tp1,
                    spread,
                    adaptiveRequired,
                    maximumRewardRR,
                    riskFloor);

            if (!geometry.Valid)
            {
                string reason =
                    geometry.Reason == "RR BELOW MINIMUM"
                        ? "REWARD TOO LOW FOR STOP • RR " +
                          geometry.NominalRR.ToString("F2") +
                          " < " +
                          adaptiveRequired.ToString("F2")
                        : geometry.Reason;

                return new PlanRewardRiskQualityResult(
                    false,
                    reason,
                    geometry.Risk,
                    geometry.Reward,
                    geometry.EffectiveRisk,
                    riskAtr,
                    geometry.NominalRR,
                    geometry.EffectiveRR,
                    adaptiveRequired,
                    geometry.MaximumRR);
            }

            double minimumEffectiveRR =
                Math.Max(
                    boundedBase *
                    EffectiveRrBaseFactor,
                    boundedBase -
                    EffectiveRrAbsoluteReduction);

            if (geometry.EffectiveRR < minimumEffectiveRR)
            {
                return new PlanRewardRiskQualityResult(
                    false,
                    "RR TOO LOW AFTER SPREAD • " +
                    geometry.EffectiveRR.ToString("F2") +
                    " < " +
                    minimumEffectiveRR.ToString("F2"),
                    geometry.Risk,
                    geometry.Reward,
                    geometry.EffectiveRisk,
                    riskAtr,
                    geometry.NominalRR,
                    geometry.EffectiveRR,
                    adaptiveRequired,
                    geometry.MaximumRR);
            }

            return new PlanRewardRiskQualityResult(
                true,
                riskAtr > preferred + 0.35
                    ? "REWARD/RISK ACCEPTED • WIDE STOP REQUIRES EXTRA REWARD"
                    : "REWARD/RISK ACCEPTED",
                geometry.Risk,
                geometry.Reward,
                geometry.EffectiveRisk,
                riskAtr,
                geometry.NominalRR,
                geometry.EffectiveRR,
                adaptiveRequired,
                geometry.MaximumRR);
        }

        private static PlanRewardRiskQualityResult CreateRewardRiskBlocked(
            string reason)
        {
            return new PlanRewardRiskQualityResult(
                false,
                reason,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);
        }

        private static bool IsFinitePositiveRewardRisk(
            double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static double SymbolTickFloor()
        {
            return RiskRewardMathRule.DistanceFloor;
        }
    }
}
