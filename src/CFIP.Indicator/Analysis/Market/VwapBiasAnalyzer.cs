// CFIP Indicator — VwapBiasAnalyzer.cs
// Single-responsibility analysis module.

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
    }
}
