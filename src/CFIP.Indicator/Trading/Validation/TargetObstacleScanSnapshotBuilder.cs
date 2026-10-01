using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TargetObstacleScanSnapshot BuildTargetObstacleScanSnapshot(
            Bars bars,
            int index,
            int direction,
            int strength,
            double equalityTolerance)
        {
            if (bars == null ||
                index < 8 ||
                index >= bars.Count ||
                !TargetObstacleCachePolicy.IsSupportedDirection(direction))
                return TargetObstacleScanSnapshot.Empty;

            int targetStart =
                Math.Max(
                    strength + 1,
                    index - Math.Max(5, TargetObstacleLookbackBars));

            int targetLast =
                Math.Max(
                    targetStart,
                    index - strength - 1);

            int liquidityStart =
                Math.Max(
                    SwingStrength,
                    index - LiquidityLookback);

            int liquidityLast =
                Math.Min(
                    index - SwingStrength,
                    bars.Count - SwingStrength - 1);

            int first =
                Math.Min(
                    targetStart,
                    liquidityStart);

            int last =
                Math.Max(
                    targetLast,
                    liquidityLast);

            List<TargetObstacleSwingPoint> swings =
                new List<TargetObstacleSwingPoint>();

            List<TargetObstacleSwingPoint> liquidityPoints =
                new List<TargetObstacleSwingPoint>();

            for (int i = first;
                 i <= last;
                 i++)
            {
                if (i >= targetStart &&
                    i <= targetLast &&
                    IsTargetObstacleSwing(
                        bars,
                        i,
                        strength,
                        direction))
                {
                    double level =
                        direction == 1
                            ? bars.HighPrices[i]
                            : bars.LowPrices[i];

                    swings.Add(
                        new TargetObstacleSwingPoint(
                            i,
                            level));
                }

                if (UseEqualHighLow &&
                    i >= liquidityStart &&
                    i <= liquidityLast)
                {
                    int plateauStart;
                    int plateauEnd;
                    double level;

                    bool canonical =
                        direction == 1
                            ? IsCanonicalSwingHigh(
                                bars,
                                i,
                                index,
                                SwingStrength,
                                out plateauStart,
                                out plateauEnd,
                                out level)
                            : IsCanonicalSwingLow(
                                bars,
                                i,
                                index,
                                SwingStrength,
                                out plateauStart,
                                out plateauEnd,
                                out level);

                    if (canonical)
                    {
                        liquidityPoints.Add(
                            new TargetObstacleSwingPoint(
                                i,
                                level));
                    }
                }
            }

            List<TargetObstacleEqualPair> pairs =
                new List<TargetObstacleEqualPair>();

            if (UseEqualHighLow &&
                !double.IsNaN(equalityTolerance) &&
                !double.IsInfinity(equalityTolerance))
            {
                List<double> priorLevels =
                    new List<double>();

                for (int i = 0;
                     i < liquidityPoints.Count;
                     i++)
                {
                    double level =
                        liquidityPoints[i].Level;

                    for (int j = 0;
                         j < priorLevels.Count;
                         j++)
                    {
                        if (!SwingPlateauRule.IsWithinAnchor(
                                priorLevels[j],
                                level,
                                equalityTolerance))
                            continue;

                        pairs.Add(
                            direction == 1
                                ? new TargetObstacleEqualPair(
                                    priorLevels[j],
                                    level,
                                    Math.Max(
                                        priorLevels[j],
                                        level))
                                : new TargetObstacleEqualPair(
                                    priorLevels[j],
                                    level,
                                    Math.Min(
                                        priorLevels[j],
                                        level)));
                    }

                    priorLevels.Add(level);
                }
            }

            return new TargetObstacleScanSnapshot(
                swings.ToArray(),
                pairs.ToArray());
        }

        private static bool IsTargetObstacleSwing(
            Bars bars,
            int index,
            int strength,
            int direction)
        {
            for (int j = 1;
                 j <= strength;
                 j++)
            {
                if (direction == 1)
                {
                    if (bars.HighPrices[index] <=
                            bars.HighPrices[index - j] ||
                        bars.HighPrices[index] <=
                            bars.HighPrices[index + j])
                        return false;
                }
                else
                {
                    if (bars.LowPrices[index] >=
                            bars.LowPrices[index - j] ||
                        bars.LowPrices[index] >=
                            bars.LowPrices[index + j])
                        return false;
                }
            }

            return true;
        }
    }
}