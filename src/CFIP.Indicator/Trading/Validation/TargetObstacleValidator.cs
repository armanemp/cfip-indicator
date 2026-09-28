using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
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
                        return true;
                }
                else
                {
                    double level =
                        bars.LowPrices[i];

                    if (level < entry &&
                        level > target + clearance)
                        return true;
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
                    return true;

                if (direction == -1 &&
                    liquidityLevel < entry &&
                    liquidityLevel >
                    target + clearance)
                    return true;
            }

            return false;
        }
    }
}
