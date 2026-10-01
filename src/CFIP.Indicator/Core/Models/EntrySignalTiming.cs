using System;

namespace cAlgo
{
    internal readonly struct EntrySignalTiming
    {
        public bool Measured { get; }
        public long CausalEventUtcTicks { get; }
        public long ActionableUtcTicks { get; }
        public long LatencyMilliseconds { get; }

        public EntrySignalTiming(
            bool measured,
            long causalEventUtcTicks,
            long actionableUtcTicks,
            long latencyMilliseconds)
        {
            Measured = measured;
            CausalEventUtcTicks = causalEventUtcTicks;
            ActionableUtcTicks = actionableUtc;
            LatencyMilliseconds =
                latencyMilliseconds < 0
                    ? 0
                    : latencyMilliseconds;
        }

        public static EntrySignalTiming NotMeasured()
        {
            return new EntrySignalTiming(false, 0, 0, 0);
        }
    }
}
