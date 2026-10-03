using System;

namespace cAlgo
{
    internal static class MtfEarlyPredictionFusionRule
    {
        public static EarlyPredictionScoreResult Evaluate(
            Frame[] frames,
            double[] weights,
            bool useLiquidityForecast,
            bool liquidityBull,
            bool liquidityBear,
            bool volumeBull,
            bool volumeBear,
            bool vwapBull,
            bool vwapBear)
        {
            double buy = 0;
            double sell = 0;

            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    Frame frame = frames[i];

                    if (frame == null ||
                        frame.Quality <= 0)
                        continue;

                    // M1 remains a precision/confirmation layer. It informs
                    // the current-entry trigger but never becomes the future
                    // directional forecast authority.
                    if (i == 0)
                        continue;

                    double weight =
                        weights != null && i < weights.Length
                            ? Math.Max(0, weights[i])
                            : 0;

                    if (weight <= 0)
                        continue;

                    double quality =
                        Math.Max(
                            0,
                            Math.Min(
                                100,
                                frame.Quality)) / 100.0;

                    buy +=
                        Math.Max(
                            0,
                            frame.BullScore) *
                        quality *
                        weight;

                    sell +=
                        Math.Max(
                            0,
                            frame.BearScore) *
                        quality *
                        weight;
                }
            }

            if (useLiquidityForecast)
            {
                if (liquidityBull)
                    buy += EarlyPredictionScoreRule.LiquidityForecastBonus;

                if (liquidityBear)
                    sell += EarlyPredictionScoreRule.LiquidityForecastBonus;
            }

            if (volumeBull)
                buy += EarlyPredictionScoreRule.VolumeBonus;

            if (volumeBear)
                sell += EarlyPredictionScoreRule.VolumeBonus;

            if (vwapBull)
                buy += EarlyPredictionScoreRule.VwapBonus;

            if (vwapBear)
                sell += EarlyPredictionScoreRule.VwapBonus;

            return EarlyPredictionScoreRule.Finalize(
                buy,
                sell);
        }
    }
}