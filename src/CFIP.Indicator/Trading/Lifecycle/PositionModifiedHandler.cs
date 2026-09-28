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
                        
                                    Position position =
                                        args.Position;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;

                                    double market =
                                        direction == 1
                                            ? Symbol.Bid
                                            : Symbol.Ask;

                                    bool brokerStopValid =
                                        position.StopLoss.HasValue &&
                                        IsFinitePositive(
                                            position.StopLoss.Value) &&
                                        IsValidManagedStop(
                                            direction,
                                            position.EntryPrice,
                                            market,
                                            position.StopLoss.Value);

                                    bool brokerTargetValid =
                                        position.TakeProfit.HasValue &&
                                        IsFinitePositive(
                                            position.TakeProfit.Value) &&
                                        IsValidTarget(
                                            direction,
                                            position.EntryPrice,
                                            position.TakeProfit.Value);

                                    _activeBrokerStop =
                                        brokerStopValid
                                            ? NormalizePrice(
                                                position.StopLoss.Value)
                                            : 0;

                                    _activeBrokerTarget =
                                        brokerTargetValid
                                            ? NormalizePrice(
                                                position.TakeProfit.Value)
                                            : 0;

                                    if (_plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId == position.Id &&
                                        _lifecycleState !=
                                            LifecycleState.ExitRequested)
                                    {
                                        _brokerProtectionRecoveryRequired =
                                            !brokerStopValid ||
                                            (SyncBrokerTakeProfit &&
                                             !brokerTargetValid);
                        
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
