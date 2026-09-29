using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double MinimumLiveTargetDistancePrice(
            int direction,
            double atr)
        {
            double brokerDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumTakeProfitDistancePrice(
                        direction));

            double volatilityDistance =
                atr > 0
                    ? atr *
                      Math.Max(
                          0.05,
                          MinimumTpSpacingAtr)
                    : 0;

            return Math.Max(
                Math.Max(
                    Symbol.PipSize,
                    Symbol.TickSize),
                Math.Max(
                    brokerDistance,
                    volatilityDistance));
        }

        private bool IsLiveTargetBrokerSafe(
            int direction,
            double entry,
            double market,
            double target,
            double atr)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(target))
                return false;

            double minimumForwardDistance =
                MinimumLiveTargetDistancePrice(
                    direction,
                    atr);

            if (!LiveExitGeometryRule.ValidateLiveTarget(
                    direction,
                    entry,
                    market,
                    target,
                    1.0,
                    minimumForwardDistance).Allowed)
                return false;

            return IsValidTarget(
                direction,
                entry,
                target);
        }
    }
}
