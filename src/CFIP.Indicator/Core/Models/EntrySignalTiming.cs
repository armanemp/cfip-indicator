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
            long causalEventUtcTicksValue,
            long actionableUtcTicksValue,
            long latencyMillisecondsValue)
        {
            Measured = measured;
            CausalEventUtcTicks = causalEventUtcTicksValue;
            ActionableUtcTicks = actionableUtcTicksValue;
            LatencyMilliseconds =
                latencyMillisecondsValue < 0
                    ? 0
                    : latencyMillisecondsValue;
        }

        public static EntrySignalTiming NotMeasured()
        {
            return new EntrySignalTiming(false, 0, 0, 0);
        }
    }
}
