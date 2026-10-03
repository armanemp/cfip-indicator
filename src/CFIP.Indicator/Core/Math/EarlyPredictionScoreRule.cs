using System;

namespace cAlgo
{
    internal readonly struct EarlyPredictionScoreResult
    {
        public double BuyStrength { get; }
        public double SellStrength { get; }
        public double TotalStrength { get; }
        public double AbsoluteStrength { get; }
        public int DirectionalShare { get; }
        public int Direction { get; }

        public EarlyPredictionScoreResult(
            double buyStrength,
            double sellStrength,
            double totalStrength,
            double absoluteStrength,
            int directionalShare,
            int direction)
        {
            BuyStrength = buyStrength;
            SellStrength = sellStrength;
            TotalStrength = totalStrength;
            AbsoluteStrength = absoluteStrength;
            DirectionalShare = directionalShare;
            Direction = direction;
        }
    }

    // Canonical early-prediction evidence fusion.
    // Weights/bonuses are named here so the prediction engine has one owner for
    // scoring semantics and cannot silently drift through scattered literals.
    internal static class EarlyPredictionScoreRule
    {
        public const double M5Weight = 0.55;
        public const double M15Weight = 0.45;
        public const double LiquidityForecastBonus = 8.0;
        public const double VolumeBonus = 2.0;
        public const double VwapBonus = 1.0;

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

                    double weight =
                        weights != null && i < weights.Length
                            ? Math.Max(0, weights[i])
                            : 0;

                    if (i == 0)
                        weight = 0; // M1 remains optional entry confirmation.

                    if (i == 7 &&
                        !useLiquidityForecast &&
                        weight <= 0)
                        continue;

                    if (weight <= 0)
                        continue;

                    double q =
                        Math.Max(
                            0,
                            Math.Min(
                                100,
                                frame.Quality)) / 100.0;

                    buy += Math.Max(0, frame.BullScore) * q * weight;
                    sell += Math.Max(0, frame.BearScore) * q * weight;
                }
            }

            if (useLiquidityForecast)
            {
                if (liquidityBull)
                    buy += LiquidityForecastBonus;

                if (liquidityBear)
                    sell += LiquidityForecastBonus;
            }

            if (volumeBull)
                buy += VolumeBonus;

            if (volumeBear)
                sell += VolumeBonus;

            if (vwapBull)
                buy += VwapBonus;

            if (vwapBear)
                sell += VwapBonus;

            double total = buy + sell;

            if (!NumericGuards.IsFiniteValue(total) || total <= 0)
                return new EarlyPredictionScoreResult(0, 0, 0, 0, 0, 0);

            double strongest = Math.Max(buy, sell);

            int share =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        100.0 * strongest / total),
                    0,
                    100);

            int direction =
                buy > sell ? 1 : sell > buy ? -1 : 0;

            return new EarlyPredictionScoreResult(
                buy,
                sell,
                total,
                Math.Max(0, strongest),
                share,
                direction);
        }

        public static EarlyPredictionScoreResult Evaluate(
            double m5Bull,
            double m5Bear,
            double m15Bull,
            double m15Bear,
            bool useLiquidityForecast,
            bool liquidityBull,
            bool liquidityBear,
            bool volumeBull,
            bool volumeBear,
            bool vwapBull,
            bool vwapBear)
        {
            double buy =
                Score(m5Bull) * M5Weight +
                Score(m15Bull) * M15Weight;

            double sell =
                Score(m5Bear) * M5Weight +
                Score(m15Bear) * M15Weight;

            if (useLiquidityForecast)
            {
                if (liquidityBull)
                    buy += LiquidityForecastBonus;

                if (liquidityBear)
                    sell += LiquidityForecastBonus;
            }

            if (volumeBull)
                buy += VolumeBonus;

            if (volumeBear)
                sell += VolumeBonus;

            if (vwapBull)
                buy += VwapBonus;

            if (vwapBear)
                sell += VwapBonus;

            double total =
                buy + sell;

            if (!NumericGuards.IsFiniteValue(total) ||
                total <= 0)
            {
                return new EarlyPredictionScoreResult(
                    0,
                    0,
                    0,
                    0,
                    0,
                    0);
            }

            double strongest =
                Math.Max(
                    buy,
                    sell);

            int share =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        100.0 *
                        strongest /
                        total),
                    0,
                    100);

            int direction =
                buy > sell
                    ? 1
                    : sell > buy
                        ? -1
                        : 0;

            return new EarlyPredictionScoreResult(
                buy,
                sell,
                total,
                Math.Max(0, strongest),
                share,
                direction);
        }

        private static double Score(
            double value)
        {
            return NumericGuards.IsFiniteValue(value)
                ? value
                : 0;
        }
    }
}
