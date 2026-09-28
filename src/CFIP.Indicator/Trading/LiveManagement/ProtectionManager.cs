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
            if (_plan == null)
                return 0;

            double candidate =
                _plan.Stop;

            if (MoveSlToBreakEven &&
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

            if (UseSpreadAwareBreakEven &&
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

            if ((EnableStructuralSlRepricing ||
                 EnableDynamicSlTrail) &&
                (structuralUpdate ||
                 !StructuralStopManagementOnly) &&
                peakRR >=
                Math.Max(
                    SlRepriceStartRR,
                    SmartTrailMinimumRR) &&
                UseSwingStructureInTrail)
            {
                double atr =
                    Atr(
                        _m5Bars,
                        closedM5);

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

            if (peakRR >= Math.Max(1.0, SmartTrailTightenAtRR))
            {
                double trailAtr =
                    Atr(_m5Bars, closedM5);

                bool momentumAligned =
                    _m5Frame != null &&
                    (_plan.Direction == 1
                        ? _m5Frame.MomentumBull && _m5Frame.StructureBull
                        : _m5Frame.MomentumBear && _m5Frame.StructureBear);

                if (trailAtr > 0 &&
                    momentumAligned)
                {
                    double room =
                        trailAtr *
                        Math.Max(
                            0.08,
                            TrailDistanceAtr -
                            SmartTrailMomentumBonusAtr);

                    double tightened =
                        _plan.Direction == 1
                            ? market - room
                            : market + room;

                    if (IsValidManagedStop(
                        _plan.Direction,
                        _plan.Entry,
                        market,
                        tightened))
                    {
                        candidate =
                            _plan.Direction == 1
                                ? Math.Max(candidate, tightened)
                                : Math.Min(candidate, tightened);
                    }
                }
            }

            if (!StructuralStopManagementOnly &&
                pressure >= SmartExitPressureThreshold &&
                peakRR >= SmartTrailMinimumRR)
            {
                double atr =
                    Atr(
                        _m5Bars,
                        closedM5);

                if (atr > 0)
                {
                    double tightRoom =
                        atr *
                        Math.Min(
                            0.45,
                            Math.Max(
                                0.10,
                                SlRepriceBreathingAtr));

                    double tightened =
                        _plan.Direction == 1
                            ? market - tightRoom
                            : market + tightRoom;

                    if (IsValidManagedStop(
                        _plan.Direction,
                        _plan.Entry,
                        market,
                        tightened))
                    {
                        candidate =
                            _plan.Direction == 1
                                ? Math.Max(
                                    candidate,
                                    tightened)
                                : Math.Min(
                                    candidate,
                                    tightened);
                    }
                }
            }

            double minimumDistance =
                Math.Max(
                    Symbol.PipSize * 2,
                    Symbol.Ask - Symbol.Bid);

            candidate =
                _plan.Direction == 1
                    ? Math.Min(
                        candidate,
                        market - minimumDistance)
                    : Math.Max(
                        candidate,
                        market + minimumDistance);

            candidate =
                NormalizePrice(candidate);

            if (!IsValidManagedStop(
                _plan.Direction,
                _plan.Entry,
                market,
                candidate))
                return _plan.Stop;

            double atrValue =
                Atr(
                    _m5Bars,
                    closedM5);

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