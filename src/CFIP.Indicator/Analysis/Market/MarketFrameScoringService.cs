using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string ResolveFrameRegime(
            Frame frame)
        {
            if (frame == null)
                return "UNKNOWN";

            if (ReferenceEquals(frame.Bars, _m5Bars))
            {
                MarketRegimeSnapshot regime =
                    GetActiveM5Regime(
                        frame.Index);

                return regime == null
                    ? "UNKNOWN"
                    : regime.Regime;
            }

            return "UNKNOWN";
        }

        private Frame ScoreMarketFrame(
            Frame f)
        {
            if (f == null ||
                f.Bars == null ||
                f.Index < 0 ||
                f.Index >= f.Bars.Count)
                return f;

            Bars bars = f.Bars;
            int index = f.Index;

            int bull = 0;
            int bear = 0;
            int evidence = 0;

            // Structure, MSS and CHOCH can describe the same causal break.
            // Count one structural event only, preserving the strongest applicable
            // label instead of stacking correlated points.
            if (StructuralEvidenceRule.HasCanonicalStructuralEvent(
                    f.StructureBull,
                    f.MssBull,
                    f.ChochBull))
            {
                if (f.StructureBull)
                    AddScore(true, 16, ref bull, ref evidence);
                else if (f.MssBull)
                    AddScore(true, 12, ref bull, ref evidence);
                else
                    AddScore(true, 9, ref bull, ref evidence);
            }

            if (StructuralEvidenceRule.HasCanonicalStructuralEvent(
                    f.StructureBear,
                    f.MssBear,
                    f.ChochBear))
            {
                if (f.StructureBear)
                    AddScore(true, 16, ref bear, ref evidence);
                else if (f.MssBear)
                    AddScore(true, 12, ref bear, ref evidence);
                else
                    AddScore(true, 9, ref bear, ref evidence);
            }
            AddScore(f.DisplacementBull, 10, ref bull, ref evidence);
            AddScore(f.DisplacementBear, 10, ref bear, ref evidence);
            AddScore(f.LiquidityBull, 10, ref bull, ref evidence);
            AddScore(f.LiquidityBear, 10, ref bear, ref evidence);
            if (f.FvgBull)
            {
                bull +=
                    5 +
                    Math.Min(
                        5,
                        f.FvgBullQuality / 20);
                evidence++;
            }

            if (f.FvgBear)
            {
                bear +=
                    5 +
                    Math.Min(
                        5,
                        f.FvgBearQuality / 20);
                evidence++;
            }

            if (f.ObBull)
            {
                bull +=
                    6 +
                    Math.Min(
                        5,
                        f.ObBullQuality / 20);
                evidence++;
            }

            if (f.ObBear)
            {
                bear +=
                    6 +
                    Math.Min(
                        5,
                        f.ObBearQuality / 20);
                evidence++;
            }

            if (f.FvgObBullConfluence &&
                Math.Min(
                    f.FvgBullQuality,
                    f.ObBullQuality) >= 75)
                bull += 4;

            if (f.FvgObBearConfluence &&
                Math.Min(
                    f.FvgBearQuality,
                    f.ObBearQuality) >= 75)
                bear += 4;
            AddScore(
                f.EqualLow,
                5,
                ref bull,
                ref evidence);

            AddScore(
                f.EqualHigh,
                5,
                ref bear,
                ref evidence);

            IndicatorEvidenceFusionResult indicatorFusion =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        ResolveFrameRegime(f),
                        f.TrendBull,
                        f.TrendBear,
                        f.MomentumBull,
                        f.MomentumBear,
                        f.MacdBull,
                        f.MacdBear,
                        f.VwapBull,
                        f.VwapBear,
                        f.VolumeBull,
                        f.VolumeBear,
                        f.VolatilityBull,
                        f.VolatilityBear,
                        f.Rsi,
                        DmiBias(
                            bars,
                            index),
                        UseEmaSlope
                            ? f.EmaSlopeAtr
                            : 0,
                        f.WaveTrendDirection,
                        f.WaveTrendQuality,
                        f.DivergenceDirection,
                        f.DivergenceQuality,
                        f.WaveTrendBullCross,
                        f.WaveTrendBearCross,
                        f.WaveTrendOversold,
                        f.WaveTrendOverbought,
                        f.OssBullVotes,
                        f.OssBearVotes,
                        f.OssIndicatorCount,
                        MinimumOssIndicatorAgreement));

            bull +=
                indicatorFusion.BullBonus;
            bear +=
                indicatorFusion.BearBonus;

            f.IndicatorConfluenceQuality =
                indicatorFusion.Quality;
            f.IndicatorConflict =
                indicatorFusion.Conflict;

            if (indicatorFusion.Conflict >= 45)
            {
                if (bull >= bear)
                    bull =
                        Math.Max(
                            0,
                            bull -
                            Math.Min(
                                6,
                                (indicatorFusion.Conflict - 40) / 10));
                else
                    bear =
                        Math.Max(
                            0,
                            bear -
                            Math.Min(
                                6,
                                (indicatorFusion.Conflict - 40) / 10));
            }

            if (AvoidRsiExhaustion)
            {
                if (f.Rsi >= 75)
                    bull = Math.Max(
                        0,
                        bull - 5);

                if (f.Rsi <= 25)
                    bear = Math.Max(
                        0,
                        bear - 5);
            }

            f.BullScore = bull;
            f.BearScore = bear;
            f.Evidence = evidence;

            if (bull >= 35 &&
                bull >= bear + 8)
                f.Direction = 1;
            else if (bear >= 35 &&
                     bear >= bull + 8)
                f.Direction = -1;

            double total =
                Math.Max(
                    1,
                    bull + bear);

            double strongest =
                100.0 *
                Math.Max(
                    bull,
                    bear) /
                total;

            double regimeContribution;

            if (f.Choppy)
            {
                regimeContribution =
                    Math.Max(
                        0,
                        8 -
                        Math.Max(
                            0,
                            f.Choppiness -
                            RangeChoppinessThreshold) *
                        0.30);
            }
            else
            {
                regimeContribution =
                    Math.Min(
                        14,
                        Math.Max(
                            0,
                            f.Adx -
                            MinimumTransitionAdx) *
                        0.35 +
                        f.RangeEfficiency * 8 +
                        Math.Min(
                            4,
                            f.EmaSpreadAtr * 2));
            }

            f.Quality =
                ClampInt(
                    (int)Math.Round(
                        strongest * 0.40 +
                        Math.Min(
                            100,
                            f.Adx * 1.45) * 0.13 +
                        Math.Min(
                            100,
                            evidence * 5) * 0.20 +
                        regimeContribution * 0.15 +
                        f.IndicatorConfluenceQuality * 0.12) -
                    Math.Min(
                        10,
                        Math.Max(
                            0,
                            f.IndicatorConflict - 35) / 6),
                    0,
                    100);

            return f;
        }
    }
}
