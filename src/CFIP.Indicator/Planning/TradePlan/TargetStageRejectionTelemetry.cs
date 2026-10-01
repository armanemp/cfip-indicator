using System;
using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const int MaxTargetStageTelemetryReasonsPerM5 = 24;
        private const int MaxTargetObstacleTelemetryReasonBucketsPerM5 = 8;

        private int _targetStageTelemetryM5 = int.MinValue;
        private HashSet<string> _targetStageTelemetryKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private Dictionary<string, TargetObstacleTelemetryAccumulator>
            _targetObstacleTelemetry =
                new Dictionary<string, TargetObstacleTelemetryAccumulator>(
                    StringComparer.Ordinal);

        private void RecordTargetObstacleObservation(
            int closedM5,
            int stage,
            string reason,
            double targetDistanceAtr,
            double obstacleDistanceAtr,
            double maximumTargetExtensionAtr)
        {
            if (!EnableOutcomeTelemetry ||
                string.IsNullOrWhiteSpace(reason))
                return;

            if (_targetStageTelemetryM5 != closedM5)
            {
                _targetStageTelemetryM5 =
                    closedM5;
                _targetStageTelemetryKeys.Clear();
                _targetObstacleTelemetry.Clear();
            }

            string key =
                "TP" +
                (stage + 1).ToString() +
                "|" +
                reason.Trim();

            TargetObstacleTelemetryAccumulator accumulator;

            if (!_targetObstacleTelemetry.TryGetValue(
                    key,
                    out accumulator))
            {
                if (_targetObstacleTelemetry.Count >=
                    MaxTargetObstacleTelemetryReasonBucketsPerM5)
                    return;

                accumulator =
                    new TargetObstacleTelemetryAccumulator();

                _targetObstacleTelemetry[key] =
                    accumulator;
            }

            accumulator.Observe(
                targetDistanceAtr,
                obstacleDistanceAtr,
                maximumTargetExtensionAtr);
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

                string message =
                    reason +
                    " • COUNT=" +
                    count.ToString();

                string obstacleKey =
                    "TP" +
                    (stage + 1).ToString() +
                    "|" +
                    reason;

                TargetObstacleTelemetryAccumulator obstacleTelemetry;

                if (_targetObstacleTelemetry.TryGetValue(
                        obstacleKey,
                        out obstacleTelemetry))
                {
                    string summary =
                        obstacleTelemetry.FormatSummary();

                    if (!string.IsNullOrWhiteSpace(summary))
                        message +=
                            " • " +
                            summary;
                }

                RecordExecutionTelemetryHistory(
                    "PLAN_TARGET",
                    closedM5,
                    "TP" +
                    (stage + 1).ToString() +
                    " REJECTED",
                    message);

                emitted++;
            }
        }
    }
}
