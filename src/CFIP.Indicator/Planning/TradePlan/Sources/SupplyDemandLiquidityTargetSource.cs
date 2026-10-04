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
        private void AddSupplyDemandAndLiquidityLevels(
                                            List<Level> levels,
                                            int closedM5,
                                            int direction,
                                            double entry,
                                            double atr)
                                        {
                                            if (_m5Bars == null)
                                                return;
                                
                                            double swing =
                                                direction == 1
                                                    ? FindSwingHighAbove(
                                                        _m5Bars,
                                                        closedM5,
                                                        entry)
                                                    : FindSwingLowBelow(
                                                        _m5Bars,
                                                        closedM5,
                                                        entry);
                                
                                            AddLevel(
                                                levels,
                                                swing,
                                                direction == 1
                                                    ? "SUPPLY_ZONE"
                                                    : "DEMAND_ZONE",
                                                "M5",
                                                0,
                                                SupplyDemandWeight);
                                
                                            if (UseEqualHighLow)
                                            {
                                                double liquidity =
                                                    direction == 1
                                                        ? FindEqualHigh(
                                                            _m5Bars,
                                                            closedM5,
                                                            entry,
                                                            atr)
                                                        : FindEqualLow(
                                                            _m5Bars,
                                                            closedM5,
                                                            entry,
                                                            atr);
                                
                                                AddLevel(
                                                    levels,
                                                    liquidity,
                                                    "LIQUIDITY_POOL",
                                                    "M5",
                                                    0,
                                                    EqualHighLowWeight);
                                            }
                                
                                            if (UseDailyWeeklyLiquidity)
                                            {
                                                int d1 =
                                                    ClosedIndex(
                                                        _d1Bars,
                                                        _m5Bars.OpenTimes[
                                                            closedM5]);
                                
                                                if (d1 > 0)
                                                {
                                                    AddLevel(
                                                        levels,
                                                        direction == 1
                                                            ? _d1Bars.HighPrices[d1]
                                                            : _d1Bars.LowPrices[d1],
                                                        "LIQUIDITY_POOL",
                                                        "D1",
                                                        1,
                                                        LiquidityPoolWeight,
                                                        TargetAgeSemanticsRule.ElapsedMinutes(
                                                            _d1Bars.OpenTimes[d1],
                                                            ClosedBarBoundaryReference(
                                                                _m5Bars,
                                                                closedM5,
                                                                Server.TimeInUtc)));
                                                }
                                            }
                                
                                            if (UseSessionLiquidityTargets)
                                            {
                                                double sessionHigh;
                                                double sessionLow;
                                
                                                GetSessionRange(
                                                    _m5Bars,
                                                    closedM5,
                                                    SessionStartUtc,
                                                    SessionEndUtc,
                                                    out sessionHigh,
                                                    out sessionLow);
                                
                                                AddLevel(
                                                    levels,
                                                    direction == 1
                                                        ? sessionHigh
                                                        : sessionLow,
                                                    "SESSION",
                                                    "M5",
                                                    0,
                                                    Math.Max(
                                                        SessionWeight,
                                                        LiquidityTargetMinimumScore));
                                            }
                                        }
    }
}
