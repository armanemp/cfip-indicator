// ============================================================================
// CFIP Indicator — OrderBlockAnalyzer.cs
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
        private Zone FindNearestOrderBlock(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr,
                                    double selectionPrice = double.NaN,
                                    bool requireHistoricalRetest = true)
                                {
                                    if (!UseOrderBlock ||
                                        bars == null ||
                                        index < 8 ||
                                        atr <= 0)
                                        return null;
                        
                                    int effectiveLookback =
                                        Math.Min(
                                            ObLookback,
                                            Math.Max(
                                                1,
                                                MaximumZoneAgeBars));

                                    int first =
                                        Math.Max(
                                            2,
                                            index -
                                            effectiveLookback);
                        
                                    Zone best = null;
                                    double bestScore =
                                        double.MinValue;
                        
                                    double market =
                                        IsFinitePositive(selectionPrice)
                                            ? selectionPrice
                                            : bars.ClosePrices[index];
                        
                                    for (int i = index - 1;
                                         i >= first;
                                         i--)
                                    {
                                        bool opposite =
                                            direction == 1
                                                ? bars.ClosePrices[i] <
                                                  bars.OpenPrices[i]
                                                : bars.ClosePrices[i] >
                                                  bars.OpenPrices[i];
                        
                                        if (!opposite)
                                            continue;
                        
                                        Zone candidate =
                                            BuildOrderBlockCandidate(
                                                bars,
                                                i,
                                                index,
                                                direction,
                                                atr);
                        
                                        if (candidate == null ||
                                            candidate.Quality <
                                            Math.Max(
                                                50,
                                                ObMinimumQuality))
                                            continue;
                        
                                        if (requireHistoricalRetest &&
                                            RequireObRetest &&
                                            !HasZoneRetest(
                                                bars,
                                                i,
                                                index,
                                                candidate.Low,
                                                candidate.High))
                                            continue;
                        
                                        double distance =
                                            DistanceToZone(
                                                market,
                                                candidate);
                        
                                        double distancePenalty =
                                            14.0 *
                                            Math.Min(
                                                1.0,
                                                distance /
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    atr * 3.0));
                        
                                        double agePenalty =
                                            Math.Min(
                                                12.0,
                                                candidate.Age * 0.35);
                        
                                        double selectionScore =
                                            candidate.Quality -
                                            distancePenalty -
                                            agePenalty;
                        
                                        if (candidate.Quality >
                                            (best == null
                                                ? 0
                                                : best.Quality))
                                        {
                                            selectionScore += 2.0;
                                        }
                        
                                        if (best == null ||
                                            selectionScore >
                                            bestScore)
                                        {
                                            best =
                                                candidate;
                                            bestScore =
                                                selectionScore;
                                        }
                                    }
                        
                                    return best;
                                }
    }
}
