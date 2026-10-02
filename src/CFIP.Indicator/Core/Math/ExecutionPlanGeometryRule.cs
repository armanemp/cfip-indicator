using System;

namespace cAlgo
{
    internal readonly struct ExecutionPlanGeometryResult
    {
        public bool Allowed { get; }
        public double Risk { get; }
        public double Reward { get; }
        public double EffectiveRisk { get; }
        public double RiskReward { get; }
        public double EffectiveRiskReward { get; }
        public double MinimumRR { get; }
        public double MaximumRR { get; }
        public string Reason { get; }

        public ExecutionPlanGeometryResult(
            bool allowed,
            RiskRewardMathResult geometry,
            string reasonOverride = null)
        {
            Allowed = allowed;
            Risk = geometry.Risk;
            Reward = geometry.Reward;
            EffectiveRisk = geometry.EffectiveRisk;
            RiskReward = geometry.NominalRR;
            EffectiveRiskReward = geometry.EffectiveRR;
            MinimumRR = geometry.MinimumRR;
            MaximumRR = geometry.MaximumRR;
            Reason =
                string.IsNullOrWhiteSpace(reasonOverride)
                    ? geometry.Reason
                    : reasonOverride;
        }

        public static ExecutionPlanGeometryResult Blocked(
            string reason)
        {
            return new ExecutionPlanGeometryResult(
                false,
                new RiskRewardMathResult(
                    false,
                    reason,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0),
                reason);
        }
    }

    internal static class ExecutionPlanGeometryRule
    {
        public static ExecutionPlanGeometryResult Evaluate(
            int direction,
            double entry,
            double stop,
            double tp1,
            double spread,
            double minimumRR,
            double maximumRR,
            double riskFloor)
        {
            RiskRewardMathResult geometry =
                RiskRewardMathRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    tp1,
                    spread,
                    Math.Max(
                        0.10,
                        minimumRR),
                    maximumRR,
                    riskFloor);

            if (!geometry.Valid)
            {
                string reason =
                    geometry.Reason == "RR BELOW MINIMUM"
                        ? "RR BELOW EXECUTION FLOOR"
                        : geometry.Reason;

                return new ExecutionPlanGeometryResult(
                    false,
                    geometry,
                    reason);
            }

            return new ExecutionPlanGeometryResult(
                true,
                geometry,
                "OK");
        }
    }
}
