using System;

namespace cAlgo
{
    internal readonly struct ExecutionIntentGeometryResult
    {
        public bool Valid { get; }
        public string Reason { get; }
        public double Entry { get; }
        public double Stop { get; }
        public double Target { get; }
        public double StopPips { get; }
        public double TargetPips { get; }

        public ExecutionIntentGeometryResult(
            bool valid,
            string reason,
            double entry,
            double stop,
            double target,
            double stopPips,
            double targetPips)
        {
            Valid = valid;
            Reason = reason ?? string.Empty;
            Entry = entry;
            Stop = stop;
            Target = target;
            StopPips = Sanitize(stopPips);
            TargetPips = Sanitize(targetPips);
        }

        private static double Sanitize(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value < 0
                ? 0
                : value;
        }
    }

    internal static class ExecutionIntentGeometryRule
    {
        public static ExecutionIntentGeometryResult Evaluate(
            int direction,
            double entry,
            double stop,
            double target,
            double pipSize)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositiveFinite(entry) ||
                !IsPositiveFinite(stop) ||
                !IsPositiveFinite(target) ||
                !IsPositiveFinite(pipSize))
            {
                return Invalid("INVALID GEOMETRY");
            }

            if (!PriceProtectionRule.ValidateStop(
                    direction,
                    entry,
                    stop,
                    0) ||
                !PriceProtectionRule.ValidateTarget(
                    direction,
                    entry,
                    target,
                    0))
            {
                return Invalid("WRONG-SIDE GEOMETRY");
            }

            double stopPips =
                Math.Abs(entry - stop) /
                pipSize;

            double targetPips =
                Math.Abs(target - entry) /
                pipSize;

            if (!IsPositiveFinite(stopPips) ||
                !IsPositiveFinite(targetPips))
            {
                return Invalid("INVALID PIP GEOMETRY");
            }

            return new ExecutionIntentGeometryResult(
                true,
                "OK",
                entry,
                stop,
                target,
                stopPips,
                targetPips);
        }

        private static ExecutionIntentGeometryResult Invalid(string reason)
        {
            return new ExecutionIntentGeometryResult(
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
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
