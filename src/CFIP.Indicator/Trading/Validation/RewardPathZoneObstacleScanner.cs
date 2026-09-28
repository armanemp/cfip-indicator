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

            int first =
                Math.Max(
                    2,
                    index -
                    Math.Max(
                        5,
                        TargetObstacleLookbackBars));

            double clearance =
                atr *
                Math.Max(
                    TargetClearanceAtr,
                    TargetObstacleBufferAtr);

            for (int i = first;
                 i <= index - 1;
                 i++)
            {
                if (direction == 1)
                {
                    double gap =
                        bars.LowPrices[i] -
                        bars.HighPrices[i - 2];

                    if (gap >=
                        atr *
                        MinimumFvgAtr)
                    {
                        double low =
                            bars.HighPrices[i - 2];

                        double high =
                            bars.LowPrices[i];

                        if (ZoneBlocksRewardPath(
                                low,
                                high,
                                entry,
                                target,
                                clearance))
                            return true;
                    }
                }
                else
                {
                    double gap =
                        bars.LowPrices[i - 2] -
                        bars.HighPrices[i];

                    if (gap >=
                        atr *
                        MinimumFvgAtr)
                    {
                        double low =
                            bars.HighPrices[i];

                        double high =
                            bars.LowPrices[i - 2];

                        if (ZoneBlocksRewardPath(
                                low,
                                high,
                                entry,
                                target,
                                clearance))
                            return true;
                    }
                }

                if (UseOrderBlock)
                {
                    Zone oppositeOb =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            index,
                            -direction,
                            atr);

                    if (oppositeOb != null &&
                        ZoneBlocksRewardPath(
                            oppositeOb.Low,
                            oppositeOb.High,
                            entry,
                            target,
                            clearance))
                        return true;
                }
            }

            return false;
        }
    }
}
