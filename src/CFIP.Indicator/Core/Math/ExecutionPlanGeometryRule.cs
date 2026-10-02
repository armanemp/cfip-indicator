using System;

namespace cAlgo
{
    internal readonly struct ExecutionPlanGeometryResult
    {
        public bool Allowed { get; }
        public double Risk { get; }
        public double Reward { get; }
        public double RiskReward { get; }
        public string Reason { get; }

        public ExecutionPlanGeometryResult(
            bool allowed,
            double risk,
            double reward,
            double riskReward,
            string reason)
        {
            Allowed = allowed;
            Risk = risk;
            Reward = reward;
            RiskReward = riskReward;
            Reason = reason ?? string.Empty;
        }
    }

    internal static class ExecutionPlanGeometryRule
    {
        public static ExecutionPlanGeometryResult Evaluate(
            int direction,
            double entry,
            double stop,
            double tp1,
            double minimumRR)
        {
            if (direction != 1 &&
                direction != -1)
                return CreateBlocked("DIRECTION INVALID");

            RiskRewardGeometryResult geometry =
                RiskRewardGeometryRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    tp1,
                    0);

            if (!geometry.Valid)
            {
                string reason =
                    geometry.Reason == "STOP SIDE INVALID"
                        ? "STOP WRONG SIDE"
                        : geometry.Reason == "TARGET SIDE INVALID"
                            ? "TP1 WRONG SIDE"
                            : geometry.Reason;

                return new ExecutionPlanGeometryResult(
                    false,
                    geometry.Risk,
                    geometry.Reward,
                    geometry.NominalRR,
                    reason);
            }

            double requiredRR =
                RiskRewardPolicyRule.NormalizeMinimum(
                    minimumRR,
                    RiskRewardPolicyRule.ExecutionMinimumFloor);

            if (!RiskRewardPolicyRule.MeetsMinimum(
                    geometry.NominalRR,
                    requiredRR))
                return new ExecutionPlanGeometryResult(
                    false,
                    geometry.Risk,
                    geometry.Reward,
                    geometry.NominalRR,
                    "RR BELOW EXECUTION FLOOR");

            return new ExecutionPlanGeometryResult(
                true,
                geometry.Risk,
                geometry.Reward,
                geometry.NominalRR,
                "OK");
        }

        private static bool IsPositiveFinite(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static ExecutionPlanGeometryResult CreateBlocked(
            string reason)
        {
            return new ExecutionPlanGeometryResult(
                false,
                0,
                0,
                0,
                reason);
        }
    }
}
