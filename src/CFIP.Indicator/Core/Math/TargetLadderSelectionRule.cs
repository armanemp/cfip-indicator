using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Chooses one coherent, monotonic TP ladder from stage candidates.
    ///
    /// The first stage is mandatory. Later stages are optional but cannot skip
    /// a stage and then resume at a later stage. This prevents a higher-stage
    /// target from becoming the next TP when an earlier TP was not materialized.
    ///
    /// Selection is global across the available stages rather than greedy:
    /// every compatible predecessor is compared before a stage candidate wins.
    /// </summary>
    internal static class TargetLadderSelectionRule
    {
        public static int[] SelectBestPath(
            int direction,
            double entry,
            double minimumSpacing,
            IReadOnlyList<TargetLadderOption>[] stages)
        {
            int[] empty =
                CreateEmptySelection(stages == null ? 0 : stages.Length);

            if ((direction != 1 && direction != -1) ||
                !IsFinite(entry) ||
                entry <= 0 ||
                !IsFinite(minimumSpacing) ||
                minimumSpacing < 0 ||
                stages == null ||
                stages.Length == 0)
                return empty;

            int stageCount =
                stages.Length;

            double[][] best =
                new double[stageCount][];

            int[][] previous =
                new int[stageCount][];

            for (int stage = 0;
                 stage < stageCount;
                 stage++)
            {
                IReadOnlyList<TargetLadderOption> options =
                    stages[stage];

                if (options == null ||
                    options.Count == 0)
                {
                    if (stage == 0)
                        return empty;

                    break;
                }

                best[stage] =
                    new double[options.Count];

                previous[stage] =
                    new int[options.Count];

                for (int i = 0;
                     i < options.Count;
                     i++)
                {
                    best[stage][i] =
                        double.NegativeInfinity;

                    previous[stage][i] = -1;
                }
            }

            for (int i = 0;
                 i < stages[0].Count;
                 i++)
            {
                TargetLadderOption option =
                    stages[0][i];

                if (!IsCandidateFinite(option) ||
                    !IsTargetSideValid(
                        direction,
                        entry,
                        option.Price))
                    continue;

                best[0][i] =
                    option.Score;
            }

            int lastReachableStage =
                0;

            for (int stage = 1;
                 stage < stageCount;
                 stage++)
            {
                bool stageReachable = false;

                for (int current = 0;
                     current < stages[stage].Count;
                     current++)
                {
                    TargetLadderOption currentOption =
                        stages[stage][current];

                    if (!IsCandidateFinite(currentOption) ||
                        !IsTargetSideValid(
                            direction,
                            entry,
                            currentOption.Price))
                        continue;

                    for (int prior = 0;
                         prior < stages[stage - 1].Count;
                         prior++)
                    {
                        if (double.IsNegativeInfinity(
                                best[stage - 1][prior]))
                            continue;

                        TargetLadderOption priorOption =
                            stages[stage - 1][prior];

                        if (!IsStrictlyProgressive(
                                direction,
                                priorOption.Price,
                                currentOption.Price,
                                minimumSpacing))
                            continue;

                        double candidateScore =
                            best[stage - 1][prior] +
                            currentOption.Score;

                        if (double.IsNaN(candidateScore) ||
                            double.IsInfinity(candidateScore))
                            continue;

                        if (candidateScore >
                            best[stage][current])
                        {
                            best[stage][current] =
                                candidateScore;
                            previous[stage][current] =
                                prior;
                            stageReachable = true;
                        }
                    }
                }

                if (!stageReachable)
                    break;

                lastReachableStage =
                    stage;
            }

            int bestStage =
                -1;
            int bestIndex =
                -1;
            double bestTotal =
                double.NegativeInfinity;

            for (int stage = 0;
                 stage <= lastReachableStage;
                 stage++)
            {
                for (int i = 0;
                     i < best[stage].Length;
                     i++)
                {
                    double total =
                        best[stage][i];

                    if (double.IsNegativeInfinity(total))
                        continue;

                    if (bestStage < 0 ||
                        total > bestTotal ||
                        (Math.Abs(total - bestTotal) <= 1e-12 &&
                         (stage > bestStage ||
                          (stage == bestStage &&
                           i < bestIndex))))
                    {
                        bestStage =
                            stage;
                        bestIndex =
                            i;
                        bestTotal =
                            total;
                    }
                }
            }

            if (bestStage < 0)
                return empty;

            int[] selected =
                CreateEmptySelection(stageCount);

            int currentIndex =
                bestIndex;

            for (int stage = bestStage;
                 stage >= 0;
                 stage--)
            {
                selected[stage] =
                    stages[stage][currentIndex].CandidateIndex;

                currentIndex =
                    stage == 0
                        ? -1
                        : previous[stage][currentIndex];

                if (stage > 0 &&
                    currentIndex < 0)
                    return empty;
            }

            return selected;
        }

        private static bool IsStrictlyProgressive(
            int direction,
            double previous,
            double current,
            double minimumSpacing)
        {
            if (direction == 1)
                return current >
                       previous +
                       minimumSpacing;

            if (direction == -1)
                return current <
                       previous -
                       minimumSpacing;

            return false;
        }

        private static bool IsTargetSideValid(
            int direction,
            double entry,
            double target)
        {
            if (direction == 1)
                return target > entry;

            if (direction == -1)
                return target < entry;

            return false;
        }

        private static bool IsCandidateFinite(
            TargetLadderOption option)
        {
            return option.CandidateIndex >= 0 &&
                   IsFinite(option.Price) &&
                   option.Price > 0 &&
                   IsFinite(option.Score);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static int[] CreateEmptySelection(
            int count)
        {
            int[] result =
                new int[Math.Max(0, count)];

            for (int i = 0;
                 i < result.Length;
                 i++)
                result[i] = -1;

            return result;
        }
    }
}
