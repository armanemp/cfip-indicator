// ============================================================================
// CFIP Indicator — MarketFrameAnalyzer.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Frame AnalyzeFrame(
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
                                f.Adx < AdxMinimum &&
                                Math.Abs(
                                    f.EmaFast -
                                    f.EmaSlow) <
                                f.Atr * 0.35;
                
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
                
                            int bull = 0;
                            int bear = 0;
                            int evidence = 0;
                
                            AddScore(f.StructureBull, 16, ref bull, ref evidence);
                            AddScore(f.StructureBear, 16, ref bear, ref evidence);
                            AddScore(f.MssBull, 12, ref bull, ref evidence);
                            AddScore(f.MssBear, 12, ref bear, ref evidence);
                            AddScore(f.ChochBull, 9, ref bull, ref evidence);
                            AddScore(f.ChochBear, 9, ref bear, ref evidence);
                            AddScore(f.DisplacementBull, 10, ref bull, ref evidence);
                            AddScore(f.DisplacementBear, 10, ref bear, ref evidence);
                            AddScore(f.LiquidityBull, 10, ref bull, ref evidence);
                            AddScore(f.LiquidityBear, 10, ref bear, ref evidence);
                            AddScore(f.FvgBull, 8, ref bull, ref evidence);
                            AddScore(f.FvgBear, 8, ref bear, ref evidence);
                            AddScore(f.ObBull, 9, ref bull, ref evidence);
                            AddScore(f.ObBear, 9, ref bear, ref evidence);
                            AddScore(f.TrendBull, 10, ref bull, ref evidence);
                            AddScore(f.TrendBear, 10, ref bear, ref evidence);
                            AddScore(f.MomentumBull, 8, ref bull, ref evidence);
                            AddScore(f.MomentumBear, 8, ref bear, ref evidence);
                            AddScore(f.RejectionBull, 6, ref bull, ref evidence);
                            AddScore(f.RejectionBear, 6, ref bear, ref evidence);
                            AddScore(
                                f.VolumeBull,
                                3,
                                ref bull,
                                ref evidence,
                                UseVolumeExpansionEvidence);
                            AddScore(
                                f.VolumeBear,
                                3,
                                ref bear,
                                ref evidence,
                                UseVolumeExpansionEvidence);
                            AddScore(
                                f.MacdBull,
                                3,
                                ref bull,
                                ref evidence,
                                UseMacdEvidence);
                            AddScore(
                                f.MacdBear,
                                3,
                                ref bear,
                                ref evidence,
                                UseMacdEvidence);
                            AddScore(
                                f.VwapBull,
                                2,
                                ref bull,
                                ref evidence,
                                UseVwapEvidence);
                            AddScore(
                                f.VwapBear,
                                2,
                                ref bear,
                                ref evidence,
                                UseVwapEvidence);
                            AddScore(
                                f.VolatilityBull,
                                2,
                                ref bull,
                                ref evidence,
                                UseHealthyVolatilityEvidence);
                            AddScore(
                                f.VolatilityBear,
                                2,
                                ref bear,
                                ref evidence,
                                UseHealthyVolatilityEvidence);
                            AddScore(f.EqualLow, 5, ref bull, ref evidence);
                            AddScore(f.EqualHigh, 5, ref bear, ref evidence);
                
                            if (f.Rsi > 50)
                                bull += 3;
                            else if (f.Rsi < 50)
                                bear += 3;
                
                            if (f.Adx >= AdxMinimum)
                            {
                                double dmi =
                                    DmiBias(
                                        bars,
                                        index);
                
                                if (dmi > 0)
                                    bull += 4;
                                else if (dmi < 0)
                                    bear += 4;
                            }
                
                            if (UseEmaSlope &&
                                index > 2)
                            {
                                double previous =
                                    Ema(
                                        bars,
                                        index - 2,
                                        true);
                
                                if (f.EmaFast > previous)
                                    bull += 3;
                                else if (f.EmaFast < previous)
                                    bear += 3;
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
                
                            f.Quality =
                                ClampInt(
                                    (int)Math.Round(
                                        strongest * 0.50 +
                                        Math.Min(
                                            100,
                                            f.Adx * 1.5) * 0.15 +
                                        Math.Min(
                                            100,
                                            evidence * 5) * 0.25 +
                                        (f.Choppy ? 0 : 10) * 0.10),
                                    0,
                                    100);
                
                            return f;
                        }
        
        private void AddScore(
                            bool condition,
                            int score,
                            ref int total,
                            ref int evidence)
                        {
                            if (!condition)
                                return;
                
                            total += score;
                            evidence++;
                        }
        
        private void AddScore(
                            bool condition,
                            int score,
                            ref int total,
                            ref int evidence,
                            bool countAsEvidence)
                        {
                            if (!condition)
                                return;
                
                            total += score;
                
                            if (countAsEvidence)
                                evidence++;
                        }
        
        private void AddFrame(
                            Frame frame,
                            double weight,
                            ref double buy,
                            ref double sell,
                            ref int evidence)
                        {
                            if (frame == null || frame.Quality <= 0 || weight <= 0)
                                return;
                
                            double scale =
                                weight / 10.0;
                
                            buy += frame.BullScore * scale;
                            sell += frame.BearScore * scale;
                
                            if (frame.Direction != 0)
                                evidence++;
                        }
        
        private string BuildReason(
                            Decision d,
                            int buyShare,
                            int sellShare)
                        {
                            return
                                (d.Direction == 1
                                    ? "BUY"
                                    : "SELL") +
                                " | CONF " +
                                d.Confidence +
                                " | EDGE " +
                                d.Edge +
                                " | SMART " +
                                d.SmartQuality +
                                " | MTF " +
                                d.TimeframeAgreement +
                                " | EVID " +
                                d.IndependentEvidence +
                                " | STRUCT " +
                                d.StructuralConfirmations +
                                " | RETEST " +
                                d.RetestQuality +
                                " | REGIME " +
                                d.Regime +
                                " | " +
                                buyShare +
                                "/" +
                                sellShare +
                                (string.IsNullOrWhiteSpace(d.BlockReason)
                                    ? ""
                                    : " | BLOCK " +
                                      d.BlockReason);
                        }
    }
}
