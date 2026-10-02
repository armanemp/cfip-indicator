using System;

namespace cAlgo
{
    internal readonly struct TargetCandidateConstraintResult
    {
        public bool Allowed { get; }
        public string Reason { get; }
        public double RiskReward { get; }

        public TargetCandidateConstraintResult(
            bool allowed,
            string reason,
            double riskReward)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
            RiskReward =
                double.IsNaN(riskReward) ||
                double.IsInfinity(riskReward)
                    ? 0
                    : Math.Max(0, riskReward);
        }
    }

    internal static class TargetCandidateConstraintRule
    {
        public static TargetCandidateConstraintResult Evaluate(
            int stage,
            int direction,
            double entry,
            double risk,
            double target,
            double atr,
            double pipSize,
            double requiredRR,
            double maximumRR,
            double maximumTargetExtensionAtr,
            double minimumTpSpacingAtr,
            double previous,
            bool spacingConflict,
            bool requireHtf,
            bool isHtf,
            int htfQuality,
            int minimumHtfQuality)
        {
            if ((direction != 1 && direction != -1) ||
                entry <= 0 ||
                risk <= 0 ||
                atr <= 0 ||
                pipSize <= 0 ||
                target <= 0 ||
                double.IsNaN(entry) ||
                double.IsInfinity(entry) ||
                double.IsNaN(risk) ||
                double.IsInfinity(risk) ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr) ||
                double.IsNaN(target) ||
                double.IsInfinity(target))
            {
                return BlockGeometry(
                    TargetCandidateRejectionReasons.InvalidGeometry);
            }

            if (!PriceProtectionRule.ValidateTarget(
                    direction,
                    entry,
                    target,
                    0))
            {
                return BlockGeometry(
                    TargetCandidateRejectionReasons.TargetSideInvalid);
            }

            if (requireHtf && !isHtf)
            {
                return BlockGeometry(
                    TargetCandidateRejectionReasons.HtfSourceRequired);
            }

            if (requireHtf &&
                htfQuality < minimumHtfQuality)
            {
                return BlockGeometry(
                    TargetCandidateRejectionReasons.HtfQualityTooLow);
            }

            double distance =
                Math.Abs(target - entry);

            double rr =
                RiskRewardGeometryRule.CalculateNominalRR(
                    entry,
                    target,
                    risk);

            if (!IsCanonicalCandidatePositiveFinite(rr))
            {
                return BlockGeometry(
                    TargetCandidateRejectionReasons.RewardRiskInvalid);
            }

            if (!RiskRewardPolicyRule.MeetsMinimum(
                    rr,
                    requiredRR))
            {
                return BlockWithRr(
                    TargetCandidateRejectionReasons.RewardRiskBelowMinimum,
                    rr);
            }

            if (!RiskRewardPolicyRule.IsWithinMaximum(
                    rr,
                    RiskRewardPolicyRule.NormalizeMaximum(
                        maximumRR,
                        requiredRR)))
            {
                return BlockWithRr(
                    TargetCandidateRejectionReasons.RewardRiskAboveMaximum,
                    rr);
            }

            if (distance >
                atr *
                Math.Max(
                    1.0,
                    maximumTargetExtensionAtr))
            {
                return BlockWithRr(
                    TargetCandidateRejectionReasons.TargetTooFar,
                    rr);
            }

            double spacing =
                atr *
                Math.Max(
                    0.05,
                    minimumTpSpacingAtr);

            if (spacingConflict)
            {
                return BlockWithRr(
                    TargetCandidateRejectionReasons.TargetSpacingConflict,
                    rr);
            }

            if (stage > 0)
            {
                if ((direction == 1 &&
                     target <= previous + spacing) ||
                    (direction == -1 &&
                     target >= previous - spacing))
                {
                    return BlockWithRr(
                        TargetCandidateRejectionReasons.TargetProgressionInvalid,
                        rr);
                }
            }

            return new TargetCandidateConstraintResult(
                true,
                string.Empty,
                rr);
        }

        private static bool IsCanonicalCandidatePositiveFinite(
            double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static TargetCandidateConstraintResult BlockGeometry(
            string reason)
        {
            return new TargetCandidateConstraintResult(
                false,
                reason,
                0);
        }

        private static TargetCandidateConstraintResult BlockWithRr(
            string reason,
            double rr)
        {
            return new TargetCandidateConstraintResult(
                false,
                reason,
                rr);
        }
    }
}
