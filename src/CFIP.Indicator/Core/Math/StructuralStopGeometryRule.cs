using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical, platform-neutral structural-stop geometry owner.
    ///
    /// This rule owns only the conversion:
    /// structural source level + timeframe ATR + configured buffer
    ///     -> normalized protective stop + risk geometry.
    ///
    /// Broker-distance validation remains owned by PriceProtectionRule through
    /// the Indicator IsValidStop adapter. Risk ceilings remain owned by
    /// StructuralStopRiskRule.
    /// </summary>
    internal static class StructuralStopGeometryRule
    {
        public static StructuralStopGeometrySnapshot Evaluate(
            int direction,
            double entry,
            double sourcePrice,
            double frameAtr,
            double riskReferenceAtr,
            double pipSize,
            string timeframe,
            double stopBufferAtr,
            double htfStopBufferAtr,
            double tickSize,
            int digits)
        {
            if (!IsValidFiniteValue(entry) ||
                !IsValidFiniteValue(sourcePrice) ||
                !IsValidFiniteValue(frameAtr) ||
                !IsValidFiniteValue(riskReferenceAtr) ||
                (direction != 1 && direction != -1))
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    sourcePrice,
                    frameAtr,
                    0,
                    "INVALID STRUCTURAL STOP INPUT");
            }

            double bufferAtr =
                ResolveBufferAtr(
                    timeframe,
                    stopBufferAtr,
                    htfStopBufferAtr);

            double rawStop =
                direction == 1
                    ? sourcePrice - frameAtr * bufferAtr
                    : sourcePrice + frameAtr * bufferAtr;

            double stop =
                NormalizePrice(
                    rawStop,
                    tickSize,
                    digits);

            if (!IsValidFiniteValue(stop))
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    sourcePrice,
                    frameAtr,
                    bufferAtr,
                    "STRUCTURAL STOP NORMALIZATION FAILED");
            }

            bool protectiveSide =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            if (!protectiveSide)
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    sourcePrice,
                    frameAtr,
                    bufferAtr,
                    "STRUCTURAL STOP SIDE INVALID");
            }

            double risk =
                Math.Abs(entry - stop);

            double riskAtr =
                risk /
                Math.Max(
                    riskReferenceAtr,
                    Math.Max(
                        tickSize,
                        1e-9));

            if (!IsValidFiniteValue(risk) ||
                !IsValidFiniteValue(riskAtr))
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    sourcePrice,
                    frameAtr,
                    bufferAtr,
                    "STRUCTURAL STOP RISK INVALID");
            }

            return new StructuralStopGeometrySnapshot(
                true,
                direction,
                entry,
                sourcePrice,
                frameAtr,
                bufferAtr,
                rawStop,
                stop,
                risk,
                riskAtr,
                "VALID");
        }

        public static StructuralStopGeometrySnapshot EvaluateFallback(
            int direction,
            double entry,
            double atr,
            double fallbackAtr,
            double tickSize,
            int digits)
        {
            if (!IsValidFiniteValue(entry) ||
                !IsValidFiniteValue(atr) ||
                !IsValidFiniteValue(fallbackAtr) ||
                (direction != 1 && direction != -1))
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    0,
                    atr,
                    fallbackAtr,
                    "INVALID FALLBACK STOP INPUT");
            }

            double rawStop =
                direction == 1
                    ? entry - atr * fallbackAtr
                    : entry + atr * fallbackAtr;

            double stop =
                NormalizePrice(
                    rawStop,
                    tickSize,
                    digits);

            if (!IsValidFiniteValue(stop))
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    0,
                    atr,
                    fallbackAtr,
                    "FALLBACK STOP NORMALIZATION FAILED");
            }

            bool protectiveSide =
                direction == 1
                    ? stop < entry
                    : stop > entry;

            if (!protectiveSide)
            {
                return StructuralStopGeometrySnapshot.CreateInvalid(
                    direction,
                    entry,
                    0,
                    atr,
                    fallbackAtr,
                    "FALLBACK STOP SIDE INVALID");
            }

            double risk =
                Math.Abs(entry - stop);

            double riskAtr =
                risk /
                Math.Max(
                    atr,
                    Math.Max(
                        tickSize,
                        1e-9));

            return new StructuralStopGeometrySnapshot(
                true,
                direction,
                entry,
                0,
                atr,
                fallbackAtr,
                rawStop,
                stop,
                risk,
                riskAtr,
                "FALLBACK");
        }

        public static double ResolveBufferAtr(
            string timeframe,
            double stopBufferAtr,
            double htfStopBufferAtr)
        {
            double baseBuffer =
                Math.Max(
                    0.02,
                    stopBufferAtr);

            return string.Equals(
                       timeframe,
                       "M5",
                       StringComparison.OrdinalIgnoreCase)
                ? baseBuffer
                : Math.Max(
                    baseBuffer,
                    Math.Max(
                        0.02,
                        htfStopBufferAtr));
        }

        private static double NormalizePrice(
            double price,
            double tickSize,
            int digits)
        {
            if (!IsValidFiniteValue(price))
                return 0;

            if (IsValidFiniteValue(tickSize))
            {
                price =
                    Math.Round(
                        price / tickSize,
                        MidpointRounding.AwayFromZero) *
                    tickSize;
            }

            return Math.Round(
                price,
                Math.Max(0, digits));
        }

        private static bool IsValidFiniteValue(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
