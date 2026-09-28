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
    }
}
