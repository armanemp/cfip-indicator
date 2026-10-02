using System;

namespace cAlgo
{
    internal readonly struct IntelligentProtectionDecision
    {
        public double Stop { get; }
        public bool Changed { get; }
        public string Reason { get; }

        public IntelligentProtectionDecision(
            double stop,
            bool changed,
            string reason)
        {
            Stop = stop;
            Changed = changed;
            Reason = reason ?? "";
        }
    }

    /// <summary>
    /// Single pure owner for live SL progression semantics.
    /// BE, structural trailing, momentum/pressure tightening and minimum
    /// progression step are evaluated together and never widen an existing stop.
    /// </summary>
    internal static class IntelligentProtectionRule
    {
        public static IntelligentProtectionDecision Evaluate(
            int direction,
            double entry,
            double market,
            double currentStop,
            double risk,
            double peakRR,
            double atr,
            double structuralStop,
            double spreadPips,
            double pipSize,
            double minimumDistance,
            double breakEvenTriggerRR,
            double breakEvenBufferPips,
            double riskFreeLockPips,
            double tp1Pips,
            bool moveToBreakEven,
            bool spreadAwareBreakEven,
            bool enableStructuralRepricing,
            bool structuralUpdate,
            bool structuralOnly,
            bool useSwingStructure,
            double slRepriceStartRR,
            double smartTrailMinimumRR,
            double trailDistanceAtr,
            double smartTrailTightenAtRR,
            double smartTrailMomentumBonusAtr,
            double slRepriceBreathingAtr,
            int exitPressure,
            int exitPressureThreshold,
            bool momentumAligned,
            double trailStepAtr,
            bool serverBreakEvenActive)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositive(entry) ||
                !IsPositive(market) ||
                !IsPositive(risk) ||
                !IsPositive(atr) ||
                !IsPositive(pipSize) ||
                !IsNonNegative(minimumDistance) ||
                !IsNonNegative(peakRR))
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "PROTECTION INPUT INVALID");

            double candidate = currentStop;
            string reason = "KEEP";

            if (!serverBreakEvenActive &&
                moveToBreakEven &&
                IsNonNegative(breakEvenTriggerRR) &&
                peakRR >=
                Math.Max(
                    0.50,
                    breakEvenTriggerRR))
            {
                double lockPips =
                    spreadAwareBreakEven
                        ? Math.Max(
                            riskFreeLockPips,
                            Math.Max(
                                0,
                                spreadPips) +
                            Math.Max(
                                0,
                                breakEvenBufferPips))
                        : Math.Max(
                            0,
                            breakEvenBufferPips);

                double be =
                    direction == 1
                        ? entry + lockPips * pipSize
                        : entry - lockPips * pipSize;

                candidate =
                    BetterStop(
                        direction,
                        be,
                        candidate)
                        ? be
                        : candidate;

                reason = "BREAK-EVEN";
            }

            bool structuralAllowed =
                enableStructuralRepricing &&
                useSwingStructure &&
                (structuralUpdate || !structuralOnly) &&
                peakRR >=
                Math.Max(
                    slRepriceStartRR,
                    smartTrailMinimumRR) &&
                IsPositive(structuralStop);

            if (structuralAllowed)
            {
                double room =
                    atr *
                    Math.Max(
                        0.10,
                        trailDistanceAtr);

                if (IsStructuralFarEnough(
                    direction,
                    market,
                    structuralStop,
                    room))
                {
                    candidate =
                        BetterStop(
                            direction,
                            structuralStop,
                            candidate)
                            ? structuralStop
                            : candidate;

                    reason = "STRUCTURAL TRAIL";
                }
            }

            if (structuralUpdate &&
                useSwingStructure &&
                peakRR >=
                Math.Max(
                    1.0,
                    smartTrailTightenAtRR) &&
                IsPositive(structuralStop))
            {
                double pressureTighten =
                    exitPressure >= exitPressureThreshold
                        ? Math.Min(
                            0.20,
                            Math.Max(
                                0,
                                slRepriceBreathingAtr))
                        : 0;

                double room =
                    atr *
                    Math.Max(
                        0.10,
                        trailDistanceAtr -
                        (momentumAligned
                            ? Math.Max(
                                0,
                                smartTrailMomentumBonusAtr)
                            : 0) -
                        pressureTighten);

                if (IsStructuralFarEnough(
                    direction,
                    market,
                    structuralStop,
                    room))
                {
                    candidate =
                        BetterStop(
                            direction,
                            structuralStop,
                            candidate)
                            ? structuralStop
                            : candidate;

                    reason =
                        pressureTighten > 0
                            ? "PRESSURE-TIGHTENED STRUCTURAL TRAIL"
                            : momentumAligned
                                ? "MOMENTUM STRUCTURAL TRAIL"
                                : "STRUCTURAL TRAIL";
                }
            }

            if (!IsPositive(candidate))
                candidate = currentStop;

            if (!BetterStop(
                    direction,
                    candidate,
                    currentStop))
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    reason);
            }

            double minimumStep =
                atr *
                Math.Max(
                    0.01,
                    trailStepAtr);

            if (Math.Abs(
                    candidate -
                    currentStop) <
                Math.Max(
                    pipSize,
                    minimumStep))
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "ADVANCE BELOW TRAIL STEP");
            }

            // The pure rule never accepts a candidate that already crosses the
            // executable market-side minimum distance. The caller performs the
            // final broker-specific managed-stop validation as before.
            double protectiveBoundary =
                direction == 1
                    ? market - minimumDistance
                    : market + minimumDistance;

            if (direction == 1 &&
                candidate >= protectiveBoundary)
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "STOP TOO CLOSE TO MARKET");

            if (direction == -1 &&
                candidate <= protectiveBoundary)
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "STOP TOO CLOSE TO MARKET");

            return new IntelligentProtectionDecision(
                candidate,
                true,
                reason);
        }

        private static bool IsPositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }

        private static bool BetterStop(
            int direction,
            double proposed,
            double current)
        {
            if (!IsPositive(proposed))
                return false;

            if (!IsPositive(current))
                return true;

            return direction == 1
                ? proposed > current
                : proposed < current;
        }

        private static bool IsStructuralFarEnough(
            int direction,
            double market,
            double structural,
            double room)
        {
            if (!IsPositive(structural) ||
                !IsNonNegative(room))
                return false;

            return direction == 1
                ? structural <= market - room
                : structural >= market + room;
        }
    }
}
