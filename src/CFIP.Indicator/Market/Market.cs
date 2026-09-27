using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace CFIP.Indicator
{
        public sealed class RuntimeSnapshot
        {
            public DateTime ServerUtc { get; private set; }
            public string Symbol { get; private set; }
            public double Bid { get; private set; }
            public double Ask { get; private set; }
            public double PipSize { get; private set; }
            public double SpreadPips { get; private set; }
            public bool SymbolTradingEnabled { get; private set; }
            public double Equity { get; private set; }
            public double FreeMargin { get; private set; }
            public double Balance { get; private set; }
            public double Margin { get; private set; }
            public double MarginLevel { get; private set; }
            public double DailyRealizedNetProfit { get; private set; }
            public DateTime TradingDayStartUtc { get; private set; }
            public BrokerConstraints BrokerConstraints { get; private set; }
            public int ManagedPositionCount { get; private set; }
            public int ManagedPendingOrderCount { get; private set; }
    
            public RuntimeSnapshot(
                DateTime serverUtc,
                string symbol,
                double bid,
                double ask,
                double pipSize,
                double spreadPips,
                bool symbolTradingEnabled,
                double equity,
                double freeMargin,
                double balance,
                double margin,
                double marginLevel,
                double dailyRealizedNetProfit,
                DateTime tradingDayStartUtc,
                BrokerConstraints brokerConstraints,
                int managedPositionCount,
                int managedPendingOrderCount)
            {
                if (bid < 0 || ask < 0)
                    throw new ArgumentOutOfRangeException("bid");
    
                ServerUtc = serverUtc;
                Symbol = symbol ?? string.Empty;
                Bid = bid;
                Ask = ask;
                PipSize = Math.Max(0, pipSize);
                SpreadPips = Math.Max(0, spreadPips);
                SymbolTradingEnabled = symbolTradingEnabled;
                Equity = Math.Max(0, equity);
                FreeMargin = Math.Max(0, freeMargin);
                Balance = Math.Max(0, balance);
                Margin = Math.Max(0, margin);
                MarginLevel = Math.Max(0, marginLevel);
                DailyRealizedNetProfit = dailyRealizedNetProfit;
                TradingDayStartUtc = tradingDayStartUtc;
                BrokerConstraints =
                    brokerConstraints ??
                    throw new ArgumentNullException("brokerConstraints");
                ManagedPositionCount =
                    Math.Max(0, managedPositionCount);
                ManagedPendingOrderCount =
                    Math.Max(0, managedPendingOrderCount);
            }
        }
    
        public enum MtfDataStatus
        {
            Ready = 0,
            PrimaryHistoryInsufficient = 1,
            MissingTimeframeData = 2,
            InvalidReference = 3,
            StaleReference = 4
        }
    
        public sealed class MtfBarSnapshot
        {
            public string Timeframe { get; private set; }
            public int ClosedIndex { get; private set; }
            public DateTime BarOpenUtc { get; private set; }
            public DateTime NextBarOpenUtc { get; private set; }
            public bool IsAvailable { get; private set; }
            public bool IsFullyClosedAtReference { get; private set; }
            public bool HasMinimumHistory { get; private set; }
    
            public MtfBarSnapshot(
                string timeframe,
                int closedIndex,
                DateTime barOpenUtc,
                DateTime nextBarOpenUtc,
                bool isAvailable,
                bool isFullyClosedAtReference,
                bool hasMinimumHistory)
            {
                Timeframe = timeframe ?? string.Empty;
                ClosedIndex = closedIndex;
                BarOpenUtc = barOpenUtc;
                NextBarOpenUtc = nextBarOpenUtc;
                IsAvailable = isAvailable;
                IsFullyClosedAtReference = isFullyClosedAtReference;
                HasMinimumHistory = hasMinimumHistory;
            }
    
            public static MtfBarSnapshot Missing(
                string timeframe)
            {
                return new MtfBarSnapshot(
                    timeframe,
                    -1,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    false,
                    false,
                    false);
            }
        }
    
        public sealed class MtfSnapshot
        {
            public DateTime ServerUtc { get; private set; }
            public DateTime ReferenceUtc { get; private set; }
            public DateTime UserLocalTime { get; private set; }
            public TimeSpan ReferenceAge { get; private set; }
            public string ChartTimeframe { get; private set; }
    
            public MtfBarSnapshot Chart { get; private set; }
            public MtfBarSnapshot M1 { get; private set; }
            public MtfBarSnapshot M5 { get; private set; }
            public MtfBarSnapshot M15 { get; private set; }
            public MtfBarSnapshot M30 { get; private set; }
            public MtfBarSnapshot H1 { get; private set; }
            public MtfBarSnapshot H4 { get; private set; }
            public MtfBarSnapshot D1 { get; private set; }
            public MtfBarSnapshot W1 { get; private set; }
    
            public MtfDataStatus DataStatus { get; private set; }
    
            public bool IsReferenceValid
            {
                get
                {
                    return ReferenceUtc != DateTime.MinValue;
                }
            }
    
            public bool IsReferenceFresh
            {
                get
                {
                    return
                        IsReferenceValid &&
                        ReferenceAge >= TimeSpan.Zero &&
                        ReferenceAge <= TimeSpan.FromMinutes(10);
                }
            }
    
            public bool IsPrimaryDecisionReady
            {
                get
                {
                    return
                        IsReferenceValid &&
                        IsReferenceFresh &&
                        M5 != null &&
                        M15 != null &&
                        M30 != null &&
                        H1 != null &&
                        H4 != null &&
                        M5.IsAvailable &&
                        M15.IsAvailable &&
                        M30.IsAvailable &&
                        H1.IsAvailable &&
                        H4.IsAvailable &&
                        M5.IsFullyClosedAtReference &&
                        M15.IsFullyClosedAtReference &&
                        M30.IsFullyClosedAtReference &&
                        H1.IsFullyClosedAtReference &&
                        H4.IsFullyClosedAtReference &&
                        M5.HasMinimumHistory &&
                        M15.HasMinimumHistory &&
                        M30.HasMinimumHistory &&
                        H1.HasMinimumHistory &&
                        H4.HasMinimumHistory;
                }
            }
    
            public bool IsAllAvailableTimeframesClosed
            {
                get
                {
                    return
                        M1 != null &&
                        M5 != null &&
                        M15 != null &&
                        M30 != null &&
                        H1 != null &&
                        H4 != null &&
                        D1 != null &&
                        W1 != null &&
                        M1.IsAvailable &&
                        M5.IsAvailable &&
                        M15.IsAvailable &&
                        M30.IsAvailable &&
                        H1.IsAvailable &&
                        H4.IsAvailable &&
                        D1.IsAvailable &&
                        W1.IsAvailable &&
                        M1.IsFullyClosedAtReference &&
                        M5.IsFullyClosedAtReference &&
                        M15.IsFullyClosedAtReference &&
                        M30.IsFullyClosedAtReference &&
                        H1.IsFullyClosedAtReference &&
                        H4.IsFullyClosedAtReference &&
                        D1.IsFullyClosedAtReference &&
                        W1.IsFullyClosedAtReference;
                }
            }
    
            public MtfSnapshot(
                DateTime serverUtc,
                DateTime referenceUtc,
                DateTime userLocalTime,
                TimeSpan referenceAge,
                string chartTimeframe,
                MtfBarSnapshot chart,
                MtfBarSnapshot m1,
                MtfBarSnapshot m5,
                MtfBarSnapshot m15,
                MtfBarSnapshot m30,
                MtfBarSnapshot h1,
                MtfBarSnapshot h4,
                MtfBarSnapshot d1,
                MtfBarSnapshot w1,
                MtfDataStatus dataStatus)
            {
                ServerUtc = serverUtc;
                ReferenceUtc = referenceUtc;
                UserLocalTime = userLocalTime;
                ReferenceAge =
                    referenceAge < TimeSpan.Zero
                        ? TimeSpan.Zero
                        : referenceAge;
                ChartTimeframe = chartTimeframe ?? string.Empty;
                Chart = chart ?? MtfBarSnapshot.Missing("CHART");
                M1 = m1 ?? MtfBarSnapshot.Missing("M1");
                M5 = m5 ?? MtfBarSnapshot.Missing("M5");
                M15 = m15 ?? MtfBarSnapshot.Missing("M15");
                M30 = m30 ?? MtfBarSnapshot.Missing("M30");
                H1 = h1 ?? MtfBarSnapshot.Missing("H1");
                H4 = h4 ?? MtfBarSnapshot.Missing("H4");
                D1 = d1 ?? MtfBarSnapshot.Missing("D1");
                W1 = w1 ?? MtfBarSnapshot.Missing("W1");
                DataStatus = dataStatus;
            }
        }
    
        public static class MtfSnapshotBuilder
        {
            public static DateTime ResolveM5Reference(
                Bars m5Bars,
                DateTime serverUtc)
            {
                if (m5Bars == null ||
                    m5Bars.Count < 2 ||
                    serverUtc == DateTime.MinValue)
                    return DateTime.MinValue;
    
                int last =
                    m5Bars.Count - 1;
    
                DateTime reference =
                    m5Bars.OpenTimes[last];
    
                if (reference > serverUtc)
                    return DateTime.MinValue;
    
                return reference;
            }
    
            public static MtfSnapshot Build(
                DateTime serverUtc,
                DateTime userLocalTime,
                Bars chartBars,
                string chartTimeframe,
                Bars m1Bars,
                Bars m5Bars,
                Bars m15Bars,
                Bars m30Bars,
                Bars h1Bars,
                Bars h4Bars,
                Bars d1Bars,
                Bars w1Bars,
                int minimumHistory)
            {
                DateTime reference =
                    ResolveM5Reference(
                        m5Bars,
                        serverUtc);
    
                if (reference == DateTime.MinValue)
                {
                    return new MtfSnapshot(
                        serverUtc,
                        DateTime.MinValue,
                        userLocalTime,
                        TimeSpan.Zero,
                        chartTimeframe,
                        MtfBarSnapshot.Missing(
                            "CHART"),
                        MtfBarSnapshot.Missing("M1"),
                        MtfBarSnapshot.Missing("M5"),
                        MtfBarSnapshot.Missing("M15"),
                        MtfBarSnapshot.Missing("M30"),
                        MtfBarSnapshot.Missing("H1"),
                        MtfBarSnapshot.Missing("H4"),
                        MtfBarSnapshot.Missing("D1"),
                        MtfBarSnapshot.Missing("W1"),
                        MtfDataStatus.InvalidReference);
                }
    
                TimeSpan age =
                    serverUtc >= reference
                        ? serverUtc - reference
                        : TimeSpan.Zero;
    
                MtfBarSnapshot chart =
                    ResolveClosedBar(
                        chartBars,
                        reference,
                        chartTimeframe,
                        minimumHistory);
    
                MtfBarSnapshot m1 =
                    ResolveClosedBar(
                        m1Bars,
                        reference,
                        "M1",
                        minimumHistory);
    
                MtfBarSnapshot m5 =
                    ResolveClosedBar(
                        m5Bars,
                        reference,
                        "M5",
                        minimumHistory);
    
                MtfBarSnapshot m15 =
                    ResolveClosedBar(
                        m15Bars,
                        reference,
                        "M15",
                        minimumHistory);
    
                MtfBarSnapshot m30 =
                    ResolveClosedBar(
                        m30Bars,
                        reference,
                        "M30",
                        minimumHistory);
    
                MtfBarSnapshot h1 =
                    ResolveClosedBar(
                        h1Bars,
                        reference,
                        "H1",
                        minimumHistory);
    
                MtfBarSnapshot h4 =
                    ResolveClosedBar(
                        h4Bars,
                        reference,
                        "H4",
                        minimumHistory);
    
                MtfBarSnapshot d1 =
                    ResolveClosedBar(
                        d1Bars,
                        reference,
                        "D1",
                        minimumHistory);
    
                MtfBarSnapshot w1 =
                    ResolveClosedBar(
                        w1Bars,
                        reference,
                        "W1",
                        minimumHistory);
    
                bool primaryHistory =
                    HasPrimaryHistory(
                        m5,
                        m15,
                        m30,
                        h1,
                        h4,
                        minimumHistory);
    
                bool allTimeframesAvailable =
                    m1.IsAvailable &&
                    m5.IsAvailable &&
                    m15.IsAvailable &&
                    m30.IsAvailable &&
                    h1.IsAvailable &&
                    h4.IsAvailable &&
                    d1.IsAvailable &&
                    w1.IsAvailable;
    
                MtfDataStatus status;
    
                if (age > TimeSpan.FromMinutes(10))
                    status =
                        MtfDataStatus.StaleReference;
                else if (!primaryHistory)
                    status =
                        MtfDataStatus.PrimaryHistoryInsufficient;
                else if (!m1.IsAvailable ||
                         !m5.IsAvailable ||
                         !m15.IsAvailable ||
                         !m30.IsAvailable ||
                         !h1.IsAvailable ||
                         !h4.IsAvailable)
                    status =
                        MtfDataStatus.MissingTimeframeData;
                else
                    status =
                        MtfDataStatus.Ready;
    
                return new MtfSnapshot(
                    serverUtc,
                    reference,
                    userLocalTime,
                    age,
                    chartTimeframe,
                    chart,
                    m1,
                    m5,
                    m15,
                    m30,
                    h1,
                    h4,
                    d1,
                    w1,
                    status);
            }
    
            public static bool HasPrimaryHistory(
                MtfBarSnapshot m5,
                MtfBarSnapshot m15,
                MtfBarSnapshot m30,
                MtfBarSnapshot h1,
                MtfBarSnapshot h4,
                int minimumHistory)
            {
                int minimum =
                    Math.Max(
                        2,
                        minimumHistory);
    
                return
                    m5 != null &&
                    m15 != null &&
                    m30 != null &&
                    h1 != null &&
                    h4 != null &&
                    m5.ClosedIndex >= minimum &&
                    m15.ClosedIndex >= minimum &&
                    m30.ClosedIndex >= minimum &&
                    h1.ClosedIndex >= minimum &&
                    h4.ClosedIndex >= minimum;
            }
    
            public static MtfBarSnapshot ResolveClosedBar(
                Bars bars,
                DateTime reference,
                string timeframe,
                int minimumHistory)
            {
                if (bars == null ||
                    bars.Count < 2 ||
                    reference == DateTime.MinValue ||
                    reference < bars.OpenTimes[0])
                    return
                        MtfBarSnapshot.Missing(
                            timeframe);
    
                int probe =
                    bars.OpenTimes.GetIndexByTime(
                        reference);
    
                if (probe < 0)
                    probe = bars.Count - 1;
    
                probe =
                    Math.Max(
                        0,
                        Math.Min(
                            probe,
                            bars.Count - 1));
    
                // The final series item is treated as potentially forming.
                // Never return it as a closed analysis bar.
                if (probe == bars.Count - 1)
                    probe--;
    
                for (int i = probe; i >= 0; i--)
                {
                    DateTime open =
                        bars.OpenTimes[i];
    
                    if (open >= reference)
                        continue;
    
                    if (i + 1 >= bars.Count)
                        continue;
    
                    DateTime nextOpen =
                        bars.OpenTimes[i + 1];
    
                    if (nextOpen > reference)
                        continue;
    
                    bool minimum =
                        i >= Math.Max(
                            2,
                            minimumHistory);
    
                    return
                        new MtfBarSnapshot(
                            timeframe,
                            i,
                            open,
                            nextOpen,
                            true,
                            true,
                            minimum);
                }
    
                return
                    MtfBarSnapshot.Missing(
                        timeframe);
            }
    
            public static bool IsCoherent(
                MtfSnapshot snapshot)
            {
                if (snapshot == null ||
                    !snapshot.IsReferenceValid ||
                    !snapshot.IsReferenceFresh)
                    return false;
    
                MtfBarSnapshot[] items =
                {
                    snapshot.M1,
                    snapshot.M5,
                    snapshot.M15,
                    snapshot.M30,
                    snapshot.H1,
                    snapshot.H4,
                    snapshot.D1,
                    snapshot.W1
                };
    
                if (!snapshot.IsPrimaryDecisionReady)
                    return false;
    
                for (int i = 0; i < items.Length; i++)
                {
                    MtfBarSnapshot item =
                        items[i];
    
                    // D1/W1 may legitimately be unavailable when history is
                    // insufficient. Missing optional data is not temporal leakage.
                    if (item == null ||
                        !item.IsAvailable)
                        continue;
    
                    if (!item.IsFullyClosedAtReference ||
                        item.ClosedIndex < 0 ||
                        item.BarOpenUtc >= snapshot.ReferenceUtc ||
                        item.NextBarOpenUtc >
                        snapshot.ReferenceUtc)
                        return false;
                }
    
                return true;
            }
        }
    
        public enum Regime
        {
            Unknown = 0,
            Trend = 1,
            Expansion = 2,
            Compression = 3,
            Range = 4,
            Transition = 5
        }
    
        public enum MarketFeature
        {
            Trend = 0,
            Momentum = 1,
            Rsi = 2,
            Dmi = 3,
            EmaSlope = 4,
            Rejection = 5,
            VolumeExpansion = 6,
            MacdBias = 7,
            VwapBias = 8,
            HealthyVolatility = 9
        }
    
        public sealed class FeatureEvidence
        {
            public MarketFeature Feature { get; private set; }
            public Direction Direction { get; private set; }
            public double Value { get; private set; }
            public int Weight { get; private set; }
            public bool Triggered { get; private set; }
            public bool CountsAsEvidence { get; private set; }
            public bool CountsAsGate { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public FeatureEvidence(
                MarketFeature feature,
                Direction direction,
                double value,
                int weight,
                bool triggered,
                bool countsAsEvidence,
                bool countsAsGate,
                Provenance provenance)
            {
                Feature = feature;
                Direction = direction;
                Value = Clamp01(value);
                Weight = Math.Max(0, weight);
                Triggered = triggered;
                CountsAsEvidence = countsAsEvidence;
                CountsAsGate = countsAsGate;
                Provenance =
                    provenance ??
                    Provenance.Direct(
                        "MARKET_MODEL",
                        feature.ToString());
            }
    
            private static double Clamp01(double value)
            {
                if (double.IsNaN(value) ||
                    double.IsInfinity(value))
                    return 0;
    
                return
                    Math.Max(
                        0,
                        Math.Min(
                            1,
                            value));
            }
        }
    
        public sealed class MarketFrame
        {
            private readonly ReadOnlyCollection<FeatureEvidence> _features;
    
            public string Timeframe { get; private set; }
            public DateTime ClosedBarTimeUtc { get; private set; }
    
            public double Open { get; private set; }
            public double High { get; private set; }
            public double Low { get; private set; }
            public double Close { get; private set; }
    
            // Raw indicator measurements.
            public double Atr { get; private set; }
            public double AtrRatio { get; private set; }
            public double Rsi { get; private set; }
            public double Adx { get; private set; }
            public double DmiBias { get; private set; }
            public double EmaFast { get; private set; }
            public double EmaSlow { get; private set; }
            public double EmaSpreadAtr { get; private set; }
            public double EmaSlopeAtr { get; private set; }
            public double MomentumAtr { get; private set; }
            public double MacdHistogram { get; private set; }
            public double Vwap { get; private set; }
            public double VolumeRatio { get; private set; }
            public double BodyAtr { get; private set; }
            public double RangeAtr { get; private set; }
    
            // Typed interpretation; this is market bias, not final trade direction.
            public Direction BiasDirection { get; private set; }
            public int BiasStrength { get; private set; }
            public int MarketQuality { get; private set; }
            public int BullScore { get; private set; }
            public int BearScore { get; private set; }
            public int BullScoreNormalized { get; private set; }
            public int BearScoreNormalized { get; private set; }
            public int IndependentEvidence { get; private set; }
            public int EnabledEvidenceFeatures { get; private set; }
    
            public Regime Regime { get; private set; }
            public int RegimeQuality { get; private set; }
            public bool Choppy { get; private set; }
            public bool HealthyVolatility { get; private set; }
            public bool DataValid { get; private set; }
    
            public IReadOnlyList<FeatureEvidence> Features
            {
                get { return _features; }
            }
    
            public int DirectionScore
            {
                get
                {
                    return
                        BiasDirection == Direction.Buy
                            ? BiasStrength
                            : BiasDirection == Direction.Sell
                                ? -BiasStrength
                                : 0;
                }
            }
    
            public MarketFrame(
                string timeframe,
                DateTime closedBarTimeUtc,
                double open,
                double high,
                double low,
                double close,
                double atr,
                double atrRatio,
                double rsi,
                double adx,
                double dmiBias,
                double emaFast,
                double emaSlow,
                double emaSpreadAtr,
                double emaSlopeAtr,
                double momentumAtr,
                double macdHistogram,
                double vwap,
                double volumeRatio,
                double bodyAtr,
                double rangeAtr,
                Direction biasDirection,
                int biasStrength,
                int marketQuality,
                int bullScore,
                int bearScore,
                int bullScoreNormalized,
                int bearScoreNormalized,
                int independentEvidence,
                int enabledEvidenceFeatures,
                Regime regime,
                int regimeQuality,
                bool choppy,
                bool healthyVolatility,
                bool dataValid,
                IList<FeatureEvidence> features)
            {
                Timeframe = timeframe ?? string.Empty;
                ClosedBarTimeUtc = closedBarTimeUtc;
                Open = open;
                High = high;
                Low = low;
                Close = close;
                Atr = Math.Max(0, atr);
                AtrRatio = Math.Max(0, atrRatio);
                Rsi = Math.Max(0, Math.Min(100, rsi));
                Adx = Math.Max(0, Math.Min(100, adx));
                DmiBias =
                    Math.Max(
                        -1,
                        Math.Min(
                            1,
                            dmiBias));
                EmaFast = emaFast;
                EmaSlow = emaSlow;
                EmaSpreadAtr = emaSpreadAtr;
                EmaSlopeAtr = emaSlopeAtr;
                MomentumAtr = momentumAtr;
                MacdHistogram = macdHistogram;
                Vwap = Math.Max(0, vwap);
                VolumeRatio = Math.Max(0, volumeRatio);
                BodyAtr = Math.Max(0, bodyAtr);
                RangeAtr = Math.Max(0, rangeAtr);
                BiasDirection = biasDirection;
                BiasStrength = Math.Max(0, Math.Min(100, biasStrength));
                MarketQuality =
                    Math.Max(0,
                        Math.Min(100, marketQuality));
                BullScore = Math.Max(0, bullScore);
                BearScore = Math.Max(0, bearScore);
                BullScoreNormalized =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            bullScoreNormalized));
                BearScoreNormalized =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            bearScoreNormalized));
                IndependentEvidence = Math.Max(0, independentEvidence);
                EnabledEvidenceFeatures =
                    Math.Max(
                        0,
                        enabledEvidenceFeatures);
                Regime = regime;
                RegimeQuality =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            regimeQuality));
                Choppy = choppy;
                HealthyVolatility = healthyVolatility;
                DataValid = dataValid;
                _features =
                    new ReadOnlyCollection<FeatureEvidence>(
                        new List<FeatureEvidence>(
                            features ??
                            new List<FeatureEvidence>()));
            }
        }
    
        public sealed class MarketModel
        {
            private readonly ReadOnlyCollection<MarketFrame> _frames;
    
            public DateTime ReferenceUtc { get; private set; }
            public bool IsCoherent { get; private set; }
            public bool IsPrimaryReady { get; private set; }
            public string DataStatus { get; private set; }
    
            public IReadOnlyList<MarketFrame> Frames
            {
                get { return _frames; }
            }
    
            public MarketFrame M5
            {
                get { return FindFrame("M5"); }
            }
    
            public MarketFrame M15
            {
                get { return FindFrame("M15"); }
            }
    
            public MarketFrame FindFrame(string timeframe)
            {
                string key = timeframe ?? string.Empty;
    
                for (int i = 0; i < _frames.Count; i++)
                {
                    if (string.Equals(
                            _frames[i].Timeframe,
                            key,
                            StringComparison.OrdinalIgnoreCase))
                        return _frames[i];
                }
    
                return null;
            }
    
            public MarketModel(
                DateTime referenceUtc,
                bool isCoherent,
                bool isPrimaryReady,
                string dataStatus,
                IList<MarketFrame> frames)
            {
                ReferenceUtc = referenceUtc;
                IsCoherent = isCoherent;
                IsPrimaryReady = isPrimaryReady;
                DataStatus = dataStatus ?? string.Empty;
                _frames =
                    new ReadOnlyCollection<MarketFrame>(
                        new List<MarketFrame>(
                            frames ??
                            new List<MarketFrame>()));
            }
        }
    
        public sealed class NativeIndicatorSet
        {
            public Bars Bars { get; private set; }
            public ExponentialMovingAverage Fast { get; private set; }
            public ExponentialMovingAverage Slow { get; private set; }
            public AverageTrueRange Atr { get; private set; }
            public RelativeStrengthIndex Rsi { get; private set; }
            public DirectionalMovementSystem Dms { get; private set; }
            public ExponentialMovingAverage MacdFast { get; private set; }
            public ExponentialMovingAverage MacdSlow { get; private set; }
    
            public NativeIndicatorSet(
                Bars bars,
                ExponentialMovingAverage fast,
                ExponentialMovingAverage slow,
                AverageTrueRange atr,
                RelativeStrengthIndex rsi,
                DirectionalMovementSystem dms,
                ExponentialMovingAverage macdFast,
                ExponentialMovingAverage macdSlow)
            {
                Bars = bars;
                Fast = fast;
                Slow = slow;
                Atr = atr;
                Rsi = rsi;
                Dms = dms;
                MacdFast = macdFast;
                MacdSlow = macdSlow;
            }
        }
    
        public sealed class NativeIndicatorCatalog
        {
            private readonly IIndicatorsAccessor _indicators;
            private readonly List<NativeIndicatorSet> _sets =
                new List<NativeIndicatorSet>();
    
            public NativeIndicatorCatalog(
                IIndicatorsAccessor indicators)
            {
                _indicators =
                    indicators ??
                    throw new ArgumentNullException("indicators");
            }
    
            public NativeIndicatorSet GetOrCreate(
                Bars bars,
                ConfigSnapshot configuration)
            {
                if (bars == null)
                    return null;
    
                for (int i = 0; i < _sets.Count; i++)
                {
                    if (ReferenceEquals(_sets[i].Bars, bars))
                        return _sets[i];
                }
    
                int fastPeriod = Math.Max(2, configuration.Get("FastEma", 21));
                int slowPeriod = Math.Max(fastPeriod + 1, configuration.Get("SlowEma", 55));
                int atrPeriod = Math.Max(2, configuration.Get("AtrPeriod", 14));
                int rsiPeriod = Math.Max(2, configuration.Get("RsiPeriod", 14));
                int adxPeriod = Math.Max(2, configuration.Get("AdxPeriod", 14));
                int macdFastPeriod = Math.Max(2, configuration.Get("MacdFastPeriod", 12));
                int macdSlowPeriod = Math.Max(macdFastPeriod + 1, configuration.Get("MacdSlowPeriod", 26));
    
                try
                {
                    var set = new NativeIndicatorSet(
                        bars,
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            fastPeriod),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            slowPeriod),
                        _indicators.AverageTrueRange(
                            bars,
                            atrPeriod,
                            MovingAverageType.WilderSmoothing),
                        _indicators.RelativeStrengthIndex(
                            bars.ClosePrices,
                            rsiPeriod),
                        _indicators.DirectionalMovementSystem(
                            bars,
                            adxPeriod,
                            MovingAverageType.WilderSmoothing),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            macdFastPeriod),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            macdSlowPeriod));
    
                    _sets.Add(set);
                    return set;
                }
                catch
                {
                    return null;
                }
            }
        }
    
        public sealed class MarketModelBuilder
        {
            private const int MinimumClosedIndex = 30;
            private readonly NativeIndicatorCatalog _catalog;
    
            public MarketModelBuilder(IIndicatorsAccessor indicators)
            {
                _catalog = new NativeIndicatorCatalog(indicators);
            }
    
            public MarketModel Build(
                RuntimeSnapshot runtime,
                MtfSnapshot mtf,
                ConfigSnapshot configuration,
                Bars chartBars,
                Bars m1Bars,
                Bars m5Bars,
                Bars m15Bars,
                Bars m30Bars,
                Bars h1Bars,
                Bars h4Bars,
                Bars d1Bars,
                Bars w1Bars)
            {
                if (mtf == null)
                    throw new ArgumentNullException("mtf");
    
                var frames = new List<MarketFrame>();
    
                if (!mtf.IsReferenceValid)
                    return new MarketModel(
                        DateTime.MinValue,
                        false,
                        false,
                        mtf.DataStatus.ToString(),
                        frames);
    
                bool coherent =
                    MtfSnapshotBuilder.IsCoherent(mtf);
    
                AddFrame(frames, "CHART", chartBars, mtf.Chart, configuration);
                AddFrame(frames, "M1", m1Bars, mtf.M1, configuration);
                AddFrame(frames, "M5", m5Bars, mtf.M5, configuration);
                AddFrame(frames, "M15", m15Bars, mtf.M15, configuration);
                AddFrame(frames, "M30", m30Bars, mtf.M30, configuration);
                AddFrame(frames, "H1", h1Bars, mtf.H1, configuration);
                AddFrame(frames, "H4", h4Bars, mtf.H4, configuration);
                AddFrame(frames, "D1", d1Bars, mtf.D1, configuration);
                AddFrame(frames, "W1", w1Bars, mtf.W1, configuration);
    
                return new MarketModel(
                    mtf.ReferenceUtc,
                    coherent,
                    mtf.IsPrimaryDecisionReady,
                    mtf.DataStatus.ToString(),
                    frames);
            }
    
            private void AddFrame(
                IList<MarketFrame> frames,
                string timeframe,
                Bars bars,
                MtfBarSnapshot snapshot,
                ConfigSnapshot configuration)
            {
                if (frames == null ||
                    snapshot == null ||
                    !snapshot.IsAvailable ||
                    !snapshot.IsFullyClosedAtReference ||
                    snapshot.ClosedIndex < MinimumClosedIndex ||
                    bars == null)
                    return;
    
                MarketFrame frame =
                    BuildFrame(
                        timeframe,
                        bars,
                        snapshot,
                        configuration);
    
                if (frame != null)
                    frames.Add(frame);
            }
    
            private MarketFrame BuildFrame(
                string timeframe,
                Bars bars,
                MtfBarSnapshot snapshot,
                ConfigSnapshot configuration)
            {
                int index = snapshot.ClosedIndex;
    
                if (index < MinimumClosedIndex ||
                    index >= bars.Count - 1)
                    return null;
    
                NativeIndicatorSet native =
                    _catalog.GetOrCreate(
                        bars,
                        configuration);
    
                if (native == null)
                    return null;
    
                double atr =
                    Read(
                        native.Atr == null
                            ? null
                            : native.Atr.Result,
                        index);
    
                double previousAtr =
                    Read(
                        native.Atr == null
                            ? null
                            : native.Atr.Result,
                        Math.Max(5, index - 10));
    
                if (atr <= 0 || previousAtr <= 0)
                    return null;
    
                double open = bars.OpenPrices[index];
                double high = bars.HighPrices[index];
                double low = bars.LowPrices[index];
                double close = bars.ClosePrices[index];
    
                double fast =
                    Read(
                        native.Fast == null
                            ? null
                            : native.Fast.Result,
                        index);
    
                double slow =
                    Read(
                        native.Slow == null
                            ? null
                            : native.Slow.Result,
                        index);
    
                double previousFast =
                    Read(
                        native.Fast == null
                            ? null
                            : native.Fast.Result,
                        Math.Max(0, index - 2));
    
                double rsi =
                    Read(
                        native.Rsi == null
                            ? null
                            : native.Rsi.Result,
                        index,
                        50);
    
                double adx =
                    Read(
                        native.Dms == null
                            ? null
                            : native.Dms.ADX,
                        index);
    
                double dmiPlus =
                    Read(
                        native.Dms == null
                            ? null
                            : native.Dms.DIPlus,
                        index);
    
                double dmiMinus =
                    Read(
                        native.Dms == null
                            ? null
                            : native.Dms.DIMinus,
                        index);
    
                double dmiBias = NormalizeDmi(dmiPlus, dmiMinus);
                double emaSpread = fast - slow;
                double emaSpreadAtr = emaSpread / atr;
                double emaSlopeAtr = (fast - previousFast) / atr;
    
                double momentumAtr =
                    (close -
                     bars.ClosePrices[Math.Max(0, index - 2)]) /
                    atr;
    
                double macdHistogram =
                    Read(
                        native.MacdFast == null
                            ? null
                            : native.MacdFast.Result,
                        index) -
                    Read(
                        native.MacdSlow == null
                            ? null
                            : native.MacdSlow.Result,
                        index);
    
                double previousMacd =
                    Read(
                        native.MacdFast == null
                            ? null
                            : native.MacdFast.Result,
                        Math.Max(0, index - 2)) -
                    Read(
                        native.MacdSlow == null
                            ? null
                            : native.MacdSlow.Result,
                        Math.Max(0, index - 2));
    
                double vwap =
                    RollingVwap(
                        bars,
                        index,
                        Math.Max(
                            10,
                            configuration.Get(
                                "VwapLookbackBars",
                                48)));
    
                double averageVolume =
                    AverageTickVolume(
                        bars,
                        index,
                        20);
    
                double volumeRatio =
                    averageVolume > 0
                        ? Math.Max(
                            0,
                            bars.TickVolumes[index]) /
                          averageVolume
                        : 0;
    
                double range =
                    Math.Max(
                        0.0000001,
                        high - low);
    
                double body =
                    Math.Abs(
                        close - open);
    
                double bodyAtr = body / atr;
                double rangeAtr = range / atr;
                double atrRatio = atr / previousAtr;
    
                int adxMinimum =
                    Math.Max(
                        0,
                        configuration.Get(
                            "AdxMinimum",
                            20));
    
                bool useSlope =
                    configuration.Get(
                        "UseEmaSlope",
                        true);
    
                bool trendBull = fast > slow && close > fast;
                bool trendBear = fast < slow && close < fast;
                bool momentumBull = momentumAtr > 0.15;
                bool momentumBear = momentumAtr < -0.15;
                bool rsiBull = rsi > 50;
                bool rsiBear = rsi < 50;
                bool dmiBull = adx >= adxMinimum && dmiBias > 0;
                bool dmiBear = adx >= adxMinimum && dmiBias < 0;
                bool slopeBull = useSlope && emaSlopeAtr > 0;
                bool slopeBear = useSlope && emaSlopeAtr < 0;
    
                bool rejectionBull =
                    IsBullishRejection(open, high, low, close);
                bool rejectionBear =
                    IsBearishRejection(open, high, low, close);
    
                bool volumeEnabled =
                    configuration.Get(
                        "UseVolumeExpansion",
                        false);
    
                double volumeThreshold =
                    Math.Max(
                        1.0,
                        configuration.Get(
                            "VolumeExpansionRatio",
                            1.15));
    
                bool volumeBull =
                    volumeEnabled &&
                    close > open &&
                    volumeRatio >= volumeThreshold;
    
                bool volumeBear =
                    volumeEnabled &&
                    close < open &&
                    volumeRatio >= volumeThreshold;
    
                bool macdEnabled =
                    configuration.Get(
                        "UseMacdBias",
                        false);
    
                bool macdBull =
                    macdEnabled &&
                    macdHistogram > 0 &&
                    macdHistogram >= previousMacd;
    
                bool macdBear =
                    macdEnabled &&
                    macdHistogram < 0 &&
                    macdHistogram <= previousMacd;
    
                bool vwapEnabled =
                    configuration.Get(
                        "UseVwapBias",
                        false);
    
                bool vwapBull =
                    vwapEnabled &&
                    close > vwap;
    
                bool vwapBear =
                    vwapEnabled &&
                    close < vwap;
    
                bool healthyEnabled =
                    configuration.Get(
                        "UseHealthyVolatility",
                        false);
    
                double minHealthyRatio =
                    Math.Max(
                        0.50,
                        configuration.Get(
                            "HealthyAtrMinimumRatio",
                            0.85));
    
                double maxHealthyRatio =
                    Math.Max(
                        minHealthyRatio,
                        configuration.Get(
                            "HealthyAtrMaximumRatio",
                            1.80));
    
                double minBodyAtr =
                    Math.Max(
                        0,
                        configuration.Get(
                            "MinimumTriggerBodyAtr",
                            0.35));
    
                bool healthyVolatility =
                    healthyEnabled &&
                    atrRatio >= minHealthyRatio &&
                    atrRatio <= maxHealthyRatio &&
                    bodyAtr >= minBodyAtr;
    
                bool qualityVolatility =
                    healthyEnabled
                        ? healthyVolatility
                        : atrRatio >= 0.50 &&
                          atrRatio <= 2.50;
    
                bool volumeEvidence =
                    configuration.Get(
                        "UseVolumeExpansionEvidence",
                        true);
    
                bool macdEvidence =
                    configuration.Get(
                        "UseMacdEvidence",
                        true);
    
                bool vwapEvidence =
                    configuration.Get(
                        "UseVwapEvidence",
                        true);
    
                bool healthyEvidence =
                    configuration.Get(
                        "UseHealthyVolatilityEvidence",
                        true);
    
                var features =
                    new List<FeatureEvidence>();
    
                int bullScore = 0;
                int bearScore = 0;
                int independentEvidence = 0;
                int enabledFeatures = 0;
    
                AddFeature(features, MarketFeature.Trend,
                    trendBull, trendBear, 10, true, true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.Momentum,
                    momentumBull, momentumBear, 8, true, true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.Rsi,
                    rsiBull, rsiBear, 3, true, false, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.Dmi,
                    dmiBull, dmiBear, 4,
                    adx >= adxMinimum,
                    true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.EmaSlope,
                    slopeBull, slopeBear, 3,
                    useSlope, false, useSlope,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.Rejection,
                    rejectionBull, rejectionBear, 6, true, false, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.VolumeExpansion,
                    volumeBull, volumeBear, 3,
                    volumeEvidence, false, volumeEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.MacdBias,
                    macdBull, macdBear, 3,
                    macdEvidence, false, macdEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.VwapBias,
                    vwapBull, vwapBear, 2,
                    vwapEvidence, false, vwapEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, MarketFeature.HealthyVolatility,
                    healthyVolatility && close > open,
                    healthyVolatility && close < open,
                    2,
                    healthyEvidence, false, healthyEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                int possibleScore =
                    SumEnabledWeights(
                        useSlope,
                        volumeEvidence && volumeEnabled,
                        macdEvidence && macdEnabled,
                        vwapEvidence && vwapEnabled,
                        healthyEvidence && healthyEnabled);
    
                bool avoidRsiExhaustion =
                    configuration.Get(
                        "AvoidRsiExhaustion",
                        true);
    
                if (avoidRsiExhaustion)
                {
                    if (rsi >= 75)
                        bullScore =
                            Math.Max(
                                0,
                                bullScore - 5);
    
                    if (rsi <= 25)
                        bearScore =
                            Math.Max(
                                0,
                                bearScore - 5);
                }
    
                int bullNormalized =
                    NormalizeScore(bullScore, possibleScore);
    
                int bearNormalized =
                    NormalizeScore(bearScore, possibleScore);
    
                Direction bias =
                    ResolveBias(
                        bullNormalized,
                        bearNormalized);
    
                int biasStrength =
                    bias == Direction.Buy
                        ? bullNormalized
                        : bias == Direction.Sell
                            ? bearNormalized
                            : 0;
    
                Regime regime =
                    DetectRegime(
                        atr,
                        previousAtr,
                        adx,
                        adxMinimum,
                        Math.Abs(
                            emaSpread));
    
                bool choppy =
                    configuration.Get(
                        "UseHistoricalChoppinessGuard",
                        true) &&
                    adx < adxMinimum &&
                    Math.Abs(emaSpread) < atr * 0.35;
    
                int regimeQuality =
                    CalculateRegimeQuality(
                        regime,
                        adx,
                        atrRatio,
                        emaSpreadAtr);
    
                int marketQuality =
                    CalculateMarketQuality(
                        bullNormalized,
                        bearNormalized,
                        adx,
                        independentEvidence,
                        choppy,
                        regimeQuality,
                        qualityVolatility);
    
                return new MarketFrame(
                    timeframe,
                    snapshot.BarOpenUtc,
                    open,
                    high,
                    low,
                    close,
                    atr,
                    atrRatio,
                    rsi,
                    adx,
                    dmiBias,
                    fast,
                    slow,
                    emaSpreadAtr,
                    emaSlopeAtr,
                    momentumAtr,
                    macdHistogram,
                    vwap,
                    volumeRatio,
                    bodyAtr,
                    rangeAtr,
                    bias,
                    biasStrength,
                    marketQuality,
                    bullScore,
                    bearScore,
                    bullNormalized,
                    bearNormalized,
                    independentEvidence,
                    enabledFeatures,
                    regime,
                    regimeQuality,
                    choppy,
                    qualityVolatility,
                    true,
                    features);
            }
    
            private static void AddFeature(
                IList<FeatureEvidence> features,
                MarketFeature feature,
                bool bull,
                bool bear,
                int weight,
                bool countsAsEvidence,
                bool countsAsGate,
                bool enabled,
                ref int bullScore,
                ref int bearScore,
                ref int independentEvidence,
                ref int enabledFeatures)
            {
                if (!enabled)
                    return;
    
                enabledFeatures++;
    
                Direction direction =
                    bull && !bear
                        ? Direction.Buy
                        : bear && !bull
                            ? Direction.Sell
                            : Direction.Wait;
    
                features.Add(
                    new FeatureEvidence(
                        feature,
                        direction,
                        direction == Direction.Wait ? 0 : 1,
                        weight,
                        direction != Direction.Wait,
                        countsAsEvidence,
                        countsAsGate,
                        Provenance.Direct(
                            "MARKET_MODEL",
                            feature.ToString())));
    
                if (direction == Direction.Buy)
                {
                    bullScore += weight;
                    if (countsAsEvidence)
                        independentEvidence++;
                }
                else if (direction == Direction.Sell)
                {
                    bearScore += weight;
                    if (countsAsEvidence)
                        independentEvidence++;
                }
            }
    
            private static int SumEnabledWeights(
                bool slope,
                bool volume,
                bool macd,
                bool vwap,
                bool healthy)
            {
                int weight = 10 + 8 + 3 + 4 + 6;
    
                if (slope) weight += 3;
                if (volume) weight += 3;
                if (macd) weight += 3;
                if (vwap) weight += 2;
                if (healthy) weight += 2;
    
                return Math.Max(1, weight);
            }
    
            private static int NormalizeScore(int score, int maximum)
            {
                return ClampInt(
                    (int)Math.Round(
                        100.0 *
                        Math.Max(0, score) /
                        Math.Max(1, maximum)),
                    0,
                    100);
            }
    
            private static Direction ResolveBias(int bull, int bear)
            {
                if (bull >= 55 && bull >= bear + 12)
                    return Direction.Buy;
    
                if (bear >= 55 && bear >= bull + 12)
                    return Direction.Sell;
    
                return Direction.Wait;
            }
    
            private static Regime DetectRegime(
                double atr,
                double previousAtr,
                double adx,
                int adxMinimum,
                double emaSpread)
            {
                if (atr <= 0 || previousAtr <= 0)
                    return Regime.Unknown;
    
                double ratio = atr / previousAtr;
    
                if (ratio >= 1.30)
                    return Regime.Expansion;
    
                if (ratio <= 0.80)
                    return Regime.Compression;
    
                if (adx < adxMinimum)
                    return Regime.Range;
    
                if (emaSpread <= atr * 0.10)
                    return Regime.Transition;
    
                return Regime.Trend;
            }
    
            private static int CalculateRegimeQuality(
                Regime regime,
                double adx,
                double atrRatio,
                double emaSpreadAtr)
            {
                double adxQuality = Math.Min(100, adx * 1.5);
    
                double volatilityQuality =
                    Math.Max(
                        0,
                        100 -
                        Math.Abs(
                            Math.Log(
                                Math.Max(
                                    0.01,
                                    atrRatio)) *
                            70));
    
                double separationQuality =
                    Math.Min(
                        100,
                        Math.Abs(
                            emaSpreadAtr) *
                        100);
    
                double bonus =
                    regime ==
                        Regime.Trend ||
                    regime ==
                        Regime.Expansion
                        ? 100
                        : regime ==
                            Regime.Transition
                            ? 55
                            : 35;
    
                return ClampInt(
                    (int)Math.Round(
                        adxQuality * 0.45 +
                        volatilityQuality * 0.25 +
                        separationQuality * 0.15 +
                        bonus * 0.15),
                    0,
                    100);
            }
    
            private static int CalculateMarketQuality(
                int bull,
                int bear,
                double adx,
                int evidence,
                bool choppy,
                int regimeQuality,
                bool healthyVolatility)
            {
                double dominance = Math.Max(bull, bear);
                double evidenceQuality = Math.Min(100, evidence * 12.5);
    
                double quality =
                    dominance * 0.45 +
                    Math.Min(100, adx * 1.5) * 0.15 +
                    evidenceQuality * 0.20 +
                    regimeQuality * 0.10 +
                    (healthyVolatility ? 100 : 60) * 0.05 +
                    (choppy ? 0 : 100) * 0.05;
    
                return ClampInt(
                    (int)Math.Round(quality),
                    0,
                    100);
            }
    
            private static bool IsBullishRejection(
                double open,
                double high,
                double low,
                double close)
            {
                double range = Math.Max(0.0000001, high - low);
                double body = Math.Abs(close - open);
                double wick = Math.Min(open, close) - low;
    
                return wick > body * 1.25 &&
                       wick / range > 0.20;
            }
    
            private static bool IsBearishRejection(
                double open,
                double high,
                double low,
                double close)
            {
                double range = Math.Max(0.0000001, high - low);
                double body = Math.Abs(close - open);
                double wick = high - Math.Max(open, close);
    
                return wick > body * 1.25 &&
                       wick / range > 0.20;
            }
    
            private static double RollingVwap(
                Bars bars,
                int index,
                int lookback)
            {
                int first =
                    Math.Max(
                        0,
                        index -
                        Math.Max(
                            10,
                            lookback - 1));
    
                double priceVolume = 0;
                double volume = 0;
    
                for (int i = first; i <= index; i++)
                {
                    double typical =
                        (bars.HighPrices[i] +
                         bars.LowPrices[i] +
                         bars.ClosePrices[i]) /
                        3.0;
    
                    double v =
                        Math.Max(
                            1.0,
                            bars.TickVolumes[i]);
    
                    priceVolume += typical * v;
                    volume += v;
                }
    
                return volume > 0
                    ? priceVolume / volume
                    : bars.ClosePrices[index];
            }
    
            private static double AverageTickVolume(
                Bars bars,
                int index,
                int lookback)
            {
                int first =
                    Math.Max(
                        0,
                        index -
                        Math.Max(
                            1,
                            lookback));
    
                double total = 0;
                int count = 0;
    
                for (int i = first; i < index; i++)
                {
                    total += Math.Max(0, bars.TickVolumes[i]);
                    count++;
                }
    
                return count > 0
                    ? total / count
                    : 0;
            }
    
            private static double NormalizeDmi(
                double plus,
                double minus)
            {
                double total =
                    Math.Max(0, plus) +
                    Math.Max(0, minus);
    
                if (total <= 0)
                    return 0;
    
                return Math.Max(
                    -1,
                    Math.Min(
                        1,
                        (plus - minus) /
                        total));
            }
    
            private static double Read(
                IndicatorDataSeries series,
                int index,
                double fallback = 0)
            {
                if (series == null ||
                    index < 0 ||
                    index >= series.Count)
                    return fallback;
    
                double value = series[index];
    
                return
                    double.IsNaN(value) ||
                    double.IsInfinity(value)
                        ? fallback
                        : value;
            }
    
            private static int ClampInt(
                int value,
                int min,
                int max)
            {
                return Math.Max(
                    min,
                    Math.Min(
                        max,
                        value));
            }
        }
    
    
}
