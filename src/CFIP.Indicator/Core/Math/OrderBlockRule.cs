using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic mathematical owner for Order Block source geometry and
    /// structural qualification. Lifecycle transitions are owned separately
    /// by OrderBlockLifecycleRule.
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

        public static bool IsOnCorrectMarketSide(
            int direction,
            double market,
            double zoneLow,
            double zoneHigh,
            double tickSize)
        {
            if (!IsValidDirection(direction) ||
                !Finite(market) ||
                !IsValidOrderBlockGeometry(zoneLow, zoneHigh) ||
                !FinitePositiveOrderBlockValue(tickSize))
                return false;

            double tolerance = tickSize;

            return direction == 1
                ? zoneHigh <= market + tolerance
                : zoneLow >= market - tolerance;
        }

        public static bool MeetsDisplacement(
            int direction,
            double open,
            double close,
            double creationAtr,
            double displacementAtr)
        {
            if (!IsValidDirection(direction) ||
                !FinitePositiveOrderBlockValue(creationAtr) ||
                !FinitePositiveOrderBlockValue(displacementAtr) ||
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
                !FinitePositiveOrderBlockValue(creationAtr) ||
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

        public static string OrderBlockIdentity(
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

        private static bool IsValidOrderBlockGeometry(
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

        private static bool FinitePositiveOrderBlockValue(double value)
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
