using System;

namespace cAlgo
{
    internal readonly struct IndicatorEvidenceFusionInput
    {
        public string Regime { get; }
        public bool TrendBull { get; }
        public bool TrendBear { get; }
        public bool MomentumBull { get; }
        public bool MomentumBear { get; }
        public bool MacdBull { get; }
        public bool MacdBear { get; }
        public bool VwapBull { get; }
        public bool VwapBear { get; }
        public bool VolumeBull { get; }
        public bool VolumeBear { get; }
        public bool VolatilityBull { get; }
        public bool VolatilityBear { get; }
        public bool UseVolume { get; }
        public bool UseMacd { get; }
        public bool UseVwap { get; }
        public bool UseHealthyVolatility { get; }
        public int WaveTrendEvidenceWeight { get; }
        public double Adx { get; }
        public double AdxMinimum { get; }
        public double Rsi { get; }
        public double DmiBias { get; }
        public double EmaSlopeAtr { get; }
        public int WaveTrendDirection { get; }
        public int WaveTrendQuality { get; }
        public int MinimumWaveTrendQuality { get; }
        public int DivergenceDirection { get; }
        public int DivergenceQuality { get; }
        public bool WaveTrendBullCross { get; }
        public bool WaveTrendBearCross { get; }
        public bool WaveTrendOversold { get; }
        public bool WaveTrendOverbought { get; }
        public int OssBullVotes { get; }
        public int OssBearVotes { get; }
        public int OssIndicatorCount { get; }
        public int OssConfluenceWeight { get; }
        public int MinimumOssAgreement { get; }

        public IndicatorEvidenceFusionInput(
            string regime,
            bool trendBull,
            bool trendBear,
            bool momentumBull,
            bool momentumBear,
            bool macdBull,
            bool macdBear,
            bool vwapBull,
            bool vwapBear,
            bool volumeBull,
            bool volumeBear,
            bool volatilityBull,
            bool volatilityBear,
            bool useVolume,
            bool useMacd,
            bool useVwap,
            bool useHealthyVolatility,
            int waveTrendEvidenceWeight,
            double adx,
            double adxMinimum,
            double rsi,
            double dmiBias,
            double emaSlopeAtr,
            int waveTrendDirection,
            int waveTrendQuality,
            int minimumWaveTrendQuality,
            int divergenceDirection,
            int divergenceQuality,
            bool waveTrendBullCross,
            bool waveTrendBearCross,
            bool waveTrendOversold,
            bool waveTrendOverbought,
            int ossBullVotes,
            int ossBearVotes,
            int ossIndicatorCount,
            int ossConfluenceWeight,
            int minimumOssAgreement)
        {
            Regime =
                FrameRegimeResolutionRule.NormalizeFrameRegimeValue(
                    regime);
            TrendBull = trendBull;
            TrendBear = trendBear;
            MomentumBull = momentumBull;
            MomentumBear = momentumBear;
            MacdBull = macdBull;
            MacdBear = macdBear;
            VwapBull = vwapBull;
            VwapBear = vwapBear;
            VolumeBull = volumeBull;
            VolumeBear = volumeBear;
            VolatilityBull = volatilityBull;
            VolatilityBear = volatilityBear;
            UseVolume = useVolume;
            UseMacd = useMacd;
            UseVwap = useVwap;
            UseHealthyVolatility = useHealthyVolatility;
            WaveTrendEvidenceWeight = Math.Max(1, waveTrendEvidenceWeight);
            Adx = adx;
            AdxMinimum = Math.Max(1.0, adxMinimum);
            Rsi = rsi;
            DmiBias = dmiBias;
            EmaSlopeAtr = emaSlopeAtr;
            WaveTrendDirection = waveTrendDirection;
            WaveTrendQuality = waveTrendQuality;
            MinimumWaveTrendQuality = minimumWaveTrendQuality;
            DivergenceDirection = divergenceDirection;
            DivergenceQuality = Math.Max(0, divergenceQuality);
            WaveTrendBullCross = waveTrendBullCross;
            WaveTrendBearCross = waveTrendBearCross;
            WaveTrendOversold = waveTrendOversold;
            WaveTrendOverbought = waveTrendOverbought;
            OssBullVotes = Math.Max(0, ossBullVotes);
            OssBearVotes = Math.Max(0, ossBearVotes);
            OssIndicatorCount = Math.Max(0, ossIndicatorCount);
            OssConfluenceWeight = Math.Max(1, ossConfluenceWeight);
            MinimumOssAgreement = Math.Max(1, minimumOssAgreement);
        }
    }

