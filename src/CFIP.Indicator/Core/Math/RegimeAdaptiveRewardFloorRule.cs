using System;

namespace cAlgo
{
    internal static class RegimeAdaptiveRewardFloorRule
    {
        internal static double ResolveAdaptiveRewardFloor(
            string regime,
            double minimumTpSpacingAtr,
            double minimumSlAtr)
        {
            double baseFloor =
                Math.Max(
                    Math.Max(0, minimumTpSpacingAtr),
                    Math.Max(0, minimumSlAtr) * 0.75);

            string normalized =
                (regime ?? string.Empty).Trim().ToUpperInvariant();

            switch (normalized)
            {
                case MarketRegimeIdentity.Compression:
                    return Math.Max(baseFloor, 1.20);

                case MarketRegimeIdentity.Range:
                    return Math.Max(baseFloor, 1.00);

                case MarketRegimeIdentity.Transition:
                    return Math.Max(baseFloor, 0.85);

                default:
                    return baseFloor;
            }
        }
    }
}
