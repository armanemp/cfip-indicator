// CFIP Indicator — HealthyVolatilityAnalyzer.cs
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
    }
}
