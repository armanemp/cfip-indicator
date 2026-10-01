using System;

namespace cAlgo
{
    internal static class TriggerThresholdRule
    {
        internal static int ResolveRequiredScore(
            bool usePrecision,
            int liveTriggerScore,
            int precisionTriggerScore)
        {
            if (liveTriggerScore < 1 ||
                liveTriggerScore > 6)
                return 0;

            if (usePrecision &&
                (precisionTriggerScore < 2 ||
                 precisionTriggerScore > 6))
                return 0;

            return usePrecision
                ? Math.Max(
                    liveTriggerScore,
                    precisionTriggerScore)
                : liveTriggerScore;
        }

        internal static bool IsScoreReady(
            int triggerScore,
            int requiredTrigger)
        {
            if (triggerScore < 0 ||
                triggerScore > 6 ||
                requiredTrigger < 1 ||
                requiredTrigger > 6)
                return false;

            return triggerScore >= requiredTrigger;
        }
    }
}
