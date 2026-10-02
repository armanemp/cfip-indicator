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
                CanonicalTimeRule.EnsureUtc(
                    causalUtc);

            DateTime actionable =
                CanonicalTimeRule.EnsureUtc(
                    actionableUtcValue);

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
