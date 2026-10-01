using System;

namespace cAlgo
{
    internal sealed class DecisionScoreCalculator
    {
        public DecisionScoreSnapshot Calculate(DecisionInputSnapshot input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return Calculate(
                new DecisionScoreInput(
                    input.M5Contribution,
                    input.M15Contribution,
                    input.M30Contribution,
                    input.H1Contribution,
                    input.H4Contribution,
                    input.D1Contribution,
                    input.W1Contribution,
                    input.SmartWeeklyContext,
                    input.UseAdvancedConfluence,
                    input.AdvancedConfluenceBuy,
                    input.AdvancedConfluenceSell,
                    input.UsePremiumDiscount,
                    input.PremiumDiscountBias,
                    input.AdaptiveRegimeWeighting,
                    input.UseHistoricalChoppinessGuard,
                    input.M5Frame == null ? 0 : input.M5Frame.Direction,
                    input.M5Frame == null ? 0 : input.M5Frame.IndicatorConfluenceQuality,
                    input.M5Frame == null ? 0 : input.M5Frame.IndicatorConflict,
                    input.M5Frame != null && input.M5Frame.Choppy,
                    input.M15Frame != null && input.M15Frame.Choppy));
        }

        public DecisionScoreSnapshot Calculate(DecisionScoreInput input)
        {
            double m5Bull = SafeNonNegative(input.M5Contribution.Bull);
            double m5Bear = SafeNonNegative(input.M5Contribution.Bear);
            double m15Bull = SafeNonNegative(input.M15Contribution.Bull);
            double m15Bear = SafeNonNegative(input.M15Contribution.Bear);
            double m30Bull = SafeNonNegative(input.M30Contribution.Bull);
            double m30Bear = SafeNonNegative(input.M30Contribution.Bear);
            double h1Bull = SafeNonNegative(input.H1Contribution.Bull);
            double h1Bear = SafeNonNegative(input.H1Contribution.Bear);
            double h4Bull = SafeNonNegative(input.H4Contribution.Bull);
            double h4Bear = SafeNonNegative(input.H4Contribution.Bear);
            double d1Bull = SafeNonNegative(input.D1Contribution.Bull);
            double d1Bear = SafeNonNegative(input.D1Contribution.Bear);
            double w1Bull =
                input.SmartWeeklyContext
                    ? SafeNonNegative(input.W1Contribution.Bull)
                    : 0;
            double w1Bear =
                input.SmartWeeklyContext
                    ? SafeNonNegative(input.W1Contribution.Bear)
                    : 0;

            double advancedBuy =
                input.UseAdvancedConfluence
                    ? SafeNonNegative(input.AdvancedConfluenceBuy)
                    : 0;
            double advancedSell =
                input.UseAdvancedConfluence
                    ? SafeNonNegative(input.AdvancedConfluenceSell)
                    : 0;

            double premiumBuy = 0;
            double premiumSell = 0;

            if (input.UsePremiumDiscount)
            {
                if (input.PremiumDiscountBias == 1)
                    premiumBuy = 6;
                else if (input.PremiumDiscountBias == -1)
                    premiumSell = 6;
            }

            double adaptiveBuy = 0;
            double adaptiveSell = 0;

            double preConflictBuy =
                m5Bull +
                m15Bull +
                m30Bull +
                h1Bull +
                h4Bull +
                d1Bull +
                w1Bull +
                advancedBuy +
                premiumBuy;

            double preConflictSell =
                m5Bear +
                m15Bear +
                m30Bear +
                h1Bear +
                h4Bear +
                d1Bear +
                w1Bear +
                advancedSell +
                premiumSell;

            double conflictPenaltyBuy = 0;
            double conflictPenaltySell = 0;

            // M1 is a trigger confirmation, not an independent directional vote.
            // Its closed-bar evidence is consumed by DecisionEvaluator after
            // higher-timeframe directional consensus has been established.

            if (input.AdaptiveRegimeWeighting)
            {
                int fusionQuality =
                    NumericGuards.ClampInt(
                        input.M5IndicatorConfluenceQuality,
                        0,
                        100);

                int conflict =
                    NumericGuards.ClampInt(
                        input.M5IndicatorConflict,
                        0,
                        100);

                if (fusionQuality >= 72 &&
                    conflict < 38 &&
                    input.M5Direction != 0)
                {
                    double bonus =
                        Math.Min(
                            3.0,
                            (fusionQuality - 70) / 8.0);

                    if (input.M5Direction == 1)
                        adaptiveBuy = bonus;
                    else
                        adaptiveSell = bonus;
                }

                preConflictBuy += adaptiveBuy;
                preConflictSell += adaptiveSell;

                if (conflict >= 50)
                {
                    double penalty =
                        Math.Min(
                            3.0,
                            (conflict - 45) / 15.0);

                    if (preConflictBuy > preConflictSell)
                        conflictPenaltyBuy = penalty;
                    else if (preConflictSell > preConflictBuy)
                        conflictPenaltySell = penalty;
                    else
                    {
                        // A perfectly balanced conflict has no direction to
                        // penalize. Apply the same bounded reduction to both.
                        conflictPenaltyBuy = penalty;
                        conflictPenaltySell = penalty;
                    }
                }
            }

            double buy =
                preConflictBuy -
                conflictPenaltyBuy;

            double sell =
                preConflictSell -
                conflictPenaltySell;

            double choppinessFactor =
                ResolveChoppinessFactor(
                    input.UseHistoricalChoppinessGuard,
                    input.M5Choppy,
                    input.M15Choppy);

            buy *= choppinessFactor;
            sell *= choppinessFactor;

            return new DecisionScoreSnapshot(
                buy,
                sell,
                m5Bull,
                m5Bear,
                m15Bull,
                m15Bear,
                m30Bull,
                m30Bear,
                h1Bull,
                h1Bear,
                h4Bull,
                h4Bear,
                d1Bull,
                d1Bear,
                w1Bull,
                w1Bear,
                advancedBuy,
                advancedSell,
                premiumBuy,
                premiumSell,
                adaptiveBuy,
                adaptiveSell,
                conflictPenaltyBuy,
                conflictPenaltySell,
                choppinessFactor);
        }

        internal static double ResolveChoppinessFactor(
            bool guardEnabled,
            bool m5Choppy,
            bool m15Choppy)
        {
            return guardEnabled && m5Choppy && m15Choppy
                ? 0.90
                : 1.0;
        }

        private static double SafeNonNegative(double value)
        {
            return
                NumericGuards.IsFiniteValue(value) &&
                value > 0
                    ? value
                    : 0;
        }
    }
}
