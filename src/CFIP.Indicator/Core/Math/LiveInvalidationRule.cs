using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic live-invalidation timing and directional adverse-move rules.
    /// Live management callers may evaluate these rules only against a canonical
    /// closed-bar market reference unless an explicit alternative stability rule exists.
    /// </summary>
    internal static class LiveInvalidationRule
    {
        public static bool ShouldEvaluateClosedBar(
            int closedM5,
            int lastEvaluatedM5)
        {
            return closedM5 >= 0 &&
                   closedM5 != lastEvaluatedM5;
        }

        public static bool TryCalculateDirectionalMove(
            int direction,
            double entry,
            double market,
            out double move)
        {
            move = 0;

            if ((direction != 1 && direction != -1) ||
                !IsFinitePositiveInput(entry) ||
                !IsFinitePositiveInput(market))
                return false;

            move =
                direction == 1
                    ? market - entry
                    : entry - market;

            return !double.IsNaN(move) &&
                   !double.IsInfinity(move);
        }

        public static int RecordExitM5(
            int previousExitM5,
            int requestedExitM5,
            bool brokerMutationSucceeded)
        {
            if (!brokerMutationSucceeded ||
                requestedExitM5 < 0)
                return previousExitM5;

            return Math.Max(
                previousExitM5,
                requestedExitM5);
        }

        private static bool IsFinitePositiveInput(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
