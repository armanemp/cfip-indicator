using System;

namespace cAlgo
{
    internal readonly struct RangeSignalQualityInput
    {
        public int Direction { get; }
        public double RangePosition { get; }
        public double RangeEfficiency { get; }
        public double Choppiness { get; }
        public double Adx { get; }
        public bool Liquidity { get; }
        public bool Structure { get; }
        public bool Displacement { get; }
        public bool WaveTrendDirectionAligned { get; }
        public bool WaveTrendReversal { get; }
        public int WaveTrendQuality { get; }
        public int IndependentEvidence { get; }
        public int StructuralConfirmations { get; }
        public int Confidence { get; }
        public int SmartQuality { get; }
        public int Edge { get; }
        public double Tp1RR { get; }

        public RangeSignalQualityInput(
            int direction,
            double rangePosition,
            double rangeEfficiency,
            double choppiness,
            double adx,
            bool liquidity,
            bool structure,
            bool displacement,
            bool waveTrendDirectionAligned,
            bool waveTrendReversal,
            int waveTrendQuality,
            int independentEvidence,
            int structuralConfirmations,
            int confidence,
            int smartQuality,
            int edge,
            double tp1RR)
        {
            Direction = direction;
            RangePosition = rangePosition;
            RangeEfficiency = rangeEfficiency;
            Choppiness = choppiness;
            Adx = adx;
            Liquidity = liquidity;
            Structure = structure;
            Displacement = displacement;
            WaveTrendDirectionAligned = waveTrendDirectionAligned;
            WaveTrendReversal = waveTrendReversal;
            WaveTrendQuality = waveTrendQuality;
            IndependentEvidence = independentEvidence;
            StructuralConfirmations = structuralConfirmations;
            Confidence = confidence;
            SmartQuality = smartQuality;
            Edge = edge;
            Tp1RR = tp1RR;
        }
    }

    internal readonly struct RangeSignalQualityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public RangeSignalQualityResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
        }
    }

    internal static class RangeSignalQualityRule
    {
        public static RangeSignalQualityResult Evaluate(
            string regime,
            RangeSignalQualityInput input)
        {
            if (!string.Equals(
                    regime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(
                        regime,
                        "COMPRESSION",
                        StringComparison.OrdinalIgnoreCase))
                    return new RangeSignalQualityResult(
                        false,
                        "COMPRESSION NO-TRADE");

                return new RangeSignalQualityResult(
                    true,
                    string.Empty);
            }

            if (input.Direction != 1 &&
                input.Direction != -1)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • NO DIRECTION");

            bool lowerEdge =
                input.RangePosition <= 0.35;
            bool upperEdge =
                input.RangePosition >= 0.65;

            bool edgeAligned =
                input.Direction == 1
                    ? lowerEdge
                    : upperEdge;

            if (!edgeAligned)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • MID-RANGE");

            if (!input.Liquidity)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • NO LIQUIDITY EVENT");

            if (!input.Structure)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • NO STRUCTURAL REVERSAL");

            if (!input.WaveTrendDirectionAligned ||
                !input.WaveTrendReversal ||
                input.WaveTrendQuality < 58)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • WAVETREND");

            if (input.IndependentEvidence < 5)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • EVIDENCE");

            if (input.StructuralConfirmations < 2)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • STRUCTURE COUNT");

            if (input.Confidence < 85)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • CONFIDENCE");

            if (input.SmartQuality < 80)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • SMART QUALITY");

            if (input.Edge < 16)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • EDGE");

            // A genuine range reversal should show at least rejection/displacement
            // even when the liquidity event itself supplied the structural impulse.
            if (!input.Displacement)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • NO DISPLACEMENT");

            if (IsFinitePositive(input.Tp1RR) &&
                input.Tp1RR < 1.80)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • LOW RR");

            return new RangeSignalQualityResult(
                true,
                "RANGE REVERSAL QUALIFIED");
        }

        private static bool IsFinitePositive(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
