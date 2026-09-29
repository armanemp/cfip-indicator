// CFIP Indicator — AutoTradeSafetyGuard.cs
// Single-responsibility market risk/suitability module.

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
                        
                                    if (UseNewsEventGuard)
                                    {
                                        string newsReason;

                                        if (NewsBlocked(
                                                TimeInUtc,
                                                out newsReason))
                                        {
                                            reason =
                                                "NEWS";
                                            return false;
                                        }
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
