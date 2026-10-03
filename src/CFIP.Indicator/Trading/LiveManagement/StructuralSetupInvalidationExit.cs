// ============================================================================
// CFIP Indicator — ReversalProtection.cs
// ============================================================================

using System;
using CFIP.Contracts;
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
                                
                                            int swingStart = -1;
                                            int swingEnd = -1;
                                            double swingLevel = 0;
                                            bool hasStructuralCandidate =
                                                _plan.Direction == 1
                                                    ? TryFindLatestSwingLow(
                                                        _m5Bars,
                                                        closedM5,
                                                        Math.Max(1, SwingStrength),
                                                        out swingStart,
                                                        out swingEnd,
                                                        out swingLevel)
                                                    : TryFindLatestSwingHigh(
                                                        _m5Bars,
                                                        closedM5,
                                                        Math.Max(1, SwingStrength),
                                                        out swingStart,
                                                        out swingEnd,
                                                        out swingLevel);

                                            double structureBuffer =
                                                atr *
                                                Math.Max(
                                                    0.02,
                                                    InvalidationStructureAtr);
                                
                                            bool structureFailure =
                                                hasStructuralCandidate &&
                                                (_plan.Direction == 1
                                                    ? market <
                                                      swingLevel -
                                                      structureBuffer
                                                    : market >
                                                      swingLevel +
                                                      structureBuffer);
                                
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
                                
                                            ManagementCommandRequestStatus closeStatus =
                                                RequestClosePosition(
                                                    position,
                                                    "STRUCTURAL INVALIDATION");

                                            if (!closeStatus.IsAccepted())
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

                                                return false;
                                            }

                                            _lastExitM5 =
                                                LiveInvalidationRule.RecordExitM5(
                                                    _lastExitM5,
                                                    closedM5,
                                                    closeStatus.IsBrokerConfirmed());

                                            // _plan remains authoritative until OnPositionClosed confirms
                                            // that the broker position is actually gone.
                                            return true;
                                        }
    }
}
