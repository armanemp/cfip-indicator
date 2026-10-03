using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private struct PanelTimeframePresentationState
        {
            public int Direction;
            public int Strength;
            public string DirectionLabel;
            public bool Ready;
        }

        private PanelTimeframePresentationState ResolvePanelTimeframeState(
            Frame frame)
        {
            if (frame == null)
            {
                return new PanelTimeframePresentationState
                {
                    Direction = 0,
                    Strength = 0,
                    DirectionLabel = "WAIT",
                    Ready = false
                };
            }

            int direction =
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    frame.Direction,
                    frame.BullScore,
                    frame.BearScore,
                    frame.TrendBull,
                    frame.TrendBear);

            if (direction == 0 || !frame.NativeIndicatorsReady)
            {
                return new PanelTimeframePresentationState
                {
                    Direction = direction,
                    Strength = 0,
                    DirectionLabel =
                        PanelFrameDirectionRule.ResolveLabel(
                            frame.Direction,
                            direction),
                    Ready = frame.NativeIndicatorsReady
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

            return new PanelTimeframePresentationState
            {
                Direction = direction,
                Strength = strength,
                DirectionLabel =
                    PanelFrameDirectionRule.ResolveLabel(
                        frame.Direction,
                        direction),
                Ready = true
            };
        }
    }
}
