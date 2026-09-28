using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Frame BuildMarketFrameEvidence(
            Bars bars,
            int index)
        {
            Frame f =
                new Frame
                {
                    Bars = bars,
                    Index = index
                };

            if (bars == null ||
                index < 30 ||
                index >= bars.Count)
                return f;

            f.Atr = Atr(bars, index);
            f.Rsi = Rsi(bars, index);
            f.Adx = Adx(bars, index);
            f.EmaFast = Ema(bars, index, true);
            f.EmaSlow = Ema(bars, index, false);

            if (ReferenceEquals(
                    bars,
                    _m5Bars))
            {
                MarketRegimeSnapshot regimeSnapshot =
                    GetActiveM5Regime(index);

                if (regimeSnapshot != null)
                {
                    f.Choppiness =
                        regimeSnapshot.Choppiness;
                    f.AtrRatio =
                        regimeSnapshot.AtrRatio;
                    f.EmaSpreadAtr =
                        regimeSnapshot.EmaSpreadAtr;
                    f.EmaSlopeAtr =
                        regimeSnapshot.EmaSlopeAtr;
                    f.RangeEfficiency =
                        regimeSnapshot.RangeEfficiency;
                }
            }
            else
            {
                double previousFast =
                    Ema(
                        bars,
                        Math.Max(
                            1,
                            index - 3),
                        true);

                double averageAtr =
                    AverageAtr(
                        bars,
                        Math.Max(
                            20,
                            index - 1),
                        20);

                f.AtrRatio =
                    averageAtr > 0
                        ? f.Atr / averageAtr
                        : 1.0;

                f.EmaSpreadAtr =
                    f.Atr > 0
                        ? Math.Abs(
                            f.EmaFast -
                            f.EmaSlow) /
                          f.Atr
                        : 0;

                f.EmaSlopeAtr =
                    f.Atr > 0
                        ? (f.EmaFast -
                           previousFast) /
                          f.Atr
                        : 0;

                f.RangeEfficiency =
                    CalculateRangeEfficiency(
                        bars,
                        index,
                        10);

                f.Choppiness =
                    f.Choppy
                        ? 65
                        : 45;
            }

            f.StructureBull =
                UseInternalStructure &&
                BullStructure(
                    bars,
                    index,
                    f.Atr);

            f.StructureBear =
                UseInternalStructure &&
                BearStructure(
                    bars,
                    index,
                    f.Atr);

            f.MssBull =
                UseMssChoch &&
                BullMss(
                    bars,
                    index,
                    f.Atr);

            f.MssBear =
                UseMssChoch &&
                BearMss(
                    bars,
                    index,
                    f.Atr);

            f.ChochBull =
                UseMssChoch &&
                BullChoch(
                    bars,
                    index);

            f.ChochBear =
                UseMssChoch &&
                BearChoch(
                    bars,
                    index);

            f.DisplacementBull =
                UseDisplacement &&
                BullDisplacement(
                    bars,
                    index,
                    f.Atr);

            f.DisplacementBear =
                UseDisplacement &&
                BearDisplacement(
                    bars,
                    index,
                    f.Atr);

            f.LiquidityBull =
                UseLiquiditySweep &&
                BullLiquiditySweep(
                    bars,
                    index,
                    f.Atr);

            f.LiquidityBear =
                UseLiquiditySweep &&
                BearLiquiditySweep(
                    bars,
                    index,
                    f.Atr);

            f.FvgBull =
                UseFvg &&
                FindNearestFvg(
                    bars,
                    index,
                    1,
                    f.Atr) != null;

            f.FvgBear =
                UseFvg &&
                FindNearestFvg(
                    bars,
                    index,
                    -1,
                    f.Atr) != null;

            f.ObBull =
                UseOrderBlock &&
                FindNearestOrderBlock(
                    bars,
                    index,
                    1,
                    f.Atr) != null;

            f.ObBear =
                UseOrderBlock &&
                FindNearestOrderBlock(
                    bars,
                    index,
                    -1,
                    f.Atr) != null;

            f.TrendBull =
                f.EmaFast > f.EmaSlow &&
                bars.ClosePrices[index] >
                f.EmaFast;

            f.TrendBear =
                f.EmaFast < f.EmaSlow &&
                bars.ClosePrices[index] <
                f.EmaFast;

            f.MomentumBull =
                Momentum(
                    bars,
                    index,
                    1,
                    f.Atr);

            f.MomentumBear =
                Momentum(
                    bars,
                    index,
                    -1,
                    f.Atr);

            f.RejectionBull =
                Rejection(
                    bars,
                    index,
                    1);

            f.RejectionBear =
                Rejection(
                    bars,
                    index,
                    -1);

            f.VolumeBull =
                HasVolumeExpansion(
                    bars,
                    index,
                    1);

            f.VolumeBear =
                HasVolumeExpansion(
                    bars,
                    index,
                    -1);

            f.MacdBull =
                HasMacdBias(
                    bars,
                    index,
                    1);

            f.MacdBear =
                HasMacdBias(
                    bars,
                    index,
                    -1);

            f.VwapBull =
                HasVwapBias(
                    bars,
                    index,
                    1);

            f.VwapBear =
                HasVwapBias(
                    bars,
                    index,
                    -1);

            f.VolatilityBull =
                HasHealthyVolatility(
                    bars,
                    index,
                    1);

            f.VolatilityBear =
                HasHealthyVolatility(
                    bars,
                    index,
                    -1);

            f.Choppy =
                UseHistoricalChoppinessGuard &&
                (f.Choppiness >=
                    Math.Max(
                        55,
                        RangeChoppinessThreshold) ||
                 (f.Adx <
                    Math.Max(
                        18,
                        MinimumTrendAdx) &&
                  f.EmaSpreadAtr <
                    Math.Max(
                        0.25,
                        MinimumTrendSpreadAtr)));

            f.EqualHigh =
                UseEqualHighLow &&
                FindEqualHigh(
                    bars,
                    index,
                    bars.ClosePrices[index],
                    f.Atr) > 0;

            f.EqualLow =
                UseEqualHighLow &&
                FindEqualLow(
                    bars,
                    index,
                    bars.ClosePrices[index],
                    f.Atr) > 0;

            if (UseOssExtendedIndicatorConfluence &&
                ReferenceEquals(bars, _m5Bars) &&
                index >= bars.Count - 3)
            {
                OssIndicatorSnapshot oss =
                    BuildOssIndicatorSnapshot(
                        bars,
                        index);

                if (oss != null)
                {
                    f.OssBullVotes =
                        oss.BullVotes;
                    f.OssBearVotes =
                        oss.BearVotes;
                    f.OssIndicatorCount =
                        oss.IndicatorCount;
                    f.OssBull =
                        oss.BullVotes >=
                        Math.Max(
                            1,
                            MinimumOssIndicatorAgreement);
                    f.OssBear =
                        oss.BearVotes >=
                        Math.Max(
                            1,
                            MinimumOssIndicatorAgreement);
                }
            }

            return f;
        }
    }
}
