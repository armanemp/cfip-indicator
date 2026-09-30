using System;

namespace cAlgo
{
    /// <summary>
    /// Pure scoring components for structural-stop selection.
    /// Existing production values are preserved; this makes reward-path and
    /// preferred-risk contributions independently testable.
    /// </summary>
    internal static class StructuralStopScoringRule
    {
        public const double RewardPathBonusMultiplier = 12.0;
        public const double RewardPathBonusCap = 18.0;
        public const double PreferredRiskFloorAtr = 0.25;
        public const double RiskBalanceBase = 20.0;
        public const double RiskBalanceMinimumSlope = 6.0;
        public const double RiskBalanceSlopeMultiplier = 12.0;
        public const double RiskBalanceWeightDivisor = 20.0;

        public static double CalculateRewardPathBonus(
            double bestTp1RR,
            double requiredRR)
        {
            if (double.IsNaN(bestTp1RR) ||
                double.IsInfinity(bestTp1RR) ||
                double.IsNaN(requiredRR) ||
                double.IsInfinity(requiredRR))
                return 0;

            return Math.Min(
                RewardPathBonusCap,
                Math.Max(
                    0,
                    (bestTp1RR - requiredRR) *
                    RewardPathBonusMultiplier));
        }

        public static double CalculateRiskBalance(
            double riskAtr,
            double preferredRiskAtr,
            double stopRiskBalanceWeight)
        {
            if (double.IsNaN(riskAtr) ||
                double.IsInfinity(riskAtr) ||
                riskAtr < 0)
                return 0;

            double preferredRisk =
                Math.Max(
                    PreferredRiskFloorAtr,
                    preferredRiskAtr);

            double riskBalance =
                Math.Max(
                    0,
                    RiskBalanceBase -
                    Math.Abs(
                        riskAtr -
                        preferredRisk) *
                    Math.Max(
                        RiskBalanceMinimumSlope,
                        RiskBalanceSlopeMultiplier *
                        Math.Max(
                            0.25,
                            stopRiskBalanceWeight /
                            18.0)));

            return riskBalance *
                Math.Max(
                    0,
                    stopRiskBalanceWeight) /
                RiskBalanceWeightDivisor;
        }
    }
}
