using System;

namespace cAlgo
{
    /// <summary>
    /// Platform-neutral deterministic helpers for canonical swing plateaus and
    /// bounded level identity. Callers must supply only bars at or before the
    /// canonical closed-bar index.
    /// </summary>
    internal static class SwingPlateauRule
    {
        public static bool TryGetHighPlateau(
            int length,
            int candidateIndex,
            int strength,
            int closedIndex,
            double equalityTolerance,
            Func<int, double> valueAt,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            plateauStart = plateauEnd = -1;
            level = 0;

            if (!ValidWindow(
                    length,
                    candidateIndex,
                    strength,
                    closedIndex,
                    equalityTolerance,
                    valueAt))
                return false;

            double candidate = valueAt(candidateIndex);
            if (!FinitePositive(candidate))
                return false;

            int left = candidateIndex;
            int right = candidateIndex;

            while (left > 0 &&
                   IsWithinAnchor(
                       candidate,
                       valueAt(left - 1),
                       equalityTolerance))
                left--;

            while (right + 1 <= closedIndex &&
                   IsWithinAnchor(
                       candidate,
                       valueAt(right + 1),
                       equalityTolerance))
                right++;

            // One plateau has exactly one canonical representative: its
            // leftmost member. This makes occurrence scans deterministic.
            if (candidateIndex != left ||
                left < strength ||
                right + strength > closedIndex)
                return false;

            double pivot = valueAt(left);

            for (int i = left - strength; i < left; i++)
            {
                if (!FinitePositive(valueAt(i)) ||
                    valueAt(i) >= pivot - equalityTolerance)
                    return false;
            }

            for (int i = right + 1; i <= right + strength; i++)
            {
                if (!FinitePositive(valueAt(i)) ||
                    valueAt(i) > pivot + equalityTolerance)
                    return false;
            }

            plateauStart = left;
            plateauEnd = right;
            level = pivot;
            return true;
        }

        public static bool TryGetLowPlateau(
            int length,
            int candidateIndex,
            int strength,
            int closedIndex,
            double equalityTolerance,
            Func<int, double> valueAt,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            plateauStart = plateauEnd = -1;
            level = 0;

            if (!ValidWindow(
                    length,
                    candidateIndex,
                    strength,
                    closedIndex,
                    equalityTolerance,
                    valueAt))
                return false;

            double candidate = valueAt(candidateIndex);
            if (!FinitePositive(candidate))
                return false;

            int left = candidateIndex;
            int right = candidateIndex;

            while (left > 0 &&
                   IsWithinAnchor(
                       candidate,
                       valueAt(left - 1),
                       equalityTolerance))
                left--;

            while (right + 1 <= closedIndex &&
                   IsWithinAnchor(
                       candidate,
                       valueAt(right + 1),
                       equalityTolerance))
                right++;

            if (candidateIndex != left ||
                left < strength ||
                right + strength > closedIndex)
                return false;

            double pivot = valueAt(left);

            for (int i = left - strength; i < left; i++)
            {
                if (!FinitePositive(valueAt(i)) ||
                    valueAt(i) <= pivot + equalityTolerance)
                    return false;
            }

            for (int i = right + 1; i <= right + strength; i++)
            {
                if (!FinitePositive(valueAt(i)) ||
                    valueAt(i) < pivot - equalityTolerance)
                    return false;
            }

            plateauStart = left;
            plateauEnd = right;
            level = pivot;
            return true;
        }

        // Equal-level membership always compares to a fixed anchor. A chain
        // such as A~B and B~C does not make C a member unless A~C too.
        public static bool IsWithinAnchor(
            double anchor,
            double candidate,
            double tolerance)
        {
            return FinitePositive(anchor) &&
                   FinitePositive(candidate) &&
                   tolerance >= 0 &&
                   Math.Abs(candidate - anchor) <= tolerance;
        }

        // Stable identity for a single causal structural break event.
        public static string BreakIdentity(
            int direction,
            int levelStartIndex,
            int closedBreakIndex)
        {
            if ((direction != 1 && direction != -1) ||
                levelStartIndex < 0 ||
                closedBreakIndex <= levelStartIndex)
                return string.Empty;

            return direction.ToString() + ":" +
                   levelStartIndex.ToString() + ":" +
                   closedBreakIndex.ToString();
        }

        private static bool ValidWindow(
            int length,
            int candidateIndex,
            int strength,
            int closedIndex,
            double equalityTolerance,
            Func<int, double> valueAt)
        {
            return length > 0 &&
                   candidateIndex >= 0 &&
                   candidateIndex < length &&
                   strength >= 1 &&
                   closedIndex >= 0 &&
                   closedIndex < length &&
                   candidateIndex + strength <= closedIndex &&
                   equalityTolerance >= 0 &&
                   valueAt != null;
        }

        private static bool FinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
