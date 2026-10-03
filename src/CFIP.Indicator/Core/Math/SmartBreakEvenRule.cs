using System;

namespace cAlgo
{
    internal readonly struct SmartBreakEvenResult
    {
        public bool Allowed { get; }
        public double TriggerPips { get; }
        public double OffsetPips { get; }
        public string Reason { get; }

        public SmartBreakEvenResult(
            bool allowed,
            double triggerPips,
            double offsetPips,
            string reason)
        {
            Allowed = allowed;
            TriggerPips = triggerPips;
            OffsetPips = offsetPips;
            Reason = reason ?? "";
        }
    }

    internal static class SmartBreakEvenRule
    {
        public static SmartBreakEvenResult Evaluate(
            double riskPips,
            double tp1Pips,
            double spreadPips,
            double triggerRR,
            double bufferPips,
            double riskFreeLockPips,
            bool spreadAware)
        {
            if (!IsFiniteSbPositive(riskPips) ||
                !IsFiniteSbPositive(tp1Pips) ||
                !IsFiniteSbNonNegative(spreadPips) ||
                !IsFiniteSbNonNegative(triggerRR) ||
                !IsFiniteSbNonNegative(bufferPips) ||
                !IsFiniteSbNonNegative(riskFreeLockPips))
                return new SmartBreakEvenResult(
                    false,
                    0,
                    0,
                    "RISK/TP NUMERIC INVALID");

            double baseTrigger =
                riskPips *
                Math.Max(0.20, triggerRR);

            double spreadFloor =
                spreadAware
                    ? Math.Max(
                        0,
                        spreadPips) +
                      Math.Max(
                        0,
                        bufferPips)
                    : Math.Max(
                        0,
                        bufferPips);

            double trigger =
                Math.Max(
                    baseTrigger,
                    spreadFloor);

            // Do not arm server BE so late that it collides with TP1.
            double maximumTrigger =
                tp1Pips * 0.75;

            if (maximumTrigger <= trigger ||
                maximumTrigger <= 0)
                return new SmartBreakEvenResult(
                    false,
                    0,
                    0,
                    "TP1 TOO CLOSE FOR SMART BE");

            trigger =
                Math.Min(
                    trigger,
                    maximumTrigger * 0.90);

            double offset =
                spreadAware
                    ? Math.Max(
                        Math.Max(
                            0,
                            riskFreeLockPips),
                        Math.Max(
                            0,
                            spreadPips) +
                        Math.Max(
                            0,
                            bufferPips))
                    : Math.Max(
                        0,
                        riskFreeLockPips);

            // Keep the locked stop inside the original risk envelope.
            offset =
                Math.Min(
                    offset,
                    riskPips * 0.50);

            if (!IsFiniteSbPositive(trigger) ||
                !IsFiniteSbNonNegative(offset))
                return new SmartBreakEvenResult(
                    false,
                    0,
                    0,
                    "SMART BE INVALID");

            return new SmartBreakEvenResult(
                true,
                trigger,
                offset,
                "SMART SERVER BREAK-EVEN");
        }

        private static bool IsFiniteSbPositive(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteSbNonNegative(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
