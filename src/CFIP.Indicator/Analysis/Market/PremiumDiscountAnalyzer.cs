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
    }
}
