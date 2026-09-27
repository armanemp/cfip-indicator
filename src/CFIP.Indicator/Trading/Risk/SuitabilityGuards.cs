// ============================================================================
// CFIP Indicator — SuitabilityGuards.cs
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
        private bool IsInsideSessionWindow(DateTime utc)
                                {
                                    int start =
                                        ClampInt(SessionStartUtc, 0, 23) * 60;
                                    int end =
                                        ClampInt(SessionEndUtc, 0, 23) * 60;
                                    int now =
                                        utc.Hour * 60 + utc.Minute;
                        
                                    if (start == end)
                                        return true;
                        
                                    return start < end
                                        ? now >= start && now < end
                                        : now >= start || now < end;
                                }
        
        private double AverageAtr(Bars bars, int index, int lookback)
                                {
                                    if (bars == null || index < 1)
                                        return 0;
                        
                                    int count =
                                        Math.Max(2, Math.Min(lookback, index));
                                    int start =
                                        Math.Max(1, index - count + 1);
                        
                                    double sum = 0;
                                    int samples = 0;
                        
                                    for (int i = start; i <= index; i++)
                                    {
                                        double value = Atr(bars, i);
                        
                                        if (value <= 0)
                                            continue;
                        
                                        sum += value;
                                        samples++;
                                    }
                        
                                    return samples > 0 ? sum / samples : 0;
                                }
        
        private int RefreshMarketSuitability(
                                    int closedM5,
                                    int direction,
                                    bool force)
                                {
                                    DateTime nowUtc = TimeInUtc;
                        
                                    int throttleSeconds =
                                        Math.Max(1, SuitabilityRecalculationSeconds);
                        
                                    bool due =
                                        force ||
                                        closedM5 != _marketSuitabilityM5 ||
                                        _marketSuitabilityDirection != direction ||
                                        (nowUtc - _lastMarketSuitabilityUtc).TotalSeconds >=
                                        throttleSeconds;
                        
                                    if (!due)
                                        return _marketSuitabilityScore;
                        
                                    string reason;
                        
                                    int score =
                                        CalculateMarketSuitability(
                                            closedM5,
                                            direction,
                                            out reason);
                        
                                    _marketSuitabilityM5 = closedM5;
                                    _marketSuitabilityDirection = direction;
                                    _marketSuitabilityScore = score;
                                    _marketSuitabilityReason = reason;
                                    _marketSuitabilityState =
                                        score >= Math.Max(50, MinimumMarketSuitability)
                                            ? "SUITABLE"
                                            : "UNSUITABLE";
                                    _lastMarketSuitabilityUtc = nowUtc;
                        
                                    return score;
                                }
        
        private bool PassesMarketSuitability(
                                    int closedM5,
                                    int direction,
                                    out string reason)
                                {
                                    int score =
                                        RefreshMarketSuitability(
                                            closedM5,
                                            direction,
                                            false);
                        
                                    reason = _marketSuitabilityReason;
                        
                                    if (!EnableMarketSuitabilityGuard)
                                        return true;
                        
                                    bool hardContextBlock =
                                        _marketSuitabilityReason ==
                                            "SYMBOL TRADING DISABLED" ||
                                        _marketSuitabilityReason ==
                                            "MARKET CLOSED" ||
                                        _marketSuitabilityReason ==
                                            "SESSION FILTER" ||
                                        _marketSuitabilityReason ==
                                            "SESSION SUITABILITY" ||
                                        _marketSuitabilityReason ==
                                            "FRIDAY CUTOFF" ||
                                        _marketSuitabilityReason ==
                                            "NEWS BLACKOUT" ||
                                        _marketSuitabilityReason ==
                                            "EVENT SHOCK / VOLATILITY";
                        
                                    if (hardContextBlock &&
                                        HardMarketSuitabilityGate)
                                        return false;
                        
                                    if (score <
                                            Math.Max(
                                                50,
                                                MinimumMarketSuitability) &&
                                        HardMarketSuitabilityGate)
                                        return false;
                        
                                    return true;
                                }
        
        private double SuitabilityRiskMultiplier()
                                {
                                    if (!UseSmartRiskScaling)
                                        return 1.0;
                        
                                    double floor =
                                        ClampDouble(
                                            MinimumSmartRiskMultiplier,
                                            0.25,
                                            1.0);
                        
                                    double confidence =
                                        _decision == null
                                            ? 0
                                            : ClampDouble(
                                                _decision.Confidence,
                                                0,
                                                100);
                        
                                    double confidenceScale =
                                        ClampDouble(
                                            (confidence - 70.0) /
                                            Math.Max(
                                                1.0,
                                                Math.Max(
                                                    70,
                                                    FullRiskConfidenceThreshold) - 70.0),
                                            0,
                                            1);
                        
                                    double suitabilityScale =
                                        ClampDouble(
                                            _marketSuitabilityScore /
                                            (double)Math.Max(
                                                60,
                                                FullRiskSuitabilityThreshold),
                                            0,
                                            1);
                        
                                    double scale =
                                        floor +
                                        (1.0 - floor) *
                                        (0.60 * confidenceScale +
                                         0.40 * suitabilityScale);
                        
                                    if (PenalizeChoppyRegimeRisk &&
                                        _m5Frame != null &&
                                        _m15Frame != null &&
                                        (_m5Frame.Choppy ||
                                         _m15Frame.Choppy))
                                        scale *= 0.82;
                        
                                    return ClampDouble(
                                        scale,
                                        floor,
                                        1.0);
                                }
        
        private bool PassesAutoTradeSafetyGuards(
                                    TradeType tradeType,
                                    double volume,
                                    out string reason)
                                {
                                    reason = "";
                        
                                    if (!IsFinitePositive(volume))
                                    {
                                        reason = "INVALID VOLUME";
                                        return false;
                                    }
                        
                                    if (UseMarketHoursGuard)
                                    {
                                        if (!Symbol.IsTradingEnabled)
                                        {
                                            reason = "SYMBOL TRADING DISABLED";
                                            return false;
                                        }
                        
                                        if (Symbol.MarketHours == null ||
                                            !Symbol.MarketHours.IsOpened())
                                        {
                                            reason = "MARKET CLOSED";
                                            return false;
                                        }
                                    }
                        
                                    if (UseAutoMarginGuard)
                                    {
                                        double freeMargin =
                                            Account.FreeMargin;
                        
                                        if (!IsFinitePositive(freeMargin))
                                        {
                                            reason = "NO FREE MARGIN";
                                            return false;
                                        }
                        
                                        double estimatedMargin =
                                            Symbol.GetEstimatedMargin(
                                                tradeType,
                                                volume);
                        
                                        if (!IsFinitePositive(estimatedMargin))
                                        {
                                            reason = "MARGIN ESTIMATE FAILED";
                                            return false;
                                        }
                        
                                        double maximumUsage =
                                            Math.Max(
                                                10,
                                                Math.Min(
                                                    100,
                                                    MaxAutoMarginUsagePercent -
                                                    Math.Max(
                                                        0,
                                                        Math.Min(
                                                            40,
                                                            MarginBufferPercent))));
                        
                                        double allowedMargin =
                                            freeMargin *
                                            maximumUsage /
                                            100.0;
                        
                                        if (estimatedMargin >
                                            allowedMargin)
                                        {
                                            reason =
                                                "MARGIN " +
                                                estimatedMargin.ToString("F0") +
                                                " > " +
                                                allowedMargin.ToString("F0");
                                            return false;
                                        }
                                    }
                        
                                    return true;
                                }
    }
}
