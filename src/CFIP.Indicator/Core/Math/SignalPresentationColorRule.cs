using cAlgo.API;

namespace cAlgo
{
    internal static class SignalPresentationColorRule
    {
        public static Color Resolve(int direction, int nineLevel, Color strongBuy, Color strongSell, Color confirmedBuy, Color confirmedSell, Color cautionBuy, Color cautionSell, Color blocked)
        {
            if (direction == 1)
            {
                if (nineLevel >= 7) return strongBuy;
                if (nineLevel >= 4) return confirmedBuy;
                if (nineLevel >= 1) return cautionBuy;
                return blocked;
            }
            if (direction == -1)
            {
                if (nineLevel >= 7) return strongSell;
                if (nineLevel >= 4) return confirmedSell;
                if (nineLevel >= 1) return cautionSell;
                return blocked;
            }
            return blocked;
        }

        public static Color ResolveTier(int direction, string tier, Color strongBuy, Color strongSell, Color confirmedBuy, Color confirmedSell, Color cautionBuy, Color cautionSell, Color blocked)
        {
            string normalized = (tier ?? string.Empty).Trim().ToUpperInvariant();
            int level = string.Equals(normalized, "STRONG", System.StringComparison.Ordinal) ? 7 :
                string.Equals(normalized, "MEDIUM", System.StringComparison.Ordinal) ? 4 :
                string.Equals(normalized, "WEAK", System.StringComparison.Ordinal) ? 1 : 0;
            return Resolve(direction, level, strongBuy, strongSell, confirmedBuy, confirmedSell, cautionBuy, cautionSell, blocked);
        }
    }
}