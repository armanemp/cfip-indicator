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
                
                            int quality = 55;

                            double rangeHigh =
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index -
                                        Math.Min(
                                            StructureLookback,
                                            40)),
                                    index);
                            double rangeLow =
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index -
                                        Math.Min(
                                            StructureLookback,
                                            40)),
                                    index);

                            double range =
                                rangeHigh -
                                rangeLow;

                            if (range > 0)
                            {
                                double position =
                                    (bars.ClosePrices[index] -
                                     rangeLow) /
                                    range;

                                if (direction == 1)
                                {
                                    if (position >= 0.86)
                                        quality -= 35;
                                    else if (position >= 0.74)
                                        quality -= 18;
                                }
                                else if (direction == -1)
                                {
                                    if (position <= 0.14)
                                        quality -= 35;
                                    else if (position <= 0.26)
                                        quality -= 18;
                                }
                            }
                
                            Zone zone =
                                FindNearestOpposingZone(
                                    bars,
                                    index,
                                    direction,
                                    atr);
                
                            if (zone != null)
                            {
                                double price =
                                    bars.ClosePrices[index];
                
                                if (price >=
                                        zone.Low -
                                        atr *
                                        ZoneProximityAtr &&
                                    price <=
                                        zone.High +
                                        atr *
                                        ZoneProximityAtr)
                                    quality += 30;
                
                                if (zone.Quality >= 80)
                                    quality += 15;
                            }
                
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
