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
            List<Level> selected =
                new List<Level>
                {
                    null, null, null, null
                };

            if (levels == null ||
                levels.Count == 0 ||
                risk <= 0 ||
                atr <= 0)
                return selected;

            double rrStep = Math.Max(0.10, StructuralTpRrStep);

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    rrStep,
                    lane);

            if (!TargetSelectionRequiredRrRule.IsMonotonicNonDecreasing(
                    requiredRR))
                return selected;

            double maximumRR =
                Math.Max(
                    requiredRR[0],
                    MaximumRewardRR);

            double minimumSpacing =
                atr *
                Math.Max(
                    0.05,
                    MinimumTpSpacingAtr);

            List<TargetLadderOption>[] stageOptions =
                new List<TargetLadderOption>[4];

            for (int stage = 0;
                 stage < 4;
                 stage++)
            {
                Dictionary<string, int> rejectionCounts =
                    new Dictionary<string, int>(StringComparer.Ordinal);

                if (!TryValidateTargetStageFeasibility(
                        requiredRR[stage],
                        maximumRR,
                        risk,
                        atr,
                        out string stageRejectionReason))
                {
                    AddTargetRejectionCount(
                        rejectionCounts,
                        stageRejectionReason);

                    RecordTargetStageRejections(
                        closedM5,
                        stage,
                        rejectionCounts);

                    break;
                }

                List<TargetLadderOption> options =
                    new List<TargetLadderOption>();

                for (int i = 0;
                     i < levels.Count;
                     i++)
                {
                    Level candidate =
                        levels[i];

                    if (!TryScoreTargetCandidate(
                            candidate,
                            closedM5,
                            entry,
                            risk,
                            direction,
                            atr,
                            requiredRR[stage],
                            maximumRR,
                            entry,
                            RequiresHtfRewardForTargetStage(
                                stage,
                                lane),
                            stage,
                            out double score,
                            out string rejectionReason))
                    {
                        AddTargetRejectionCount(
                            rejectionCounts,
                            rejectionReason);
                        continue;
                    }

                    options.Add(
                        new TargetLadderOption(
                            i,
                            candidate.Price,
                            score));
                }

                if (options.Count == 0)
                {
                    if (rejectionCounts.Count == 0)
                    {
                        AddTargetRejectionCount(
                            rejectionCounts,
                            TargetCandidateRejectionReasons.InvalidGeometry);
                    }

                    RecordTargetStageRejections(
                        closedM5,
                        stage,
                        rejectionCounts);

                    break;
                }

                stageOptions[stage] =
                    options;
            }

            int[] selectedIndices =
                TargetLadderSelectionRule.SelectBestPath(
                    direction,
                    entry,
                    minimumSpacing,
                    stageOptions);

            if (selectedIndices.Length == 0 ||
                selectedIndices[0] < 0)
                return selected;

            for (int stage = 0;
                 stage < selectedIndices.Length;
                 stage++)
            {
                int index =
                    selectedIndices[stage];

                if (index < 0 ||
                    index >= levels.Count)
                    continue;

                selected[stage] =
                    levels[index];
            }

            return selected;
        }
    }
}
