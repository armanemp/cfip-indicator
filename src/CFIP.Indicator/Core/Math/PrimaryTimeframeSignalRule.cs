using System;

namespace cAlgo
{
    internal sealed class PrimaryTimeframeSignalResult
    {
        public bool Allowed { get; }
        public bool M5Aligned { get; }
        public bool M1Confirmed { get; }
        public string State { get; }
        public string Reason { get; }

        public PrimaryTimeframeSignalResult(
            bool allowed,
            bool m5Aligned,
            bool m1Confirmed,
            string state,
            string reason)
        {
            Allowed = allowed;
            M5Aligned = m5Aligned;
            M1Confirmed = m1Confirmed;
            State = state ?? string.Empty;
            Reason = reason ?? string.Empty;
        }
    }

    internal static class PrimaryTimeframeSignalRule
    {
        internal static bool IsPrimarySource(string sourceTimeframe)
        {
            return
                string.Equals(
                    sourceTimeframe,
                    ExecutionTimeframePolicy.PrimaryExecution,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    sourceTimeframe,
                    "H1",
                    StringComparison.OrdinalIgnoreCase);
        }

        internal static PrimaryTimeframeSignalResult Evaluate(
            string sourceTimeframe,
            int sourceDirection,
            int sourceQuality,
            int minimumSourceQuality,
            int m5Direction,
            bool m1ConfirmationAvailable,
            int m1Direction,
            bool m1Confirmed)
        {
            if (!IsPrimarySource(sourceTimeframe))
            {
                return new PrimaryTimeframeSignalResult(
                    false,
                    false,
                    false,
                    "NOT PRIMARY",
                    "SOURCE TIMEFRAME IS NOT M15/H1");
            }

            if (sourceDirection != 1 &&
                sourceDirection != -1)
            {
                return new PrimaryTimeframeSignalResult(
                    false,
                    false,
                    false,
                    "PRIMARY BLOCKED",
                    "SOURCE DIRECTION UNAVAILABLE");
            }

            if (sourceQuality < Math.Max(0, minimumSourceQuality))
            {
                return new PrimaryTimeframeSignalResult(
                    false,
                    false,
                    false,
                    "PRIMARY BLOCKED",
                    "SOURCE QUALITY BELOW PRIMARY FLOOR");
            }

            bool m5Aligned =
                m5Direction == sourceDirection;

            bool confirmed =
                m1ConfirmationAvailable &&
                m1Confirmed &&
                m1Direction == sourceDirection;

            string state;

            if (m5Aligned && confirmed)
            {
                state =
                    "PRIMARY " +
                    sourceTimeframe.Trim().ToUpperInvariant() +
                    " • M5 ALIGNED • M1 CONFIRMED";
            }
            else if (confirmed)
            {
                state =
                    "PRIMARY " +
                    sourceTimeframe.Trim().ToUpperInvariant() +
                    " • M1 CONFIRMED";
            }
            else if (m5Aligned)
            {
                state =
                    "PRIMARY " +
                    sourceTimeframe.Trim().ToUpperInvariant() +
                    " • M5 ALIGNED";
            }
            else
            {
                state =
                    "PRIMARY " +
                    sourceTimeframe.Trim().ToUpperInvariant() +
                    " • SOURCE VALID • LTF TUNING PENDING";
            }

            return new PrimaryTimeframeSignalResult(
                true,
                m5Aligned,
                confirmed,
                state,
                m5Aligned || confirmed
                    ? "PRIMARY SOURCE VALID"
                    : "PRIMARY SOURCE VALID • WAITING FOR M5/M1 TUNING");
        }
    }
}