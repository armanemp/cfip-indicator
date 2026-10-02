using System;

namespace cAlgo
{
    internal readonly struct PlanRewardRiskQualityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }
        public double RiskAtr { get; }
        public double NominalRR { get; }
        public double EffectiveRR { get; }
        public double RequiredRR { get; }

        public PlanRewardRiskQualityResult(
            bool allowed,
            string reason,
            double riskAtr,
            double nominalRR,
            double effectiveRR,
            double requiredRR)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
            RiskAtr = Math.Max(0, riskAtr);
            NominalRR = Math.Max(0, nominalRR);
            EffectiveRR = Math.Max(0, effectiveRR);
            RequiredRR = Math.Max(0, requiredRR);
        }
    }

    internal static class PlanRewardRiskQualityRule
    {
        // Named internal policy constants: fixed safety floors/margins only.
        // Their values are preserved from the pre-E8 implementation.
        internal const double BaseMinimumRrFloor =
            RiskRewardPolicyRule.PlanBaseMinimumFloor;
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
            double maximumStopRiskAtr)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositiveRewardRisk(entry) ||
                !IsFinitePositiveRewardRisk(stop) ||
                !IsFinitePositiveRewardRisk(tp1) ||
                !IsFinitePositiveRewardRisk(atr))
            {
                return CreateRewardRiskBlocked("INVALID REWARD/RISK GEOMETRY");
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

            RiskRewardGeometryResult geometry =
                RiskRewardGeometryRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    tp1,
                    spread);

            if (!geometry.Valid)
            {
                return CreateRewardRiskBlocked(
                    geometry.Reason == "TARGET SIDE INVALID"
                        ? "TP1 SIDE INVALID"
                        : geometry.Reason);
            }

            double risk =
                geometry.Risk;

            double riskAtr =
                risk /
                Math.Max(
                    SymbolTickFloor(),
                    atr);

            if (!IsFinitePositiveRewardRisk(riskAtr))
                return CreateRewardRiskBlocked("RISK ATR INVALID");

            double boundedBase =
                RiskRewardPolicyRule.NormalizeMinimum(
                    baseMinimumRR,
                    BaseMinimumRrFloor);

            double preferred =
                Math.Max(
                    PreferredStopRiskAtrFloor,
                    preferredStopRiskAtr);

            double maximum =
                Math.Max(
                    Math.Max(
                        preferred,
                        MaximumStopRiskAtrFloor),
                    maximumStopRiskAtr);

            if (riskAtr > maximum)
            {
                return new PlanRewardRiskQualityResult(
                    false,
                    "STOP RISK TOO HIGH • " +
                    riskAtr.ToString("F2") +
                    " ATR > " +
                    maximum.ToString("F2"),
                    riskAtr,
                    0,
                    0,
                    boundedBase);
            }

            double adaptiveRequired =
                RiskRewardPolicyRule.CalculateAdaptiveMinimum(
                    boundedBase,
                    riskAtr,
                    preferred,
                    AdaptiveStopExcessRrCap,
                    AdaptiveStopExcessRrMultiplier);

            double nominalRR =
                geometry.NominalRR;

            double effectiveRR =
                geometry.EffectiveRR;

            if (!IsFinitePositiveRewardRisk(nominalRR) ||
                !IsFinitePositiveRewardRisk(effectiveRR))
                return CreateRewardRiskBlocked("RR INVALID");

            if (!RiskRewardPolicyRule.MeetsMinimum(
                    nominalRR,
                    adaptiveRequired))
            {
                return new PlanRewardRiskQualityResult(
                    false,
                    "REWARD TOO LOW FOR STOP • RR " +
                    nominalRR.ToString("F2") +
                    " < " +
                    adaptiveRequired.ToString("F2"),
                    riskAtr,
                    nominalRR,
                    effectiveRR,
                    adaptiveRequired);
            }

            double minimumEffectiveRR =
                RiskRewardPolicyRule.CalculateEffectiveMinimum(
                    boundedBase,
                    EffectiveRrBaseFactor,
                    EffectiveRrAbsoluteReduction);

            if (!RiskRewardPolicyRule.MeetsMinimum(
                    effectiveRR,
                    minimumEffectiveRR))
            {
                return new PlanRewardRiskQualityResult(
                    false,
                    "RR TOO LOW AFTER SPREAD • " +
                    effectiveRR.ToString("F2") +
                    " < " +
                    minimumEffectiveRR.ToString("F2"),
                    riskAtr,
                    nominalRR,
                    effectiveRR,
                    adaptiveRequired);
            }

            return new PlanRewardRiskQualityResult(
                true,
                riskAtr > preferred + 0.35
                    ? "REWARD/RISK ACCEPTED • WIDE STOP REQUIRES EXTRA REWARD"
                    : "REWARD/RISK ACCEPTED",
                riskAtr,
                nominalRR,
                effectiveRR,
                adaptiveRequired);
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
            return 1e-12;
        }
    }
}
