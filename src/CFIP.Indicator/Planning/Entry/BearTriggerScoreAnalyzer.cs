// CFIP Indicator — BearTriggerScoreAnalyzer.cs
// Single-responsibility entry/trigger module.

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
private int BearTriggerScore(
                            Bars bars,
                            int index)
                        {
                            int score = 0;
                
                            double atr = Atr(bars, index);
                
                            double range =
                                Math.Max(
                                    Symbol.PipSize,
                                    bars.HighPrices[index] -
                                    bars.LowPrices[index]);
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (bars.ClosePrices[index] <
                                bars.OpenPrices[index])
                                score++;
                
                            if (body >= atr * MinimumTriggerBodyAtr)
                                score++;
                
                            if ((bars.HighPrices[index] -
                                 bars.ClosePrices[index]) /
                                range >= MinimumCloseLocation)
                                score++;
                
                            if (bars.ClosePrices[index] <
                                Ema(bars, index, true))
                                score++;
                
                            if (Rsi(bars, index) < 50)
                                score++;
                
                            if (DmiBias(bars, index) < 0)
                                score++;
                
                            if (UseDisplacement &&
                                body >= atr * DisplacementAtr)
                                score++;
                
                            return Math.Min(
                                6,
                                score);
                        }
    }
}
