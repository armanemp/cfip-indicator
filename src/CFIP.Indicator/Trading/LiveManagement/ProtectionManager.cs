// ============================================================================
// CFIP Indicator — ProtectionManager.cs
// ============================================================================

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double CalculateProtectedStop(
            double market,
            double peakRR,
            int closedM5,
            bool structuralUpdate)
        {
            if (_plan == null ||
                !IsFinitePositive(market) ||
                double.IsNaN(peakRR) ||
                double.IsInfinity(peakRR))
                return 0;

            double atr =
                _m5Frame != null &&
                _m5Frame.Index == closedM5 &&
                _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : Atr(
                        _m5Bars,
                        closedM5);

            if (!IsFinitePositive(atr) ||
                !IsFinitePositive(_plan.Risk))
                return IsValidManagedStop(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    _plan.Stop)
                    ? _plan.Stop
                    : 0;

            double structuralStop = 0;

            if (UseSwingStructureInTrail)
            {
                structuralStop =
                    _plan.Direction == 1
                        ? FindSwingLowBelow(
                            _m5Bars,
                            closedM5,
                            market)
                        : FindSwingHighAbove(
                            _m5Bars,
                            closedM5,
                            market);
            }

            double spreadPips =
                Math.Max(
                    0,
                    (Symbol.Ask - Symbol.Bid) /
                    Math.Max(
                        Symbol.PipSize,
                        1e-9));

            int pressure =
                CalculateSmartExitPressure(
                    market,
                    peakRR);

            bool momentumAligned =
                _m5Frame != null &&
                (_plan.Direction == 1
                    ? _m5Frame.MomentumBull &&
                      _m5Frame.StructureBull
                    : _m5Frame.MomentumBear &&
                      _m5Frame.StructureBear);

            AdaptiveProtectionProfile protectionProfile =
                AdaptiveProtectionProfileRule.Resolve(
                    _decision == null
                        ? "UNKNOWN"
                        : _decision.Regime,
                    _plan.Risk /
                    Math.Max(
                        Symbol.PipSize,
                        atr),
                    _decision == null
                        ? 0
                        : _decision.Confidence,
                    _decision == null
                        ? 0
                        : _decision.SmartQuality,
                    pressure,
                    momentumAligned,
                    SlRepriceBreathingAtr);

            IntelligentProtectionDecision decision =
                IntelligentProtectionRule.Evaluate(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    _plan.Stop,
                    _plan.Risk,
                    peakRR,
                    atr,
                    structuralStop,
                    spreadPips,
                    Symbol.PipSize,
                    Math.Max(
                        Symbol.TickSize,
                        MinimumProtectionDistancePriceForDirection(
                            _plan.Direction)),
                    protectionProfile.BreakEvenRewardAtr,
                    BreakEvenBufferPips,
                    RiskFreeLockPips,
                    MoveSlToBreakEven,
                    UseSpreadAwareBreakEven,
                    EnableStructuralSlRepricing,
                    structuralUpdate,
                    StructuralStopManagementOnly,
                    UseSwingStructureInTrail,
                    protectionProfile.TrailStartRewardAtr,
                    protectionProfile.BreathingAtr,
                    protectionProfile.TightenRewardAtr,
                    SmartTrailMomentumBonusAtr,
                    SlRepriceBreathingAtr,
                    pressure,
                    SmartExitPressureThreshold,
                    momentumAligned,
                    TrailStepAtr,
                    _serverSideBreakEvenActive,
                    _plan.Tp1);

            double candidate =
                NormalizePrice(
                    decision.Stop);

            if (!IsFinitePositive(candidate))
                return _plan.Stop;

            double minimumDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumProtectionDistancePriceForDirection(
                        _plan.Direction));

            if (!LiveExitGeometryRule.IsProtectiveStop(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    candidate,
                    minimumDistance))
                return _plan.Stop;

            if (!IsValidManagedStop(
                _plan.Direction,
                _plan.Entry,
                market,
                candidate))
                return IsValidManagedStop(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    _plan.Stop)
                    ? _plan.Stop
                    : 0;

            return
                ProtectionProgressionRule.ShouldAdvanceStop(
                    _plan.Direction,
                    _plan.Stop,
                    candidate)
                    ? candidate
                    : _plan.Stop;
        }
    }
}
