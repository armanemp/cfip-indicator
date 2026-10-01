using System;

namespace cAlgo
{
    /// <summary>
    /// Single semantic owner for divergence thresholds and scoring constants.
    /// Values mirror the existing production behavior; this phase centralizes
    /// them rather than tuning them without replay/outcome evidence.
    /// </summary>
    internal static class DivergenceThresholdRule
    {
        public const int MinimumQuality = 55;
        public const int ConflictQualityMargin = 10;
        public const int StrongConflictQuality = 70;

        public const double RegularPriceAtr = 0.10;
        public const double HiddenPriceAtr = 0.08;
        public const double RegularRsiDelta = 2.0;
        public const double HiddenRsiDelta = 2.0;
        public const double RegularWaveDelta = 1.50;
        public const double HiddenWaveDelta = 1.50;

        public const int RecentBoostBarsStrong = 12;
        public const int RecentBoostBarsModerate = 20;
        public const int RecentBoostStrong = 10;
        public const int RecentBoostModerate = 5;

        public const int QualityBase = 45;
        public const int PriceExcursionContributionCap = 22;
        public const double PriceExcursionContributionPerAtr = 20.0;
        public const int OscillatorAgreementContribution = 9;
        public const int RegularTypeBonus = 7;
        public const int HiddenTypeBonus = 3;

        public static bool MeetsStrongConflictQuality(
            int quality)
        {
            return quality >= StrongConflictQuality;
        }

        public static double MinimumRegularPriceExcursion(double atr)
        {
            return Math.Max(
                0.10,
                atr * RegularPriceAtr);
        }

        public static double MinimumHiddenPriceExcursion(double atr)
        {
            return Math.Max(
                0.08,
                atr * HiddenPriceAtr);
        }

        public static int ResolveRecentBoost(
            int index,
            int newerIndex)
        {
            if (newerIndex >= index - RecentBoostBarsStrong)
                return RecentBoostStrong;

            return newerIndex >= index - RecentBoostBarsModerate
                ? RecentBoostModerate
                : 0;
        }

        public static int CalculateQuality(
            double priceExcursionAtr,
            int oscillatorAgreement,
            bool hidden,
            int recentBoost)
        {
            if (double.IsNaN(priceExcursionAtr) ||
                double.IsInfinity(priceExcursionAtr) ||
                priceExcursionAtr < 0 ||
                oscillatorAgreement < 0)
                return 0;

            int quality =
                QualityBase +
                Math.Min(
                    PriceExcursionContributionCap,
                    (int)Math.Round(
                        priceExcursionAtr *
                        PriceExcursionContributionPerAtr)) +
                oscillatorAgreement *
                OscillatorAgreementContribution +
                (hidden
                    ? HiddenTypeBonus
                    : RegularTypeBonus) +
                Math.Max(
                    0,
                    recentBoost);

            return Math.Max(
                0,
                Math.Min(
                    100,
                    quality));
        }
    }
}
