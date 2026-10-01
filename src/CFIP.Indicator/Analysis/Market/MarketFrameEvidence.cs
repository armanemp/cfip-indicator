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

            Native native =
                GetNative(bars);

            f.NativeIndicatorsReady =
                NativeIndicatorReadinessRule.IsFrameReady(
                    index,
                    native == null || native.Atr == null
                        ? 0
                        : native.Atr.Result.Count,
                    native == null || native.Rsi == null
                        ? 0
                        : native.Rsi.Result.Count,
                    native == null || native.Dms == null
                        ? 0
                        : native.Dms.ADX.Count,
                    native == null || native.Fast == null
                        ? 0
                        : native.Fast.Result.Count,
                    native == null || native.Slow == null
                        ? 0
                        : native.Slow.Result.Count,
                    Math.Max(2, AtrPeriod),
                    Math.Max(2, RsiPeriod),
                    Math.Max(2, AdxPeriod),
                    Math.Max(2, FastEma),
                    Math.Max(3, SlowEma),
                    f.Atr,
                    f.Rsi,
                    f.Adx,
                    f.EmaFast,
                    f.EmaSlow);

            if (!f.NativeIndicatorsReady)
                return f;

            ApplyWaveTrendEvidence(
                f,
                bars,
                index);

            DivergenceResult divergence =
                AnalyzeDivergence(
                    bars,
                    index);

            f.DivergenceDirection =
                divergence.Direction;
            f.DivergenceQuality =
                divergence.Quality;
            f.DivergenceType =
                divergence.Type;
            f.RegularDivergenceBull =
                divergence.RegularBull;
            f.RegularDivergenceBear =
                divergence.RegularBear;
            f.HiddenDivergenceBull =
                divergence.HiddenBull;
            f.HiddenDivergenceBear =
                divergence.HiddenBear;

            MarketRegimeSnapshot regimeSnapshot =
                AnalyzeMarketRegime(
                    bars,
                    index);

            if (regimeSnapshot != null)
            {
                f.Regime =
                    FrameRegimeResolutionRule.ResolveFrameSnapshotRegime(
                        regimeSnapshot);
                f.RegimeQuality =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            regimeSnapshot.Quality));
                f.RegimeStability =
                    Math.Max(
                        1,
                        Math.Min(
                            3,
                            regimeSnapshot.Stability));
                f.PreviousRegime =
                    MarketRegimeIdentity.NormalizeMarketRegime(
                        regimeSnapshot.PreviousRegime);
                f.RegimeTransition =
                    MarketRegimeTransitionRule.Resolve(
                        f.PreviousRegime,
                        f.Regime);
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
            else
            {
                f.Regime =
                    FrameRegimeResolutionRule.Unknown;
                f.RegimeQuality = 0;
                f.RegimeStability = 0;
                f.PreviousRegime = MarketRegimeIdentity.Unknown;
                f.RegimeTransition = MarketRegimeTransitionRule.Unknown;
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

            Zone bullFvg =
                UseFvg
                    ? FindNearestFvg(
                        bars,
                        index,
                        1,
                        f.Atr)
                    : null;

            Zone bearFvg =
                UseFvg
                    ? FindNearestFvg(
                        bars,
                        index,
                        -1,
                        f.Atr)
                    : null;

            Zone bullOb =
                UseOrderBlock
                    ? FindNearestOrderBlock(
                        bars,
                        index,
                        1,
                        f.Atr)
                    : null;

            Zone bearOb =
                UseOrderBlock
                    ? FindNearestOrderBlock(
                        bars,
                        index,
                        -1,
                        f.Atr)
                    : null;

            f.FvgBull =
                bullFvg != null;
            f.FvgBear =
                bearFvg != null;
            f.FvgBullQuality =
                bullFvg == null
                    ? 0
                    : bullFvg.Quality;
            f.FvgBearQuality =
                bearFvg == null
                    ? 0
                    : bearFvg.Quality;

            f.ObBull =
                bullOb != null;
            f.ObBear =
                bearOb != null;
            f.ObBullQuality =
                bullOb == null
                    ? 0
                    : bullOb.Quality;
            f.ObBearQuality =
                bearOb == null
                    ? 0
                    : bearOb.Quality;

            f.FvgObBullConfluence =
                bullFvg != null &&
                bullOb != null &&
                bullFvg.High >= bullOb.Low &&
                bullOb.High >= bullFvg.Low;

            f.FvgObBearConfluence =
                bearFvg != null &&
                bearOb != null &&
                bearFvg.High >= bearOb.Low &&
                bearOb.High >= bearFvg.Low;

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

            bool healthyVolatility =
                HasHealthyVolatility(
                    bars,
                    index);

            f.VolatilityBull =
                healthyVolatility &&
                bars.ClosePrices[index] >
                bars.OpenPrices[index];

            f.VolatilityBear =
                healthyVolatility &&
                bars.ClosePrices[index] <
                bars.OpenPrices[index];

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
