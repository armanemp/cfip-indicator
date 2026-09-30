using System;

namespace cAlgo
{
    internal static class SignalTraceLineageRule
    {
        public static bool MatchesClosedBar(
            int traceClosedM5,
            long traceBarOpenTimeUtcTicks,
            int sourceCreatedM5,
            long sourceBarOpenTimeUtcTicks)
        {
            return
                traceClosedM5 >= 0 &&
                traceBarOpenTimeUtcTicks > 0 &&
                sourceCreatedM5 == traceClosedM5 &&
                sourceBarOpenTimeUtcTicks == traceBarOpenTimeUtcTicks;
        }

        public static bool CanJoinOutcome(
            string traceId,
            string outcomeTraceId)
        {
            return
                !string.IsNullOrWhiteSpace(traceId) &&
                !string.IsNullOrWhiteSpace(outcomeTraceId) &&
                string.Equals(
                    traceId,
                    outcomeTraceId,
                    StringComparison.Ordinal);
        }
    }
}
