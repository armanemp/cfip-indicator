// ============================================================================
// CFIP Indicator — TradingSessionFilter.cs
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
        private bool SessionAllowed(DateTime utc)
                        {
                            if (!UseSessionFilter)
                                return true;
                
                            if (SessionStartUtc <= SessionEndUtc)
                                return utc.Hour >= SessionStartUtc &&
                                       utc.Hour < SessionEndUtc;
                
                            return utc.Hour >= SessionStartUtc ||
                                   utc.Hour < SessionEndUtc;
                        }
        
        private bool FridayAllowed(DateTime utc)
                        {
                            if (!AvoidFridayLateEntry ||
                                utc.DayOfWeek != DayOfWeek.Friday)
                                return true;
                
                            return utc.Hour < FridayCutoffUtc;
                        }
        
        private bool SpreadAllowed(
                            Bars bars,
                            int index)
                        {
                            if (!UseSpreadFilter)
                                return true;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            if (atr <= 0)
                                return false;
                
                            double spread =
                                Math.Max(
                                    0,
                                    Symbol.Ask -
                                    Symbol.Bid);
                
                            return spread <=
                                   atr *
                                   MaximumSpreadAtr;
                        }
        
        private bool VolatilityBlocked(
                            Bars bars,
                            int index)
                        {
                            if ((!UseVolatilityGuard &&
                                 !UseVolatilityEventGuard) ||
                                bars == null ||
                                index < 10)
                                return false;
                
                            int window =
                                UseVolatilityEventGuard
                                    ? Math.Max(
                                        0,
                                        EventGuardCooldownBars)
                                    : 0;
                
                            int first =
                                Math.Max(
                                    1,
                                    index - window);
                
                            for (int i = first;
                                 i <= index;
                                 i++)
                            {
                                double barAtr =
                                    Atr(
                                        bars,
                                        i);
                
                                double priorAtr =
                                    Atr(
                                        bars,
                                        Math.Max(
                                            5,
                                            i - 10));
                
                                if (barAtr <= 0 ||
                                    priorAtr <= 0)
                                    continue;
                
                                double range =
                                    bars.HighPrices[i] -
                                    bars.LowPrices[i];
                
                                if (range >=
                                        barAtr *
                                        EventShockRangeAtr &&
                                    barAtr >=
                                        priorAtr *
                                        EventShockAtrExpansion)
                                    return true;
                            }
                
                            return false;
                        }
        
        private bool NewsBlocked(
                            DateTime utc,
                            out string reason)
                        {
                            reason = "";
                
                            if (string.IsNullOrWhiteSpace(
                                    NewsBlackoutUtc))
                                return false;
                
                            string[] items =
                                NewsBlackoutUtc.Split(
                                    new[] { ',', ';', '|' },
                                    StringSplitOptions.RemoveEmptyEntries);
                
                            int current =
                                utc.Hour * 60 +
                                utc.Minute;
                
                            for (int i = 0;
                                 i < items.Length;
                                 i++)
                            {
                                string[] parts =
                                    items[i]
                                    .Trim()
                                    .Split('-');
                
                                if (parts.Length != 2)
                                    continue;
                
                                int start;
                                int end;
                
                                if (!TryParseMinutes(
                                        parts[0],
                                        out start) ||
                                    !TryParseMinutes(
                                        parts[1],
                                        out end))
                                    continue;
                
                                bool blocked =
                                    start <= end
                                        ? current >= start &&
                                          current <= end
                                        : current >= start ||
                                          current <= end;
                
                                if (blocked)
                                {
                                    reason =
                                        "NEWS BLACKOUT";
                                    return true;
                                }
                            }
                
                            return false;
                        }
    }
}
