// ============================================================================
// CFIP Indicator — SuitabilityCalculator.cs
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
        private int CalculateMarketSuitability(
                                    int closedM5,
                                    int direction,
                                    out string reason)
                                {
                                    reason = "OK";
                        
                                    if (direction != 1 && direction != -1)
                                    {
                                        reason = "NO DIRECTION";
                                        return 0;
                                    }
                        
                                    if (_m5Bars == null ||
                                        _m5Frame == null ||
                                        _m15Frame == null)
                                    {
                                        reason = "MTF DATA";
                                        return 0;
                                    }
                        
                                    if (!Symbol.IsTradingEnabled)
                                    {
                                        reason = "SYMBOL TRADING DISABLED";
                                        return 0;
                                    }
                        
                                    if (Symbol.MarketHours == null ||
                                        !Symbol.MarketHours.IsOpened())
                                    {
                                        reason = "MARKET CLOSED";
                                        return 0;
                                    }
                        
                                    DateTime nowUtc = TimeInUtc;
                                    int score = 50;
                        
                                    bool inSession =
                                        _marketStateSnapshot != null &&
                                        _marketStateSnapshot.M5.ClosedIndex == closedM5
                                            ? _marketStateSnapshot.SessionOpen
                                            : IsInsideSessionWindow(nowUtc);
                        
                                    score += inSession ? 10 : -8;
                        
                                    if (UseSessionFilter &&
                                        !SessionAllowed(nowUtc))
                                    {
                                        reason = "SESSION FILTER";
                                        return 25;
                                    }
                        
                                    if (RequireSessionSuitability &&
                                        !inSession)
                                    {
                                        reason = "SESSION SUITABILITY";
                                        return 30;
                                    }
                        
                                    if (!FridayAllowed(nowUtc))
                                    {
                                        reason = "FRIDAY CUTOFF";
                                        return 25;
                                    }
                        
                                    string newsReason;
                        
                                    if (NewsBlocked(nowUtc, out newsReason))
                                    {
                                        reason = newsReason;
                                        return 20;
                                    }
                        
                                    if (VolatilityBlocked(_m5Bars, closedM5))
                                    {
                                        reason = "EVENT SHOCK / VOLATILITY";
                                        return 25;
                                    }
                        
                                    double atr =
                                        Atr(_m5Bars, closedM5);
                        
                                    if (atr <= 0)
                                    {
                                        reason = "ATR UNAVAILABLE";
                                        return 0;
                                    }
                        
                                    double spreadRatio =
                                        Math.Max(0, Symbol.Ask - Symbol.Bid) /
                                        Math.Max(atr, Symbol.TickSize);
                        
                                    if (spreadRatio <= MaximumSpreadAtr * 0.50)
                                        score += 18;
                                    else if (spreadRatio <= MaximumSpreadAtr)
                                        score += 8;
                                    else
                                        score -= 22;
                        
                                    double averageAtr =
                                        AverageAtr(_m5Bars, closedM5 - 1, 20);
                        
                                    double volatilityRatio =
                                        averageAtr > 0 ? atr / averageAtr : 1.0;
                        
                                    if (volatilityRatio >=
                                            Math.Max(0.75, HealthyAtrMinimumRatio) &&
                                        volatilityRatio <=
                                            Math.Max(1.10, HealthyAtrMaximumRatio))
                                        score += 12;
                                    else if (volatilityRatio < 0.60 ||
                                             volatilityRatio > 2.25)
                                        score -= 15;
                                    else
                                        score += 4;
                        
                                    if (_marketStateSnapshot != null &&
                                        _marketStateSnapshot.M5.Direction == direction)
                                        score += 9;
                                    else if (_marketStateSnapshot != null &&
                                             _marketStateSnapshot.M5.Direction == -direction)
                                        score -= 10;
                        
                                    if (_marketStateSnapshot != null &&
                                        _marketStateSnapshot.M15.Direction == direction)
                                        score += 9;
                                    else if (_marketStateSnapshot != null &&
                                             _marketStateSnapshot.M15.Direction == -direction)
                                        score -= 10;
                        
                                    if (_m30Frame != null)
                                    {
                                        if (_marketStateSnapshot != null &&
                                            _marketStateSnapshot.M30.Direction == direction)
                                            score += 5;
                                        else if (_marketStateSnapshot != null &&
                                                 _marketStateSnapshot.M30.Direction == -direction)
                                            score -= 6;
                                    }
                        
                                    if (_h1Frame != null)
                                    {
                                        if (_marketStateSnapshot != null &&
                                            _marketStateSnapshot.H1.Direction == direction)
                                            score += 4;
                                        else if (_marketStateSnapshot != null &&
                                                 _marketStateSnapshot.H1.Direction == -direction)
                                            score -= 5;
                                    }
                        
                                    if (_h4Frame != null)
                                    {
                                        if (_marketStateSnapshot != null &&
                                            _marketStateSnapshot.H4.Direction == direction)
                                            score += 3;
                                        else if (_marketStateSnapshot != null &&
                                                 _marketStateSnapshot.H4.Direction == -direction)
                                            score -= 4;
                                    }
                        
                                    double averageAdx =
                                        (_m5Frame.Adx + _m15Frame.Adx) / 2.0;
                        
                                    if (averageAdx >= 25)
                                        score += 12;
                                    else if (averageAdx >= 20)
                                        score += 7;
                                    else if (averageAdx < 15)
                                        score -= 8;
                        
                                    if (_m5Frame.Choppy && _m15Frame.Choppy)
                                        score -= 18;
                                    else if (_m5Frame.Choppy || _m15Frame.Choppy)
                                        score -= 7;
                        
                                    if (_decision != null &&
                                        _decision.TimeframeAgreement >=
                                        MinimumTimeframeAgreement)
                                        score += 8;
                                    else if (_decision != null &&
                                             _decision.TimeframeAgreement < 60)
                                        score -= 8;
                        
                                    if (_decision != null &&
                                        _decision.IndependentEvidence >=
                                        MinimumIndependentEvidence)
                                        score += 5;
                        
                                    if (UseDailyPivots &&
                                        _d1Bars != null &&
                                        _d1Bars.Count >= 3)
                                    {
                                        int d1Index =
                                            _lastMtfClosedContext == null
                                                ? -1
                                                : _lastMtfClosedContext.D1;
                        
                                        if (d1Index > 0)
                                        {
                                            int previous = d1Index;
                                            double pivot =
                                                (_d1Bars.HighPrices[previous] +
                                                 _d1Bars.LowPrices[previous] +
                                                 _d1Bars.ClosePrices[previous]) / 3.0;
                        
                                            double market =
                                                direction == 1
                                                    ? Symbol.Ask
                                                    : Symbol.Bid;
                        
                                            if ((direction == 1 && market >= pivot) ||
                                                (direction == -1 && market <= pivot))
                                                score += 5;
                                            else
                                                score -= 3;
                                        }
                                    }
                        
                                    score = ClampInt(score, 0, 100);
                        
                                    reason =
                                        score >= Math.Max(50, MinimumMarketSuitability)
                                            ? "SUITABILITY " + score
                                            : "SUITABILITY " + score +
                                              " < " + MinimumMarketSuitability;
                        
                                    return score;
                                }
    }
}
