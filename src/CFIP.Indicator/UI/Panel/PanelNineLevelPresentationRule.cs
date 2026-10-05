using cAlgo.API;

namespace cAlgo
{
    internal static class PanelNineLevelPresentationRule
    {
        internal static int NormalizeLevel(int level)
        {
            return NumericGuards.ClampInt(level, 0, 9);
        }

        internal static int ArrowCountForLevel(int level)
        {
            int normalized = NormalizeLevel(level);
            return normalized <= 0
                ? 0
                : ((normalized - 1) % 3) + 1;
        }

        internal static string TierForLevel(int level)
        {
            int normalized = NormalizeLevel(level);

            if (normalized <= 0)
                return "NONE";

            if (normalized <= 3)
                return "WEAK";

            if (normalized <= 6)
                return "MEDIUM";

            return "STRONG";
        }

        internal static string LevelLabel(int level)
        {
            int normalized = NormalizeLevel(level);

            if (normalized <= 0)
                return "WAIT";

            return
                TierForLevel(normalized) +
                " " +
                ArrowCountForLevel(normalized).ToString() +
                "  •  L" +
                normalized.ToString();
        }

        internal static Color ResolveColor(
            int direction,
            int level,
            Color strongBuy,
            Color strongSell,
            Color confirmedBuy,
            Color confirmedSell,
            Color cautionBuy,
            Color cautionSell,
            Color blockedReaction)
        {
            if (direction == 0)
                return blockedReaction;

            int normalized = NormalizeLevel(level);

            if (normalized <= 0)
                return blockedReaction;

            Color baseColor;

            if (normalized <= 3)
            {
                baseColor =
                    direction > 0
                        ? cautionBuy
                        : cautionSell;
            }
            else if (normalized <= 6)
            {
                baseColor =
                    direction > 0
                        ? confirmedBuy
                        : confirmedSell;
            }
            else
            {
                baseColor =
                    direction > 0
                        ? strongBuy
                        : strongSell;
            }

            int subLevel =
                ArrowCountForLevel(normalized);

            int alpha =
                subLevel == 1
                    ? 158
                    : subLevel == 2
                        ? 208
                        : 255;

            return Color.FromArgb(alpha, baseColor);
        }
    }
}
