using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private MarketRegimeSnapshot AnalyzeMarketRegime(
            Bars bars,
            int index)
        {
            MarketRegimeSnapshot snapshot =
                new MarketRegimeSnapshot
                {
                    Regime = "UNKNOWN",
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

            if (atr <= 0 ||
                baselineAtr <= 0)
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

            int stability = 1;

            if (ReferenceEquals(bars, _m5Bars) && index > 40)
            {
                MarketRegimeSnapshot previous =
                    AnalyzeMarketRegime(
                        bars,
                        index - 1);

                if (previous != null &&
                    previous.Regime == regime)
                {
                    stability++;

                    if (index > 41)
                    {
                        MarketRegimeSnapshot beforePrevious =
                            AnalyzeMarketRegime(
                                bars,
                                index - 2);

                        if (beforePrevious != null &&
                            beforePrevious.Regime == regime)
                            stability++;
                    }
                }
            }

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