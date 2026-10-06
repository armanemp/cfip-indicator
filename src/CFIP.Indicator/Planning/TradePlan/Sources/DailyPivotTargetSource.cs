using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void AddDailyPivotLevels(
                                            List<Level> levels,
                                            int direction,
                                            double entry,
                                            double atr,
                                            DateTime reference)
                                        {
                                            if (!UseDailyPivots ||
                                                _d1Bars == null ||
                                                _d1Bars.Count < 3 ||
                                                atr <= 0)
                                                return;
                                
                                            int index =
                                                ClosedIndex(
                                                    _d1Bars,
                                                    reference);
                                
                                            if (index <= 0)
                                                return;
                                
                                            int closedIndex = index;
                                
                                            double high = _d1Bars.HighPrices[closedIndex];
                                            double low = _d1Bars.LowPrices[closedIndex];
                                            double close = _d1Bars.ClosePrices[closedIndex];
                                
                                            if (high <= low ||
                                                !IsFinitePositive(close))
                                                return;
                                
                                            double pivot = (high + low + close) / 3.0;
                                            double range = high - low;
                                
                                            double r1 = 2.0 * pivot - low;
                                            double s1 = 2.0 * pivot - high;
                                            double r2 = pivot + range;
                                            double s2 = pivot - range;
                                            double r3 = high + 2.0 * (pivot - low);
                                            double s3 = low - 2.0 * (high - pivot);
                                
                                            double weight =
                                                Math.Max(
                                                    1,
                                                    DailyPivotWeight);

                                            double sourceAgeMinutes =
                                                TargetAgeSemanticsRule.ElapsedMinutes(
                                                    _d1Bars.OpenTimes[closedIndex],
                                                    reference);
                                
                                            AddLevel(levels, pivot, "PIVOT", "D1", 1, weight, sourceAgeMinutes);
                                
                                            if (direction == 1)
                                            {
                                                AddLevel(levels, r1, "PIVOT_R1", "D1", 1, weight + 4, sourceAgeMinutes);
                                                AddLevel(levels, r2, "PIVOT_R2", "D1", 1, weight + 8, sourceAgeMinutes);
                                                AddLevel(levels, r3, "PIVOT_R3", "D1", 1, weight + 10, sourceAgeMinutes);
                                            }
                                            else if (direction == -1)
                                            {
                                                AddLevel(levels, s1, "PIVOT_S1", "D1", 1, weight + 4, sourceAgeMinutes);
                                                AddLevel(levels, s2, "PIVOT_S2", "D1", 1, weight + 8, sourceAgeMinutes);
                                                AddLevel(levels, s3, "PIVOT_S3", "D1", 1, weight + 10, sourceAgeMinutes);
                                            }
                                        }
    }
}
