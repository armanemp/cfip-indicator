// ============================================================================
// CFIP Indicator — MarketContextAnalyzer.cs
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
        private bool HasVolumeExpansion(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseVolumeExpansion ||
                                bars == null ||
                                index < 25)
                                return false;
                
                            double average = 0;
                            int count = 0;
                            int first =
                                Math.Max(
                                    0,
                                    index - 20);
                
                            for (int i = first;
                                 i < index;
                                 i++)
                            {
                                average +=
                                    Math.Max(
                                        0,
                                        bars.TickVolumes[i]);
                
                                count++;
                            }
                
                            if (count == 0 ||
                                average <= 0)
                                return false;
                
                            average /= count;
                
                            bool directional =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      bars.OpenPrices[index]
                                    : bars.ClosePrices[index] <
                                      bars.OpenPrices[index];
                
                            return
                                directional &&
                                bars.TickVolumes[index] >=
                                average *
                                Math.Max(
                                    1.0,
                                    VolumeExpansionRatio);
                        }
        
        private bool HasMacdBias(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseMacdBias ||
                                bars == null ||
                                index < 35)
                                return false;
                
                            Native set =
                                GetNative(bars);
                
                            if (set == null ||
                                set.MacdFast == null ||
                                set.MacdSlow == null ||
                                index >= set.MacdFast.Result.Count ||
                                index >= set.MacdSlow.Result.Count)
                                return false;
                
                            double histogram =
                                set.MacdFast.Result[index] -
                                set.MacdSlow.Result[index];
                
                            int previousIndex =
                                Math.Max(
                                    0,
                                    index - 2);
                
                            double previous =
                                set.MacdFast.Result[previousIndex] -
                                set.MacdSlow.Result[previousIndex];
                
                            if (double.IsNaN(histogram) ||
                                double.IsInfinity(histogram) ||
                                double.IsNaN(previous) ||
                                double.IsInfinity(previous))
                                return false;
                
                            return
                                direction == 1
                                    ? histogram > 0 &&
                                      histogram >= previous
                                    : histogram < 0 &&
                                      histogram <= previous;
                        }
        
        private bool HasVwapBias(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseVwapBias ||
                                bars == null ||
                                index < 20)
                                return false;
                
                            int first =
                                Math.Max(
                                    0,
                                    index -
                                    Math.Max(
                                        10,
                                        VwapLookbackBars - 1));
                
                            double priceVolume = 0;
                            double volume = 0;
                
                            for (int i = first;
                                 i <= index;
                                 i++)
                            {
                                double typical =
                                    (bars.HighPrices[i] +
                                     bars.LowPrices[i] +
                                     bars.ClosePrices[i]) /
                                    3.0;
                
                                double v =
                                    Math.Max(
                                        1.0,
                                        bars.TickVolumes[i]);
                
                                priceVolume +=
                                    typical *
                                    v;
                
                                volume +=
                                    v;
                            }
                
                            if (volume <= 0)
                                return false;
                
                            double vwap =
                                priceVolume /
                                volume;
                
                            return
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      vwap
                                    : bars.ClosePrices[index] <
                                      vwap;
                        }
        
        private bool HasHealthyVolatility(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseHealthyVolatility ||
                                bars == null ||
                                index < 30)
                                return false;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            double oldAtr =
                                Atr(
                                    bars,
                                    Math.Max(
                                        5,
                                        index - 10));
                
                            if (atr <= 0 ||
                                oldAtr <= 0)
                                return false;
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            bool directional =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      bars.OpenPrices[index]
                                    : bars.ClosePrices[index] <
                                      bars.OpenPrices[index];
                
                            double minRatio =
                                Math.Max(
                                    0.50,
                                    HealthyAtrMinimumRatio);
                
                            double maxRatio =
                                Math.Max(
                                    minRatio,
                                    HealthyAtrMaximumRatio);
                
                            return
                                directional &&
                                body >=
                                atr *
                                MinimumTriggerBodyAtr &&
                                atr >=
                                oldAtr *
                                minRatio &&
                                atr <=
                                oldAtr *
                                maxRatio;
                        }
        
        private int PremiumDiscountBias(
                            Bars bars,
                            int index)
                        {
                            if (!UsePremiumDiscount ||
                                bars == null ||
                                index < 10)
                                return 0;
                
                            double high =
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index -
                                        StructureLookback),
                                    index);
                
                            double low =
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index -
                                        StructureLookback),
                                    index);
                
                            if (high <= low)
                                return 0;
                
                            double midpoint =
                                (high + low) * 0.5;
                
                            if (bars.ClosePrices[index] <
                                midpoint)
                                return 1;
                
                            if (bars.ClosePrices[index] >
                                midpoint)
                                return -1;
                
                            return 0;
                        }
        
        private double LiveBias(
                            int chartIndex,
                            int direction)
                        {
                            if (Bars == null ||
                                Bars.Count < 10)
                                return 0;
                
                            int i =
                                Math.Max(
                                    1,
                                    Math.Min(
                                        chartIndex,
                                        Bars.Count - 1));
                
                            double fast =
                                Ema(
                                    Bars,
                                    i,
                                    true);
                
                            double slow =
                                Ema(
                                    Bars,
                                    i,
                                    false);
                
                            if (direction == 1 &&
                                Bars.ClosePrices[i] > fast &&
                                fast > slow)
                                return 5;
                
                            if (direction == -1 &&
                                Bars.ClosePrices[i] < fast &&
                                fast < slow)
                                return 5;
                
                            return 0;
                        }
    }
}
