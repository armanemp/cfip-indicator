// ============================================================================
// CFIP Indicator — PositionModifiedHandler.cs
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
        private void OnPositionModified(
                                    PositionModifiedEventArgs args)
                                {

                                    if (args == null ||
                                        args.Position == null ||
                                        !IsManagedPosition(args.Position))
                                        return;

                                    MarkBrokerStateDirty();
                        
                                    Position position =
                                        args.Position;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;

                                    bool boundToActivePlan =
                                        _plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId > 0 &&
                                        _plan.PositionId == position.Id;

                                    if (!boundToActivePlan)
                                        return;

                                    bool protectionHealthy =
                                        EvaluateBrokerProtection(
                                            position,
                                            out bool brokerStopValid,
                                            out bool brokerTargetValid);

                                    ApplyBrokerConfirmedProtectionState(
                                        position.Id,
                                        position.EntryPrice,
                                        brokerStopValid
                                            ? position.StopLoss
                                            : (double?)null,
                                        brokerTargetValid
                                            ? position.TakeProfit
                                            : (double?)null,
                                        true);

                                    if (_plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId == position.Id &&
                                        _lifecycleState !=
                                            LifecycleState.ExitRequested)
                                    {
                                        _brokerProtectionRecoveryRequired =
                                            !protectionHealthy;
                        
                                        SetLifecycleState(
                                            _brokerProtectionRecoveryRequired
                                                ? LifecycleState.RecoveryRequired
                                                : LifecycleState.LivePosition,
                                            _brokerProtectionRecoveryRequired
                                                ? "BROKER POSITION MODIFIED • PROTECTION MISSING OR INVALID"
                                                : "BROKER POSITION MODIFIED");
                                    }
                                }
    }
}
