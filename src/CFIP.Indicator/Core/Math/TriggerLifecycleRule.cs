using System;

namespace cAlgo
{
    internal static class TriggerLifecycleRule
    {
        internal static bool ShouldReset(
            int decisionM5,
            int decisionDirection,
            int currentDecisionM5,
            int currentDirection)
        {
            return
                decisionM5 != currentDecisionM5 ||
                decisionDirection != currentDirection;
        }

        internal static bool CanEvaluateLiveM1Confirmation(
            int decisionM5,
            int liveM5,
            int parentM5,
            DateTime m1Open,
            DateTime m1NextOpen,
            DateTime m5Open,
            DateTime m5NextOpen,
            DateTime reference)
        {
            if (decisionM5 < 0 ||
                liveM5 <= decisionM5 ||
                parentM5 != liveM5)
                return false;

            return M1TriggerRule.IsClosedM1InsideM5Window(
                m1Open,
                m1NextOpen,
                m5Open,
                m5NextOpen,
                reference);
        }

        internal static bool ShouldRecordNewConfirmation(
            int confirmedM1,
            int candidateM1,
            bool ready)
        {
            return
                ready &&
                candidateM1 >= 0 &&
                candidateM1 != confirmedM1;
        }

        internal static bool IsConfirmed(
            bool m5Ready,
            bool useM1Trigger,
            bool latchedM1)
        {
            return
                m5Ready &&
                (!useM1Trigger || latchedM1);
        }
    }
}
