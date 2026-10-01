using System;

namespace cAlgo
{
    internal readonly struct EntryTrapRiskResult
    {
        public int Risk { get; }
        public bool Block { get; }
        public string Reason { get; }
        public string Context { get; }

        public EntryTrapRiskResult(
            int risk,
            bool block,
            string reason,
            string context = "NONE")
        {
            Risk = NumericGuards.ClampInt(risk, 0, 100);
            Block = block;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? "NONE"
                    : reason;
            Context =
                string.IsNullOrWhiteSpace(context)
                    ? "NONE"
                    : context;
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
            EntryTrapRiskResult result =
                Evaluate(
                    direction,
                    rangePosition,
                    adverseM5Atr,
                    adverseM1Atr,
                    opposingRegularDivergenceQuality,
                    supportiveHiddenDivergence,
                    false,
                    false,
                    false,
                    false,
                    false);

            string mappedReason =
                result.Reason.IndexOf(
                    EntryTrapRiskPolicy.AdverseM5Reason,
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                result.Reason.IndexOf(
                    EntryTrapRiskPolicy.AdverseM1Reason,
                    StringComparison.OrdinalIgnoreCase) >= 0
                    ? "ADVERSE MOMENTUM"
                    : result.Reason.IndexOf(
                          EntryTrapRiskPolicy.ExtremeReason,
                          StringComparison.OrdinalIgnoreCase) >= 0 &&
                      result.Reason.IndexOf(
                          EntryTrapRiskPolicy.DivergenceReason,
                          StringComparison.OrdinalIgnoreCase) >= 0
                        ? "EXTREME LOCATION + OPPOSING DIVERGENCE"
                        : result.Reason.IndexOf(
                              EntryTrapRiskPolicy.DivergenceReason,
                              StringComparison.OrdinalIgnoreCase) >= 0
                            ? "OPPOSING REGULAR DIVERGENCE"
                            : result.Reason.IndexOf(
                                  EntryTrapRiskPolicy.ExtremeReason,
                                  StringComparison.OrdinalIgnoreCase) >= 0
                                ? "EXTREME ENTRY LOCATION"
                                : result.Reason;

            return new EntryTrapRiskResult(
                result.Risk,
                result.Block,
                mappedReason);
        }

        internal static EntryTrapRiskResult Evaluate(
            int direction,
            double rangePosition,
            double adverseM5Atr,
            double adverseM1Atr,
            int opposingRegularDivergenceQuality,
            bool supportiveHiddenDivergence,
            bool isRetest,
            bool insideZone,
            bool adverseM5PreZone,
            bool adverseM1PreZone,
            bool adverseEvidenceKnown)
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
                m5 >= EntryTrapRiskPolicy.StrongAdverseM5Atr ||
                m1 >= EntryTrapRiskPolicy.StrongAdverseM1Atr;

            if (strongAdverseMomentum)
                risk = Math.Max(risk, EntryTrapRiskPolicy.StrongAdverseRiskFloor);

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
            else if (m5 >= EntryTrapRiskPolicy.AdverseM5BlockAtr ||
                     m1 >= EntryTrapRiskPolicy.AdverseM1BlockAtr)
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

            bool block =
                extreme ||
                divergence >= EntryActionabilityPolicy.DivergenceMediumQuality ||
                risk >= EntryTrapRiskPolicy.StrongAdverseRiskFloor ||
                m5 >= EntryTrapRiskPolicy.AdverseM5BlockAtr ||
                m1 >= EntryTrapRiskPolicy.AdverseM1BlockAtr;

            if (block)
            {
                reason =
                    EntryTrapRiskPolicy.ResolveBlockReason(
                        extreme,
                        divergence,
                        EntryActionabilityPolicy.DivergenceMediumQuality,
                        m5,
                        m1,
                        risk);
            }

            string context =
                EntryTrapRiskPolicy.ResolveRetestContext(
                    isRetest,
                    insideZone,
                    adverseM5PreZone,
                    adverseM1PreZone,
                    adverseEvidenceKnown);

            return new EntryTrapRiskResult(
                risk,
                block,
                reason,
                context);
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
