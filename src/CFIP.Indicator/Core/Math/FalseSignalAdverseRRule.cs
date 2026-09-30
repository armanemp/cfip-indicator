using System;

namespace cAlgo
{
    /// <summary>
    /// Single semantic owner for False-Signal Adverse-R thresholds.
    /// The requested threshold is never silently tuned; it is validated and,
    /// when a protective stop is known, bounded to the currently protected
    /// adverse-R envelope so software invalidation cannot sit behind broker SL.
    /// </summary>
    internal static class FalseSignalAdverseRRule
    {
        public const double MinimumSoftAdverseR = 0.25;
        public const double MinimumHardAdverseR = 1.00;
        public const double MaximumConfiguredAdverseR = 5.00;

        public static bool TryResolveThresholds(
            int direction,
            double entry,
            double risk,
            double configuredAdverseR,
            double protectedStop,
            out double softAdverseR,
            out double hardAdverseR,
            out double protectedStopR)
        {
            softAdverseR = 0;
            hardAdverseR = 0;
            protectedStopR = 0;

            if ((direction != 1 && direction != -1) ||
                !IsFinitePositiveInput(entry) ||
                !IsFinitePositiveInput(risk) ||
                double.IsNaN(configuredAdverseR) ||
                double.IsInfinity(configuredAdverseR) ||
                configuredAdverseR < MinimumSoftAdverseR ||
                configuredAdverseR > MaximumConfiguredAdverseR)
                return false;

            double requestedSoft =
                configuredAdverseR;

            double requestedHard =
                configuredAdverseR < MinimumHardAdverseR
                    ? MinimumHardAdverseR
                    : configuredAdverseR;

            if (TryCalculateProtectedStopR(
                    direction,
                    entry,
                    risk,
                    protectedStop,
                    out protectedStopR))
            {
                softAdverseR =
                    Math.Min(
                        requestedSoft,
                        protectedStopR);

                hardAdverseR =
                    Math.Min(
                        requestedHard,
                        protectedStopR);

                return softAdverseR > 0 &&
                       hardAdverseR > 0;
            }

            softAdverseR =
                requestedSoft;

            hardAdverseR =
                requestedHard;

            return true;
        }

        private static bool TryCalculateProtectedStopR(
            int direction,
            double entry,
            double risk,
            double protectedStop,
            out double stopR)
        {
            stopR = 0;

            if (!IsFinitePositiveInput(protectedStop))
                return false;

            double adverseDistance =
                direction == 1
                    ? entry - protectedStop
                    : protectedStop - entry;

            if (adverseDistance <= 0 ||
                double.IsNaN(adverseDistance) ||
                double.IsInfinity(adverseDistance))
                return false;

            stopR =
                adverseDistance /
                risk;

            return stopR > 0 &&
                   !double.IsNaN(stopR) &&
                   !double.IsInfinity(stopR);
        }

        private static bool IsFinitePositiveInput(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
