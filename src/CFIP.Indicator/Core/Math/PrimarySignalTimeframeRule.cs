using System;

namespace cAlgo
{
    internal readonly struct PrimaryTimeframeTuning
    {
        public string State { get; }
        public int Agreement { get; }
        public bool Conflict { get; }

        public PrimaryTimeframeTuning(
            string state,
            int agreement,
            bool conflict)
        {
            State = state ?? "UNKNOWN";
            Agreement = Math.Max(
                0,
                Math.Min(
                    100,
                    agreement));
            Conflict = conflict;
        }
    }

    internal static class PrimarySignalTimeframeRule
    {
        internal static bool IsPrimary(
            string timeframe)
        {
            return
                string.Equals(
                    timeframe,
                    "M15",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    timeframe,
                    "H1",
                    StringComparison.OrdinalIgnoreCase);
        }

        internal static int PrimaryPriority(
            string timeframe)
        {
            if (string.Equals(
                    timeframe,
                    "H1",
                    StringComparison.OrdinalIgnoreCase))
                return 5200;

            if (string.Equals(
                    timeframe,
                    "M15",
                    StringComparison.OrdinalIgnoreCase))
                return 5150;

            return 0;
        }

        internal static PrimaryTimeframeTuning ResolveTuning(
            int primaryDirection,
            int m5Direction,
            int m1Direction)
        {
            if (primaryDirection != 1 &&
                primaryDirection != -1)
                return new PrimaryTimeframeTuning(
                    "NO PRIMARY DIRECTION",
                    0,
                    false);

            bool m5Known =
                m5Direction == 1 ||
                m5Direction == -1;

            bool m1Known =
                m1Direction == 1 ||
                m1Direction == -1;

            bool m5Agree =
                m5Known &&
                m5Direction == primaryDirection;

            bool m1Agree =
                m1Known &&
                m1Direction == primaryDirection;

            bool m5Conflict =
                m5Known &&
                !m5Agree;

            bool m1Conflict =
                m1Known &&
                !m1Agree;

            if (m5Agree && m1Agree)
                return new PrimaryTimeframeTuning(
                    "M5+M1 ALIGNED",
                    100,
                    false);

            if (m5Conflict && m1Conflict)
                return new PrimaryTimeframeTuning(
                    "LOWER-TF CONFLICT",
                    0,
                    true);

            if (m5Agree)
                return new PrimaryTimeframeTuning(
                    "M5 TUNED",
                    70,
                    false);

            if (m1Agree)
                return new PrimaryTimeframeTuning(
                    "M1 TUNED",
                    60,
                    false);

            if (!m5Known && !m1Known)
                return new PrimaryTimeframeTuning(
                    "LOWER-TF PENDING",
                    0,
                    false);

            return new PrimaryTimeframeTuning(
                "LOWER-TF MIXED",
                30,
                false);
        }

        internal static string ResolveLevelEvidence(
            Frame frame,
            int direction)
        {
            if (frame == null ||
                (direction != 1 &&
                 direction != -1))
                return "NO LEVEL EVIDENCE";

            bool fvg =
                direction == 1
                    ? frame.FvgBull
                    : frame.FvgBear;

            bool ob =
                direction == 1
                    ? frame.ObBull
                    : frame.ObBear;

            bool confluence =
                direction == 1
                    ? frame.FvgObBullConfluence
                    : frame.FvgObBearConfluence;

            if (confluence)
                return "OB+FVG";

            if (ob && fvg)
                return "OB+FVG AREA";

            if (ob)
                return "OB";

            if (fvg)
                return "FVG";

            return "STRUCTURE / LIQUIDITY";
        }
    }
}
