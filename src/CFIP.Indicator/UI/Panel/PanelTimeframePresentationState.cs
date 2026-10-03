using cAlgo.API;

using System;

namespace cAlgo
{
    internal struct PanelTimeframePresentationState
    {
        public int Direction;
        public int Strength;
        public string DirectionLabel;
        public bool Ready;
        public Color Color;
    }

    public partial class CFIPIndicator : Indicator
    {
        private PanelTimeframePresentationState ResolvePanelTimeframeState(
            Frame frame)
        {
            if (frame == null ||
                !frame.NativeIndicatorsReady)
            {
                return new PanelTimeframePresentationState
                {
                    Direction = 0,
                    Strength = 0,
                    DirectionLabel = "WAIT",
                    Ready = false,
                    Color = PanelSecondaryTextColor
                };
            }

            int direction =
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    frame.Direction,
                    frame.BullScore,
                    frame.BearScore,
                    frame.TrendBull,
                    frame.TrendBear);

            if (direction == 0)
            {
                return new PanelTimeframePresentationState
                {
                    Direction = 0,
                    Strength = 0,
                    DirectionLabel = "NEUTRAL",
                    Ready = true,
                    Color = PanelSecondaryTextColor
                };
            }

            int score =
                direction == 1
                    ? frame.BullScore
                    : frame.BearScore;

            double adx =
                double.IsNaN(frame.Adx) ||
                double.IsInfinity(frame.Adx)
                    ? 0
                    : frame.Adx;

            int strength =
                score >= 70 || adx >= 25
                    ? 3
                    : score >= 55 || adx >= 20
                        ? 2
                        : 1;

            Color color =
                direction == 1
                    ? strength >= 3
                        ? StrongBuyArrowColor
                        : strength == 2
                            ? ConfirmedBuyArrowColor
                            : CautionBuyArrowColor
                    : strength >= 3
                        ? StrongSellArrowColor
                        : strength == 2
                            ? ConfirmedSellArrowColor
                            : CautionSellArrowColor;

            return new PanelTimeframePresentationState
            {
                Direction = direction,
                Strength = strength,
                DirectionLabel =
                    PanelFrameDirectionRule.ResolveLabel(
                        frame.Direction,
                        direction),
                Ready = true,
                Color = color
            };
        }
    }
}
