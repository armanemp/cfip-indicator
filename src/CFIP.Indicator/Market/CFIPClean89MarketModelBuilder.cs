// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89MarketModelBuilder
        {
            private const int MinimumClosedIndex = 30;
            private readonly CFIPClean89NativeIndicatorCatalog _catalog;
    
            public CFIPClean89MarketModelBuilder(IIndicatorsAccessor indicators)
            {
                _catalog = new CFIPClean89NativeIndicatorCatalog(indicators);
            }
    
            public CFIPClean89MarketModel Build(
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89MtfSnapshot mtf,
                CFIPClean89ConfigSnapshot configuration,
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
    
                var frames = new List<CFIPClean89MarketFrame>();
    
                if (!mtf.IsReferenceValid)
                    return new CFIPClean89MarketModel(
                        DateTime.MinValue,
                        false,
                        false,
                        mtf.DataStatus.ToString(),
                        frames);
    
                bool coherent =
                    CFIPClean89MtfSnapshotBuilder.IsCoherent(mtf);
    
                AddFrame(frames, "CHART", chartBars, mtf.Chart, configuration);
                AddFrame(frames, "M1", m1Bars, mtf.M1, configuration);
                AddFrame(frames, "M5", m5Bars, mtf.M5, configuration);
                AddFrame(frames, "M15", m15Bars, mtf.M15, configuration);
                AddFrame(frames, "M30", m30Bars, mtf.M30, configuration);
                AddFrame(frames, "H1", h1Bars, mtf.H1, configuration);
                AddFrame(frames, "H4", h4Bars, mtf.H4, configuration);
                AddFrame(frames, "D1", d1Bars, mtf.D1, configuration);
                AddFrame(frames, "W1", w1Bars, mtf.W1, configuration);
    
                return new CFIPClean89MarketModel(
                    mtf.ReferenceUtc,
                    coherent,
                    mtf.IsPrimaryDecisionReady,
                    mtf.DataStatus.ToString(),
                    frames);
            }
    
            private void AddFrame(
                IList<CFIPClean89MarketFrame> frames,
                string timeframe,
                Bars bars,
                CFIPClean89MtfBarSnapshot snapshot,
                CFIPClean89ConfigSnapshot configuration)
            {
                if (frames == null ||
                    snapshot == null ||
                    !snapshot.IsAvailable ||
                    !snapshot.IsFullyClosedAtReference ||
                    snapshot.ClosedIndex < MinimumClosedIndex ||
                    bars == null)
                    return;
    
                CFIPClean89MarketFrame frame =
                    BuildFrame(
                        timeframe,
                        bars,
                        snapshot,
                        configuration);
    
                if (frame != null)
                    frames.Add(frame);
            }
    
            private CFIPClean89MarketFrame BuildFrame(
                string timeframe,
                Bars bars,
                CFIPClean89MtfBarSnapshot snapshot,
                CFIPClean89ConfigSnapshot configuration)
            {
                int index = snapshot.ClosedIndex;
    
                if (index < MinimumClosedIndex ||
                    index >= bars.Count - 1)
                    return null;
    
                CFIPClean89NativeIndicatorSet native =
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
                    new List<CFIPClean89FeatureEvidence>();
    
                int bullScore = 0;
                int bearScore = 0;
                int independentEvidence = 0;
                int enabledFeatures = 0;
    
                AddFeature(features, CFIPClean89MarketFeature.Trend,
                    trendBull, trendBear, 10, true, true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.Momentum,
                    momentumBull, momentumBear, 8, true, true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.Rsi,
                    rsiBull, rsiBear, 3, true, false, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.Dmi,
                    dmiBull, dmiBear, 4,
                    adx >= adxMinimum,
                    true, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.EmaSlope,
                    slopeBull, slopeBear, 3,
                    useSlope, false, useSlope,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.Rejection,
                    rejectionBull, rejectionBear, 6, true, false, true,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.VolumeExpansion,
                    volumeBull, volumeBear, 3,
                    volumeEvidence, false, volumeEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.MacdBias,
                    macdBull, macdBear, 3,
                    macdEvidence, false, macdEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.VwapBias,
                    vwapBull, vwapBear, 2,
                    vwapEvidence, false, vwapEnabled,
                    ref bullScore, ref bearScore,
                    ref independentEvidence, ref enabledFeatures);
    
                AddFeature(features, CFIPClean89MarketFeature.HealthyVolatility,
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
    
                CFIPClean89Direction bias =
                    ResolveBias(
                        bullNormalized,
                        bearNormalized);
    
                int biasStrength =
                    bias == CFIPClean89Direction.Buy
                        ? bullNormalized
                        : bias == CFIPClean89Direction.Sell
                            ? bearNormalized
                            : 0;
    
                CFIPClean89Regime regime =
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
    
                return new CFIPClean89MarketFrame(
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
                IList<CFIPClean89FeatureEvidence> features,
                CFIPClean89MarketFeature feature,
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
    
                CFIPClean89Direction direction =
                    bull && !bear
                        ? CFIPClean89Direction.Buy
                        : bear && !bull
                            ? CFIPClean89Direction.Sell
                            : CFIPClean89Direction.Wait;
    
                features.Add(
                    new CFIPClean89FeatureEvidence(
                        feature,
                        direction,
                        direction == CFIPClean89Direction.Wait ? 0 : 1,
                        weight,
                        direction != CFIPClean89Direction.Wait,
                        countsAsEvidence,
                        countsAsGate,
                        CFIPClean89Provenance.Direct(
                            "MARKET_MODEL",
                            feature.ToString())));
    
                if (direction == CFIPClean89Direction.Buy)
                {
                    bullScore += weight;
                    if (countsAsEvidence)
                        independentEvidence++;
                }
                else if (direction == CFIPClean89Direction.Sell)
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
    
            private static CFIPClean89Direction ResolveBias(int bull, int bear)
            {
                if (bull >= 55 && bull >= bear + 12)
                    return CFIPClean89Direction.Buy;
    
                if (bear >= 55 && bear >= bull + 12)
                    return CFIPClean89Direction.Sell;
    
                return CFIPClean89Direction.Wait;
            }
    
            private static CFIPClean89Regime DetectRegime(
                double atr,
                double previousAtr,
                double adx,
                int adxMinimum,
                double emaSpread)
            {
                if (atr <= 0 || previousAtr <= 0)
                    return CFIPClean89Regime.Unknown;
    
                double ratio = atr / previousAtr;
    
                if (ratio >= 1.30)
                    return CFIPClean89Regime.Expansion;
    
                if (ratio <= 0.80)
                    return CFIPClean89Regime.Compression;
    
                if (adx < adxMinimum)
                    return CFIPClean89Regime.Range;
    
                if (emaSpread <= atr * 0.10)
                    return CFIPClean89Regime.Transition;
    
                return CFIPClean89Regime.Trend;
            }
    
            private static int CalculateRegimeQuality(
                CFIPClean89Regime regime,
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
                        CFIPClean89Regime.Trend ||
                    regime ==
                        CFIPClean89Regime.Expansion
                        ? 100
                        : regime ==
                            CFIPClean89Regime.Transition
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
