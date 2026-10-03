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
                    // A range setup must have enough geometric travel to support
                    // the canonical 2.25 TP1-RR floor. A 1 ATR candidate can
                    // otherwise survive this gate and later present a weak TP.
                    return Math.Max(baseFloor, 2.25);

                case MarketRegimeIdentity.Transition:
                    return Math.Max(baseFloor, 0.85);

                default:
                    return baseFloor;
            }
        }
    }
}
