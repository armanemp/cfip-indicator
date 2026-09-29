using System;

namespace cAlgo
{
    internal sealed class DecisionScoreCalculator
    {
        public DecisionScoreSnapshot Calculate(DecisionInputSnapshot input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            double buy =
                input.M5Contribution.Bull +
                input.M15Contribution.Bull +
                input.M30Contribution.Bull +
                input.H1Contribution.Bull +
                input.H4Contribution.Bull +
                input.D1Contribution.Bull;

            double sell =
                input.M5Contribution.Bear +
                input.M15Contribution.Bear +
                input.M30Contribution.Bear +
                input.H1Contribution.Bear +
                input.H4Contribution.Bear +
                input.D1Contribution.Bear;

            if (input.SmartWeeklyContext)
            {
                buy += input.W1Contribution.Bull;
                sell += input.W1Contribution.Bear;
            }

            if (input.UseAdvancedConfluence)
            {
                buy += input.AdvancedConfluenceBuy;
                sell += input.AdvancedConfluenceSell;
            }

            if (input.UsePremiumDiscount)
            {
                if (input.PremiumDiscountBias == 1)
                    buy += 6;
                else if (input.PremiumDiscountBias == -1)
                    sell += 6;
            }

            // M1 is a trigger confirmation, not an independent directional vote.
            // Its closed-bar evidence is consumed by DecisionEvaluator after
            // the higher-timeframe directional consensus has been established.

            if (input.AdaptiveRegimeWeighting &&
                input.M5Frame != null)
            {
                // Frame scoring already contains the canonical regime-aware
                // indicator fusion. Do not re-add the same raw signals here.
                int fusionQuality =
                    NumericGuards.ClampInt(
                        input.M5Frame.IndicatorConfluenceQuality,
                        0,
                        100);

                int conflict =
                    NumericGuards.ClampInt(
                        input.M5Frame.IndicatorConflict,
                        0,
                        100);

                if (fusionQuality >= 72 &&
                    conflict < 38 &&
                    input.M5Frame.Direction != 0)
                {
                    double bonus =
                        Math.Min(
                            3.0,
                            (fusionQuality - 70) / 8.0);

                    if (input.M5Frame.Direction == 1)
                        buy += bonus;
                    else
                        sell += bonus;
                }

                if (conflict >= 50)
                {
                    // Strong cross-indicator conflict reduces directional
                    // certainty instead of creating a false extra vote.
                    if (buy >= sell)
                        buy -=
                            Math.Min(
                                3.0,
                                (conflict - 45) / 15.0);
                    else
                        sell -=
                            Math.Min(
                                3.0,
                                (conflict - 45) / 15.0);
                }
            }

            if (input.UseHistoricalChoppinessGuard &&
                input.M5Frame != null &&
                input.M15Frame != null &&
                input.M5Frame.Choppy &&
                input.M15Frame.Choppy)
            {
                buy *= 0.90;
                sell *= 0.90;
            }

            return new DecisionScoreSnapshot(buy, sell);
        }
    }
}
