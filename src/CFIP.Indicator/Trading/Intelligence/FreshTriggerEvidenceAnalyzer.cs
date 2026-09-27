// CFIP Indicator — FreshTriggerEvidenceAnalyzer.cs
// Single-responsibility intelligence module.

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
        private int FreshTriggerEvidence(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null || index < 5)
                                return 0;
                
                            int evidence = 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (direction == 1 &&
                                bars.ClosePrices[index] >
                                bars.OpenPrices[index])
                                evidence++;
                
                            if (direction == -1 &&
                                bars.ClosePrices[index] <
                                bars.OpenPrices[index])
                                evidence++;
                
                            if (atr > 0 &&
                                body >=
                                atr *
                                MinimumTriggerBodyAtr)
                                evidence++;
                
                            if (direction == 1 &&
                                bars.ClosePrices[index] >
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 5),
                                    index - 1))
                                evidence++;
                
                            if (direction == -1 &&
                                bars.ClosePrices[index] <
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 5),
                                    index - 1))
                                evidence++;
                
                            return evidence;
                        }
    }
}