    internal readonly struct IndicatorEvidenceFusionResult
    {
        public int BullBonus { get; }
        public int BearBonus { get; }
        public int Conflict { get; }
        public int Quality { get; }

        public IndicatorEvidenceFusionResult(
            int bullBonus,
            int bearBonus,
            int conflict,
            int quality)
        {
            BullBonus = NumericGuards.ClampInt(bullBonus, 0, 30);
            BearBonus = NumericGuards.ClampInt(bearBonus, 0, 30);
            Conflict = NumericGuards.ClampInt(conflict, 0, 100);
            Quality = NumericGuards.ClampInt(quality, 0, 100);
        }
    }

    internal static class IndicatorEvidenceFusionRule
    {
        public static IndicatorEvidenceFusionResult Evaluate(
            IndicatorEvidenceFusionInput input)
        {
            double trendBull = 0;
            double trendBear = 0;
            double momentumBull = 0;
            double momentumBear = 0;
            double contextBull = 0;
            double contextBear = 0;

            if (input.TrendBull) trendBull += 2.0;
            if (input.TrendBear) trendBear += 2.0;

            if (input.Adx >= input.AdxMinimum)
            {
                if (input.DmiBias > 0) trendBull += 1.5;
                else if (input.DmiBias < 0) trendBear += 1.5;
            }

            if (input.EmaSlopeAtr >= 0.08) trendBull += 1.25;
            else if (input.EmaSlopeAtr <= -0.08) trendBear += 1.25;

            if (input.MomentumBull) momentumBull += 1.5;
            if (input.MomentumBear) momentumBear += 1.5;
            if (input.UseMacd)
            {
                if (input.MacdBull) momentumBull += 1.25;
                if (input.MacdBear) momentumBear += 1.25;
            }

            if (input.Rsi >= 55) momentumBull += 0.75;
            else if (input.Rsi <= 45) momentumBear += 0.75;

            if (WaveTrendEvidenceRule.MeetsMinimumQuality(
                    input.WaveTrendQuality,
                    input.MinimumWaveTrendQuality))
            {
                double waveWeight =
                    Math.Min(
                        2.75,
                        1.75 +
                        input.WaveTrendEvidenceWeight * 0.125);

                if (input.WaveTrendDirection == 1)
                    momentumBull += waveWeight;
                else if (input.WaveTrendDirection == -1)
                    momentumBear += waveWeight;

                if (input.WaveTrendBullCross)
                    momentumBull += 1.0;
                if (input.WaveTrendBearCross)
                    momentumBear += 1.0;

                if (input.WaveTrendOversold &&
                    input.WaveTrendDirection == 1)
                    momentumBull += 1.25;

                if (input.WaveTrendOverbought &&
                    input.WaveTrendDirection == -1)
                    momentumBear += 1.25;
            }

            if (input.UseVwap)
            {
                if (input.VwapBull) contextBull += 1.0;
                if (input.VwapBear) contextBear += 1.0;
            }

            if (input.UseVolume)
            {
                if (input.VolumeBull) contextBull += 1.25;
                if (input.VolumeBear) contextBear += 1.25;
            }

            if (input.UseHealthyVolatility)
            {
                if (input.VolatilityBull) contextBull += 0.75;
                if (input.VolatilityBear) contextBear += 0.75;
            }

            ApplyRegimeWeights(
                input.Regime,
                ref trendBull,
                ref trendBear,
                ref momentumBull,
                ref momentumBear,
                ref contextBull,
                ref contextBear);

            if (DivergenceThresholdRule.MeetsStrongConflictQuality(
                    input.DivergenceQuality))
            {
                if (input.DivergenceDirection == -1)
                    momentumBull =
                        Math.Max(
                            0,
                            momentumBull - 2.0);
                else if (input.DivergenceDirection == 1)
                    momentumBear =
                        Math.Max(
                            0,
                            momentumBear - 2.0);
            }

            if (input.OssIndicatorCount >= input.MinimumOssAgreement)
            {
                int maxVotes =
                    Math.Max(
                        input.OssBullVotes,
                        input.OssBearVotes);

                double purity =
                    maxVotes /
                    (double)Math.Max(
                        1,
                        input.OssIndicatorCount);

                if (purity >= 0.60)
                {
                    double ossScale =
                        Math.Max(
                            0.50,
                            Math.Min(
                                1.50,
                                input.OssConfluenceWeight /
                                4.0));

                    double ossBonus =
                        Math.Min(
                            3.0,
                            (maxVotes -
                             input.MinimumOssAgreement +
                             1) * 0.75) *
                        ossScale;

                    if (input.OssBullVotes > input.OssBearVotes)
                        contextBull += ossBonus;
                    else if (input.OssBearVotes > input.OssBullVotes)
                        contextBear += ossBonus;
                }
            }

            double totalBull =
                trendBull +
                momentumBull +
                contextBull;

            double totalBear =
                trendBear +
                momentumBear +
                contextBear;

            double directionalTotal =
                Math.Max(
                    1.0,
                    totalBull +
                    totalBear);

            double dominant =
                Math.Max(
                    totalBull,
                    totalBear);

            double weaker =
                Math.Min(
                    totalBull,
                    totalBear);

            int conflict =
                (int)Math.Round(
                    100.0 *
                    weaker /
                    directionalTotal);

            double dominantQuality =
                100.0 *
                dominant /
                directionalTotal;

            int quality =
                (int)Math.Round(
                    dominantQuality -
                    conflict * 0.25);

            int dominantBonus =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        Math.Min(
                            18.0,
                            dominant)),
                    0,
                    18);

