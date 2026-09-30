using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxTargetStageTelemetryReasonsPerM5 = 24;
        private int _targetStageTelemetryM5 = int.MinValue;
        private HashSet<string> _targetStageTelemetryKeys =
            new HashSet<string>(StringComparer.Ordinal);
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
                    rrStep,
                    lane);

            double maximumRR =
                Math.Max(
                    requiredRR[0],
                    MaximumRewardRR);

            for (int stage = 0;
                 stage < 4;
                 stage++)
            {
                Dictionary<string, int> rejectionCounts =
                    new Dictionary<string, int>(
                        StringComparer.Ordinal);

                if (requiredRR[stage] >
                    maximumRR)
                {
                    AddTargetRejectionCount(
                        rejectionCounts,
                        TargetCandidateRejectionReasons.StageRequiredAboveMaximum);
                    RecordTargetStageRejections(
                        closedM5,
                        stage,
                        rejectionCounts);
                    continue;
                }

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
                            out double score,
                            out string rejectionReason))
                    {
                        AddTargetRejectionCount(
                            rejectionCounts,
                            rejectionReason);
                        continue;
                    }

                    if (score > bestScore)
                    {
                        bestScore =
                            score;
                        best =
                            candidate;
                    }
                }

                if (best != null)
                {
                    selected[stage] =
                        best;
                }
                else
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
                }
            }

            return selected;
        }

        private static void AddTargetRejectionCount(
            Dictionary<string, int> counts,
            string reason)
        {
            string normalized =
                string.IsNullOrWhiteSpace(reason)
                    ? TargetCandidateRejectionReasons.InvalidGeometry
                    : reason.Trim();

            int current;
            counts.TryGetValue(
                normalized,
                out current);

            counts[normalized] =
                current + 1;
        }

        private void RecordTargetStageRejections(
            int closedM5,
            int stage,
            Dictionary<string, int> counts)
        {
            if (!EnableOutcomeTelemetry ||
                counts == null ||
                counts.Count == 0)
                return;

            if (_targetStageTelemetryM5 != closedM5)
            {
                _targetStageTelemetryM5 =
                    closedM5;
                _targetStageTelemetryKeys.Clear();
            }

            List<string> reasons =
                new List<string>(
                    counts.Keys);

            reasons.Sort(
                (left, right) =>
                {
                    int countCompare =
                        counts[right].CompareTo(
                            counts[left]);

                    if (countCompare != 0)
                        return countCompare;

                    return string.CompareOrdinal(
                        left,
                        right);
                });

            int emitted = 0;

            for (int i = 0;
                 i < reasons.Count &&
                 emitted <
                 MaxTargetStageTelemetryReasonsPerM5;
                 i++)
            {
                string reason =
                    reasons[i];

                string key =
                    "TP" +
                    (stage + 1).ToString() +
                    "|" +
                    reason;

                if (!_targetStageTelemetryKeys.Add(key))
                    continue;

                int count =
                    counts[reason];

                RecordExecutionTelemetryHistory(
                    "PLAN_TARGET",
                    closedM5,
                    "TP" +
                    (stage + 1).ToString() +
                    " REJECTED",
                    reason +
                    " • COUNT=" +
                    count.ToString());

                emitted++;
            }
        }
    }
}
