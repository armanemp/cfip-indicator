using System;
using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private bool TryBuildTargetLadderStageOptions(
            List<Level> levels,
            int closedM5,
            double entry,
            double risk,
            int direction,
            double atr,
            OpportunityLane lane,
            double[] requiredRR,
            double maximumRR,
            out List<TargetLadderOption>[] stageOptions)
        {
            stageOptions =
                new List<TargetLadderOption>[4];

            for (int stage = 0;
                 stage < stageOptions.Length;
                 stage++)
            {
                Dictionary<string, int> rejectionCounts =
                    new Dictionary<string, int>(
                        StringComparer.Ordinal);

                if (!TryValidateTargetStageFeasibility(
                        requiredRR[stage],
                        maximumRR,
                        risk,
                        atr,
                        out string stageReason))
                {
                    AddTargetRejectionCount(
                        rejectionCounts,
                        stageReason);

                    RecordTargetStageRejections(
                        closedM5,
                        stage,
                        rejectionCounts);

                    return stage > 0;
                }

                List<TargetLadderOption> options =
                    new List<TargetLadderOption>();

                bool requireHtf =
                    RequiresHtfRewardForTargetStage(
                        stage,
                        lane);

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
                            requireHtf,
                            stage,
                            out double score,
                            out string reason))
                    {
                        AddTargetRejectionCount(
                            rejectionCounts,
                            reason);
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

                    return stage > 0;
                }

                stageOptions[stage] =
                    options;
            }

            return true;
        }
    }
}
