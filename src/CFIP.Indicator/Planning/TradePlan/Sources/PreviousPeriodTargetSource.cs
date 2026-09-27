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
        private void AddPreviousPeriodLevels(
                                            List<Level> levels,
                                            int direction,
                                            double entry,
                                            double atr,
                                            DateTime reference)
                                        {
                                            int d1 =
                                                ClosedIndex(
                                                    _d1Bars,
                                                    reference);
                                
                                            int w1 =
                                                ClosedIndex(
                                                    _w1Bars,
                                                    reference);
                                
                                            if (UseDailyWeeklyLiquidity &&
                                                d1 > 0)
                                            {
                                                AddLevel(
                                                    levels,
                                                    direction == 1
                                                        ? _d1Bars.HighPrices[d1 - 1]
                                                        : _d1Bars.LowPrices[d1 - 1],
                                                    "PREVIOUS_DAY",
                                                    "D1",
                                                    1,
                                                    PreviousDayWeight);
                                            }
                                
                                            if (UseDailyWeeklyLiquidity &&
                                                w1 > 0)
                                            {
                                                AddLevel(
                                                    levels,
                                                    direction == 1
                                                        ? _w1Bars.HighPrices[w1 - 1]
                                                        : _w1Bars.LowPrices[w1 - 1],
                                                    "PREVIOUS_WEEK",
                                                    "W1",
                                                    1,
                                                    PreviousWeekWeight);
                                            }
                                        }
    }
}
