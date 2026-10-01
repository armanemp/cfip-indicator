using System;

namespace cAlgo
{
    internal static class MarketRegimeClassifier
    {
        public static string Classify(
            MarketRegimeClassificationInput input,
            double compressionAtrRatio,
            double expansionAtrRatio,
            double highVolatilityAtrRatio,
            double rangeChoppinessThreshold,
            double rangeEfficiencyThreshold,
            double microRangeWidthAtr,
            double microRangeReturnAtr,
            double minimumTrendAdx,
            double minimumTransitionAdx,
            double minimumTrendEfficiency,
            double trendChoppinessThreshold,
            double minimumTrendSpreadAtr)
        {
            if (input == null)
                return MarketRegimeIdentity.Unknown;

            bool microRange =
                input.RangeWidthAtr <=
                    Math.Max(
                        1.0,
                        microRangeWidthAtr) &&
                input.ReturnAtr <=
                    Math.Max(
                        0.20,
                        microRangeReturnAtr) &&
                input.Choppiness >=
                    Math.Max(
                        55,
                        rangeChoppinessThreshold);

            if ((input.AtrRatio <=
                    Math.Min(
                        0.95,
                        compressionAtrRatio) &&
                 input.Choppiness >=
                    Math.Max(
                        52,
                        rangeChoppinessThreshold) &&
                 input.RangeEfficiency <=
                    Math.Max(
                        0.35,
                        rangeEfficiencyThreshold)) ||
                (microRange &&
                 input.Adx <
                    Math.Max(
                        20,
                        minimumTrendAdx) &&
                 input.RangeEfficiency <=
                    Math.Max(
                        0.35,
                        rangeEfficiencyThreshold)))
            {
                return MarketRegimeIdentity.Compression;
            }

            if (input.AtrRatio >=
                    Math.Max(
                        1.45,
                        highVolatilityAtrRatio) &&
                (input.Adx <
                    Math.Max(
                        18,
                        minimumTrendAdx) ||
                 input.Choppiness >=
                    Math.Max(
                        55,
                        rangeChoppinessThreshold)))
            {
                return MarketRegimeIdentity.HighVolatility;
            }

            if (input.Adx <
                    Math.Max(
                        20,
                        minimumTrendAdx) &&
                input.Choppiness >=
                    Math.Max(
                        55,
                        rangeChoppinessThreshold) &&
                input.RangeEfficiency <=
                    Math.Max(
                        0.30,
                        rangeEfficiencyThreshold))
            {
                return MarketRegimeIdentity.Range;
            }

            if (input.AtrRatio >=
                    Math.Max(
                        1.25,
                        expansionAtrRatio) &&
                (input.Adx >=
                    Math.Max(
                        18,
                        minimumTransitionAdx) ||
                 input.RangeEfficiency >=
                    Math.Max(
                        0.30,
                        minimumTrendEfficiency)))
            {
                return MarketRegimeIdentity.Expansion;
            }

            if (input.Adx >=
                    Math.Max(
                        20,
                        minimumTrendAdx) &&
                input.Choppiness <=
                    Math.Min(
                        60,
                        trendChoppinessThreshold) &&
                input.RangeEfficiency >=
                    Math.Max(
                        0.25,
                        minimumTrendEfficiency) &&
                input.EmaSpreadAtr >=
                    Math.Max(
                        0.20,
                        minimumTrendSpreadAtr))
            {
                return MarketRegimeIdentity.Trend;
            }

            return MarketRegimeIdentity.Transition;
        }

        public static int Quality(
            string regime,
            MarketRegimeClassificationInput input)
        {
            if (input == null)
                return 35;

            int quality;

            switch (regime)
            {
                case MarketRegimeIdentity.Trend:
                    quality =
                        (int)Math.Round(
                            48 +
                            Math.Min(
                                35,
                                input.Adx * 1.25) +
                            input.RangeEfficiency * 18 -
                            Math.Max(
                                0,
                                input.Choppiness - 38) * 0.40 +
                            Math.Min(
                                12,
                                input.EmaSpreadAtr * 8));
                    break;

                case MarketRegimeIdentity.Expansion:
                    quality =
                        (int)Math.Round(
                            55 +
                            Math.Min(
                                30,
                                input.Adx * 0.90) +
                            input.RangeEfficiency * 18 -
                            Math.Max(
                                0,
                                input.Choppiness - 45) * 0.50);
                    break;

                case MarketRegimeIdentity.Range:
                    quality =
                        (int)Math.Round(
                            32 +
                            Math.Max(
                                0,
                                18 - input.Adx) +
                            Math.Max(
                                0,
                                0.35 -
                                input.RangeEfficiency) * 15);
                    break;

                case MarketRegimeIdentity.Compression:
                    quality = 22;
                    break;

                case MarketRegimeIdentity.HighVolatility:
                    quality =
                        (int)Math.Round(
                            34 +
                            Math.Min(
                                20,
                                input.Adx) +
                            input.RangeEfficiency * 10 -
                            Math.Max(
                                0,
                                input.AtrRatio - 1.50) * 12);
                    break;

                case MarketRegimeIdentity.Transition:
                    quality =
                        (int)Math.Round(
                            40 +
                            Math.Min(
                                15,
                                input.Adx * 0.60) +
                            input.RangeEfficiency * 10);
                    break;

                default:
                    quality = 35;
                    break;
            }

            return Math.Max(
                0,
                Math.Min(
                    100,
                    quality));
        }
    }
}