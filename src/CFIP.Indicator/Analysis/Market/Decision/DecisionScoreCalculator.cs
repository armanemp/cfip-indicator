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

            if (input.UseM1Trigger &&
                input.M5Frame != null)
            {
                if (input.M5Frame.Direction == 1)
                    buy += 3;
                else if (input.M5Frame.Direction == -1)
                    sell += 3;
            }

            if (input.AdaptiveRegimeWeighting &&
                input.M5Frame != null)
            {
                if (string.Equals(input.Regime, "EXPANSION", StringComparison.OrdinalIgnoreCase))
                {
                    if (input.M5Frame.DisplacementBull)
                        buy += 4;
                    if (input.M5Frame.DisplacementBear)
                        sell += 4;
                    if (input.M5Frame.VolumeBull)
                        buy += 2;
                    if (input.M5Frame.VolumeBear)
                        sell += 2;
                }
                else if (string.Equals(input.Regime, "RANGE", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(input.Regime, "TRANSITION", StringComparison.OrdinalIgnoreCase))
                {
                    if (input.M5Frame.LiquidityBull)
                        buy += 3;
                    if (input.M5Frame.LiquidityBear)
                        sell += 3;
                    if (input.M5Frame.FvgBull)
                        buy += 2;
                    if (input.M5Frame.FvgBear)
                        sell += 2;
                    if (input.M5Frame.ObBull)
                        buy += 2;
                    if (input.M5Frame.ObBear)
                        sell += 2;
                }
                else if (string.Equals(input.Regime, "COMPRESSION", StringComparison.OrdinalIgnoreCase))
                {
                    buy *= 0.95;
                    sell *= 0.95;
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