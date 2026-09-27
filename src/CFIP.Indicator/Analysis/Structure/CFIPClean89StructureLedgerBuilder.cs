// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89StructureLedgerBuilder
        {
            private const int MinimumIndex = 35;
    
            public CFIPClean89StructureSnapshot Build(
                CFIPClean89MtfSnapshot mtf,
                CFIPClean89MarketModel market,
                CFIPClean89ConfigSnapshot configuration,
                Bars m5Bars, Bars m15Bars, Bars m30Bars, Bars h1Bars, Bars h4Bars,
                Bars d1Bars, Bars w1Bars)
            {
                var events = new List<CFIPClean89StructureEventRecord>();
                var zones = new List<CFIPClean89ZoneRecord>();
                var liquidity = new List<CFIPClean89LiquidityRecord>();
    
                if (mtf == null || market == null ||
                    !mtf.IsPrimaryDecisionReady)
                {
                    return new CFIPClean89StructureSnapshot(
                        mtf == null ? DateTime.MinValue : mtf.ReferenceUtc,
                        false, false, CFIPClean89Direction.Wait, 0,
                        events, zones, liquidity,
                        new CFIPClean89PremiumDiscountState(false, 0, 0, 0));
                }
    
                BuildTimeframe(events, zones, liquidity, "M5", m5Bars, mtf.M5, market.M5, configuration, true);
                BuildTimeframe(events, zones, liquidity, "M15", m15Bars, mtf.M15, market.M15, configuration, false);
                BuildTimeframe(events, zones, liquidity, "M30", m30Bars, mtf.M30, market.FindFrame("M30"), configuration, false);
                BuildTimeframe(events, zones, liquidity, "H1", h1Bars, mtf.H1, market.FindFrame("H1"), configuration, false);
                BuildTimeframe(events, zones, liquidity, "H4", h4Bars, mtf.H4, market.FindFrame("H4"), configuration, false);
    
                AddPriorDayWeekLiquidity(liquidity, mtf, d1Bars, w1Bars, configuration);
                AddSessionLiquidity(liquidity, mtf, m5Bars, configuration);
                AddDailyPivots(liquidity, mtf, d1Bars, configuration);
                MarkForecastLiquidity(
                    liquidity,
                    market.M5,
                    configuration);
                MarkConfluence(zones, liquidity, configuration);
    
                var direction = ResolveDirection(events);
                int quality = CalculateQuality(events, zones, liquidity, direction);
                var pd = BuildPremiumDiscount(m5Bars, mtf.M5.ClosedIndex, configuration);
    
                return new CFIPClean89StructureSnapshot(
                    mtf.ReferenceUtc,
                    CFIPClean89MtfSnapshotBuilder.IsCoherent(mtf),
                    mtf.IsPrimaryDecisionReady,
                    direction,
                    quality,
                    events, zones, liquidity, pd);
            }
    
            private void BuildTimeframe(
                IList<CFIPClean89StructureEventRecord> events,
                IList<CFIPClean89ZoneRecord> zones,
                IList<CFIPClean89LiquidityRecord> liquidity,
                string timeframe, Bars bars, CFIPClean89MtfBarSnapshot snapshot,
                CFIPClean89MarketFrame frame, CFIPClean89ConfigSnapshot cfg, bool primary)
            {
                if (bars == null || snapshot == null || frame == null ||
                    !snapshot.IsAvailable || !snapshot.IsFullyClosedAtReference ||
                    snapshot.ClosedIndex < MinimumIndex || !frame.DataValid)
                    return;
    
                int index = snapshot.ClosedIndex;
                double atr = frame.Atr;
                if (atr <= 0) return;
    
                int strength = Math.Max(1, cfg.Get("SwingStrength", 3));
                int lookback = Math.Max(20, cfg.Get("StructureLookback", 60));
                double breakAtr = Math.Max(0, cfg.Get("StructureBreakAtr", 0.02));
    
                int swingHigh = FindLatestSwingHigh(bars, index, strength, lookback);
                int swingLow = FindLatestSwingLow(bars, index, strength, lookback);
    
                if (cfg.Get("UseInternalStructure", true))
                {
                    if (swingHigh >= 0)
                        AddEvent(events, CFIPClean89StructureEventKind.SwingHigh,
                            CFIPClean89Direction.Sell, timeframe, bars, swingHigh,
                            bars.HighPrices[swingHigh], 0, 68, "confirmed swing high");
    
                    if (swingLow >= 0)
                        AddEvent(events, CFIPClean89StructureEventKind.SwingLow,
                            CFIPClean89Direction.Buy, timeframe, bars, swingLow,
                            bars.LowPrices[swingLow], 0, 68, "confirmed swing low");
    
                    if (swingHigh >= 0 &&
                        bars.ClosePrices[index] > bars.HighPrices[swingHigh] + atr * breakAtr)
                    {
                        double strengthAtr =
                            (bars.ClosePrices[index] - bars.HighPrices[swingHigh]) / atr;
                        AddEvent(events, CFIPClean89StructureEventKind.BreakOfStructure,
                            CFIPClean89Direction.Buy, timeframe, bars, index,
                            bars.ClosePrices[index], strengthAtr,
                            Clamp(70 + (int)Math.Round(Math.Min(20, strengthAtr * 8))),
                            "close beyond latest swing high");
    
                        if (IsBearishPreBreak(bars, index, swingHigh))
                            AddEvent(events, CFIPClean89StructureEventKind.MarketStructureShift,
                                CFIPClean89Direction.Buy, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 82,
                                "bullish break after bearish sequence");
    
                        if (IsCharacterBreakBull(bars, index, strength, lookback, atr, breakAtr))
                            AddEvent(events, CFIPClean89StructureEventKind.ChangeOfCharacter,
                                CFIPClean89Direction.Buy, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 78,
                                "bullish change of character");
                    }
    
                    if (swingLow >= 0 &&
                        bars.ClosePrices[index] < bars.LowPrices[swingLow] - atr * breakAtr)
                    {
                        double strengthAtr =
                            (bars.LowPrices[swingLow] - bars.ClosePrices[index]) / atr;
                        AddEvent(events, CFIPClean89StructureEventKind.BreakOfStructure,
                            CFIPClean89Direction.Sell, timeframe, bars, index,
                            bars.ClosePrices[index], strengthAtr,
                            Clamp(70 + (int)Math.Round(Math.Min(20, strengthAtr * 8))),
                            "close beyond latest swing low");
    
                        if (IsBullishPreBreak(bars, index, swingLow))
                            AddEvent(events, CFIPClean89StructureEventKind.MarketStructureShift,
                                CFIPClean89Direction.Sell, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 82,
                                "bearish break after bullish sequence");
    
                        if (IsCharacterBreakBear(bars, index, strength, lookback, atr, breakAtr))
                            AddEvent(events, CFIPClean89StructureEventKind.ChangeOfCharacter,
                                CFIPClean89Direction.Sell, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 78,
                                "bearish change of character");
                    }
                }
    
                if (cfg.Get("UseDisplacement", true))
                {
                    double bodyAtr =
                        Math.Abs(bars.ClosePrices[index] - bars.OpenPrices[index]) / atr;
                    double threshold = Math.Max(0, cfg.Get("DisplacementAtr", 0.80));
                    CFIPClean89Direction d =
                        bars.ClosePrices[index] > bars.OpenPrices[index]
                            ? CFIPClean89Direction.Buy
                            : bars.ClosePrices[index] < bars.OpenPrices[index]
                                ? CFIPClean89Direction.Sell
                                : CFIPClean89Direction.Wait;
    
                    if (d != CFIPClean89Direction.Wait && bodyAtr >= threshold)
                        AddEvent(events, CFIPClean89StructureEventKind.Displacement,
                            d, timeframe, bars, index, bars.ClosePrices[index], bodyAtr,
                            Clamp(70 + (int)Math.Round(Math.Min(25, bodyAtr * 10))),
                            "closed body exceeds displacement ATR threshold");
                }
    
                if (cfg.Get("UseFvg", true))
                    BuildFvgZones(zones, bars, index, timeframe, atr, cfg);
    
                if (cfg.Get("UseOrderBlock", true))
                    BuildObZones(zones, bars, index, timeframe, atr, cfg);
    
                if (cfg.Get("UseEqualHighLow", true))
                    BuildEqualLiquidity(liquidity, bars, index, timeframe, atr, cfg);
    
                if (cfg.Get("UseLiquiditySweep", true))
                    BuildSweepLiquidity(liquidity, events, bars, index, timeframe, atr, cfg);
    
                if (primary && cfg.Get("UseExtendedLiquidityMap", true))
                    AddSwingLiquidity(liquidity, bars, index, timeframe, atr, cfg);
            }
    
            private void BuildFvgZones(
                IList<CFIPClean89ZoneRecord> zones, Bars bars, int index,
                string timeframe, double atr, CFIPClean89ConfigSnapshot cfg)
            {
                int lookback = Math.Max(3, cfg.Get("FvgLookback", 80));
                double minGap = Math.Max(0, cfg.Get("MinimumFvgAtr", 0.05));
                int maxAge = Math.Max(1, cfg.Get("MaximumZoneAgeBars", 120));
                int start = Math.Max(2, index - lookback);
                int count = 0;
    
                for (int i = index; i >= start && count < 24; i--)
                {
                    double bullGap = bars.LowPrices[i] - bars.HighPrices[i - 2];
                    if (bullGap >= atr * minGap)
                    {
                        zones.Add(BuildFvg(
                            bars, i, index, timeframe, CFIPClean89Direction.Buy,
                            bars.HighPrices[i - 2], bars.LowPrices[i],
                            bullGap, atr, maxAge, cfg, "three-candle imbalance FVG"));
                        count++;
                    }
    
                    double bearGap = bars.LowPrices[i - 2] - bars.HighPrices[i];
                    if (bearGap >= atr * minGap && count < 24)
                    {
                        zones.Add(BuildFvg(
                            bars, i, index, timeframe, CFIPClean89Direction.Sell,
                            bars.HighPrices[i], bars.LowPrices[i - 2],
                            bearGap, atr, maxAge, cfg, "three-candle imbalance FVG"));
                        count++;
                    }
    
                    if (cfg.Get("UseTwoBarImbalanceFvg", false) &&
                        count < 24 &&
                        i >= 1)
                    {
                        double twoBarBull =
                            bars.LowPrices[i] -
                            bars.HighPrices[i - 1];
    
                        if (twoBarBull >= atr * minGap)
                        {
                            zones.Add(BuildFvg(
                                bars, i, index, timeframe, CFIPClean89Direction.Buy,
                                bars.HighPrices[i - 1], bars.LowPrices[i],
                                twoBarBull, atr, maxAge, cfg,
                                "two-bar imbalance FVG"));
                            count++;
                        }
    
                        double twoBarBear =
                            bars.LowPrices[i - 1] -
                            bars.HighPrices[i];
    
                        if (twoBarBear >= atr * minGap && count < 24)
                        {
                            zones.Add(BuildFvg(
                                bars, i, index, timeframe, CFIPClean89Direction.Sell,
                                bars.HighPrices[i], bars.LowPrices[i - 1],
                                twoBarBear, atr, maxAge, cfg,
                                "two-bar imbalance FVG"));
                            count++;
                        }
                    }
                }
            }
    
            private CFIPClean89ZoneRecord BuildFvg(
                Bars bars, int created, int current, string timeframe,
                CFIPClean89Direction direction, double lower, double upper,
                double gap, double atr, int maxAge,
                CFIPClean89ConfigSnapshot cfg,
                string rule)
            {
                bool retested = false, partial = false, consumed = false;
                bool partialEnabled =
                    cfg.Get(
                        "EnableFvgPartialMitigation",
                        true);
                double currentLower = lower, currentUpper = upper;
                bool breakByWicks = cfg.Get("FvgBreakByWicks", true);
    
                for (int i = created + 1; i <= current; i++)
                {
                    bool overlap =
                        bars.HighPrices[i] >= lower &&
                        bars.LowPrices[i] <= upper;
    
                    if (!overlap) continue;
                    retested = true;
    
                    if (direction == CFIPClean89Direction.Buy)
                    {
                        if (partialEnabled &&
                            bars.LowPrices[i] > lower)
                        {
                            partial = true;
                            currentLower =
                                Math.Max(
                                    lower,
                                    Math.Min(
                                        upper,
                                        bars.LowPrices[i]));
                        }
    
                        if ((breakByWicks && bars.LowPrices[i] <= lower) ||
                            (!breakByWicks && bars.ClosePrices[i] <= lower))
                        {
                            // Full consumption is a safety invariant; it cannot
                            // be disabled by a presentation/configuration toggle.
                            consumed = true;
                            break;
                        }
                    }
                    else
                    {
                        if (partialEnabled &&
                            bars.HighPrices[i] < upper)
                        {
                            partial = true;
                            currentUpper =
                                Math.Min(
                                    upper,
                                    Math.Max(
                                        lower,
                                        bars.HighPrices[i]));
                        }
    
                        if ((breakByWicks && bars.HighPrices[i] >= upper) ||
                            (!breakByWicks && bars.ClosePrices[i] >= upper))
                        {
                            consumed = true;
                            break;
                        }
                    }
                }
    
                int age = Math.Max(0, current - created);
                bool expired = age > maxAge;
                bool eligible = !consumed && !expired && currentUpper > currentLower;
    
                if (consumed) { currentLower = 0; currentUpper = 0; }
    
                var lifecycle =
                    consumed ? CFIPClean89ZoneLifecycle.Consumed :
                    expired ? CFIPClean89ZoneLifecycle.Expired :
                    partial ? CFIPClean89ZoneLifecycle.PartiallyMitigated :
                    retested ? CFIPClean89ZoneLifecycle.Retested :
                    CFIPClean89ZoneLifecycle.Active;
    
                int quality = Clamp(
                    55 +
                    (int)Math.Round(Math.Min(20, gap / Math.Max(0.0000001, atr) * 12)) +
                    (retested ? 8 : 0) -
                    (partial ? 6 : 0) -
                    (expired ? 25 : 0));
    
                return new CFIPClean89ZoneRecord(
                    "CFIP89|" + timeframe + "|FVG|" + direction + "|" + created,
                    CFIPClean89ZoneKind.FairValueGap, direction, timeframe, created,
                    bars.OpenTimes[created], lower, upper,
                    eligible ? currentLower : 0,
                    eligible ? currentUpper : 0,
                    age, retested, partial, consumed, consumed, eligible,
                    quality, gap / Math.Max(0.0000001, atr),
                    false, false, lifecycle,
                    CFIPClean89Provenance.Direct(timeframe, rule ?? "FVG"));
            }
    
            private void BuildObZones(
                IList<CFIPClean89ZoneRecord> zones, Bars bars, int index,
                string timeframe, double atr, CFIPClean89ConfigSnapshot cfg)
            {
                int lookback = Math.Max(5, cfg.Get("ObStructureLookback", 60));
                double threshold = Math.Max(0, cfg.Get("ObDisplacementAtr", 0.80));
                bool require = cfg.Get("RequireObDisplacement", true);
                bool bodyOnly = cfg.Get("ObUseBodyForZone", true);
                int maxAge = Math.Max(1, cfg.Get("MaximumZoneAgeBars", 120));
                int start = Math.Max(2, index - lookback);
                int count = 0;
    
                for (int impulse = index; impulse >= start && count < 16; impulse--)
                {
                    double bodyAtr =
                        Math.Abs(bars.ClosePrices[impulse] - bars.OpenPrices[impulse]) / Math.Max(0.0000001, atr);
    
                    if (require && bodyAtr < threshold) continue;
    
                    CFIPClean89Direction direction =
                        bars.ClosePrices[impulse] > bars.OpenPrices[impulse]
                            ? CFIPClean89Direction.Buy
                            : bars.ClosePrices[impulse] < bars.OpenPrices[impulse]
                                ? CFIPClean89Direction.Sell
                                : CFIPClean89Direction.Wait;
    
                    if (direction == CFIPClean89Direction.Wait) continue;
    
                    int source = -1;
                    for (int j = impulse - 1; j >= start; j--)
                    {
                        bool opposite =
                            direction == CFIPClean89Direction.Buy
                                ? bars.ClosePrices[j] < bars.OpenPrices[j]
                                : bars.ClosePrices[j] > bars.OpenPrices[j];
                        if (opposite) { source = j; break; }
                    }
    
                    if (source < 0) continue;
    
                    double bodyLow = Math.Min(bars.OpenPrices[source], bars.ClosePrices[source]);
                    double bodyHigh = Math.Max(bars.OpenPrices[source], bars.ClosePrices[source]);
                    double low = bodyOnly ? bodyLow : bars.LowPrices[source];
                    double high = bodyOnly ? bodyHigh : bars.HighPrices[source];
    
                    bool retested = false, consumed = false;
                    for (int k = source + 1; k <= index; k++)
                    {
                        bool overlap =
                            bars.HighPrices[k] >= low &&
                            bars.LowPrices[k] <= high;
    
                        if (overlap) retested = true;
    
                        bool breach =
                            direction == CFIPClean89Direction.Buy
                                ? bars.LowPrices[k] <= low
                                : bars.HighPrices[k] >= high;
    
                        if (breach) { consumed = true; break; }
                    }
    
                    int age = Math.Max(0, index - source);
                    bool expired = age > maxAge;
                    bool eligible = !consumed && !expired && high > low;
                    var lifecycle =
                        consumed ? CFIPClean89ZoneLifecycle.Consumed :
                        expired ? CFIPClean89ZoneLifecycle.Expired :
                        retested ? CFIPClean89ZoneLifecycle.Retested :
                        CFIPClean89ZoneLifecycle.Active;
    
                    int quality = Clamp(
                        60 + (int)Math.Round(Math.Min(25, bodyAtr * 12)) +
                        (retested ? 5 : 0) - (expired ? 20 : 0));
    
                    zones.Add(new CFIPClean89ZoneRecord(
                        "CFIP89|" + timeframe + "|OB|" + direction + "|" + source,
                        CFIPClean89ZoneKind.OrderBlock, direction, timeframe, source,
                        bars.OpenTimes[source], low, high,
                        eligible ? low : 0, eligible ? high : 0,
                        age, retested, retested && !consumed,
                        consumed || expired, consumed, eligible,
                        quality, bodyAtr, false, false, lifecycle,
                        CFIPClean89Provenance.Direct(timeframe, "opposite candle before displacement")));
                    count++;
                }
            }
    
            private void BuildEqualLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                Bars bars, int index, string timeframe,
                double atr, CFIPClean89ConfigSnapshot cfg)
            {
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                int strength = Math.Max(1, cfg.Get("SwingStrength", 3));
                double tolerance =
                    atr * Math.Max(0, cfg.Get("EqualLevelToleranceAtr", 0.08));
    
                int high = FindLatestSwingHigh(bars, index, strength, lookback);
                int low = FindLatestSwingLow(bars, index, strength, lookback);
    
                if (high >= 0)
                {
                    for (int i = high - strength; i >= Math.Max(1, index - lookback); i--)
                        if (IsSwingHigh(bars, i, strength) &&
                            Math.Abs(bars.HighPrices[high] - bars.HighPrices[i]) <= tolerance)
                        {
                            AddLiquidity(
                                liquidity, CFIPClean89LiquidityKind.EqualHigh,
                                CFIPClean89LiquiditySide.Above,
                                CFIPClean89Direction.Wait, timeframe, bars, i,
                                (bars.HighPrices[high] + bars.HighPrices[i]) / 2.0,
                                tolerance, 78, false, "equal highs pool");
                            break;
                        }
                }
    
                if (low >= 0)
                {
                    for (int i = low - strength; i >= Math.Max(1, index - lookback); i--)
                        if (IsSwingLow(bars, i, strength) &&
                            Math.Abs(bars.LowPrices[low] - bars.LowPrices[i]) <= tolerance)
                        {
                            AddLiquidity(
                                liquidity, CFIPClean89LiquidityKind.EqualLow,
                                CFIPClean89LiquiditySide.Below,
                                CFIPClean89Direction.Wait, timeframe, bars, i,
                                (bars.LowPrices[low] + bars.LowPrices[i]) / 2.0,
                                tolerance, 78, false, "equal lows pool");
                            break;
                        }
                }
            }
    
            private void BuildSweepLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                IList<CFIPClean89StructureEventRecord> events,
                Bars bars, int index, string timeframe,
                double atr, CFIPClean89ConfigSnapshot cfg)
            {
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                double minDepth = Math.Max(0, atr * cfg.Get("LiquiditySweepMinimumDepthAtr", 0.05));
                double priorLow = Lowest(bars, Math.Max(0, index - lookback), index - 1);
                double priorHigh = Highest(bars, Math.Max(0, index - lookback), index - 1);
    
                double lowPen = Math.Max(0, priorLow - bars.LowPrices[index]);
                if (lowPen >= minDepth && bars.ClosePrices[index] > priorLow)
                {
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SwingLow,
                        CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Buy,
                        timeframe, bars, index, priorLow, minDepth, 90, true,
                        "bullish liquidity sweep");
    
                    AddEvent(events, CFIPClean89StructureEventKind.LiquiditySweep,
                        CFIPClean89Direction.Buy, timeframe, bars, index, priorLow,
                        lowPen / Math.Max(0.0000001, atr), 90,
                        "low penetration and recovery");
                }
    
                double highPen = Math.Max(0, bars.HighPrices[index] - priorHigh);
                if (highPen >= minDepth && bars.ClosePrices[index] < priorHigh)
                {
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SwingHigh,
                        CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Sell,
                        timeframe, bars, index, priorHigh, minDepth, 90, true,
                        "bearish liquidity sweep");
    
                    AddEvent(events, CFIPClean89StructureEventKind.LiquiditySweep,
                        CFIPClean89Direction.Sell, timeframe, bars, index, priorHigh,
                        highPen / Math.Max(0.0000001, atr), 90,
                        "high penetration and recovery");
                }
            }
    
            private void AddSwingLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                Bars bars, int index, string timeframe, double atr,
                CFIPClean89ConfigSnapshot cfg)
            {
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                int strength = Math.Max(1, cfg.Get("SwingStrength", 3));
                int high = FindLatestSwingHigh(bars, index, strength, lookback);
                int low = FindLatestSwingLow(bars, index, strength, lookback);
    
                if (high >= 0)
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SwingHigh,
                        CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                        timeframe, bars, high, bars.HighPrices[high], atr * 0.02, 72,
                        false, "latest confirmed swing high");
    
                if (low >= 0)
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SwingLow,
                        CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                        timeframe, bars, low, bars.LowPrices[low], atr * 0.02, 72,
                        false, "latest confirmed swing low");
            }
    
            private void AddPriorDayWeekLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89MtfSnapshot mtf, Bars d1Bars, Bars w1Bars,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseDailyWeeklyLiquidity", true)) return;
    
                if (d1Bars != null && mtf.D1.IsAvailable && mtf.D1.ClosedIndex > 0)
                {
                    int p = mtf.D1.ClosedIndex - 1;
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.PriorDayHigh,
                        CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                        "D1", d1Bars, p, d1Bars.HighPrices[p], 0, 82, false, "prior day high");
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.PriorDayLow,
                        CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                        "D1", d1Bars, p, d1Bars.LowPrices[p], 0, 82, false, "prior day low");
                }
    
                if (w1Bars != null && mtf.W1.IsAvailable && mtf.W1.ClosedIndex > 0)
                {
                    int p = mtf.W1.ClosedIndex - 1;
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.PriorWeekHigh,
                        CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                        "W1", w1Bars, p, w1Bars.HighPrices[p], 0, 86, false, "prior week high");
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.PriorWeekLow,
                        CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                        "W1", w1Bars, p, w1Bars.LowPrices[p], 0, 86, false, "prior week low");
                }
            }
    
            private void AddSessionLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89MtfSnapshot mtf, Bars bars,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseSessionLiquidityTargets", true) ||
                    bars == null || !mtf.M5.IsAvailable) return;
    
                int startHour = Math.Max(0, Math.Min(23, cfg.Get("SessionStartUtc", 6)));
                int endHour = Math.Max(0, Math.Min(23, cfg.Get("SessionEndUtc", 20)));
    
                DateTime sessionStart = new DateTime(
                    mtf.ReferenceUtc.Year, mtf.ReferenceUtc.Month, mtf.ReferenceUtc.Day,
                    startHour, 0, 0, DateTimeKind.Utc);
    
                DateTime sessionEnd = new DateTime(
                    mtf.ReferenceUtc.Year, mtf.ReferenceUtc.Month, mtf.ReferenceUtc.Day,
                    endHour, 0, 0, DateTimeKind.Utc);
    
                if (endHour <= startHour)
                {
                    if (mtf.ReferenceUtc < sessionStart) sessionStart = sessionStart.AddDays(-1);
                    sessionEnd = sessionStart.AddDays(1);
                    sessionEnd = new DateTime(
                        sessionEnd.Year, sessionEnd.Month, sessionEnd.Day,
                        endHour, 0, 0, DateTimeKind.Utc);
                }
    
                double high = double.MinValue, low = double.MaxValue;
                int highIndex = -1, lowIndex = -1;
                int first = Math.Max(0, mtf.M5.ClosedIndex - 700);
    
                for (int i = first; i <= mtf.M5.ClosedIndex; i++)
                {
                    DateTime t = bars.OpenTimes[i];
                    if (t < sessionStart || t >= sessionEnd) continue;
    
                    if (bars.HighPrices[i] > high) { high = bars.HighPrices[i]; highIndex = i; }
                    if (bars.LowPrices[i] < low) { low = bars.LowPrices[i]; lowIndex = i; }
                }
    
                if (highIndex >= 0)
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SessionHigh,
                        CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                        "M5", bars, highIndex, high, 0, 74, false, "configured session high");
    
                if (lowIndex >= 0)
                    AddLiquidity(liquidity, CFIPClean89LiquidityKind.SessionLow,
                        CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                        "M5", bars, lowIndex, low, 0, 74, false, "configured session low");
            }
    
            private void AddDailyPivots(
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89MtfSnapshot mtf, Bars d1Bars,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseDailyPivots", true) ||
                    d1Bars == null || !mtf.D1.IsAvailable ||
                    mtf.D1.ClosedIndex <= 0) return;
    
                int p = mtf.D1.ClosedIndex - 1;
                double h = d1Bars.HighPrices[p], l = d1Bars.LowPrices[p], c = d1Bars.ClosePrices[p];
                double pivot = (h + l + c) / 3.0, range = h - l;
    
                AddLiquidity(liquidity, CFIPClean89LiquidityKind.DailyPivot,
                    CFIPClean89LiquiditySide.Neutral, CFIPClean89Direction.Wait,
                    "D1", d1Bars, p, pivot, 0, 62, false, "daily pivot");
    
                AddLiquidity(liquidity, CFIPClean89LiquidityKind.DailyR1,
                    CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                    "D1", d1Bars, p, 2 * pivot - l, 0, 64, false, "daily R1");
    
                AddLiquidity(liquidity, CFIPClean89LiquidityKind.DailyR2,
                    CFIPClean89LiquiditySide.Above, CFIPClean89Direction.Wait,
                    "D1", d1Bars, p, pivot + range, 0, 66, false, "daily R2");
    
                AddLiquidity(liquidity, CFIPClean89LiquidityKind.DailyS1,
                    CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                    "D1", d1Bars, p, 2 * pivot - h, 0, 64, false, "daily S1");
    
                AddLiquidity(liquidity, CFIPClean89LiquidityKind.DailyS2,
                    CFIPClean89LiquiditySide.Below, CFIPClean89Direction.Wait,
                    "D1", d1Bars, p, pivot - range, 0, 66, false, "daily S2");
            }
    
            private void MarkForecastLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89MarketFrame m5,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseLiquidityForecast", true) ||
                    m5 == null ||
                    !m5.DataValid)
                    return;
    
                double price = m5.Close;
    
                for (int i = 0; i < liquidity.Count; i++)
                {
                    CFIPClean89LiquidityRecord item = liquidity[i];
    
                    bool ahead =
                        (item.Side == CFIPClean89LiquiditySide.Above &&
                         item.Price > price) ||
                        (item.Side == CFIPClean89LiquiditySide.Below &&
                         item.Price < price);
    
                    if (!ahead ||
                        item.Swept ||
                        item.ForecastCandidate)
                        continue;
    
                    liquidity[i] =
                        new CFIPClean89LiquidityRecord(
                            item.Id,
                            item.Kind,
                            item.Side,
                            item.SweepDirection,
                            item.Timeframe,
                            item.BarIndex,
                            item.TimeUtc,
                            item.Price,
                            item.Tolerance,
                            item.Distance,
                            item.Penetration,
                            item.Swept,
                            true,
                            item.Quality,
                            item.Provenance);
                }
            }
    
            private void MarkConfluence(
                IList<CFIPClean89ZoneRecord> zones,
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89ConfigSnapshot cfg)
            {
                bool enabled = cfg.Get("UseZoneConfluence", true);
    
                for (int i = 0; i < zones.Count; i++)
                {
                    var z = zones[i];
                    bool fvg = z.Kind == CFIPClean89ZoneKind.OrderBlock &&
                               HasOverlap(zones, z, CFIPClean89ZoneKind.FairValueGap);
                    bool liq = enabled && HasNearbyLiquidity(liquidity, z);
    
                    // Keep base zone quality independent. Confluence is carried
                    // explicitly by typed flags and consumed once by Decision quality.
                    int q = Clamp(z.Quality);
    
                    zones[i] = new CFIPClean89ZoneRecord(
                        z.Id, z.Kind, z.Direction, z.Timeframe, z.CreatedIndex, z.CreatedUtc,
                        z.OriginalLower, z.OriginalUpper, z.CurrentLower, z.CurrentUpper,
                        z.AgeBars, z.Retested, z.Mitigated, z.Invalidated, z.Consumed, z.ExecutionEligible,
                        q, z.DisplacementAtr, liq, fvg, z.Lifecycle, z.Provenance);
                }
            }
    
            private bool HasOverlap(
                IList<CFIPClean89ZoneRecord> zones, CFIPClean89ZoneRecord source,
                CFIPClean89ZoneKind kind)
            {
                for (int i = 0; i < zones.Count; i++)
                {
                    var z = zones[i];
                    if (z.Id == source.Id || z.Kind != kind ||
                        z.Direction != source.Direction || z.Consumed || z.Invalidated)
                        continue;
                    if (z.CurrentUpper >= source.CurrentLower &&
                        z.CurrentLower <= source.CurrentUpper)
                        return true;
                }
                return false;
            }
    
            private bool HasNearbyLiquidity(
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89ZoneRecord zone)
            {
                double width = Math.Max(0, zone.CurrentUpper - zone.CurrentLower);
                for (int i = 0; i < liquidity.Count; i++)
                {
                    if (liquidity[i].Price >= zone.CurrentLower - width &&
                        liquidity[i].Price <= zone.CurrentUpper + width)
                        return true;
                }
                return false;
            }
    
            private CFIPClean89Direction ResolveDirection(
                IList<CFIPClean89StructureEventRecord> events)
            {
                CFIPClean89StructureEventRecord bull = null, bear = null;
                for (int i = 0; i < events.Count; i++)
                {
                    var e = events[i];
                    if (!IsDirectional(e.Kind)) continue;
    
                    if (e.Direction == CFIPClean89Direction.Buy &&
                        (bull == null || e.TimeUtc > bull.TimeUtc)) bull = e;
    
                    if (e.Direction == CFIPClean89Direction.Sell &&
                        (bear == null || e.TimeUtc > bear.TimeUtc)) bear = e;
                }
    
                if (bull == null && bear == null) return CFIPClean89Direction.Wait;
                if (bear == null) return CFIPClean89Direction.Buy;
                if (bull == null) return CFIPClean89Direction.Sell;
                return bull.TimeUtc > bear.TimeUtc
                    ? CFIPClean89Direction.Buy
                    : CFIPClean89Direction.Sell;
            }
    
            private bool IsDirectional(CFIPClean89StructureEventKind kind)
            {
                return kind == CFIPClean89StructureEventKind.BreakOfStructure ||
                       kind == CFIPClean89StructureEventKind.MarketStructureShift ||
                       kind == CFIPClean89StructureEventKind.ChangeOfCharacter ||
                       kind == CFIPClean89StructureEventKind.Displacement ||
                       kind == CFIPClean89StructureEventKind.LiquiditySweep;
            }
    
            private int CalculateQuality(
                IList<CFIPClean89StructureEventRecord> events,
                IList<CFIPClean89ZoneRecord> zones,
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89Direction direction)
            {
                int eventScore = 0, eventCount = 0, zoneScore = 0, zoneCount = 0, swept = 0;
    
                for (int i = 0; i < events.Count; i++)
                    if (events[i].Direction == direction && IsDirectional(events[i].Kind))
                    { eventScore += events[i].Quality; eventCount++; }
    
                for (int i = 0; i < zones.Count; i++)
                    if (zones[i].Direction == direction && zones[i].ExecutionEligible)
                    { zoneScore += zones[i].Quality; zoneCount++; }
    
                for (int i = 0; i < liquidity.Count; i++)
                    if (liquidity[i].Swept) swept += liquidity[i].Quality;
    
                double e = eventCount > 0 ? (double)eventScore / eventCount : 0;
                double z = zoneCount > 0 ? (double)zoneScore / zoneCount : 0;
    
                return Clamp((int)Math.Round(
                    e * 0.55 + Math.Min(100, z) * 0.30 + Math.Min(100, swept) * 0.15));
            }
    
            private CFIPClean89PremiumDiscountState BuildPremiumDiscount(
                Bars bars, int index, CFIPClean89ConfigSnapshot cfg)
            {
                if (!cfg.Get("UsePremiumDiscount", true) || bars == null || index <= 5)
                    return new CFIPClean89PremiumDiscountState(false, 0, 0, 0);
    
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                double high = Highest(bars, Math.Max(0, index - lookback), index);
                double low = Lowest(bars, Math.Max(0, index - lookback), index);
                double range = high - low;
                if (range <= 0) return new CFIPClean89PremiumDiscountState(false, 0, 0, 0);
    
                return new CFIPClean89PremiumDiscountState(
                    true, low, high, (bars.ClosePrices[index] - low) / range);
            }
    
            private bool IsBearishPreBreak(Bars bars, int index, int referenceHigh)
            {
                if (index < 3) return false;
                double level = referenceHigh >= 0 ? bars.HighPrices[referenceHigh] :
                               Highest(bars, Math.Max(0, index - 8), index - 1);
                return bars.ClosePrices[index - 1] > level;
            }
    
            private bool IsBullishPreBreak(Bars bars, int index, int referenceLow)
            {
                if (index < 3) return false;
                double level = referenceLow >= 0 ? bars.LowPrices[referenceLow] :
                               Lowest(bars, Math.Max(0, index - 8), index - 1);
                return bars.ClosePrices[index - 1] < level;
            }
    
            private bool IsCharacterBreakBull(
                Bars bars, int index, int strength, int lookback, double atr, double breakAtr)
            {
                int h = FindLatestSwingHigh(bars, index - 1, strength, lookback);
                return h >= 0 &&
                       bars.ClosePrices[index] > bars.HighPrices[h] + atr * breakAtr &&
                       bars.ClosePrices[index - 1] <= bars.HighPrices[h];
            }
    
            private bool IsCharacterBreakBear(
                Bars bars, int index, int strength, int lookback, double atr, double breakAtr)
            {
                int l = FindLatestSwingLow(bars, index - 1, strength, lookback);
                return l >= 0 &&
                       bars.ClosePrices[index] < bars.LowPrices[l] - atr * breakAtr &&
                       bars.ClosePrices[index - 1] >= bars.LowPrices[l];
            }
    
            private int FindLatestSwingHigh(Bars bars, int index, int strength, int lookback)
            {
                int start = Math.Max(strength, index - Math.Max(strength * 2, lookback));
                int end = Math.Min(bars.Count - strength - 1, index - strength);
                for (int i = end; i >= start; i--)
                    if (IsSwingHigh(bars, i, strength)) return i;
                return -1;
            }
    
            private int FindLatestSwingLow(Bars bars, int index, int strength, int lookback)
            {
                int start = Math.Max(strength, index - Math.Max(strength * 2, lookback));
                int end = Math.Min(bars.Count - strength - 1, index - strength);
                for (int i = end; i >= start; i--)
                    if (IsSwingLow(bars, i, strength)) return i;
                return -1;
            }
    
            private bool IsSwingHigh(Bars bars, int index, int strength)
            {
                if (bars == null || index - strength < 0 || index + strength >= bars.Count) return false;
                double level = bars.HighPrices[index];
                for (int i = 1; i <= strength; i++)
                    if (bars.HighPrices[index - i] > level ||
                        bars.HighPrices[index + i] > level) return false;
                return true;
            }
    
            private bool IsSwingLow(Bars bars, int index, int strength)
            {
                if (bars == null || index - strength < 0 || index + strength >= bars.Count) return false;
                double level = bars.LowPrices[index];
                for (int i = 1; i <= strength; i++)
                    if (bars.LowPrices[index - i] < level ||
                        bars.LowPrices[index + i] < level) return false;
                return true;
            }
    
            private double Highest(Bars bars, int start, int end)
            {
                if (bars == null || bars.Count == 0 || start > end) return 0;
                double value = double.MinValue;
                for (int i = Math.Max(0, start); i <= Math.Min(bars.Count - 1, end); i++)
                    value = Math.Max(value, bars.HighPrices[i]);
                return value == double.MinValue ? 0 : value;
            }
    
            private double Lowest(Bars bars, int start, int end)
            {
                if (bars == null || bars.Count == 0 || start > end) return 0;
                double value = double.MaxValue;
                for (int i = Math.Max(0, start); i <= Math.Min(bars.Count - 1, end); i++)
                    value = Math.Min(value, bars.LowPrices[i]);
                return value == double.MaxValue ? 0 : value;
            }
    
            private void AddEvent(
                IList<CFIPClean89StructureEventRecord> events,
                CFIPClean89StructureEventKind kind, CFIPClean89Direction direction,
                string timeframe, Bars bars, int index, double price,
                double strengthAtr, int quality, string detail)
            {
                events.Add(new CFIPClean89StructureEventRecord(
                    "CFIP89|" + timeframe + "|" + kind + "|" + index,
                    kind, direction, timeframe, index, bars.OpenTimes[index], price,
                    strengthAtr, quality,
                    CFIPClean89Provenance.Direct("STRUCTURE", detail)));
            }
    
            private void AddLiquidity(
                IList<CFIPClean89LiquidityRecord> list,
                CFIPClean89LiquidityKind kind, CFIPClean89LiquiditySide side,
                CFIPClean89Direction sweepDirection, string timeframe, Bars bars,
                int index, double price, double tolerance, int quality,
                bool swept, string detail)
            {
                int safe = Math.Max(0, Math.Min(index, bars.Count - 1));
                double reference = bars.ClosePrices[safe];
                list.Add(new CFIPClean89LiquidityRecord(
                    "CFIP89|" + timeframe + "|" + kind + "|" + index,
                    kind, side, sweepDirection, timeframe, safe, bars.OpenTimes[safe],
                    price, tolerance, Math.Abs(price - reference),
                    swept ? tolerance : 0, swept, false, quality,
                    CFIPClean89Provenance.Direct(timeframe, detail)));
            }
    
            private int Clamp(int value)
            {
                return Math.Max(0, Math.Min(100, value));
            }
        }
}
