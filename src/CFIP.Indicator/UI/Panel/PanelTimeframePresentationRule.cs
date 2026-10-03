using System;

namespace cAlgo
{
    internal static class PanelTimeframePresentationRule
    {
        public static PanelTimeframePresentationState Resolve(
            Frame frame)
        {
            if (frame == null)
                return new PanelTimeframePresentationState(
                    0,
                    0,
                    "WAIT");

            int direction =
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    frame.Direction,
                    frame.BullScore,
                    frame.BearScore,
                    frame.TrendBull,
                    frame.TrendBear);

            string label =
                PanelFrameDirectionRule.ResolveLabel(
                    frame.Direction,
                    direction);

            int strength =
                ResolveStrength(
                    frame,
                    direction);

            return new PanelTimeframePresentationState(
                direction,
                strength,
                label);
        }

        private static int ResolveStrength(
            Frame frame,
            int direction)
        {
            if (frame == null ||
                direction == 0 ||
                !frame.NativeIndicatorsReady)
                return 0;

            int score =
                direction == 1
                    ? frame.BullScore
                    : frame.BearScore;

            double adx =
                frame.Adx;

            if (score >= 70 ||
                adx >= 25)
                return 3;

            if (score >= 55 ||
                adx >= 20)
                return 2;

            return 1;
        }
    }
}
