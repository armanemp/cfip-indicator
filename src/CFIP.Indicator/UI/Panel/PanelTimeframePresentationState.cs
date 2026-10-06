using cAlgo.API;

using System;

namespace cAlgo
{
    internal struct PanelTimeframePresentationState
    {
        public int Direction;
        public int Strength;
        public int Level;
        public string Tier;
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
                    Level = 0,
                    Tier = "NONE",
                    DirectionLabel = "WAIT",
                    Ready = false,
                    Color = PanelSecondaryTextColor
                };
            }

            double livePrice =
                Symbol.Ask > 0 &&
                Symbol.Bid > 0
                    ? (Symbol.Ask + Symbol.Bid) * 0.5
                    : Symbol.Ask;

            MtfTrendStrengthResult trendStrength =
                MtfTrendStrengthRule.Evaluate(
                    new[] { frame },
                    new[] { 1.0 },
                    livePrice);

            int direction =
                trendStrength.Direction;

            int level =
                PanelNineLevelPresentationRule.NormalizeLevel(
                    trendStrength.Level);

            if (direction == 0 ||
                level <= 0)
            {
                return new PanelTimeframePresentationState
                {
                    Direction = 0,
                    Strength = 0,
                    Level = 0,
                    Tier = "NONE",
                    DirectionLabel = "NEUTRAL",
                    Ready = true,
                    Color = PanelSecondaryTextColor
                };
            }

            int strength =
                PanelNineLevelPresentationRule.ArrowCountForLevel(
                    level);

            string tier =
                PanelNineLevelPresentationRule.TierForLevel(
                    level);

            Color color =
                PanelNineLevelPresentationRule.ResolveColor(
                    direction,
                    level,
                    StrongBuyArrowColor,
                    StrongSellArrowColor,
                    ConfirmedBuyArrowColor,
                    ConfirmedSellArrowColor,
                    CautionBuyArrowColor,
                    CautionSellArrowColor,
                    BlockedReactionArrowColor);

            return new PanelTimeframePresentationState
            {
                Direction = direction,
                Strength = strength,
                Level = level,
                Tier = tier,
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
