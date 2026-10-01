using System;

namespace cAlgo
{
    internal readonly struct EntryTrapRiskResult
    {
        public int Risk { get; }
        public bool Block { get; }
        public string Reason { get; }

        public EntryTrapRiskResult(
            int risk,
            bool block,
            string reason)
        {
            Risk = NumericGuards.ClampInt(risk, 0, 100);
            Block = block;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? "NONE"
                    : reason;
        }

        public static EntryTrapRiskResult CreateNoTrapRisk()
        {
            return new EntryTrapRiskResult(
                0,
                false,
                "NONE");
        }
    }

    internal static class EntryTrapRiskRule
    {
        internal static EntryTrapRiskResult Evaluate(
            int direction,
            double rangePosition,
            double adverseM5Atr,
            double adverseM1Atr,
            int opposingRegularDivergenceQuality,
            bool supportiveHiddenDivergence)
        {
            if (direction != 1 &&
                direction != -1)
                return EntryTrapRiskResult.CreateNoTrapRisk();

            if (double.IsNaN(rangePosition) ||
                double.IsInfinity(rangePosition))
                rangePosition = 0.5;

            rangePosition =
                Math.Max(
                    0,
                    Math.Min(
                        1,
                        rangePosition));

            double m5 =
                IsValidAdverseAtrValue(adverseM5Atr)
                    ? adverseM5Atr
                    : 0;

            double m1 =
                IsValidAdverseAtrValue(adverseM1Atr)
                    ? adverseM1Atr
                    : 0;

            int divergence =
                NumericGuards.ClampInt(
                    opposingRegularDivergenceQuality,
                    0,
                    100);

            bool extreme =
                direction == 1
                    ? rangePosition >= EntryActionabilityPolicy.LongExtremeRangePosition
                    : rangePosition <= EntryActionabilityPolicy.ShortExtremeRangePosition;

            bool nearExtreme =
                direction == 1
                    ? rangePosition >= EntryActionabilityPolicy.LongNearExtremeRangePosition
                    : rangePosition <= EntryActionabilityPolicy.ShortNearExtremeRangePosition;

            int risk = 0;

            if (extreme)
                risk += EntryActionabilityPolicy.ExtremeLocationRisk;
            else if (nearExtreme)
                risk += EntryActionabilityPolicy.NearExtremeLocationRisk;

            risk +=
                Math.Min(
                    EntryActionabilityPolicy.M5AdverseRiskCap,
                    (int)Math.Round(
                        Math.Max(
                            0,
                            m5) *
                        EntryActionabilityPolicy.M5AdverseRiskAtrWeight));

            risk +=
                Math.Min(
                    EntryActionabilityPolicy.M1AdverseRiskCap,
                    (int)Math.Round(
                        Math.Max(
                            0,
                            m1) *
                        EntryActionabilityPolicy.M1AdverseRiskAtrWeight));

            if (divergence >= EntryActionabilityPolicy.DivergenceHighQuality)
                risk += EntryActionabilityPolicy.DivergenceHighRisk;
            else if (divergence >= EntryActionabilityPolicy.DivergenceMediumQuality)
                risk += EntryActionabilityPolicy.NearExtremeLocationRisk;
            else if (divergence >= EntryActionabilityPolicy.DivergenceLowQuality)
                risk += EntryActionabilityPolicy.DivergenceLowRisk;

            if (supportiveHiddenDivergence)
                risk -= EntryActionabilityPolicy.SupportiveHiddenDivergenceRiskAdjustment;

            bool strongAdverseMomentum =
                m5 >= EntryActionabilityPolicy.StrongAdverseM5Atr ||
                m1 >= EntryActionabilityPolicy.StrongAdverseM1Atr;

            if (strongAdverseMomentum)
                risk = Math.Max(risk, EntryActionabilityPolicy.StrongAdverseRiskFloor);

            risk =
                NumericGuards.ClampInt(
                    risk,
                    0,
                    100);

            string reason;

            if (strongAdverseMomentum)
            {
                reason =
                    "ADVERSE MOMENTUM";
            }
            else if (divergence >= EntryActionabilityPolicy.DivergenceMediumQuality &&
                     extreme)
            {
                reason =
                    "EXTREME LOCATION + OPPOSING DIVERGENCE";
            }
            else if (divergence >= EntryActionabilityPolicy.DivergenceMediumQuality)
            {
                reason =
                    "OPPOSING REGULAR DIVERGENCE";
            }
            else if (extreme)
            {
                reason =
                    "EXTREME ENTRY LOCATION";
            }
            else if (m5 >= EntryActionabilityPolicy.AdverseM5BlockAtr ||
                     m1 >= EntryActionabilityPolicy.AdverseM1BlockAtr)
            {
                reason =
                    "ADVERSE MOMENTUM";
            }
            else if (nearExtreme)
            {
                reason =
                    "NEAR EXTREME LOCATION";
            }
            else if (supportiveHiddenDivergence)
            {
                reason =
                    "SUPPORTIVE HIDDEN DIVERGENCE";
            }
            else
            {
                reason =
                    "NONE";
            }

            return new EntryTrapRiskResult(
                risk,
                extreme ||
                divergence >= EntryActionabilityPolicy.DivergenceMediumQuality ||
                risk >= 75 ||
                m5 >= EntryActionabilityPolicy.AdverseM5BlockAtr ||
                m1 >= EntryActionabilityPolicy.AdverseM1BlockAtr,
                reason);
        }

        private static bool IsValidAdverseAtrValue(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
