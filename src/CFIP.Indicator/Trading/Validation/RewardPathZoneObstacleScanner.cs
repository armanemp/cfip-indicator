using System;
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
                !IsFinitePositive(target))
                return false;

            double clearance =
                atr *
                Math.Max(
                    TargetClearanceAtr,
                    TargetObstacleBufferAtr);

            int opposingDirection =
                RewardPathObstacleRule.OpposingDirection(
                    direction);

            if (opposingDirection == 0)
                return false;

            if (UseFvg)
            {
                FindNearestFvg(
                    bars,
                    index,
                    opposingDirection,
                    atr,
                    false,
                    entry);

                Zone[] fvgCandidates;

                if (TryGetCachedFvgCandidates(
                        opposingDirection.ToString() + "|0",
                        out fvgCandidates))
                {
                    for (int i = 0;
                         i < fvgCandidates.Length;
                         i++)
                    {
                        Zone candidate =
                            fvgCandidates[i];

                        if (candidate == null ||
                            !RewardPathObstacleRule.IsOpposingZone(
                                direction,
                                candidate.Direction))
                            continue;

                        if (ZoneBlocksRewardPath(
                                candidate.Low,
                                candidate.High,
                                entry,
                                target,
                                clearance))
                            return true;
                    }
                }
            }

            if (UseOrderBlock)
            {
                FindNearestOrderBlock(
                    bars,
                    index,
                    opposingDirection,
                    atr,
                    entry,
                    false);

                Zone[] obCandidates;

                if (TryGetCachedObCandidates(
                        opposingDirection.ToString() + "|0",
                        out obCandidates))
                {
                    for (int i = 0;
                         i < obCandidates.Length;
                         i++)
                    {
                        Zone candidate =
                            obCandidates[i];

                        if (candidate == null ||
                            candidate.OrderBlockLifecycle ==
                            OrderBlockLifecycleState.Broken ||
                            candidate.OrderBlockLifecycle == null ||
                            !RewardPathObstacleRule.IsOpposingZone(
                                direction,
                                candidate.Direction))
                            continue;

                        if (ZoneBlocksRewardPath(
                                candidate.Low,
                                candidate.High,
                                entry,
                                target,
                                clearance))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
