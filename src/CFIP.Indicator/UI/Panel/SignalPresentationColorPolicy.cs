using cAlgo.API;

namespace cAlgo
{
    internal static class SignalPresentationColorPolicy
    {
        public static Color Resolve(
            CFIPIndicator indicator,
            int direction,
            int strength,
            string state)
        {
            if (indicator == null || direction == 0)
                return indicator == null ? Color.White : indicator.PanelTextColor;

            string normalizedState =
                string.IsNullOrWhiteSpace(state)
                    ? "WATCH"
                    : state.Trim().ToUpperInvariant();

            if (normalizedState == "REACTION")
                return indicator.BlockedReactionArrowColor;

            int normalizedStrength =
                Math.Max(1, Math.Min(3, strength));

            if (direction > 0)
            {
                return normalizedStrength >= 3
                    ? indicator.StrongBuyArrowColor
                    : normalizedStrength == 2
                        ? indicator.ConfirmedBuyArrowColor
                        : indicator.CautionBuyArrowColor;
            }

            return normalizedStrength >= 3
                ? indicator.StrongSellArrowColor
                : normalizedStrength == 2
                    ? indicator.ConfirmedSellArrowColor
                    : indicator.CautionSellArrowColor;
        }
    }
}
