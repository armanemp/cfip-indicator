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
            OpportunityLane lane = OpportunityLane.Strategic)
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

            double rrStep =
                Math.Max(
                    0.10,
                    StructuralTpRrStep);

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    rrStep);

            double maximumRR =
                Math.Max(
                    requiredRR[0],
                    MaximumRewardRR);

            for (int stage = 0;
                 stage < 4;
                 stage++)
            {
                if (requiredRR[stage] >
                    maximumRR)
                    continue;

                double previous =
                    FindPreviousSelectedTargetPrice(
                        selected,
                        stage,
                        entry);

                bool requireHtf =
                    RequiresHtfRewardForTargetStage(
                        stage,
                        lane);

                Level best = null;
                double bestScore =
                    double.MinValue;

                for (int i = 0;
                     i < levels.Count;
                     i++)
                {
                    Level candidate =
                        levels[i];

                    if (!TryScoreTargetCandidate(
                            candidate,
                            selected,
                            closedM5,
                            entry,
                            risk,
                            direction,
                            atr,
                            requiredRR[stage],
                            maximumRR,
                            previous,
                            requireHtf,
                            stage,
                            out double score))
                        continue;

                    if (score > bestScore)
                    {
                        bestScore =
                            score;
                        best =
                            candidate;
                    }
                }

                if (best != null)
                    selected[stage] =
                        best;
            }

            return selected;
        }
    }
}
