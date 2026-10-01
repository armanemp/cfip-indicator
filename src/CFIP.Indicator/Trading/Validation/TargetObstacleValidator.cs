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
            Reason = reason ??
                TargetCandidateRejectionReasons.InvalidGeometry;

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
                (direction != 1 &&
                 direction != -1))
                return new TargetObstacleEvaluation(
                    false,
                    TargetCandidateRejectionReasons.InvalidGeometry,
                    -1);

            double clearance =
                atr *
                Math.Max(
                    TargetClearanceAtr,
                    TargetObstacleBufferAtr);

            int strength =
                Math.Max(
                    1,
                    Math.Min(
                        SwingStrength,
                        3));

            int start =
                Math.Max(
                    strength + 1,
                    index -
                    Math.Max(
                        5,
                        TargetObstacleLookbackBars));

            int last =
                Math.Max(
                    start,
                    index -
                    strength -
                    1);

            for (int i = start;
                 i <= last;
                 i++)
            {
                bool swing = true;

                for (int j = 1;
                     j <= strength;
                     j++)
                {
                    if (direction == 1)
                    {
                        if (bars.HighPrices[i] <=
                            bars.HighPrices[i - j] ||
                            bars.HighPrices[i] <=
                            bars.HighPrices[i + j])
                        {
                            swing = false;
                            break;
                        }
                    }
                    else
                    {
                        if (bars.LowPrices[i] >=
                            bars.LowPrices[i - j] ||
                            bars.LowPrices[i] >=
                            bars.LowPrices[i + j])
                        {
                            swing = false;
                            break;
                        }
                    }
                }

                if (!swing)
                    continue;

                if (direction == 1)
                {
                    double level =
                        bars.HighPrices[i];

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
                    double level =
                        bars.LowPrices[i];

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
                double liquidityLevel =
                    direction == 1
                        ? FindEqualHigh(
                            bars,
                            index,
                            entry,
                            atr)
                        : FindEqualLow(
                            bars,
                            index,
                            entry,
                            atr);

                if (direction == 1 &&
                    liquidityLevel > entry &&
                    liquidityLevel <
                    target - clearance)
                {
                    return new TargetObstacleEvaluation(
                        true,
                        TargetCandidateRejectionReasons.EqualHighLowObstacle,
                        Math.Abs(
                            liquidityLevel - entry) /
                        atr);
                }

                if (direction == -1 &&
                    liquidityLevel < entry &&
                    liquidityLevel >
                    target + clearance)
                {
                    return new TargetObstacleEvaluation(
                        true,
                        TargetCandidateRejectionReasons.EqualHighLowObstacle,
                        Math.Abs(
                            liquidityLevel - entry) /
                        atr);
                }
            }

            return new TargetObstacleEvaluation(
                false,
                string.Empty,
                -1);
        }
    }
}
