// ============================================================================
// CFIP Indicator — ZoneLookup.cs
// ============================================================================

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
        // Internal FVG engine: standard 3-candle FVG plus optional 2-bar imbalance, with body/wick-aware partial mitigation. No chart objects are created here.
                                private Zone FindNearestFvgForExecution(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr,
                                    double market)
                                {
                                    return FindNearestFvg(
                                        bars,
                                        index,
                                        direction,
                                        atr,
                                        false,
                                        market,
                                        true);
                                }
        
        private Zone FindNearestOrderBlockForExecution(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr,
                                    double market)
                                {
                                    return FindNearestOrderBlock(
                                        bars,
                                        index,
                                        direction,
                                        atr,
                                        market,
                                        false);
                                }
        
        private Zone FindNearestOpposingZone(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr)
                                {
                                    Zone fvg =
                                        FindNearestFvg(
                                            bars,
                                            index,
                                            direction,
                                            atr);
                        
                                    Zone ob =
                                        FindNearestOrderBlock(
                                            bars,
                                            index,
                                            direction,
                                            atr);
                        
                                    if (fvg == null)
                                        return ob;
                        
                                    if (ob == null)
                                        return fvg;
                        
                                    return
                                        ob.Quality >=
                                        fvg.Quality
                                            ? ob
                                            : fvg;
                                }
        
        private double DistanceToZone(
                                    double price,
                                    Zone zone)
                                {
                                    if (zone == null)
                                        return double.MaxValue;
                        
                                    if (price < zone.Low)
                                        return zone.Low - price;
                        
                                    if (price > zone.High)
                                        return price - zone.High;
                        
                                    return 0;
                                }
    }
}
