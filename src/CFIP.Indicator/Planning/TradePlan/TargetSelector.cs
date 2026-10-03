using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private List<Level> SelectTargets(
            List<Level> levels,
            int closedM5,
            double entry,
            double risk,
            int direction,
            double atr,
            OpportunityLane lane)
        {
            List<Level> selected = new List<Level>
            {
                null, null, null, null
            };

            if (levels == null ||
                levels.Count == 0 ||
                risk <= 0 ||
                atr <= 0)
                return selected;

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    risk,
                    atr,
                    lane);

            if (!TargetSelectionRequiredRrRule.IsMonotonicNonDecreasing(
                    requiredRR))
                return selected;

            double maximumRR =
                Math.Max(requiredRR[0], MaximumRewardRR);

            if (!TryBuildTargetLadderStageOptions(
                    levels,
                    closedM5,
                    entry,
                    risk,
                    direction,
                    atr,
                    lane,
                    requiredRR,
                    maximumRR,
                    out List<TargetLadderOption>[] stageOptions))
                return selected;

            int[] selectedIndices =
                TargetLadderSelectionRule.SelectBestPath(
                    direction,
                    entry,
                    atr * Math.Max(0.05, MinimumTpSpacingAtr),
                    stageOptions);

            if (selectedIndices.Length == 0 ||
                selectedIndices[0] < 0)
                return selected;

            for (int stage = 0;
                 stage < selectedIndices.Length &&
                 stage < selected.Count;
                 stage++)
            {
                int index = selectedIndices[stage];

                if (index >= 0 &&
                    index < levels.Count)
                    selected[stage] = levels[index];
            }

            return selected;
        }
    }
}
