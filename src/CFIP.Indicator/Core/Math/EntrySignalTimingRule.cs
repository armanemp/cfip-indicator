using System;

namespace cAlgo
{
    internal static class EntrySignalTimingRule
    {
        public static EntrySignalTiming Measure(
            DateTime causalUtc,
            DateTime actionableUtcValue)
        {
            if (causalUtc == DateTime.MinValue ||
                actionableUtcValue == DateTime.MinValue)
                return EntrySignalTiming.NotMeasured();

            DateTime causal =
                causalUtc.Kind == DateTimeKind.Utc
                    ? causalUtc
                    : causalUtc.ToUniversalTime();

            DateTime actionable =
                actionableUtcValue.Kind == DateTimeKind.Utc
                    ? actionableUtcValue
                    : actionableUtcValue.ToUniversalTime();

            if (actionable < causal)
                return EntrySignalTiming.NotMeasured();

            long milliseconds =
                (long)Math.Max(
                    0,
                    Math.Round(
                        (actionable - causal).TotalMilliseconds));

            return new EntrySignalTiming(
                true,
                causal.Ticks,
                actionable.Ticks,
                milliseconds);
        }
    }
}
