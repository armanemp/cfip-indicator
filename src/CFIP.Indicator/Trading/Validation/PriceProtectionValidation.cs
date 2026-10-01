// ============================================================================
// CFIP Indicator — PriceProtectionValidation.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double MinimumTakeProfitDistancePrice()
        {
            return MinimumTakeProfitDistancePriceForDirection(0);
        }

        private double MinimumTakeProfitDistancePriceForDirection(
            int direction)
        {
            try
            {
                double distance =
                    Math.Max(
                        0,
                        Symbol.MinTakeProfitDistance);

                if (distance <= 0)
                    return Math.Max(
                        Symbol.TickSize,
                        Symbol.PipSize);

                if (Symbol.MinDistanceType ==
                    SymbolMinDistanceType.Pips)
                    return distance *
                        Math.Max(
                            Symbol.PipSize,
                            Symbol.TickSize);

                double referencePrice =
                    direction == -1
                        ? Symbol.Ask
                        : Symbol.Bid;

                if (!IsFinitePositive(referencePrice))
                    referencePrice = Symbol.Bid;

                if (!IsFinitePositive(referencePrice))
                    return Math.Max(
                        Symbol.TickSize,
                        Symbol.PipSize);

                return referencePrice *
                    distance /
                    100.0;
            }
            catch
            {
                return Math.Max(
                    Symbol.TickSize,
                    Symbol.PipSize);
            }
        }

        private bool IsValidTarget(
            int direction,
            double entry,
            double target)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(target))
                return false;

            double minimumDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumTakeProfitDistancePriceForDirection(
                        direction));

            return PriceProtectionRule.ValidateTarget(
                direction,
                entry,
                target,
                minimumDistance);
        }

        private double MinimumProtectionDistancePrice()
        {
            return MinimumProtectionDistancePriceForDirection(0);
        }

        private double MinimumProtectionDistancePriceForDirection(
            int direction)
        {
            try
            {
                double distance =
                    Math.Max(
                        0,
                        Symbol.MinStopLossDistance);

                if (distance <= 0)
                    return Math.Max(
                        Symbol.TickSize,
                        Symbol.PipSize);

                if (Symbol.MinDistanceType ==
                    SymbolMinDistanceType.Pips)
                    return distance *
                        Math.Max(
                            Symbol.PipSize,
                            Symbol.TickSize);

                double referencePrice =
                    direction == -1
                        ? Symbol.Ask
                        : Symbol.Bid;

                if (!IsFinitePositive(referencePrice))
                    referencePrice = Symbol.Bid;

                if (!IsFinitePositive(referencePrice))
                    return Math.Max(
                        Symbol.TickSize,
                        Symbol.PipSize);

                return referencePrice *
                    distance /
                    100.0;
            }
            catch
            {
                return Math.Max(
                    Symbol.TickSize,
                    Symbol.PipSize);
            }
        }

        private bool IsValidStop(
            int direction,
            double entry,
            double stop)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(stop))
                return false;

            double minimumDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumProtectionDistancePriceForDirection(
                        direction));

            return PriceProtectionRule.ValidateStop(
                direction,
                entry,
                stop,
                minimumDistance);
        }

        private bool IsExistingManagedStopHealthy(
            int direction,
            double entry,
            double stop)
        {
            return ManagedStopProtectionRule.IsExistingStopHealthy(
                direction,
                entry,
                stop);
        }

        private bool IsValidManagedStop(
            int direction,
            double entry,
            double market,
            double stop)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(stop))
                return false;

            double minimumDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumProtectionDistancePrice());

            return ManagedStopProtectionRule.Validate(
                direction,
                entry,
                market,
                stop,
                minimumDistance);
        }
    }
}