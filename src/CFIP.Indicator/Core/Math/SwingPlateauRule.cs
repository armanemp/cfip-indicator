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
            double[] highs,
            int candidateIndex,
            int strength,
            int closedIndex,
            double equalityTolerance,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            plateauStart = plateauEnd = -1;
            level = 0;
            if (highs == null || strength < 1 || candidateIndex < 0 ||
                closedIndex >= highs.Length || closedIndex < candidateIndex + strength ||
                candidateIndex >= highs.Length || equalityTolerance < 0 ||
                !FinitePositive(highs[candidateIndex]))
                return false;

            int left = candidateIndex;
            int right = candidateIndex;
            while (left > 0 && Math.Abs(highs[left - 1] - highs[candidateIndex]) <= equalityTolerance)
                left--;
            while (right + 1 <= closedIndex && Math.Abs(highs[right + 1] - highs[candidateIndex]) <= equalityTolerance)
                right++;

            // Canonical representative is the leftmost bar of the contiguous plateau.
            if (candidateIndex != left || left < strength || right + strength > closedIndex)
                return false;

            double pivot = highs[left];
            for (int i = left - strength; i < left; i++)
                if (!FinitePositive(highs[i]) || highs[i] >= pivot - equalityTolerance)
                    return false;
            for (int i = right + 1; i <= right + strength; i++)
                if (!FinitePositive(highs[i]) || highs[i] > pivot + equalityTolerance)
                    return false;

            plateauStart = left;
            plateauEnd = right;
            level = pivot;
            return true;
        }

        public static bool TryGetLowPlateau(
            double[] lows,
            int candidateIndex,
            int strength,
            int closedIndex,
            double equalityTolerance,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            plateauStart = plateauEnd = -1;
            level = 0;
            if (lows == null || strength < 1 || candidateIndex < 0 ||
                closedIndex >= lows.Length || closedIndex < candidateIndex + strength ||
                candidateIndex >= lows.Length || equalityTolerance < 0 ||
                !FinitePositive(lows[candidateIndex]))
                return false;

            int left = candidateIndex;
            int right = candidateIndex;
            while (left > 0 && Math.Abs(lows[left - 1] - lows[candidateIndex]) <= equalityTolerance)
                left--;
            while (right + 1 <= closedIndex && Math.Abs(lows[right + 1] - lows[candidateIndex]) <= equalityTolerance)
                right++;

            if (candidateIndex != left || left < strength || right + strength > closedIndex)
                return false;

            double pivot = lows[left];
            for (int i = left - strength; i < left; i++)
                if (!FinitePositive(lows[i]) || lows[i] <= pivot + equalityTolerance)
                    return false;
            for (int i = right + 1; i <= right + strength; i++)
                if (!FinitePositive(lows[i]) || lows[i] < pivot - equalityTolerance)
                    return false;

            plateauStart = left;
            plateauEnd = right;
            level = pivot;
            return true;
        }

        // A candidate joins a cluster only if it is within tolerance of the
        // fixed anchor. This prevents transitive tolerance chaining.
        public static bool IsWithinAnchor(double anchor, double candidate, double tolerance)
        {
            return FinitePositive(anchor) && FinitePositive(candidate) &&
                   tolerance >= 0 && Math.Abs(candidate - anchor) <= tolerance;
        }

        // A structural event identity is the direction + canonical level
        // identity + closed-bar index; consumers can use this key to deduplicate
        // BOS/MSS/CHOCH labels for the same break.
        public static string BreakIdentity(int direction, int levelStartIndex, int closedBreakIndex)
        {
            if ((direction != 1 && direction != -1) || levelStartIndex < 0 || closedBreakIndex <= levelStartIndex)
                return string.Empty;
            return direction.ToString() + ":" + levelStartIndex.ToString() + ":" + closedBreakIndex.ToString();
        }

        private static bool FinitePositive(double value)
        {
            return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
