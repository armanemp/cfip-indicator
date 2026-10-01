using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Canonical required-RR ladder used by every target-selection context.
    /// This rule is intentionally platform-neutral so the ordering invariant
    /// can be tested without the cTrader host.
    /// </summary>
    internal static class TargetSelectionRequiredRrRule
    {
        public static double[] Build(
            double rrStep,
            OpportunityLane lane,
            double tp1MinimumRR,
            double tp2MinimumRR,
            double tp3MinimumRR,
            double tp4MinimumRR,
            double minimumRequiredRR,
            double minimumTradeRR,
            double tacticalOpportunityMinimumRR)
        {
            double step = Math.Max(0.10, rrStep);

            double canonicalMinimum =
                Math.Max(0, minimumRequiredRR);

            double tp1 =
                IsTacticalLane(lane)
                    ? Math.Max(
                        Math.Max(
                            Math.Max(0, tp1MinimumRR),
                            canonicalMinimum),
                        Math.Max(
                            1.0,
                            Math.Max(
                                0,
                                tacticalOpportunityMinimumRR)))
                    : Math.Max(
                        Math.Max(0, tp1MinimumRR),
                        IsFinite(minimumTradeRR)
                            ? Math.Max(0, Math.Min(minimumTradeRR, double.MaxValue))
                            : canonicalMinimum);

            // Strategic plans retain the existing TP1 contract:
            // max(TP1 minimum, canonical MinimumRequiredRR).
            if (!IsTacticalLane(lane))
            {
                tp1 =
                    Math.Max(
                        Math.Max(0, tp1MinimumRR),
                        canonicalMinimum);
            }

            double tp2 =
                Math.Max(
                    Math.Max(0, tp2MinimumRR),
                    tp1 + step);

            double tp3 =
                Math.Max(
                    Math.Max(0, tp3MinimumRR),
                    tp2 + step);

            double tp4 =
                Math.Max(
                    Math.Max(0, tp4MinimumRR),
                    tp3 + step);

            return new[]
            {
                tp1,
                tp2,
                tp3,
                tp4
            };
        }

        public static bool IsMonotonicNonDecreasing(
            IReadOnlyList<double> requiredRR)
        {
            if (requiredRR == null || requiredRR.Count != 4)
                return false;

            double previous = 0;

            for (int i = 0; i < requiredRR.Count; i++)
            {
                double current = requiredRR[i];

                if (!IsFiniteNonNegative(current) ||
                    (i > 0 && current < previous))
                    return false;

                previous = current;
            }

            return true;
        }

        private static bool IsTacticalLane(
            OpportunityLane lane)
        {
            return lane == OpportunityLane.Tactical ||
                   lane == OpportunityLane.CounterHtfTactical ||
                   lane == OpportunityLane.MicroReaction;
        }

        private static bool IsFinite(
            double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsFiniteNonNegative(
            double value)
        {
            return IsFinite(value) && value >= 0;
        }
    }
}
