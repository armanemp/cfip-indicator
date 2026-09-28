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

            double efficiency =
                CalculateRangeEfficiency(
                    bars,
                    index,
                    Math.Max(
                        10,
                        RegimeLookbackBars));

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

            string regime;

            if (atrRatio <=
                    Math.Min(
                        0.95,
                        CompressionAtrRatio) &&
                choppiness >=
                    Math.Max(
                        52,
                        RangeChoppinessThreshold) &&
                efficiency <=
                    Math.Max(
                        0.35,
                        RangeEfficiencyThreshold))
            {
                regime = "COMPRESSION";
            }
            else if (atrRatio >=
                         Math.Max(
                             1.45,
                             HighVolatilityAtrRatio) &&
                     (adx < Math.Max(18, MinimumTrendAdx) ||
                      choppiness >=
                      Math.Max(
                          55,
                          RangeChoppinessThreshold)))
            {
                regime = "HIGH_VOLATILITY";
            }
            else if (adx < Math.Max(20, MinimumTrendAdx) &&
                     choppiness >=
                     Math.Max(
                         55,
                         RangeChoppinessThreshold) &&
                     efficiency <=
                     Math.Max(
                         0.30,
                         RangeEfficiencyThreshold))
            {
                regime = "RANGE";
            }
            else if (atrRatio >=
                         Math.Max(
                             1.25,
                             ExpansionAtrRatio) &&
                     (adx >= Math.Max(18, MinimumTransitionAdx) ||
                      efficiency >=
                      Math.Max(
                          0.30,
                          MinimumTrendEfficiency)))
            {
                regime = "EXPANSION";
            }
            else if (adx >=
                         Math.Max(
                             20,
                             MinimumTrendAdx) &&
                     choppiness <=
                         Math.Min(
                             60,
                             TrendChoppinessThreshold) &&
                     efficiency >=
                         Math.Max(
                             0.25,
                             MinimumTrendEfficiency) &&
                     emaSpreadAtr >=
                         Math.Max(
                             0.20,
                             MinimumTrendSpreadAtr))
            {
                regime = "TREND";
            }
            else
            {
                regime = "TRANSITION";
            }

            int quality;

            switch (regime)
            {
                case "TREND":
                    quality =
                        (int)Math.Round(
                            48 +
                            Math.Min(
                                35,
                                adx * 1.25) +
                            efficiency * 18 -
                            Math.Max(
                                0,
                                choppiness - 38) * 0.40 +
                            Math.Min(
                                12,
                                emaSpreadAtr * 8));
                    break;

                case "EXPANSION":
                    quality =
                        (int)Math.Round(
                            55 +
                            Math.Min(
                                30,
                                adx * 0.90) +
                            efficiency * 18 -
                            Math.Max(
                                0,
                                choppiness - 45) * 0.50);
                    break;

                case "RANGE":
                    quality =
                        (int)Math.Round(
                            32 +
                            Math.Max(
                                0,
                                18 -
                                adx) +
                            Math.Max(
                                0,
                                0.35 -
                                efficiency) * 15);
                    break;

                case "COMPRESSION":
                    quality =
                        22;
                    break;

                case "HIGH_VOLATILITY":
                    quality =
                        (int)Math.Round(
                            34 +
                            Math.Min(
                                20,
                                adx) +
                            efficiency * 10 -
                            Math.Max(
                                0,
                                atrRatio - 1.50) * 12);
                    break;

                case "TRANSITION":
                    quality =
                        (int)Math.Round(
                            40 +
                            Math.Min(
                                15,
                                adx * 0.60) +
                            efficiency * 10);
                    break;

                default:
                    quality = 35;
                    break;
            }

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