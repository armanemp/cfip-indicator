using System;

namespace cAlgo
{
    internal static class EntrySignalTimingRule
    {
        public static EntrySignalTiming Measure(
            DateTime causalEventUtc,
            DateTime actionableUtc)
        {
            if (causalEventUtc == DateTime.MinValue ||
                actionableUtc == DateTime.MinValue)
                return EntrySignalTiming.NotMeasured();

            DateTime causal =
                causalEventUtc.Kind == DateTimeKind.Utc
                    ? causalEventUtc
                    : causalEventUtc.ToUniversalTime();

            DateTime actionable =
                actionableUtc.Kind == DateTimeKind.Utc
                    ? actionableUtc
                    : actionableUtc.ToUniversalTime();

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
