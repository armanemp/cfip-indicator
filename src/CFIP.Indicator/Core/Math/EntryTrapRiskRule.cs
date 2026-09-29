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
                    ? rangePosition >= 0.85
                    : rangePosition <= 0.15;

            bool nearExtreme =
                direction == 1
                    ? rangePosition >= 0.75
                    : rangePosition <= 0.25;

            int risk = 0;

            if (extreme)
                risk += 45;
            else if (nearExtreme)
                risk += 25;

            risk +=
                Math.Min(
                    28,
                    (int)Math.Round(
                        Math.Max(
                            0,
                            m5) *
                        60));

            risk +=
                Math.Min(
                    18,
                    (int)Math.Round(
                        Math.Max(
                            0,
                            m1) *
                        45));

            if (divergence >= 86)
                risk += 32;
            else if (divergence >= 78)
                risk += 25;
            else if (divergence >= 70)
                risk += 16;

            if (supportiveHiddenDivergence)
                risk -= 10;

            risk =
                NumericGuards.ClampInt(
                    risk,
                    0,
                    100);

            string reason;

            if (divergence >= 78 &&
                extreme)
            {
                reason =
                    "EXTREME LOCATION + OPPOSING DIVERGENCE";
            }
            else if (divergence >= 78)
            {
                reason =
                    "OPPOSING REGULAR DIVERGENCE";
            }
            else if (extreme)
            {
                reason =
                    "EXTREME ENTRY LOCATION";
            }
            else if (m5 >= 0.30 ||
                     m1 >= 0.45)
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
                risk >= 75 ||
                m5 >= 0.30 ||
                m1 >= 0.45,
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
