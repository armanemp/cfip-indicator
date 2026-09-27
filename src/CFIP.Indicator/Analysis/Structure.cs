using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace CFIP.Indicator
{
        public enum StructureEventKind
        {
            SwingHigh, SwingLow, BreakOfStructure, MarketStructureShift,
            ChangeOfCharacter, Displacement, LiquiditySweep
        }
    
        public enum ZoneKind { FairValueGap, OrderBlock }
    
        public enum ZoneLifecycle
        {
            Active, Retested, PartiallyMitigated, Consumed, Invalidated, Expired
        }
    
        public enum LiquidityKind
        {
            EqualHigh, EqualLow, SwingHigh, SwingLow,
            PriorDayHigh, PriorDayLow, PriorWeekHigh, PriorWeekLow,
            SessionHigh, SessionLow, DailyPivot, DailyR1, DailyR2, DailyS1, DailyS2,
            Forecast
        }
    
        public enum LiquiditySide { Neutral, Above, Below }
    
        public sealed class StructureEventRecord
        {
            public string Id { get; private set; }
            public StructureEventKind Kind { get; private set; }
            public Direction Direction { get; private set; }
            public string Timeframe { get; private set; }
            public int BarIndex { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public double Price { get; private set; }
            public double StrengthAtr { get; private set; }
            public int Quality { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public StructureEventRecord(
                string id, StructureEventKind kind,
                Direction direction, string timeframe,
                int barIndex, DateTime timeUtc, double price,
                double strengthAtr, int quality,
                Provenance provenance)
            {
                Id = id ?? string.Empty;
                Kind = kind; Direction = direction; Timeframe = timeframe ?? string.Empty;
                BarIndex = barIndex; TimeUtc = timeUtc; Price = price;
                StrengthAtr = Math.Max(0, strengthAtr);
                Quality = Math.Max(0, Math.Min(100, quality));
                Provenance = provenance ?? Provenance.Direct("STRUCTURE", kind.ToString());
            }
        }
    
        public sealed class ZoneRecord
        {
            public string Id { get; private set; }
            public ZoneKind Kind { get; private set; }
            public Direction Direction { get; private set; }
            public string Timeframe { get; private set; }
            public int CreatedIndex { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public double OriginalLower { get; private set; }
            public double OriginalUpper { get; private set; }
            public double CurrentLower { get; private set; }
            public double CurrentUpper { get; private set; }
            public int AgeBars { get; private set; }
            public bool Retested { get; private set; }
            public bool Mitigated { get; private set; }
            public bool Invalidated { get; private set; }
            public bool Consumed { get; private set; }
            public bool ExecutionEligible { get; private set; }
            public int Quality { get; private set; }
            public double DisplacementAtr { get; private set; }
            public bool LiquidityConfluence { get; private set; }
            public bool FvgConfluence { get; private set; }
            public ZoneLifecycle Lifecycle { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public ZoneRecord(
                string id, ZoneKind kind,
                Direction direction, string timeframe,
                int createdIndex, DateTime createdUtc,
                double originalLower, double originalUpper,
                double currentLower, double currentUpper,
                int ageBars, bool retested, bool mitigated,
                bool invalidated, bool consumed, bool executionEligible,
                int quality, double displacementAtr,
                bool liquidityConfluence, bool fvgConfluence,
                ZoneLifecycle lifecycle,
                Provenance provenance)
            {
                Id = id ?? string.Empty; Kind = kind; Direction = direction;
                Timeframe = timeframe ?? string.Empty; CreatedIndex = createdIndex;
                CreatedUtc = createdUtc; OriginalLower = originalLower; OriginalUpper = originalUpper;
                CurrentLower = currentLower; CurrentUpper = currentUpper;
                AgeBars = Math.Max(0, ageBars); Retested = retested; Mitigated = mitigated;
                Invalidated = invalidated; Consumed = consumed; ExecutionEligible = executionEligible;
                Quality = Math.Max(0, Math.Min(100, quality));
                DisplacementAtr = Math.Max(0, displacementAtr);
                LiquidityConfluence = liquidityConfluence; FvgConfluence = fvgConfluence;
                Lifecycle = lifecycle;
                Provenance = provenance ?? Provenance.Direct("ZONE", kind.ToString());
            }
    
            public bool Contains(double price)
            {
                return price >= CurrentLower && price <= CurrentUpper;
            }
        }
    
        public sealed class LiquidityRecord
        {
            public string Id { get; private set; }
            public LiquidityKind Kind { get; private set; }
            public LiquiditySide Side { get; private set; }
            public Direction SweepDirection { get; private set; }
            public string Timeframe { get; private set; }
            public int BarIndex { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public double Price { get; private set; }
            public double Tolerance { get; private set; }
            public double Distance { get; private set; }
            public double Penetration { get; private set; }
            public bool Swept { get; private set; }
            public bool ForecastCandidate { get; private set; }
            public int Quality { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public LiquidityRecord(
                string id, LiquidityKind kind,
                LiquiditySide side, Direction sweepDirection,
                string timeframe, int barIndex, DateTime timeUtc,
                double price, double tolerance, double distance, double penetration,
                bool swept, bool forecastCandidate, int quality,
                Provenance provenance)
            {
                Id = id ?? string.Empty; Kind = kind; Side = side; SweepDirection = sweepDirection;
                Timeframe = timeframe ?? string.Empty; BarIndex = barIndex; TimeUtc = timeUtc;
                Price = price; Tolerance = Math.Max(0, tolerance); Distance = Math.Max(0, distance);
                Penetration = Math.Max(0, penetration); Swept = swept;
                ForecastCandidate = forecastCandidate;
                Quality = Math.Max(0, Math.Min(100, quality));
                Provenance = provenance ?? Provenance.Direct("LIQUIDITY", kind.ToString());
            }
        }
    
        public sealed class PremiumDiscountState
        {
            public bool Available { get; private set; }
            public double RangeLow { get; private set; }
            public double RangeHigh { get; private set; }
            public double Midpoint { get { return Available ? (RangeLow + RangeHigh) / 2.0 : 0; } }
            public double ValueRatio { get; private set; }
            public bool IsDiscount { get { return Available && ValueRatio < 0.5; } }
            public bool IsPremium { get { return Available && ValueRatio > 0.5; } }
    
            public PremiumDiscountState(
                bool available, double rangeLow, double rangeHigh, double valueRatio)
            {
                Available = available; RangeLow = rangeLow; RangeHigh = rangeHigh;
                ValueRatio = Math.Max(0, Math.Min(1, valueRatio));
            }
        }
    
        public sealed class StructureSnapshot
        {
            private readonly ReadOnlyCollection<StructureEventRecord> _events;
            private readonly ReadOnlyCollection<ZoneRecord> _zones;
            private readonly ReadOnlyCollection<LiquidityRecord> _liquidity;
    
            public DateTime ReferenceUtc { get; private set; }
            public bool IsCoherent { get; private set; }
            public bool IsPrimaryReady { get; private set; }
            public Direction CurrentStructureDirection { get; private set; }
            public int StructureQuality { get; private set; }
            public PremiumDiscountState PremiumDiscount { get; private set; }
            public IReadOnlyList<StructureEventRecord> Events { get { return _events; } }
            public IReadOnlyList<ZoneRecord> Zones { get { return _zones; } }
            public IReadOnlyList<LiquidityRecord> Liquidity { get { return _liquidity; } }
    
            public StructureSnapshot(
                DateTime referenceUtc, bool isCoherent, bool isPrimaryReady,
                Direction direction, int quality,
                IList<StructureEventRecord> events,
                IList<ZoneRecord> zones,
                IList<LiquidityRecord> liquidity,
                PremiumDiscountState premiumDiscount)
            {
                ReferenceUtc = referenceUtc; IsCoherent = isCoherent; IsPrimaryReady = isPrimaryReady;
                CurrentStructureDirection = direction;
                StructureQuality = Math.Max(0, Math.Min(100, quality));
                _events = new ReadOnlyCollection<StructureEventRecord>(
                    new List<StructureEventRecord>(events ?? new List<StructureEventRecord>()));
                _zones = new ReadOnlyCollection<ZoneRecord>(
                    new List<ZoneRecord>(zones ?? new List<ZoneRecord>()));
                _liquidity = new ReadOnlyCollection<LiquidityRecord>(
                    new List<LiquidityRecord>(liquidity ?? new List<LiquidityRecord>()));
                PremiumDiscount = premiumDiscount ?? new PremiumDiscountState(false, 0, 0, 0);
            }
    
            public bool HasEvent(StructureEventKind kind, Direction direction)
            {
                for (int i = 0; i < _events.Count; i++)
                    if (_events[i].Kind == kind && _events[i].Direction == direction) return true;
                return false;
            }
    
            public ZoneRecord FindNearestZone(
                Direction direction, ZoneKind kind,
                double price, bool executionEligibleOnly)
            {
                ZoneRecord best = null; double bestDistance = double.MaxValue;
                for (int i = 0; i < _zones.Count; i++)
                {
                    var z = _zones[i];
                    if (z.Direction != direction || z.Kind != kind ||
                        z.Consumed || z.Invalidated || z.Lifecycle == ZoneLifecycle.Expired)
                        continue;
                    if (executionEligibleOnly && !z.ExecutionEligible) continue;
    
                    double distance =
                        price <= z.CurrentLower ? z.CurrentLower - price :
                        price >= z.CurrentUpper ? price - z.CurrentUpper : 0;
    
                    if (distance < bestDistance) { bestDistance = distance; best = z; }
                }
                return best;
            }
    
            public LiquidityRecord FindNearestLiquidity(
                LiquiditySide side, double price, bool unsweptOnly)
            {
                LiquidityRecord best = null; double bestDistance = double.MaxValue;
                for (int i = 0; i < _liquidity.Count; i++)
                {
                    var x = _liquidity[i];
                    if (x.Side != side || (unsweptOnly && x.Swept)) continue;
                    double distance = Math.Abs(x.Price - price);
                    if (distance < bestDistance) { bestDistance = distance; best = x; }
                }
                return best;
            }
        }
    
        public sealed class StructureLedgerBuilder
        {
            private const int MinimumIndex = 35;
    
            public StructureSnapshot Build(
                MtfSnapshot mtf,
                MarketModel market,
                ConfigSnapshot configuration,
                Bars m5Bars, Bars m15Bars, Bars m30Bars, Bars h1Bars, Bars h4Bars,
                Bars d1Bars, Bars w1Bars)
            {
                var events = new List<StructureEventRecord>();
                var zones = new List<ZoneRecord>();
                var liquidity = new List<LiquidityRecord>();
    
                if (mtf == null || market == null ||
                    !mtf.IsPrimaryDecisionReady)
                {
                    return new StructureSnapshot(
                        mtf == null ? DateTime.MinValue : mtf.ReferenceUtc,
                        false, false, Direction.Wait, 0,
                        events, zones, liquidity,
                        new PremiumDiscountState(false, 0, 0, 0));
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
    
                return new StructureSnapshot(
                    mtf.ReferenceUtc,
                    MtfSnapshotBuilder.IsCoherent(mtf),
                    mtf.IsPrimaryDecisionReady,
                    direction,
                    quality,
                    events, zones, liquidity, pd);
            }
    
            private void BuildTimeframe(
                IList<StructureEventRecord> events,
                IList<ZoneRecord> zones,
                IList<LiquidityRecord> liquidity,
                string timeframe, Bars bars, MtfBarSnapshot snapshot,
                MarketFrame frame, ConfigSnapshot cfg, bool primary)
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
                        AddEvent(events, StructureEventKind.SwingHigh,
                            Direction.Sell, timeframe, bars, swingHigh,
                            bars.HighPrices[swingHigh], 0, 68, "confirmed swing high");
    
                    if (swingLow >= 0)
                        AddEvent(events, StructureEventKind.SwingLow,
                            Direction.Buy, timeframe, bars, swingLow,
                            bars.LowPrices[swingLow], 0, 68, "confirmed swing low");
    
                    if (swingHigh >= 0 &&
                        bars.ClosePrices[index] > bars.HighPrices[swingHigh] + atr * breakAtr)
                    {
                        double strengthAtr =
                            (bars.ClosePrices[index] - bars.HighPrices[swingHigh]) / atr;
                        AddEvent(events, StructureEventKind.BreakOfStructure,
                            Direction.Buy, timeframe, bars, index,
                            bars.ClosePrices[index], strengthAtr,
                            Clamp(70 + (int)Math.Round(Math.Min(20, strengthAtr * 8))),
                            "close beyond latest swing high");
    
                        if (IsBearishPreBreak(bars, index, swingHigh))
                            AddEvent(events, StructureEventKind.MarketStructureShift,
                                Direction.Buy, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 82,
                                "bullish break after bearish sequence");
    
                        if (IsCharacterBreakBull(bars, index, strength, lookback, atr, breakAtr))
                            AddEvent(events, StructureEventKind.ChangeOfCharacter,
                                Direction.Buy, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 78,
                                "bullish change of character");
                    }
    
                    if (swingLow >= 0 &&
                        bars.ClosePrices[index] < bars.LowPrices[swingLow] - atr * breakAtr)
                    {
                        double strengthAtr =
                            (bars.LowPrices[swingLow] - bars.ClosePrices[index]) / atr;
                        AddEvent(events, StructureEventKind.BreakOfStructure,
                            Direction.Sell, timeframe, bars, index,
                            bars.ClosePrices[index], strengthAtr,
                            Clamp(70 + (int)Math.Round(Math.Min(20, strengthAtr * 8))),
                            "close beyond latest swing low");
    
                        if (IsBullishPreBreak(bars, index, swingLow))
                            AddEvent(events, StructureEventKind.MarketStructureShift,
                                Direction.Sell, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 82,
                                "bearish break after bullish sequence");
    
                        if (IsCharacterBreakBear(bars, index, strength, lookback, atr, breakAtr))
                            AddEvent(events, StructureEventKind.ChangeOfCharacter,
                                Direction.Sell, timeframe, bars, index,
                                bars.ClosePrices[index], strengthAtr, 78,
                                "bearish change of character");
                    }
                }
    
                if (cfg.Get("UseDisplacement", true))
                {
                    double bodyAtr =
                        Math.Abs(bars.ClosePrices[index] - bars.OpenPrices[index]) / atr;
                    double threshold = Math.Max(0, cfg.Get("DisplacementAtr", 0.80));
                    Direction d =
                        bars.ClosePrices[index] > bars.OpenPrices[index]
                            ? Direction.Buy
                            : bars.ClosePrices[index] < bars.OpenPrices[index]
                                ? Direction.Sell
                                : Direction.Wait;
    
                    if (d != Direction.Wait && bodyAtr >= threshold)
                        AddEvent(events, StructureEventKind.Displacement,
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
                IList<ZoneRecord> zones, Bars bars, int index,
                string timeframe, double atr, ConfigSnapshot cfg)
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
                            bars, i, index, timeframe, Direction.Buy,
                            bars.HighPrices[i - 2], bars.LowPrices[i],
                            bullGap, atr, maxAge, cfg, "three-candle imbalance FVG"));
                        count++;
                    }
    
                    double bearGap = bars.LowPrices[i - 2] - bars.HighPrices[i];
                    if (bearGap >= atr * minGap && count < 24)
                    {
                        zones.Add(BuildFvg(
                            bars, i, index, timeframe, Direction.Sell,
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
                                bars, i, index, timeframe, Direction.Buy,
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
                                bars, i, index, timeframe, Direction.Sell,
                                bars.HighPrices[i], bars.LowPrices[i - 1],
                                twoBarBear, atr, maxAge, cfg,
                                "two-bar imbalance FVG"));
                            count++;
                        }
                    }
                }
            }
    
            private ZoneRecord BuildFvg(
                Bars bars, int created, int current, string timeframe,
                Direction direction, double lower, double upper,
                double gap, double atr, int maxAge,
                ConfigSnapshot cfg,
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
    
                    if (direction == Direction.Buy)
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
                    consumed ? ZoneLifecycle.Consumed :
                    expired ? ZoneLifecycle.Expired :
                    partial ? ZoneLifecycle.PartiallyMitigated :
                    retested ? ZoneLifecycle.Retested :
                    ZoneLifecycle.Active;
    
                int quality = Clamp(
                    55 +
                    (int)Math.Round(Math.Min(20, gap / Math.Max(0.0000001, atr) * 12)) +
                    (retested ? 8 : 0) -
                    (partial ? 6 : 0) -
                    (expired ? 25 : 0));
    
                return new ZoneRecord(
                    "CFIP|" + timeframe + "|FVG|" + direction + "|" + created,
                    ZoneKind.FairValueGap, direction, timeframe, created,
                    bars.OpenTimes[created], lower, upper,
                    eligible ? currentLower : 0,
                    eligible ? currentUpper : 0,
                    age, retested, partial, consumed, consumed, eligible,
                    quality, gap / Math.Max(0.0000001, atr),
                    false, false, lifecycle,
                    Provenance.Direct(timeframe, rule ?? "FVG"));
            }
    
            private void BuildObZones(
                IList<ZoneRecord> zones, Bars bars, int index,
                string timeframe, double atr, ConfigSnapshot cfg)
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
    
                    Direction direction =
                        bars.ClosePrices[impulse] > bars.OpenPrices[impulse]
                            ? Direction.Buy
                            : bars.ClosePrices[impulse] < bars.OpenPrices[impulse]
                                ? Direction.Sell
                                : Direction.Wait;
    
                    if (direction == Direction.Wait) continue;
    
                    int source = -1;
                    for (int j = impulse - 1; j >= start; j--)
                    {
                        bool opposite =
                            direction == Direction.Buy
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
                            direction == Direction.Buy
                                ? bars.LowPrices[k] <= low
                                : bars.HighPrices[k] >= high;
    
                        if (breach) { consumed = true; break; }
                    }
    
                    int age = Math.Max(0, index - source);
                    bool expired = age > maxAge;
                    bool eligible = !consumed && !expired && high > low;
                    var lifecycle =
                        consumed ? ZoneLifecycle.Consumed :
                        expired ? ZoneLifecycle.Expired :
                        retested ? ZoneLifecycle.Retested :
                        ZoneLifecycle.Active;
    
                    int quality = Clamp(
                        60 + (int)Math.Round(Math.Min(25, bodyAtr * 12)) +
                        (retested ? 5 : 0) - (expired ? 20 : 0));
    
                    zones.Add(new ZoneRecord(
                        "CFIP|" + timeframe + "|OB|" + direction + "|" + source,
                        ZoneKind.OrderBlock, direction, timeframe, source,
                        bars.OpenTimes[source], low, high,
                        eligible ? low : 0, eligible ? high : 0,
                        age, retested, retested && !consumed,
                        consumed || expired, consumed, eligible,
                        quality, bodyAtr, false, false, lifecycle,
                        Provenance.Direct(timeframe, "opposite candle before displacement")));
                    count++;
                }
            }
    
            private void BuildEqualLiquidity(
                IList<LiquidityRecord> liquidity,
                Bars bars, int index, string timeframe,
                double atr, ConfigSnapshot cfg)
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
                                liquidity, LiquidityKind.EqualHigh,
                                LiquiditySide.Above,
                                Direction.Wait, timeframe, bars, i,
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
                                liquidity, LiquidityKind.EqualLow,
                                LiquiditySide.Below,
                                Direction.Wait, timeframe, bars, i,
                                (bars.LowPrices[low] + bars.LowPrices[i]) / 2.0,
                                tolerance, 78, false, "equal lows pool");
                            break;
                        }
                }
            }
    
            private void BuildSweepLiquidity(
                IList<LiquidityRecord> liquidity,
                IList<StructureEventRecord> events,
                Bars bars, int index, string timeframe,
                double atr, ConfigSnapshot cfg)
            {
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                double minDepth = Math.Max(0, atr * cfg.Get("LiquiditySweepMinimumDepthAtr", 0.05));
                double priorLow = Lowest(bars, Math.Max(0, index - lookback), index - 1);
                double priorHigh = Highest(bars, Math.Max(0, index - lookback), index - 1);
    
                double lowPen = Math.Max(0, priorLow - bars.LowPrices[index]);
                if (lowPen >= minDepth && bars.ClosePrices[index] > priorLow)
                {
                    AddLiquidity(liquidity, LiquidityKind.SwingLow,
                        LiquiditySide.Below, Direction.Buy,
                        timeframe, bars, index, priorLow, minDepth, 90, true,
                        "bullish liquidity sweep");
    
                    AddEvent(events, StructureEventKind.LiquiditySweep,
                        Direction.Buy, timeframe, bars, index, priorLow,
                        lowPen / Math.Max(0.0000001, atr), 90,
                        "low penetration and recovery");
                }
    
                double highPen = Math.Max(0, bars.HighPrices[index] - priorHigh);
                if (highPen >= minDepth && bars.ClosePrices[index] < priorHigh)
                {
                    AddLiquidity(liquidity, LiquidityKind.SwingHigh,
                        LiquiditySide.Above, Direction.Sell,
                        timeframe, bars, index, priorHigh, minDepth, 90, true,
                        "bearish liquidity sweep");
    
                    AddEvent(events, StructureEventKind.LiquiditySweep,
                        Direction.Sell, timeframe, bars, index, priorHigh,
                        highPen / Math.Max(0.0000001, atr), 90,
                        "high penetration and recovery");
                }
            }
    
            private void AddSwingLiquidity(
                IList<LiquidityRecord> liquidity,
                Bars bars, int index, string timeframe, double atr,
                ConfigSnapshot cfg)
            {
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                int strength = Math.Max(1, cfg.Get("SwingStrength", 3));
                int high = FindLatestSwingHigh(bars, index, strength, lookback);
                int low = FindLatestSwingLow(bars, index, strength, lookback);
    
                if (high >= 0)
                    AddLiquidity(liquidity, LiquidityKind.SwingHigh,
                        LiquiditySide.Above, Direction.Wait,
                        timeframe, bars, high, bars.HighPrices[high], atr * 0.02, 72,
                        false, "latest confirmed swing high");
    
                if (low >= 0)
                    AddLiquidity(liquidity, LiquidityKind.SwingLow,
                        LiquiditySide.Below, Direction.Wait,
                        timeframe, bars, low, bars.LowPrices[low], atr * 0.02, 72,
                        false, "latest confirmed swing low");
            }
    
            private void AddPriorDayWeekLiquidity(
                IList<LiquidityRecord> liquidity,
                MtfSnapshot mtf, Bars d1Bars, Bars w1Bars,
                ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseDailyWeeklyLiquidity", true)) return;
    
                if (d1Bars != null && mtf.D1.IsAvailable && mtf.D1.ClosedIndex > 0)
                {
                    int p = mtf.D1.ClosedIndex - 1;
                    AddLiquidity(liquidity, LiquidityKind.PriorDayHigh,
                        LiquiditySide.Above, Direction.Wait,
                        "D1", d1Bars, p, d1Bars.HighPrices[p], 0, 82, false, "prior day high");
                    AddLiquidity(liquidity, LiquidityKind.PriorDayLow,
                        LiquiditySide.Below, Direction.Wait,
                        "D1", d1Bars, p, d1Bars.LowPrices[p], 0, 82, false, "prior day low");
                }
    
                if (w1Bars != null && mtf.W1.IsAvailable && mtf.W1.ClosedIndex > 0)
                {
                    int p = mtf.W1.ClosedIndex - 1;
                    AddLiquidity(liquidity, LiquidityKind.PriorWeekHigh,
                        LiquiditySide.Above, Direction.Wait,
                        "W1", w1Bars, p, w1Bars.HighPrices[p], 0, 86, false, "prior week high");
                    AddLiquidity(liquidity, LiquidityKind.PriorWeekLow,
                        LiquiditySide.Below, Direction.Wait,
                        "W1", w1Bars, p, w1Bars.LowPrices[p], 0, 86, false, "prior week low");
                }
            }
    
            private void AddSessionLiquidity(
                IList<LiquidityRecord> liquidity,
                MtfSnapshot mtf, Bars bars,
                ConfigSnapshot cfg)
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
                    AddLiquidity(liquidity, LiquidityKind.SessionHigh,
                        LiquiditySide.Above, Direction.Wait,
                        "M5", bars, highIndex, high, 0, 74, false, "configured session high");
    
                if (lowIndex >= 0)
                    AddLiquidity(liquidity, LiquidityKind.SessionLow,
                        LiquiditySide.Below, Direction.Wait,
                        "M5", bars, lowIndex, low, 0, 74, false, "configured session low");
            }
    
            private void AddDailyPivots(
                IList<LiquidityRecord> liquidity,
                MtfSnapshot mtf, Bars d1Bars,
                ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseDailyPivots", true) ||
                    d1Bars == null || !mtf.D1.IsAvailable ||
                    mtf.D1.ClosedIndex <= 0) return;
    
                int p = mtf.D1.ClosedIndex - 1;
                double h = d1Bars.HighPrices[p], l = d1Bars.LowPrices[p], c = d1Bars.ClosePrices[p];
                double pivot = (h + l + c) / 3.0, range = h - l;
    
                AddLiquidity(liquidity, LiquidityKind.DailyPivot,
                    LiquiditySide.Neutral, Direction.Wait,
                    "D1", d1Bars, p, pivot, 0, 62, false, "daily pivot");
    
                AddLiquidity(liquidity, LiquidityKind.DailyR1,
                    LiquiditySide.Above, Direction.Wait,
                    "D1", d1Bars, p, 2 * pivot - l, 0, 64, false, "daily R1");
    
                AddLiquidity(liquidity, LiquidityKind.DailyR2,
                    LiquiditySide.Above, Direction.Wait,
                    "D1", d1Bars, p, pivot + range, 0, 66, false, "daily R2");
    
                AddLiquidity(liquidity, LiquidityKind.DailyS1,
                    LiquiditySide.Below, Direction.Wait,
                    "D1", d1Bars, p, 2 * pivot - h, 0, 64, false, "daily S1");
    
                AddLiquidity(liquidity, LiquidityKind.DailyS2,
                    LiquiditySide.Below, Direction.Wait,
                    "D1", d1Bars, p, pivot - range, 0, 66, false, "daily S2");
            }
    
            private void MarkForecastLiquidity(
                IList<LiquidityRecord> liquidity,
                MarketFrame m5,
                ConfigSnapshot cfg)
            {
                if (!cfg.Get("UseLiquidityForecast", true) ||
                    m5 == null ||
                    !m5.DataValid)
                    return;
    
                double price = m5.Close;
    
                for (int i = 0; i < liquidity.Count; i++)
                {
                    LiquidityRecord item = liquidity[i];
    
                    bool ahead =
                        (item.Side == LiquiditySide.Above &&
                         item.Price > price) ||
                        (item.Side == LiquiditySide.Below &&
                         item.Price < price);
    
                    if (!ahead ||
                        item.Swept ||
                        item.ForecastCandidate)
                        continue;
    
                    liquidity[i] =
                        new LiquidityRecord(
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
                IList<ZoneRecord> zones,
                IList<LiquidityRecord> liquidity,
                ConfigSnapshot cfg)
            {
                bool enabled = cfg.Get("UseZoneConfluence", true);
    
                for (int i = 0; i < zones.Count; i++)
                {
                    var z = zones[i];
                    bool fvg = z.Kind == ZoneKind.OrderBlock &&
                               HasOverlap(zones, z, ZoneKind.FairValueGap);
                    bool liq = enabled && HasNearbyLiquidity(liquidity, z);
    
                    // Keep base zone quality independent. Confluence is carried
                    // explicitly by typed flags and consumed once by Decision quality.
                    int q = Clamp(z.Quality);
    
                    zones[i] = new ZoneRecord(
                        z.Id, z.Kind, z.Direction, z.Timeframe, z.CreatedIndex, z.CreatedUtc,
                        z.OriginalLower, z.OriginalUpper, z.CurrentLower, z.CurrentUpper,
                        z.AgeBars, z.Retested, z.Mitigated, z.Invalidated, z.Consumed, z.ExecutionEligible,
                        q, z.DisplacementAtr, liq, fvg, z.Lifecycle, z.Provenance);
                }
            }
    
            private bool HasOverlap(
                IList<ZoneRecord> zones, ZoneRecord source,
                ZoneKind kind)
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
                IList<LiquidityRecord> liquidity,
                ZoneRecord zone)
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
    
            private Direction ResolveDirection(
                IList<StructureEventRecord> events)
            {
                StructureEventRecord bull = null, bear = null;
                for (int i = 0; i < events.Count; i++)
                {
                    var e = events[i];
                    if (!IsDirectional(e.Kind)) continue;
    
                    if (e.Direction == Direction.Buy &&
                        (bull == null || e.TimeUtc > bull.TimeUtc)) bull = e;
    
                    if (e.Direction == Direction.Sell &&
                        (bear == null || e.TimeUtc > bear.TimeUtc)) bear = e;
                }
    
                if (bull == null && bear == null) return Direction.Wait;
                if (bear == null) return Direction.Buy;
                if (bull == null) return Direction.Sell;
                return bull.TimeUtc > bear.TimeUtc
                    ? Direction.Buy
                    : Direction.Sell;
            }
    
            private bool IsDirectional(StructureEventKind kind)
            {
                return kind == StructureEventKind.BreakOfStructure ||
                       kind == StructureEventKind.MarketStructureShift ||
                       kind == StructureEventKind.ChangeOfCharacter ||
                       kind == StructureEventKind.Displacement ||
                       kind == StructureEventKind.LiquiditySweep;
            }
    
            private int CalculateQuality(
                IList<StructureEventRecord> events,
                IList<ZoneRecord> zones,
                IList<LiquidityRecord> liquidity,
                Direction direction)
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
    
            private PremiumDiscountState BuildPremiumDiscount(
                Bars bars, int index, ConfigSnapshot cfg)
            {
                if (!cfg.Get("UsePremiumDiscount", true) || bars == null || index <= 5)
                    return new PremiumDiscountState(false, 0, 0, 0);
    
                int lookback = Math.Max(10, cfg.Get("LiquidityLookback", 40));
                double high = Highest(bars, Math.Max(0, index - lookback), index);
                double low = Lowest(bars, Math.Max(0, index - lookback), index);
                double range = high - low;
                if (range <= 0) return new PremiumDiscountState(false, 0, 0, 0);
    
                return new PremiumDiscountState(
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
                IList<StructureEventRecord> events,
                StructureEventKind kind, Direction direction,
                string timeframe, Bars bars, int index, double price,
                double strengthAtr, int quality, string detail)
            {
                events.Add(new StructureEventRecord(
                    "CFIP|" + timeframe + "|" + kind + "|" + index,
                    kind, direction, timeframe, index, bars.OpenTimes[index], price,
                    strengthAtr, quality,
                    Provenance.Direct("STRUCTURE", detail)));
            }
    
            private void AddLiquidity(
                IList<LiquidityRecord> list,
                LiquidityKind kind, LiquiditySide side,
                Direction sweepDirection, string timeframe, Bars bars,
                int index, double price, double tolerance, int quality,
                bool swept, string detail)
            {
                int safe = Math.Max(0, Math.Min(index, bars.Count - 1));
                double reference = bars.ClosePrices[safe];
                list.Add(new LiquidityRecord(
                    "CFIP|" + timeframe + "|" + kind + "|" + index,
                    kind, side, sweepDirection, timeframe, safe, bars.OpenTimes[safe],
                    price, tolerance, Math.Abs(price - reference),
                    swept ? tolerance : 0, swept, false, quality,
                    Provenance.Direct(timeframe, detail)));
            }
    
            private int Clamp(int value)
            {
                return Math.Max(0, Math.Min(100, value));
            }
        }
    
    
}
