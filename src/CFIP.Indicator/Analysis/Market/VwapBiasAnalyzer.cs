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
                                index < 0 ||
                                index >= bars.Count)
                                return false;

                            int length =
                                Math.Max(
                                    10,
                                    VwapLookbackBars);

                            if (index < length - 1)
                                return false;

                            int first =
                                index -
                                length +
                                1;

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
                                        0,
                                        bars.TickVolumes[i]);

                                priceVolume =
                                    VwapBiasRule.AccumulatePriceVolume(
                                        priceVolume,
                                        typical,
                                        v);

                                volume =
                                    VwapBiasRule.AccumulateVolume(
                                        volume,
                                        v);
                            }

                            if (volume <= 0)
                                return false;

                            double vwap =
                                priceVolume /
                                volume;

                            return VwapBiasRule.IsDirectional(
                                bars.ClosePrices[index],
                                vwap,
                                direction);
                        }
    }
}
