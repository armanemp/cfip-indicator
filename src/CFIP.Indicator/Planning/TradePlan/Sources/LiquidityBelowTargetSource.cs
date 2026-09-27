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
        private double FindNextLiquidityBelow(
                                            Bars bars,
                                            int index,
                                            double price)
                                        {
                                            if (bars == null ||
                                                index < 10)
                                                return 0;
                                
                                            int first =
                                                Math.Max(
                                                    2,
                                                    index -
                                                    LiquidityLookback);
                                
                                            double best = 0;
                                
                                            for (int i = first;
                                                 i <= index - 2;
                                                 i++)
                                            {
                                                double low =
                                                    bars.LowPrices[i];
                                
                                                if (low >= price)
                                                    continue;
                                
                                                if (best <= 0 ||
                                                    low > best)
                                                    best = low;
                                            }
                                
                                            return best;
                                        }
    }
}
