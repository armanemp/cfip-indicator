// ============================================================================
// CFIP Indicator — NativeIndicatorRegistry.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

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
    }
}
