using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical, platform-neutral semantics for structural break events.
    /// A break is an event only when price crosses the same structural level
    /// from the non-broken side on the current closed bar.
    /// </summary>
    internal static class StructuralEventRule
    {
        public static bool IsFreshBreak(
            int direction,
            double previousClose,
            double currentClose,
            double level,
            double atr,
            double breakAtr)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFiniteStructuralInput(previousClose) ||
                !IsFiniteStructuralInput(currentClose) ||
                !IsFinitePositiveStructuralInput(level) ||
                !IsFinitePositiveStructuralInput(atr) ||
                !IsFiniteNonNegativeStructuralInput(breakAtr))
                return false;

            double threshold =
                direction == 1
                    ? level + atr * breakAtr
                    : level - atr * breakAtr;

            return direction == 1
                ? previousClose <= threshold &&
                  currentClose > threshold
                : previousClose >= threshold &&
                  currentClose < threshold;
        }

        public static bool IsChangeOfCharacter(
            int direction,
            bool priorOppositeStructure,
            bool freshBreak)
        {
            return (direction == 1 || direction == -1) &&
                   priorOppositeStructure &&
                   freshBreak;
        }

        public static string EventIdentity(
            int direction,
            string eventType,
            int sourceIndex,
            int closedIndex)
        {
            if ((direction != 1 && direction != -1) ||
                string.IsNullOrWhiteSpace(eventType) ||
                sourceIndex < 0 ||
                closedIndex <= sourceIndex)
                return string.Empty;

            return direction.ToString() +
                   ":" +
                   eventType.Trim().ToUpperInvariant() +
                   ":" +
                   sourceIndex.ToString() +
                   ":" +
                   closedIndex.ToString();
        }

        private static bool IsFiniteStructuralInput(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsFinitePositiveStructuralInput(double value)
        {
            return IsFiniteStructuralInput(value) && value > 0;
        }

        private static bool IsFiniteNonNegativeStructuralInput(double value)
        {
            return IsFiniteStructuralInput(value) && value >= 0;
        }
    }
}
