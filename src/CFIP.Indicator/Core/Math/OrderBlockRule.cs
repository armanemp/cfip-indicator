using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic mathematical owner for Order Block source geometry,
    /// structural qualification and lifecycle boundaries.
    /// </summary>
    internal static class OrderBlockRule
    {
        public static bool IsOppositeSourceCandle(
            int direction,
            double open,
            double close)
        {
            if (direction == 1)
                return Finite(open) &&
                       Finite(close) &&
                       close < open;

            if (direction == -1)
                return Finite(open) &&
                       Finite(close) &&
                       close > open;

            return false;
        }

        public static bool TryGetZone(
            int direction,
            bool useBody,
            double open,
            double close,
            double high,
            double low,
            out double zoneLow,
            out double zoneHigh)
        {
            zoneLow = 0;
            zoneHigh = 0;

            if (!Finite(open) ||
                !Finite(close) ||
                !Finite(high) ||
                !Finite(low) ||
                low > high ||
                !IsOppositeSourceCandle(
                    direction,
                    open,
                    close))
                return false;

            double bodyLow =
                Math.Min(
                    open,
                    close);

            double bodyHigh =
                Math.Max(
                    open,
                    close);

            zoneLow =
                useBody
                    ? bodyLow
                    : low;

            zoneHigh =
                useBody
                    ? bodyHigh
                    : high;

            return Finite(zoneLow) &&
                   Finite(zoneHigh) &&
                   zoneLow < zoneHigh;
        }

        public static bool MeetsDisplacement(
            int direction,
            double open,
            double close,
            double creationAtr,
            double displacementAtr)
        {
            if (!IsValidDirection(direction) ||
                !FinitePositive(creationAtr) ||
                !FinitePositive(displacementAtr) ||
                !Finite(open) ||
                !Finite(close))
                return false;

            double body =
                Math.Abs(
                    close -
                    open);

            bool directional =
                direction == 1
                    ? close > open
                    : close < open;

            return directional &&
                   body >=
                   creationAtr *
                   displacementAtr;
        }

        public static bool BreaksStructure(
            int direction,
            double close,
            double priorExtreme,
            double creationAtr,
            double breakAtr)
        {
            if (!IsValidDirection(direction) ||
                !Finite(close) ||
                !Finite(priorExtreme) ||
                !FinitePositive(creationAtr) ||
                !FiniteNonNegative(breakAtr))
                return false;

            double threshold =
                creationAtr *
                breakAtr;

            return direction == 1
                ? close >
                  priorExtreme +
                  threshold
                : close <
                  priorExtreme -
                  threshold;
        }

        public static double GetMitigationProbe(
            int direction,
            double open,
            double close,
            double high,
            double low,
            bool breakByWicks)
        {
            if (direction == 1)
                return breakByWicks
                    ? low
                    : Math.Min(
                        open,
                        close);

            if (direction == -1)
                return breakByWicks
                    ? high
                    : Math.Max(
                        open,
                        close);

            return double.NaN;
        }

        public static bool IsFullyMitigated(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe)
        {
            if (!IsValidDirection(direction) ||
                !IsValidGeometry(
                    zoneLow,
                    zoneHigh) ||
                !Finite(probe))
                return false;

            return direction == 1
                ? probe <= zoneLow
                : probe >= zoneHigh;
        }

        public static bool TryApplyOrderBlockPartialMitigation(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe,
            double tickSize,
            out double managedLow,
            out double managedHigh,
            out bool partiallyMitigated,
            out double remainingRatio)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;
            partiallyMitigated = false;
            remainingRatio = 0;

            if (!IsValidDirection(direction) ||
                !IsValidGeometry(
                    zoneLow,
                    zoneHigh) ||
                !Finite(probe) ||
                !FinitePositive(tickSize))
                return false;

            if (IsFullyMitigated(
                    direction,
                    zoneLow,
                    zoneHigh,
                    probe))
                return false;

            if (direction == 1 &&
                probe < managedHigh)
            {
                managedHigh =
                    Math.Max(
                        managedLow,
                        probe);
                partiallyMitigated = true;
            }
            else if (direction == -1 &&
                     probe > managedLow)
            {
                managedLow =
                    Math.Min(
                        managedHigh,
                        probe);
                partiallyMitigated = true;
            }

            double originalWidth =
                zoneHigh -
                zoneLow;

            double managedWidth =
                managedHigh -
                managedLow;

            remainingRatio =
                managedWidth /
                Math.Max(
                    tickSize,
                    originalWidth);

            return managedLow < managedHigh &&
                   managedWidth >
                   tickSize &&
                   remainingRatio >
                   0.05;
        }

        public static string Identity(
            int direction,
            int createdIndex,
            bool useBodyForZone)
        {
            if (!IsValidDirection(direction) ||
                createdIndex < 0)
                return string.Empty;

            return
                direction.ToString() +
                ":" +
                createdIndex.ToString() +
                ":" +
                (useBodyForZone
                    ? "BODY"
                    : "WICK");
        }

        private static bool IsValidGeometry(
            double low,
            double high)
        {
            return
                Finite(low) &&
                Finite(high) &&
                low < high;
        }

        private static bool IsValidDirection(int direction)
        {
            return
                direction == 1 ||
                direction == -1;
        }

        private static bool Finite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }

        private static bool FinitePositive(double value)
        {
            return
                Finite(value) &&
                value > 0;
        }

        private static bool FiniteNonNegative(double value)
        {
            return
                Finite(value) &&
                value >= 0;
        }
    }
}
