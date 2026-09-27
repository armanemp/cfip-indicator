using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RegisterAllNative()
                {
                    RegisterNative(_m1Bars);
                    RegisterNative(_m5Bars);
                    RegisterNative(_m15Bars);
                    RegisterNative(_m30Bars);
                    RegisterNative(_h1Bars);
                    RegisterNative(_h4Bars);
                    RegisterNative(_d1Bars);
                    RegisterNative(_w1Bars);
                }
        
                private Native RegisterNative(Bars bars)
                {
                    if (bars == null)
                        return null;
        
                    Native existing =
                        _native.FirstOrDefault(x => ReferenceEquals(x.Bars, bars));
        
                    if (existing != null)
                        return existing;
        
                    Native set = new Native();
                    set.Bars = bars;
        
                    try
                    {
                        set.Fast =
                            Indicators.ExponentialMovingAverage(
                                bars.ClosePrices,
                                Math.Max(2, FastEma));
        
                        set.Slow =
                            Indicators.ExponentialMovingAverage(
                                bars.ClosePrices,
                                Math.Max(3, SlowEma));
        
                        set.Atr =
                            Indicators.AverageTrueRange(
                                bars,
                                Math.Max(2, AtrPeriod),
                                MovingAverageType.WilderSmoothing);
        
                        set.Rsi =
                            Indicators.RelativeStrengthIndex(
                                bars.ClosePrices,
                                Math.Max(2, RsiPeriod));
        
                        set.Dms =
                            Indicators.DirectionalMovementSystem(
                                bars,
                                Math.Max(2, AdxPeriod),
                                MovingAverageType.WilderSmoothing);
        
                        int macdFastPeriod =
                            Math.Max(
                                2,
                                MacdFastPeriod);
        
                        int macdSlowPeriod =
                            Math.Max(
                                macdFastPeriod + 1,
                                MacdSlowPeriod);
        
                        set.MacdFast =
                            Indicators.ExponentialMovingAverage(
                                bars.ClosePrices,
                                macdFastPeriod);
        
                        set.MacdSlow =
                            Indicators.ExponentialMovingAverage(
                                bars.ClosePrices,
                                macdSlowPeriod);
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP indicator initialization failed: {0}",
                            ex.Message);
                    }
        
                    _native.Add(set);
                    return set;
                }
        
                private Native GetNative(Bars bars)
                {
                    if (bars == null)
                        return null;
        
                    Native set =
                        _native.FirstOrDefault(
                            x => ReferenceEquals(x.Bars, bars));
        
                    return set ?? RegisterNative(bars);
                }
        
                private double Ema(Bars bars, int index, bool fast)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 0;
        
                    Native set = GetNative(bars);
                    if (set == null)
                        return 0;
        
                    ExponentialMovingAverage ema =
                        fast ? set.Fast : set.Slow;
        
                    if (ema == null || index >= ema.Result.Count)
                        return 0;
        
                    return SafePositive(ema.Result[index]);
                }
        
                private double Atr(Bars bars, int index)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 0;
        
                    Native set = GetNative(bars);
        
                    if (set == null ||
                        set.Atr == null ||
                        index >= set.Atr.Result.Count)
                        return 0;
        
                    return SafePositive(set.Atr.Result[index]);
                }
        
                private double Rsi(Bars bars, int index)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 50;
        
                    Native set = GetNative(bars);
        
                    if (set == null ||
                        set.Rsi == null ||
                        index >= set.Rsi.Result.Count)
                        return 50;
        
                    double value = set.Rsi.Result[index];
        
                    return
                        double.IsNaN(value) ||
                        double.IsInfinity(value)
                            ? 50
                            : Clamp(value, 0, 100);
                }
        
                private double Adx(Bars bars, int index)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 0;
        
                    Native set = GetNative(bars);
        
                    if (set == null ||
                        set.Dms == null ||
                        index >= set.Dms.ADX.Count)
                        return 0;
        
                    double value = set.Dms.ADX[index];
        
                    return
                        double.IsNaN(value) ||
                        double.IsInfinity(value)
                            ? 0
                            : Clamp(value, 0, 100);
                }
        
                private double DmiBias(Bars bars, int index)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 0;
        
                    Native set = GetNative(bars);
        
                    if (set == null ||
                        set.Dms == null ||
                        index >= set.Dms.DIPlus.Count ||
                        index >= set.Dms.DIMinus.Count)
                        return 0;
        
                    double plus = set.Dms.DIPlus[index];
                    double minus = set.Dms.DIMinus[index];
        
                    if (double.IsNaN(plus) ||
                        double.IsInfinity(plus) ||
                        double.IsNaN(minus) ||
                        double.IsInfinity(minus))
                        return 0;
        
                    double total = plus + minus;
        
                    return
                        total <= 0
                            ? 0
                            : Clamp(
                                (plus - minus) / total,
                                -1,
                                1);
                }
    }
}
