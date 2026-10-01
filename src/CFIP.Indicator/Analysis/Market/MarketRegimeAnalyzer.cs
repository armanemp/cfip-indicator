using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly MarketRegimeFrameCache _marketRegimeFrameCache =
            new MarketRegimeFrameCache();
        private MarketRegimeSnapshot AnalyzeMarketRegime(
            Bars bars,
            int index)
        {
            if (bars == null ||
                index < 0 ||
                index >= bars.Count)
                return null;

            if (!ReferenceEquals(
                    bars,
                    _m5Bars))
            {
                if (_marketRegimeFrameCache.TryGetSnapshot(
                        bars,
                        index,
                        out MarketRegimeSnapshot cached))
                    return cached;

                MarketRegimeSnapshot nonM5Snapshot =
                    AnalyzeMarketRegimeCore(
                        bars,
                        index);

                ApplyRegimeTransition(
                    bars,
                    index,
                    nonM5Snapshot);

                _marketRegimeFrameCache.StoreSnapshot(
                    bars,
                    index,
                    nonM5Snapshot);

                return nonM5Snapshot;
            }

            MarketRegimeSnapshot snapshot =
                GetM5RegimeCoreSnapshot(
                    bars,
                    index);

            if (snapshot == null ||
                index <= 40)
                return snapshot;

            int stability = 1;

            MarketRegimeSnapshot previous =
                GetM5RegimeCoreSnapshot(
                    bars,
                    index - 1);

            if (previous != null &&
                previous.Regime == snapshot.Regime)
            {
                stability++;

                MarketRegimeSnapshot beforePrevious =
                    GetM5RegimeCoreSnapshot(
                        bars,
                        index - 2);

                if (beforePrevious != null &&
                    beforePrevious.Regime == snapshot.Regime)
                    stability++;
            }

            snapshot.PreviousRegime =
                previous == null
                    ? MarketRegimeIdentity.Unknown
                    : previous.Regime;
            snapshot.RegimeTransition =
                MarketRegimeTransitionRule.Resolve(
                    snapshot.PreviousRegime,
                    snapshot.Regime);

            snapshot.Stability =
                Math.Max(
                    1,
                    Math.Min(
                        3,
                        stability));

            return snapshot;
        }

        private void ApplyRegimeTransition(
            Bars bars,
            int index,
            MarketRegimeSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            if (index <= 40)
            {
                snapshot.PreviousRegime = MarketRegimeIdentity.Unknown;
                snapshot.RegimeTransition =
                    MarketRegimeTransitionRule.Resolve(
                        MarketRegimeIdentity.Unknown,
                        snapshot.Regime);
                return;
            }

            MarketRegimeSnapshot previous =
                AnalyzeMarketRegimeCore(
                    bars,
                    index - 1);

            snapshot.PreviousRegime =
                previous == null
                    ? MarketRegimeIdentity.Unknown
                    : previous.Regime;
            snapshot.RegimeTransition =
                MarketRegimeTransitionRule.Resolve(
                    snapshot.PreviousRegime,
                    snapshot.Regime);
        }

        private MarketRegimeSnapshot GetM5RegimeCoreSnapshot(
            Bars bars,
            int index)
        {
            if (bars == null || index < 0)
                return null;

            if (_m5RegimeCoreCache.TryGetRecentCore(
                    bars,
                    index,
                    out MarketRegimeSnapshot cached))
                return cached;

            MarketRegimeSnapshot snapshot =
                AnalyzeMarketRegimeCore(
                    bars,
                    index);

            _m5RegimeCoreCache.StoreRecentCore(
                bars,
                index,
                snapshot);

            return snapshot;
        }

        private MarketRegimeSnapshot AnalyzeMarketRegimeCore(
            Bars bars,
            int index)
        {
            MarketRegimeSnapshot snapshot =
                new MarketRegimeSnapshot
                {
                    Regime = MarketRegimeIdentity.Unknown,
                    Quality = 35,
                    Direction = 0,
                    Choppiness = 100,
                    AtrRatio = 1,
                    EmaSpreadAtr = 0,
                    EmaSlopeAtr = 0,
                    RangeEfficiency = 0,
                    Adx = 0,
                    DmiBias = 0,
                    ReturnAtr = 0,
                    Reason = "INSUFFICIENT DATA"
                };

            if (bars == null ||
                index < 40 ||
                index >= bars.Count)
                return snapshot;

            double atr =
                Atr(
                    bars,
                    index);

            double baselineAtr =
                AverageAtr(
                    bars,
                    Math.Max(
                        20,
                        index - 1),
                    20);

            if (!NativeIndicatorReadinessRule.IsFinitePositiveNative(atr) ||
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(baselineAtr))
                return snapshot;

            double fast =
                Ema(
                    bars,
                    index,
                    true);

            double slow =
                Ema(
                    bars,
                    index,
                    false);

            double previousFast =
                Ema(
                    bars,
                    Math.Max(
                        1,
                        index - 3),
                    true);

            if (!NativeIndicatorReadinessRule.IsFinitePositiveNative(fast) ||
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(slow) ||
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(previousFast))
                return snapshot;

            double atrRatio =
                atr /
                baselineAtr;

            double emaSpreadAtr =
                Math.Abs(
                    fast -
                    slow) /
                atr;

            double emaSlopeAtr =
                (fast -
                 previousFast) /
                atr;

            int regimeLookback =
                Math.Max(
                    10,
                    RegimeLookbackBars);

            double efficiency =
                CalculateRangeEfficiency(
                    bars,
                    index,
                    regimeLookback);

            int rangeFirst =
                Math.Max(
                    0,
                    index -
                    regimeLookback +
                    1);

            double rangeHigh =
                bars.HighPrices[rangeFirst];
            double rangeLow =
                bars.LowPrices[rangeFirst];

            for (int i = rangeFirst + 1;
                 i <= index;
                 i++)
            {
                rangeHigh =
                    Math.Max(
                        rangeHigh,
                        bars.HighPrices[i]);
                rangeLow =
                    Math.Min(
                        rangeLow,
                        bars.LowPrices[i]);
            }

            double rangeWidthAtr =
                atr > 0
                    ? Math.Max(
                        0,
                        rangeHigh - rangeLow) /
                      atr
                    : 0;

            double choppiness =
                CalculateChoppinessIndex(
                    bars,
                    index,
                    Math.Max(
                        10,
                        ChoppinessPeriod));

            double adx =
                Adx(
                    bars,
                    index);

            double dmiBias =
                DmiBias(
                    bars,
                    index);

            double returnAtr =
                Math.Abs(
                    bars.ClosePrices[index] -
                    bars.ClosePrices[
                        Math.Max(
                            0,
                            index -
                            Math.Max(
                                3,
                                RegimeReturnBars))]) /
                atr;

            int direction =
                dmiBias > 0
                    ? 1
                    : dmiBias < 0
                        ? -1
                        : emaSlopeAtr > 0
                            ? 1
                            : emaSlopeAtr < 0
                                ? -1
                                : 0;

            MarketRegimeClassificationInput classification =
                new MarketRegimeClassificationInput
                {
                    AtrRatio = atrRatio,
                    Choppiness = choppiness,
                    RangeEfficiency = efficiency,
                    RangeWidthAtr = rangeWidthAtr,
                    ReturnAtr = returnAtr,
                    Adx = adx,
                    EmaSpreadAtr = emaSpreadAtr
                };

            string regime =
                MarketRegimeClassifier.Classify(
                    classification,
                    CompressionAtrRatio,
                    ExpansionAtrRatio,
                    HighVolatilityAtrRatio,
                    RangeChoppinessThreshold,
                    RangeEfficiencyThreshold,
                    MicroRangeWidthAtr,
                    MicroRangeReturnAtr,
                    MinimumTrendAdx,
                    MinimumTransitionAdx,
                    MinimumTrendEfficiency,
                    TrendChoppinessThreshold,
                    MinimumTrendSpreadAtr);

            int quality =
                MarketRegimeClassifier.Quality(
                    regime,
                    classification);

            // Stability is intentionally non-recursive. The previous
            // implementation walked backward through the entire M5 history
            // by calling AnalyzeMarketRegime() from itself, which could grow
            // the call stack by hundreds/thousands of frames and trigger a
            // process-level stack overflow during startup/live refresh.
            int stability = 1;

            snapshot.Regime = regime;
            snapshot.Quality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality));
            snapshot.Stability =
                Math.Max(
                    1,
                    Math.Min(
                        3,
                        stability));
            snapshot.Direction = direction;
            snapshot.Choppiness = choppiness;
            snapshot.AtrRatio = atrRatio;
            snapshot.EmaSpreadAtr = emaSpreadAtr;
            snapshot.EmaSlopeAtr = emaSlopeAtr;
            snapshot.RangeEfficiency = efficiency;
            snapshot.RangeWidthAtr = rangeWidthAtr;
            snapshot.Adx = adx;
            snapshot.DmiBias = dmiBias;
            snapshot.ReturnAtr = returnAtr;
            snapshot.Reason =
                regime +
                " | ADX " +
                Math.Round(adx, 1) +
                " | CHOP " +
                Math.Round(choppiness, 1) +
                " | ATRx " +
                Math.Round(atrRatio, 2) +
                " | EFF " +
                Math.Round(efficiency, 2);

            return snapshot;
        }
    }
}