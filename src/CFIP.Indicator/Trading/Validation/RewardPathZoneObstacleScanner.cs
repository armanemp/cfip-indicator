using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasOpposingZonePathObstacle(
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
                return false;

            double clearance =
                atr *
                Math.Max(
                    TargetClearanceAtr,
                    TargetObstacleBufferAtr);

            Zone[] obstacles =
                GetOpposingZonePathObstacles(
                    bars,
                    index,
                    direction,
                    atr);

            for (int i = 0;
                 i < obstacles.Length;
                 i++)
            {
                Zone obstacle =
                    obstacles[i];

                if (obstacle == null ||
                    obstacle.Direction !=
                    -direction)
                    continue;

                if (RewardPathGeometryRule.BlocksRewardPath(
                        obstacle.Low,
                        obstacle.High,
                        entry,
                        target,
                        clearance))
                    return true;
            }

            return false;
        }

        private Zone[] GetOpposingZonePathObstacles(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            if (bars == null ||
                index < 8 ||
                index >= bars.Count ||
                atr <= 0 ||
                (direction != 1 &&
                 direction != -1))
                return Array.Empty<Zone>();

            ResetZoneLookupCacheIfNeeded(
                bars,
                index);

            string cacheKey =
                direction.ToString();

            if (TryGetCachedOpposingZonePathObstacles(
                    cacheKey,
                    out Zone[] cached))
                return cached;

            List<Zone> obstacles =
                new List<Zone>();

            int opposingDirection =
                -direction;

            int first =
                Math.Max(
                    2,
                    index -
                    Math.Max(
                        5,
                        TargetObstacleLookbackBars));

            for (int i = first;
                 i <= index - 1;
                 i++)
            {
                if (UseFvg)
                {
                    double creationAtr =
                        Atr(
                            bars,
                            i);

                    double low;
                    double high;
                    double gap;

                    if (creationAtr > 0 &&
                        FvgRule.TryGetThreeBarGap(
                            opposingDirection,
                            bars.HighPrices[i - 2],
                            bars.LowPrices[i - 2],
                            bars.HighPrices[i],
                            bars.LowPrices[i],
                            out low,
                            out high,
                            out gap) &&
                        FvgRule.MeetsMinimumGap(
                            gap,
                            creationAtr,
                            MinimumFvgAtr))
                    {
                        Zone fvg =
                            BuildManagedFvgZone(
                                bars,
                                i,
                                index,
                                opposingDirection,
                                low,
                                high,
                                gap,
                                false,
                                creationAtr);

                        if (fvg != null)
                            obstacles.Add(
                                fvg);
                    }
                }

                if (UseOrderBlock)
                {
                    Zone oppositeOb =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            index,
                            opposingDirection,
                            atr);

                    if (oppositeOb != null &&
                        oppositeOb.OrderBlockLifecycle !=
                        OrderBlockLifecycleState.Broken)
                        obstacles.Add(
                            oppositeOb);
                }
            }

            return StoreCachedOpposingZonePathObstacles(
                cacheKey,
                obstacles);
        }
    }
}
