// ============================================================================
// CFIP Indicator — ProtectionManager.cs
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

            double candidate =
                _plan.Stop;

            double atr =
                _m5Frame != null &&
                _m5Frame.Index == closedM5 &&
                _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : Atr(
                        _m5Bars,
                        closedM5);

            if (!_serverSideBreakEvenActive &&
                MoveSlToBreakEven &&
                peakRR >= BreakEvenTriggerRR)
            {
                double be =
                    _plan.Direction == 1
                        ? _plan.Entry +
                          BreakEvenBufferPips *
                          Symbol.PipSize
                        : _plan.Entry -
                          BreakEvenBufferPips *
                          Symbol.PipSize;

                candidate =
                    _plan.Direction == 1
                        ? Math.Max(candidate, be)
                        : Math.Min(candidate, be);
            }

            if (!_serverSideBreakEvenActive &&
                UseSpreadAwareBreakEven &&
                MoveSlToBreakEven &&
                peakRR >= Math.Max(0.50, BreakEvenTriggerRR))
            {
                double spreadPips =
                    Math.Max(
                        0,
                        (Symbol.Ask - Symbol.Bid) /
                        Math.Max(Symbol.PipSize, 1e-9));

                double lockPips =
                    Math.Max(
                        RiskFreeLockPips,
                        spreadPips + BreakEvenBufferPips);

                double be =
                    _plan.Direction == 1
                        ? _plan.Entry + lockPips * Symbol.PipSize
                        : _plan.Entry - lockPips * Symbol.PipSize;

                if (IsValidManagedStop(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    be))
                {
                    candidate =
                        _plan.Direction == 1
                            ? Math.Max(candidate, be)
                            : Math.Min(candidate, be);
                }
            }

            if (EnableStructuralSlRepricing &&
                (structuralUpdate ||
                 !StructuralStopManagementOnly) &&
                peakRR >=
                Math.Max(
                    SlRepriceStartRR,
                    SmartTrailMinimumRR) &&
                UseSwingStructureInTrail)
            {
                if (atr > 0)
                {
                    double structural =
                        _plan.Direction == 1
                            ? FindSwingLowBelow(
                                _m5Bars,
                                closedM5,
                                market)
                            : FindSwingHighAbove(
                                _m5Bars,
                                closedM5,
                                market);

                    if (IsFinitePositive(structural))
                    {
                        double room =
                            atr *
                            Math.Max(
                                0.10,
                                TrailDistanceAtr);

                        if (_plan.Direction == 1 &&
                            structural <= market - room)
                            candidate =
                                Math.Max(candidate, structural);
                        else if (_plan.Direction == -1 &&
                                 structural >= market + room)
                            candidate =
                                Math.Min(candidate, structural);
                    }
                }
            }

            int pressure =
                CalculateSmartExitPressure(
                    market,
                    peakRR);

            // Smart trailing is structural rather than a raw market-price chase.
            // Once break-even is secured, further progression is evaluated only on a
            // newly closed M5 bar and from a structural swing candidate.
            if (structuralUpdate &&
                peakRR >=
                Math.Max(
                    1.0,
                    SmartTrailTightenAtRR) &&
                UseSwingStructureInTrail &&
                atr > 0)
            {
                bool momentumAligned =
                    _m5Frame != null &&
                    (_plan.Direction == 1
                        ? _m5Frame.MomentumBull && _m5Frame.StructureBull
                        : _m5Frame.MomentumBear && _m5Frame.StructureBear);

                double pressureTighten =
                    pressure >=
                    SmartExitPressureThreshold
                        ? Math.Min(
                            0.20,
                            Math.Max(
                                0,
                                SlRepriceBreathingAtr))
                        : 0;

                double room =
                    atr *
                    Math.Max(
                        0.10,
                        TrailDistanceAtr -
                        (momentumAligned
                            ? SmartTrailMomentumBonusAtr
                            : 0) -
                        pressureTighten);

                double structural =
                    _plan.Direction == 1
                        ? FindSwingLowBelow(
                            _m5Bars,
                            closedM5,
                            market)
                        : FindSwingHighAbove(
                            _m5Bars,
                            closedM5,
                            market);

                if (IsFinitePositive(structural))
                {
                    bool enoughRoom =
                        _plan.Direction == 1
                            ? structural <= market - room
                            : structural >= market + room;

                    bool valid =
                        enoughRoom &&
                        IsValidManagedStop(
                            _plan.Direction,
                            _plan.Entry,
                            market,
                            structural);

                    if (valid)
                    {
                        candidate =
                            _plan.Direction == 1
                                ? Math.Max(candidate, structural)
                                : Math.Min(candidate, structural);
                    }
                }
            }

            candidate =
                NormalizePrice(candidate);

            if (!LiveExitGeometryRule.IsProtectiveStop(
                    _plan.Direction,
                    _plan.Entry,
                    market,
                    candidate,
                    Math.Max(
                        Symbol.TickSize,
                        MinimumProtectionDistancePriceForDirection(
                            _plan.Direction)))
                return _plan.Stop;

            // Never derive a new stop merely because the market moved. If a
            // structural candidate is temporarily invalid against broker price
            // constraints, keep the last protected level unchanged.
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

            double atrValue = atr;

            double step =
                atrValue *
                Math.Max(
                    0.01,
                    TrailStepAtr);

            if (!BetterStop(
                _plan.Direction,
                candidate,
                _plan.Stop) ||
                Math.Abs(
                    candidate -
                    _plan.Stop) <
                Math.Max(
                    Symbol.PipSize,
                    step))
                return _plan.Stop;

            return candidate;
        }

        private bool BetterStop(
            int direction,
            double proposed,
            double current)
        {
            if (!IsFinitePositive(proposed) ||
                !IsFinitePositive(current))
                return false;

            return direction == 1
                ? proposed > current
                : proposed < current;
        }
    }
}