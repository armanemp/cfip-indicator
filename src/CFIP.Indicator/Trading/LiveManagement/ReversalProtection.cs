// ============================================================================
// CFIP Indicator — ReversalProtection.cs
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
        private bool CheckProfitExhaustionExit(
                                            int closedM5,
                                            double market,
                                            double peakRR)
                                        {
                                            if (!EnableProfitExhaustionProtection ||
                                                _plan == null ||
                                                !_plan.IsLivePosition ||
                                                peakRR <
                                                Math.Max(
                                                    0.8,
                                                    ExhaustionMinimumPeakRR) ||
                                                _plan.Risk <= 0)
                                                return false;
                                
                                            double favorable =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk) *
                                                peakRR;
                                
                                            if (favorable <= 0)
                                                return false;
                                
                                            double retracement =
                                                _plan.Direction == 1
                                                    ? _peakPrice - market
                                                    : market - _peakPrice;
                                
                                            if (retracement <= 0)
                                                return false;
                                
                                            double retracementPercent =
                                                retracement /
                                                favorable *
                                                100.0;
                                
                                            if (retracementPercent <
                                                Math.Max(
                                                    15,
                                                    ExhaustionRetracementPercent))
                                                return false;
                                
                                            double currentRR =
                                                _plan.Direction == 1
                                                    ? (market - _plan.Entry) /
                                                      Math.Max(
                                                          Symbol.PipSize,
                                                          _plan.Risk)
                                                    : (_plan.Entry - market) /
                                                      Math.Max(
                                                          Symbol.PipSize,
                                                          _plan.Risk);
                                
                                            int pressure =
                                                CalculateSmartExitPressure(
                                                    market,
                                                    currentRR);
                                
                                            if (pressure <
                                                Math.Max(
                                                    55,
                                                    ExhaustionPressureThreshold))
                                                return false;
                                
                                            int opposite =
                                                _plan.Direction * -1;
                                
                                            bool oppositeStructure =
                                                _m5Frame != null &&
                                                (opposite == 1
                                                    ? (_m5Frame.StructureBull ||
                                                       _m5Frame.MssBull ||
                                                       _m5Frame.ChochBull)
                                                    : (_m5Frame.StructureBear ||
                                                       _m5Frame.MssBear ||
                                                       _m5Frame.ChochBear));
                                
                                            bool oppositeReaction =
                                                _reaction != null &&
                                                _reaction.Direction == opposite &&
                                                _reaction.Confidence >=
                                                Math.Max(
                                                    LiveReactionThreshold,
                                                    LiveReversalMinimumConfidence) &&
                                                _reaction.IndependentEvidence >=
                                                Math.Max(
                                                    1,
                                                    ExhaustionMinimumOppositeEvidence);
                                
                                            if (!oppositeStructure &&
                                                !oppositeReaction)
                                                return false;
                                
                                            Position position =
                                                GetManagedLivePositionForPlan();
                                
                                            if (position == null ||
                                                position.NetProfit <= 0)
                                                return false;
                                
                                            double protectedProfit =
                                                position.NetProfit;
                                
                                            if (!RequestLivePlanExit(
                                                    closedM5,
                                                    "PROFIT EXHAUSTION"))
                                                return false;
                                
                                            SendUnifiedAlert(
                                                "EXHAUSTION-CLOSE|" +
                                                position.Id,
                                                "CFIP EXHAUSTION EXIT REQUESTED | #" +
                                                position.Id +
                                                " | +" +
                                                protectedProfit.ToString("F2") +
                                                " | PEAK RR " +
                                                peakRR.ToString("F2") +
                                                " | RETRACE " +
                                                retracementPercent.ToString("F0") +
                                                "% | PRESSURE " +
                                                pressure,
                                                _plan.Direction,
                                                true);
                                
                                            SetAutoTradingState(
                                                "EXECUTED",
                                                "EXHAUSTION EXIT REQUESTED #" +
                                                position.Id);
                                
                                            return true;
                                        }
        
        private bool CheckStructuralSetupInvalidation(
                                            int closedM5,
                                            double market)
                                        {
                                            if (!EnableSetupInvalidation ||
                                                _plan == null ||
                                                !_plan.IsLivePosition ||
                                                _m5Bars == null ||
                                                closedM5 < 30 ||
                                                !IsFinitePositive(market))
                                                return false;
                                
                                            double atr =
                                                Atr(
                                                    _m5Bars,
                                                    closedM5);
                                
                                            if (!IsFinitePositive(atr))
                                                return false;
                                
                                            int swingLookback =
                                                Math.Max(
                                                    10,
                                                    Math.Min(
                                                        StructureLookback,
                                                        closedM5 - 1));
                                
                                            int swingStart =
                                                Math.Max(
                                                    1,
                                                    closedM5 -
                                                    swingLookback);
                                
                                            double swingHigh =
                                                _m5Bars.HighPrices[swingStart];
                                
                                            double swingLow =
                                                _m5Bars.LowPrices[swingStart];
                                
                                            for (int i = swingStart + 1;
                                                 i < closedM5;
                                                 i++)
                                            {
                                                swingHigh =
                                                    Math.Max(
                                                        swingHigh,
                                                        _m5Bars.HighPrices[i]);
                                
                                                swingLow =
                                                    Math.Min(
                                                        swingLow,
                                                        _m5Bars.LowPrices[i]);
                                            }
                                
                                            double structureBuffer =
                                                atr *
                                                Math.Max(
                                                    0.02,
                                                    InvalidationStructureAtr);
                                
                                            bool structureFailure =
                                                _plan.Direction == 1
                                                    ? swingLow > 0 &&
                                                      market <
                                                      swingLow -
                                                      structureBuffer
                                                    : swingHigh > 0 &&
                                                      market >
                                                      swingHigh +
                                                      structureBuffer;
                                
                                            double risk =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk);
                                
                                            double adverseR =
                                                _plan.Direction == 1
                                                    ? (_plan.Entry - market) /
                                                      risk
                                                    : (market - _plan.Entry) /
                                                      risk;
                                
                                            double maxAdverseR =
                                                Math.Max(
                                                    0.30,
                                                    InvalidationMaxAdverseR);
                                
                                            if (adverseR >= maxAdverseR)
                                                structureFailure = true;
                                
                                            bool mtfFlip = false;
                                
                                            if (_m5Frame != null)
                                            {
                                                mtfFlip =
                                                    _plan.Direction == 1
                                                        ? _m5Frame.Direction == -1 &&
                                                          (_m5Frame.MssBear ||
                                                           _m5Frame.ChochBear)
                                                        : _m5Frame.Direction == 1 &&
                                                          (_m5Frame.MssBull ||
                                                           _m5Frame.ChochBull);
                                            }
                                
                                            double zoneTolerance =
                                                atr *
                                                Math.Max(
                                                    0.02,
                                                    InvalidationZoneCloseAtr);
                                
                                            bool zoneFailure =
                                                _plan.Direction == 1
                                                    ? market <
                                                      _plan.Stop -
                                                      zoneTolerance &&
                                                      (_m5Frame == null ||
                                                       _m5Frame.StructureBear ||
                                                       _m5Frame.MssBear ||
                                                       _m5Frame.ChochBear)
                                                    : market >
                                                      _plan.Stop +
                                                      zoneTolerance &&
                                                      (_m5Frame == null ||
                                                       _m5Frame.StructureBull ||
                                                       _m5Frame.MssBull ||
                                                       _m5Frame.ChochBull);
                                
                                            bool invalid =
                                                (structureFailure ||
                                                 zoneFailure) &&
                                                (!RequireMtfFlipForInvalidation ||
                                                 mtfFlip ||
                                                 adverseR >= maxAdverseR);
                                
                                            if (!invalid)
                                                return false;
                                
                                            int score =
                                                (structureFailure ? 40 : 0) +
                                                (zoneFailure ? 30 : 0) +
                                                (mtfFlip ? 30 : 0);
                                
                                            if (AlertOnInvalidated &&
                                                _lastInvalidationAlertM5 !=
                                                closedM5)
                                            {
                                                SendUnifiedAlert(
                                                    "STRUCT-INVALID|" +
                                                    closedM5,
                                                    "CFIP STRUCTURAL INVALIDATION | " +
                                                    (_plan.Direction == 1
                                                        ? "BUY"
                                                        : "SELL") +
                                                    " | SCORE " +
                                                    ClampInt(
                                                        score,
                                                        0,
                                                        100),
                                                    0,
                                                    true);
                                
                                                _lastInvalidationAlertM5 =
                                                    closedM5;
                                            }
                                
                                            Position position =
                                                GetManagedPositionById(
                                                    _plan.PositionId);
                                
                                            if (position == null)
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.Closed,
                                                    "STRUCTURE INVALIDATED • POSITION ALREADY CLOSED");
                                                return false;
                                            }
                                
                                            SetLifecycleState(
                                                LifecycleState.ExitRequested,
                                                "STRUCTURAL INVALIDATION");
                                
                                            if (!TryClosePosition(
                                                    position,
                                                    "STRUCTURAL INVALIDATION"))
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.RecoveryRequired,
                                                    "STRUCTURAL EXIT REJECTED");
                                
                                                _autoExecutionBlockReason =
                                                    "STRUCTURAL EXIT REJECTED";
                                
                                                SendUnifiedAlert(
                                                    "STRUCT-INVALID-EXIT-FAILED|" +
                                                    position.Id,
                                                    "CFIP STRUCTURAL INVALIDATION • BROKER EXIT REJECTED | #" +
                                                    position.Id,
                                                    _plan.Direction,
                                                    true);
                                            }
                                
                                            _lastExitM5 = closedM5;
                                
                                            // _plan remains authoritative until OnPositionClosed confirms
                                            // that the broker position is actually gone.
                                            return true;
                                        }
        
        private int CalculateSmartExitPressure(
                                            double market,
                                            double currentRR)
                                        {
                                            if (_plan == null)
                                                return 0;
                                
                                            int pressure = 0;
                                            int opposite = _plan.Direction * -1;
                                
                                            if (_reaction != null &&
                                                _reaction.Direction == opposite)
                                            {
                                                pressure +=
                                                    Math.Min(
                                                        30,
                                                        Math.Max(
                                                            0,
                                                            _reaction.Confidence / 3));
                                
                                                if (_reaction.IndependentEvidence >=
                                                    MinimumLiveReactionEvidence)
                                                    pressure += 10;
                                            }
                                
                                            if (_m5Frame != null)
                                            {
                                                bool structure =
                                                    opposite == 1
                                                        ? _m5Frame.StructureBull
                                                        : _m5Frame.StructureBear;
                                
                                                bool reversal =
                                                    opposite == 1
                                                        ? (_m5Frame.MssBull ||
                                                           _m5Frame.ChochBull)
                                                        : (_m5Frame.MssBear ||
                                                           _m5Frame.ChochBear);
                                
                                                bool force =
                                                    opposite == 1
                                                        ? (_m5Frame.DisplacementBull &&
                                                           _m5Frame.LiquidityBull)
                                                        : (_m5Frame.DisplacementBear &&
                                                           _m5Frame.LiquidityBear);
                                
                                                if (structure)
                                                    pressure += 15;
                                
                                                if (reversal)
                                                    pressure += 15;
                                
                                                if (force)
                                                    pressure += 20;
                                            }
                                
                                            double atr =
                                                Atr(
                                                    _m5Bars,
                                                    Math.Max(
                                                        1,
                                                        _m5Bars.Count - 2));
                                
                                            if (atr > 0)
                                            {
                                                Zone zone =
                                                    FindNearestOpposingZone(
                                                        _m5Bars,
                                                        Math.Max(
                                                            1,
                                                            _m5Bars.Count - 2),
                                                        _plan.Direction,
                                                        atr);
                                
                                                if (zone != null &&
                                                    DistanceToZone(
                                                        market,
                                                        zone) <=
                                                    atr *
                                                    Math.Max(
                                                        0.05,
                                                        ZoneProximityAtr))
                                                    pressure += 10;
                                            }
                                
                                            if (currentRR < 0)
                                                pressure += 10;
                                
                                            return ClampInt(
                                                pressure,
                                                0,
                                                100);
                                        }
        
        private string GetSmartExitMode()
                                        {
                                            if (_plan == null)
                                                return "NO ACTIVE PLAN";
                                
                                            double risk =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk);
                                
                                            double currentRR =
                                                _plan.Direction == 1
                                                    ? (_lastMarket - _plan.Entry) / risk
                                                    : (_plan.Entry - _lastMarket) / risk;
                                
                                            int pressure =
                                                CalculateSmartExitPressure(
                                                    _lastMarket,
                                                    currentRR);
                                
                                            if (pressure >=
                                                SmartExitPressureThreshold)
                                                return "PROTECT";
                                
                                            if (pressure >=
                                                LiveReactionWatchThreshold)
                                                return "WATCH";
                                
                                            return "HOLD";
                                        }
    }
}
