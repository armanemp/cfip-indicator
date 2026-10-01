using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal readonly struct TargetObstacleEvaluation
    {
        public bool Blocked { get; }
        public string Reason { get; }
        public double ObstacleDistanceAtr { get; }

        public TargetObstacleEvaluation(
            bool blocked,
            string reason,
            double obstacleDistanceAtr)
        {
            Blocked = blocked;
            Reason = reason ?? TargetCandidateRejectionReasons.InvalidGeometry;
            ObstacleDistanceAtr =
                double.IsNaN(obstacleDistanceAtr) ||
                double.IsInfinity(obstacleDistanceAtr) ||
                obstacleDistanceAtr < 0
                    ? -1
                    : obstacleDistanceAtr;
        }
    }

    public partial class CFIPIndicator : Indicator
    {
        private readonly TargetObstacleScanCache _targetObstacleScanCache =
            new TargetObstacleScanCache();

        private bool HasTargetObstacle(
            Bars bars,
            int index,
            int direction,
            double entry,
            double target,
            double atr)
        {
            return EvaluateTargetObstacle(
                bars,
                index,
                direction,
                entry,
                target,
                atr).Blocked;
        }

        private TargetObstacleEvaluation EvaluateTargetObstacle(
            Bars bars,
            int index,
            int direction,
            double entry,
            double target,
            double atr)
        {
            if (bars == null ||
                index < 8 ||
                atr <= 0 ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(target) ||
                (direction != 1 && direction != -1))
                return new TargetObstacleEvaluation(
                    false,
                    TargetCandidateRejectionReasons.InvalidGeometry,
                    -1);

            double clearance =
                atr * Math.Max(TargetClearanceAtr, TargetObstacleBufferAtr);

            int strength =
                Math.Max(1, Math.Min(SwingStrength, 3));

            int start =
                Math.Max(
                    strength + 1,
                    index - Math.Max(5, TargetObstacleLookbackBars));

            int last =
                Math.Max(
                    start,
                    index - strength - 1);

            TargetObstacleScanSnapshot snapshot =
                GetTargetObstacleScanSnapshot(
                    bars,
                    index,
                    direction,
                    atr,
                    strength);

            TargetObstacleSwingPoint[] swingPoints =
                snapshot.SwingPoints;

            for (int i = 0; i < swingPoints.Length; i++)
            {
                TargetObstacleSwingPoint point =
                    swingPoints[i];

                if (point.Index < start ||
                    point.Index > last)
                    continue;

                double level =
                    point.Level;

                if (direction == 1)
                {
                    if (level > entry &&
                        level < target - clearance)
                    {
                        return new TargetObstacleEvaluation(
                            true,
                            TargetCandidateRejectionReasons.M5Obstacle,
                            Math.Abs(level - entry) / atr);
                    }
                }
                else
                {
                    if (level < entry &&
                        level > target + clearance)
                    {
                        return new TargetObstacleEvaluation(
                            true,
                            TargetCandidateRejectionReasons.M5Obstacle,
                            Math.Abs(level - entry) / atr);
                    }
                }
            }

            if (UseEqualHighLow)
            {
                TargetObstacleEqualPair[] pairs =
                    snapshot.EqualPairs;

                for (int i = 0; i < pairs.Length; i++)
                {
                    TargetObstacleEqualPair pair =
                        pairs[i];

                    if (direction == 1 &&
                        pair.FirstLevel > entry &&
                        pair.SecondLevel > entry &&
                        pair.ResolvedLevel < target - clearance)
                    {
                        return new TargetObstacleEvaluation(
                            true,
                            TargetCandidateRejectionReasons.EqualHighLowObstacle,
                            Math.Abs(pair.ResolvedLevel - entry) / atr);
                    }

                    if (direction == -1 &&
                        pair.FirstLevel < entry &&
                        pair.SecondLevel < entry &&
                        pair.ResolvedLevel > target + clearance)
                    {
                        return new TargetObstacleEvaluation(
                            true,
                            TargetCandidateRejectionReasons.EqualHighLowObstacle,
                            Math.Abs(pair.ResolvedLevel - entry) / atr);
                    }
                }
            }

            return new TargetObstacleEvaluation(
                false,
                string.Empty,
                -1);
        }

        private TargetObstacleScanSnapshot GetTargetObstacleScanSnapshot(
            Bars bars,
            int index,
            int direction,
            double atr,
            int strength)
        {
            double equalityTolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr * Math.Max(0.02, EqualLevelToleranceAtr));

            TargetObstacleCacheKey key =
                new TargetObstacleCacheKey(
                    bars.Count,
                    index,
                    bars.OpenTimes[index].Ticks,
                    direction,
                    strength,
                    TargetObstacleLookbackBars,
                    LiquidityLookback,
                    UseEqualHighLow,
                    equalityTolerance,
                    Symbol.PipSize);

            TargetObstacleScanSnapshot snapshot;

            if (_targetObstacleScanCache.TryGetSnapshot(
                    bars,
                    key,
                    out snapshot))
                return snapshot;

            snapshot =
                BuildTargetObstacleScanSnapshot(
                    bars,
                    index,
                    direction,
                    strength,
                    equalityTolerance);

            _targetObstacleScanCache.StoreSnapshot(
                bars,
                key,
                snapshot);

            return snapshot;
        }
    }
}