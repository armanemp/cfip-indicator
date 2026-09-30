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
        private void AddSmartExtraTargetLevels(
                                            List<Level> levels,
                                            int closedM5,
                                            int direction,
                                            double entry,
                                            double atr)
                                        {
                                            if (!UseExtendedLiquidityMap ||
                                                _m5Bars == null)
                                                return;
                                            List<double> forecasts =
                                                direction == 1
                                                    ? FindLiquidityLevelsAbove(
                                                        _m5Bars,
                                                        closedM5,
                                                        entry,
                                                        atr)
                                                    : FindLiquidityLevelsBelow(
                                                        _m5Bars,
                                                        closedM5,
                                                        entry,
                                                        atr);

                                            for (int i = 0;
                                                 i < forecasts.Count;
                                                 i++)
                                            {
                                                AddLevel(
                                                    levels,
                                                    forecasts[i],
                                                    "LIQUIDITY_FORECAST",
                                                    "M5",
                                                    0,
                                                    Math.Max(
                                                        LiquidityPoolWeight,
                                                        LiquidityTargetMinimumScore));
                                            }
                                
                                            if (UseSessionLiquidityTargets)
                                            {
                                                double high;
                                                double low;
                                
                                                GetSessionRange(
                                                    _m5Bars,
                                                    closedM5,
                                                    SessionStartUtc,
                                                    SessionEndUtc,
                                                    out high,
                                                    out low);
                                
                                                AddLevel(
                                                    levels,
                                                    direction == 1
                                                        ? high
                                                        : low,
                                                    "SESSION_FORECAST",
                                                    "M5",
                                                    0,
                                                    Math.Max(
                                                        SessionWeight,
                                                        LiquidityTargetMinimumScore));
                                            }
                                        }
    }
}
