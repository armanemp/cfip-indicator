// CFIP Indicator — EntryLocationQualityAnalyzer.cs
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
        private int EntryLocationQuality(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 10)
                                return 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            if (atr <= 0)
                                return 0;
                
                            int quality = 40;
                
                            Zone fvg =
                                FindEfficientFvg(
                                    bars,
                                    index,
                                    direction,
                                    atr);

                            Zone ob =
                                FindEfficientOrderBlock(
                                    bars,
                                    index,
                                    direction,
                                    atr);

                            double price =
                                bars.ClosePrices[index];

                            bool fvgNear =
                                fvg != null &&
                                price >=
                                    fvg.Low -
                                    atr * ZoneProximityAtr &&
                                price <=
                                    fvg.High +
                                    atr * ZoneProximityAtr;

                            bool obNear =
                                ob != null &&
                                price >=
                                    ob.Low -
                                    atr * ZoneProximityAtr &&
                                price <=
                                    ob.High +
                                    atr * ZoneProximityAtr;

                            if (fvgNear)
                                quality +=
                                    18 +
                                    Math.Min(
                                        8,
                                        fvg.Quality / 10);

                            if (obNear)
                                quality +=
                                    18 +
                                    Math.Min(
                                        8,
                                        ob.Quality / 10);

                            if (fvgNear &&
                                obNear &&
                                fvg.High >= ob.Low &&
                                ob.High >= fvg.Low)
                                quality += 10;
                
                            if (PremiumDiscountBias(
                                    bars,
                                    index) ==
                                direction)
                                quality += 10;
                
                            return ClampInt(
                                quality,
                                0,
                                100);
                        }
    }
}
