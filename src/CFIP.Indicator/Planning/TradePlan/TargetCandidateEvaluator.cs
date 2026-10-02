using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryScoreTargetCandidate(
            Level candidate,
            int closedM5,
            double entry,
            double risk,
            int direction,
            double atr,
            double requiredStageRR,
            double maximumRR,
            double previous,
            bool requireHtf,
            int stage,
            out double score,
            out string rejectionReason)
        {
            score = double.MinValue;
            rejectionReason =
                TargetCandidateRejectionReasons.InvalidGeometry;

            if (candidate == null ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(risk) ||
                !IsFinitePositive(atr))
            {
                rejectionReason =
                    TargetCandidateRejectionReasons.InvalidGeometry;
                return false;
            }

            bool htf =
                IsHtfTimeframe(
                    candidate.Timeframe);

            if (!TargetAgeSemanticsRule.IsAllowed(
                    candidate.Timeframe,
                    candidate.Age,
                    candidate.SourceAgeMinutes,
                    MaximumSetupAgeBars,
                    MaximumZoneAgeBars))
            {
                rejectionReason =
                    htf
                        ? TargetCandidateRejectionReasons.HtfTargetTooOld
                        : TargetCandidateRejectionReasons.M5SetupTooOld;
                return false;
            }



            double distance =
                Math.Abs(
                    candidate.Price -
                    entry);

            double minimumCandidateRR =
                requiredStageRR;

            if (htf)
                minimumCandidateRR =
                    Math.Max(
                        minimumCandidateRR,
                        MinimumHtfTargetRR);

            TargetCandidateConstraintResult constraint =
                TargetCandidateConstraintRule.Evaluate(
                    stage,
                    direction,
                    entry,
                    risk,
                    candidate.Price,
                    atr,
                    Symbol.PipSize,
                    minimumCandidateRR,
                    maximumRR,
                    MaximumTargetExtensionAtr,
                    MinimumTpSpacingAtr,
                    previous,
                    false,
                    requireHtf,
                    htf,
                    (int)Math.Round(candidate.Score),
                    MinimumHtfRewardQuality);

            if (!constraint.Allowed)
            {
                rejectionReason =
                    constraint.Reason;
                return false;
            }

            double rr =
                constraint.RiskReward;

            if (RejectTargetObstacle)
            {
                TargetObstacleEvaluation m5Obstacle =
                    EvaluateTargetObstacle(
                        _m5Bars,
                        closedM5,
                        direction,
                        entry,
                        candidate.Price,
                        atr);

                if (m5Obstacle.Blocked)
                {
                    RecordTargetObstacleObservation(
                        closedM5,
                        stage,
                        m5Obstacle.Reason,
                        distance /
                        atr,
                        m5Obstacle.ObstacleDistanceAtr,
                        MaximumTargetExtensionAtr);

                    rejectionReason =
                        m5Obstacle.Reason;
                    return false;
                }

                if (HasOpposingZonePathObstacle(
                        _m5Bars,
                        closedM5,
                        direction,
                        entry,
                        candidate.Price,
                        atr))
                {
                    RecordTargetObstacleObservation(
                        closedM5,
                        stage,
                        TargetCandidateRejectionReasons.OpposingZoneObstacle,
                        distance /
                        atr,
                        -1,
                        MaximumTargetExtensionAtr);

                    rejectionReason =
                        TargetCandidateRejectionReasons.OpposingZoneObstacle;
                    return false;
                }

                if (stage >= 1 &&
                    HasHigherTfZonePathObstacle(
                        _m5Bars.OpenTimes[
                            closedM5],
                        direction,
                        entry,
                        candidate.Price))
                {
                    RecordTargetObstacleObservation(
                        closedM5,
                        stage,
                        TargetCandidateRejectionReasons.HtfZoneObstacle,
                        distance /
                        atr,
                        -1,
                        MaximumTargetExtensionAtr);

                    rejectionReason =
                        TargetCandidateRejectionReasons.HtfZoneObstacle;
                    return false;
                }
            }

            double normalizedDistance =
                distance /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            bool liquidity =
                candidate.Kind.IndexOf(
                    "LIQUIDITY",
                    StringComparison.OrdinalIgnoreCase) >= 0;

            bool zone =
                candidate.Kind.IndexOf(
                    "FVG",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                candidate.Kind.IndexOf(
                    "ORDER_BLOCK",
                    StringComparison.OrdinalIgnoreCase) >= 0;

            score =
                TargetCandidateRewardScoreRule.Calculate(
                    candidate.Score,
                    rr,
                    requiredStageRR,
                    SmartTargetNearestBias,
                    normalizedDistance,
                    htf,
                    liquidity,
                    zone,
                    candidate.Hits,
                    stage,
                    HtfRewardBonus,
                    LiquidityRewardBonus,
                    ZoneRewardBonus);

            return true;
        }
    }
}
