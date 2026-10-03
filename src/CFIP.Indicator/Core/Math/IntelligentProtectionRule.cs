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
            Reason = reason ?? string.Empty;
        }
    }

    /// <summary>
    /// Canonical live-stop progression policy.
    ///
    /// The Indicator owns the analytical candidate (structure, pressure,
    /// momentum and profit state); this pure rule combines that candidate with
    /// BE/trailing policy. It never widens an existing stop and never follows
    /// raw market price without structural evidence.
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
            double breakEvenTriggerRewardAtr,
            double breakEvenBufferPips,
            double riskFreeLockPips,
            bool moveToBreakEven,
            bool spreadAwareBreakEven,
            bool enableStructuralRepricing,
            bool structuralUpdate,
            bool structuralOnly,
            bool useSwingStructure,
            double trailStartRewardAtr,
            double trailDistanceAtr,
            double tightenRewardAtr,
            double smartTrailMomentumBonusAtr,
            double slRepriceBreathingAtr,
            int exitPressure,
            int exitPressureThreshold,
            bool momentumAligned,
            double trailStepAtr,
            bool serverBreakEvenActive,
            double tp1)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFiniteProtectionPrice(entry) ||
                !IsFiniteProtectionPrice(market) ||
                !IsFiniteProtectionPrice(risk) ||
                !IsFiniteProtectionNonNegative(peakRR) ||
                !IsFiniteProtectionPrice(atr) ||
                !IsFiniteProtectionPrice(pipSize) ||
                !IsFiniteProtectionNonNegative(minimumDistance))
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "PROTECTION INPUT INVALID");
            }

            double candidate =
                IsFiniteProtectionPrice(currentStop)
                    ? currentStop
                    : 0;

            string reason =
                "KEEP";

            double riskPips =
                risk /
                pipSize;

            double tp1Pips =
                IsFiniteProtectionPrice(tp1)
                    ? Math.Abs(tp1 - entry) /
                      pipSize
                    : 0;

            if (!serverBreakEvenActive &&
                moveToBreakEven &&
                tp1Pips > 0)
            {
                SmartBreakEvenResult be =
                    SmartBreakEvenRule.Evaluate(
                        riskPips,
                        tp1Pips,
                        Math.Max(0, spreadPips),
                        Math.Max(
                            0,
                            breakEvenTriggerRewardAtr /
                            Math.Max(1e-12, risk / atr)),
                        Math.Max(0, breakEvenBufferPips),
                        Math.Max(0, riskFreeLockPips),
                        spreadAwareBreakEven);

                if (be.Allowed &&
                    peakRR * risk / atr >= breakEvenTriggerRewardAtr)
                {
                    double breakEven =
                        direction == 1
                            ? entry + be.OffsetPips * pipSize
                            : entry - be.OffsetPips * pipSize;

                    if (IsBetterStop(
                            direction,
                            breakEven,
                            candidate))
                    {
                        candidate = breakEven;
                        reason = "BREAK-EVEN";
                    }
                }
            }

            bool structuralAllowed =
                enableStructuralRepricing &&
                useSwingStructure &&
                (structuralUpdate || !structuralOnly) &&
                peakRR * risk / atr >=
                Math.Max(0, trailStartRewardAtr) &&
                IsFiniteProtectionPrice(structuralStop);

            if (structuralAllowed)
            {
                double room =
                    atr *
                    Math.Max(
                        0.10,
                        Math.Max(0, trailDistanceAtr));

                if (IsStructuralFarEnough(
                        direction,
                        market,
                        structuralStop,
                        room) &&
                    IsBetterStop(
                        direction,
                        structuralStop,
                        candidate))
                {
                    candidate = structuralStop;
                    reason = "STRUCTURAL TRAIL";
                }
            }

            if (structuralUpdate &&
                useSwingStructure &&
                peakRR * (risk / Math.Max(SymbolTickFloor(), atr)) >=
                Math.Max(0, tightenRewardAtr) &&
                IsFiniteProtectionPrice(structuralStop))
            {
                double pressureTighten =
                    exitPressure >= Math.Max(0, exitPressureThreshold)
                        ? Math.Min(
                            0.20,
                            Math.Max(0, slRepriceBreathingAtr))
                        : 0;

                double room =
                    atr *
                    Math.Max(
                        0.10,
                        Math.Max(0, trailDistanceAtr) -
                        (momentumAligned
                            ? Math.Max(0, smartTrailMomentumBonusAtr)
                            : 0) -
                        pressureTighten);

                if (IsStructuralFarEnough(
                        direction,
                        market,
                        structuralStop,
                        room) &&
                    IsBetterStop(
                        direction,
                        structuralStop,
                        candidate))
                {
                    candidate = structuralStop;
                    reason =
                        pressureTighten > 0
                            ? "PRESSURE-TIGHTENED STRUCTURAL TRAIL"
                            : momentumAligned
                                ? "MOMENTUM STRUCTURAL TRAIL"
                                : "STRUCTURAL TRAIL";
                }
            }

            if (!IsBetterStop(
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
                    Math.Max(0, trailStepAtr));

            if (Math.Abs(candidate - currentStop) <
                Math.Max(
                    pipSize,
                    minimumStep))
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "ADVANCE BELOW TRAIL STEP");
            }

            double protectiveBoundary =
                direction == 1
                    ? market - minimumDistance
                    : market + minimumDistance;

            if (direction == 1 &&
                candidate >= protectiveBoundary)
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "STOP TOO CLOSE TO MARKET");
            }

            if (direction == -1 &&
                candidate <= protectiveBoundary)
            {
                return new IntelligentProtectionDecision(
                    currentStop,
                    false,
                    "STOP TOO CLOSE TO MARKET");
            }

            return new IntelligentProtectionDecision(
                candidate,
                true,
                reason);
        }

        private static bool IsBetterStop(
            int direction,
            double proposed,
            double current)
        {
            if (!IsFiniteProtectionPrice(proposed))
                return false;

            if (!IsFiniteProtectionPrice(current))
                return true;

            return ProtectionProgressionRule.ShouldAdvanceStop(
                direction,
                current,
                proposed);
        }

        private static bool IsStructuralFarEnough(
            int direction,
            double market,
            double stop,
            double room)
        {
            if (!IsFiniteProtectionPrice(market) ||
                !IsFiniteProtectionPrice(stop) ||
                !IsFiniteProtectionNonNegative(room))
                return false;

            return direction == 1
                ? stop <= market - room
                : direction == -1 &&
                  stop >= market + room;
        }

        private static bool IsFiniteProtectionPrice(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteProtectionNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
