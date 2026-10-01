// CFIP Indicator — PremiumDiscountAnalyzer.cs
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
                
                            return PremiumDiscountBiasRule.Evaluate(
                                bars.ClosePrices[index],
                                high,
                                low);
                        }
    }
}