            if (totalBull >= totalBear)
            {
                return new IndicatorEvidenceFusionResult(
                    dominantBonus,
                    NumericGuards.ClampInt(
                        (int)Math.Round(
                            Math.Min(
                                10.0,
                                totalBear * 0.55)),
                        0,
                        10),
                    conflict,
                    quality);
            }

            return new IndicatorEvidenceFusionResult(
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        Math.Min(
                            10.0,
                            totalBull * 0.55)),
                    0,
                    10),
                dominantBonus,
                conflict,
                quality);
        }

        private static void ApplyRegimeWeights(
            string regime,
            ref double trendBull,
            ref double trendBear,
            ref double momentumBull,
            ref double momentumBear,
            ref double contextBull,
            ref double contextBear)
        {
            // UNKNOWN is intentionally neutral. A frame without a confirmed
            // regime must not receive a directional regime preference.
            double trendWeight = 1.0;
            double momentumWeight = 1.0;
            double contextWeight = 1.0;

            if (string.Equals(
                    regime,
                    MarketRegimeIdentity.Trend,
                    StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 1.30;
                momentumWeight = 1.05;
                contextWeight = 0.90;
            }
            else if (string.Equals(
                         regime,
                         MarketRegimeIdentity.Expansion,
                         StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 1.05;
                momentumWeight = 1.25;
                contextWeight = 1.20;
            }
            else if (string.Equals(
                         regime,
                         MarketRegimeIdentity.Range,
                         StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 0.60;
                momentumWeight = 1.20;
                contextWeight = 1.15;
            }
            else if (string.Equals(
                         regime,
                         MarketRegimeIdentity.Transition,
                         StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 0.80;
                momentumWeight = 0.95;
                contextWeight = 0.85;
            }
            else if (string.Equals(
                         regime,
                         MarketRegimeIdentity.HighVolatility,
                         StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 0.90;
                momentumWeight = 1.20;
                contextWeight = 1.15;
            }
            else if (string.Equals(
                         regime,
                         MarketRegimeIdentity.Compression,
                         StringComparison.OrdinalIgnoreCase))
            {
                trendWeight = 0.50;
                momentumWeight = 0.75;
                contextWeight = 0.50;
            }

            trendBull *= trendWeight;
            trendBear *= trendWeight;
            momentumBull *= momentumWeight;
            momentumBear *= momentumWeight;
            contextBull *= contextWeight;
            contextBear *= contextWeight;
        }
    }
}
