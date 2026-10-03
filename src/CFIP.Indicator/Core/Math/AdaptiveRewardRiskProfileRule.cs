using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical adaptive reward/risk profile.
    ///
    /// RR is an observed consequence of the selected stop and target geometry.
    /// This rule derives the required reward from current risk-in-ATR, regime,
    /// opportunity lane and setup quality instead of imposing one fixed RR.
    /// </summary>
    internal readonly struct AdaptiveRewardRiskProfile
    {
        public double RequiredTp1RR { get; }
        public double RequiredTp2RR { get; }
        public double RequiredTp3RR { get; }
        public double RequiredTp4RR { get; }
        public double RequiredRewardAtr { get; }
        public double StageRewardAtr { get; }
        public double MaximumExtensionAtr { get; }

        public AdaptiveRewardRiskProfile(
            double tp1,
            double tp2,
            double tp3,
            double tp4,
            double rewardAtr,
            double stageRewardAtr,
            double maximumExtensionAtr)
        {
            RequiredTp1RR = Sanitize(tp1);
            RequiredTp2RR = Sanitize(tp2);
            RequiredTp3RR = Sanitize(tp3);
            RequiredTp4RR = Sanitize(tp4);
            RequiredRewardAtr = Sanitize(rewardAtr);
            StageRewardAtr = Sanitize(stageRewardAtr);
            MaximumExtensionAtr = Sanitize(maximumExtensionAtr);
        }

        private static double Sanitize(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value)
                ? 0
                : Math.Max(0, value);
        }
    }

    internal static class AdaptiveRewardRiskProfileRule
    {
        private const double MinimumRiskAtr = 0.20;
        private const double MinimumRewardAtr = 0.90;
        private const double MaximumRewardAtr = 3.20;
        private const double MinimumStageAtr = 0.30;
        private const double MaximumStageAtr = 0.90;

        public static AdaptiveRewardRiskProfile Resolve(
            string regime,
            OpportunityLane lane,
            double riskAtr,
            int confidence,
            int smartQuality,
            int targetQuality,
            int structuralQuality,
            double maximumExtensionAtr)
        {
            double normalizedRisk =
                Clamp(
                    riskAtr,
                    MinimumRiskAtr,
                    3.50);

            double quality =
                Clamp(
                    (Math.Max(0, confidence) * 0.45) +
                    (Math.Max(0, smartQuality) * 0.30) +
                    (Math.Max(0, targetQuality) * 0.15) +
                    (Math.Max(0, structuralQuality) * 0.10),
                    0,
                    100);

            double rewardAtr =
                1.05 +
                Math.Min(1.10, normalizedRisk * 0.45);

            string normalizedRegime =
                (regime ?? string.Empty).Trim().ToUpperInvariant();

            switch (normalizedRegime)
            {
                case MarketRegimeIdentity.Range:
                    rewardAtr += 0.20;
                    break;

                case MarketRegimeIdentity.Compression:
                    rewardAtr += 0.30;
                    break;

                case MarketRegimeIdentity.Expansion:
                    rewardAtr += 0.05;
                    break;

                case MarketRegimeIdentity.Transition:
                    rewardAtr += 0.10;
                    break;
            }

            if (lane == OpportunityLane.CounterHtfTactical)
                rewardAtr += 0.20;
            else if (lane == OpportunityLane.MicroReaction)
                rewardAtr -= 0.05;

            // High-quality entries can tolerate a slightly more ambitious
            // reward path; weak setups are never rescued by a generous RR.
            rewardAtr +=
                (quality - 70) *
                0.003;

            rewardAtr =
                Clamp(
                    rewardAtr,
                    MinimumRewardAtr,
                    MaximumRewardAtr);

            double stageRewardAtr =
                Clamp(
                    0.35 +
                    normalizedRisk * 0.10 +
                    (100 - quality) * 0.0015,
                    MinimumStageAtr,
                    MaximumStageAtr);

            double tp1 =
                rewardAtr /
                normalizedRisk;

            double tp2 =
                (rewardAtr + stageRewardAtr) /
                normalizedRisk;

            double tp3 =
                (rewardAtr + stageRewardAtr * 2.0) /
                normalizedRisk;

            double tp4 =
                (rewardAtr + stageRewardAtr * 3.0) /
                normalizedRisk;

            double extension =
                Math.Max(
                    rewardAtr + stageRewardAtr * 2.0,
                    Math.Max(
                        1.0,
                        maximumExtensionAtr));

            // Quality and structural confirmation can justify reaching further,
            // but the caller's configured extension remains a hard safety ceiling.
            double qualityExtension =
                extension +
                Math.Max(0, quality - 75) * 0.006 +
                Math.Max(0, structuralQuality - 60) * 0.004;

            extension =
                Math.Min(
                    Math.Max(1.0, maximumExtensionAtr),
                    qualityExtension);

            return new AdaptiveRewardRiskProfile(
                tp1,
                tp2,
                tp3,
                tp4,
                rewardAtr,
                stageRewardAtr,
                extension);
        }

        public static double ResolveRequiredTp1RR(
            string regime,
            OpportunityLane lane,
            double riskAtr,
            int confidence,
            int smartQuality,
            int targetQuality,
            int structuralQuality,
            double maximumExtensionAtr)
        {
            return Resolve(
                regime,
                lane,
                riskAtr,
                confidence,
                smartQuality,
                targetQuality,
                structuralQuality,
                maximumExtensionAtr).RequiredTp1RR;
        }

        private static double Clamp(
            double value,
            double minimum,
            double maximum)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return minimum;

            return Math.Max(
                minimum,
                Math.Min(
                    maximum,
                    value));
        }
    }
}
