using System;

namespace cAlgo
{
    internal sealed class DecisionEvaluator
    {
        public Decision Evaluate(DecisionInputSnapshot input)
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

            double temperature = Math.Max(1.0, input.SmartScoreTemperature);
            double centered = (buy - sell) / temperature;

            double expBuy = Math.Exp(NumericGuards.Clamp(centered, -12, 12));
            double expSell = Math.Exp(NumericGuards.Clamp(-centered, -12, 12));
            double total = Math.Max(1e-9, expBuy + expSell);

            int buyShare = NumericGuards.ClampInt(
                (int)Math.Round(100.0 * expBuy / total),
                0,
                100);
            int sellShare = 100 - buyShare;

            Decision d = new Decision
            {
                BuyShare = buyShare,
                SellShare = sellShare,
                Regime = input.Regime
            };

            int strongestShare = Math.Max(buyShare, sellShare);

            d.Direction =
                strongestShare >= Math.Max(50, input.MinimumSmartDirectionShare)
                    ? (buyShare >= sellShare ? 1 : -1)
                    : 0;

            d.Edge = Math.Abs(buyShare - sellShare);

            d.TimeframeAgreement =
                input.TimeframeAgreement == null
                    ? 0
                    : input.TimeframeAgreement(
                        d.Direction,
                        input.Reference);

            d.IndependentEvidence =
                input.IndependentEvidence == null
                    ? 0
                    : input.IndependentEvidence(d.Direction);

            d.StructuralConfirmations =
                input.StructuralConfirmations == null
                    ? 0
                    : input.StructuralConfirmations(d.Direction);

            d.RegimeQuality =
                NumericGuards.ClampInt(
                    input.RegimeQuality == null
                        ? 0
                        : input.RegimeQuality(input.Regime),
                    0,
                    100);

            d.SmartQuality =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        strongestShare * 0.28 +
                        d.TimeframeAgreement * 0.23 +
                        Math.Min(100, d.IndependentEvidence * 10) * 0.20 +
                        Math.Min(100, d.StructuralConfirmations * 16) * 0.17 +
                        d.RegimeQuality * 0.12),
                    0,
                    100);

            if (d.Direction == 0)
            {
                d.Confidence = strongestShare;
                d.TriggerReady = false;
                d.EntryAllowed = false;
                d.BlockReason = "SMART CONSENSUS";
                d.Reason =
                    "NEUTRAL | BUY " +
                    buyShare +
                    " | SELL " +
                    sellShare;
                return d;
            }

            int baseConfidence =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        strongestShare * 0.45 +
                        d.TimeframeAgreement * 0.25 +
                        d.SmartQuality * 0.30),
                    0,
                    100);

            d.Confidence =
                input.CalibrateConfidence == null
                    ? baseConfidence
                    : input.CalibrateConfidence(
                        baseConfidence,
                        d.Direction);

            if (input.HigherTfPenalty > 0)
            {
                bool h1Against =
                    input.H1Frame != null &&
                    input.H1Frame.Direction != 0 &&
                    input.H1Frame.Direction != d.Direction;

                bool h4Against =
                    input.H4Frame != null &&
                    input.H4Frame.Direction != 0 &&
                    input.H4Frame.Direction != d.Direction;

                bool d1Against =
                    input.D1Frame != null &&
                    input.D1Frame.Direction != 0 &&
                    input.D1Frame.Direction != d.Direction;

                if (h1Against || h4Against || d1Against)
                {
                    int penalty =
                        input.HigherTfPenalty +
                        (d1Against ? input.HigherTfPenalty / 2 : 0);

                    d.Confidence =
                        NumericGuards.ClampInt(
                            d.Confidence - penalty,
                            0,
                            100);
                }
            }

            d.RetestQuality =
                input.RetestQuality == null
                    ? 0
                    : NumericGuards.ClampInt(
                        input.RetestQuality(
                            input.ClosedM5,
                            d.Direction),
                        0,
                        100);

            d.TriggerReady =
                input.ClosedBarTriggerReady == null ||
                input.ClosedBarTriggerReady(
                    input.ClosedM5,
                    d.Direction);

            return d;
        }
    }
}