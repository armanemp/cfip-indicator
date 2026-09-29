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

            if (!IsPositiveFinite(entry) ||
                !IsPositiveFinite(stop) ||
                !IsPositiveFinite(tp1))
                return CreateBlocked("LEVEL GEOMETRY INVALID");

            double risk =
                Math.Abs(entry - stop);

            if (!IsPositiveFinite(risk))
                return CreateBlocked("RISK INVALID");

            bool protectiveStop =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            bool progressiveTarget =
                direction == 1
                    ? tp1 > entry
                    : tp1 < entry;

            if (!protectiveStop)
                return CreateBlocked("STOP WRONG SIDE");

            if (!progressiveTarget)
                return CreateBlocked("TP1 WRONG SIDE");

            double reward =
                Math.Abs(tp1 - entry);

            double rr =
                reward / risk;

            if (!IsPositiveFinite(rr))
                return CreateBlocked("RR INVALID");

            if (rr + 1e-9 <
                Math.Max(0.10, minimumRR))
                return new ExecutionPlanGeometryResult(
                    false,
                    risk,
                    reward,
                    rr,
                    "RR BELOW EXECUTION FLOOR");

            return new ExecutionPlanGeometryResult(
                true,
                risk,
                reward,
                rr,
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
