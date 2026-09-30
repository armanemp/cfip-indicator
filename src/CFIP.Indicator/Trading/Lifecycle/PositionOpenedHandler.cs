// ============================================================================
// CFIP Indicator — PositionOpenedHandler.cs
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
        private void OnPositionOpened(
                                    PositionOpenedEventArgs args)
                                {

                                    if (args == null ||
                                        args.Position == null ||
                                        !IsManagedPosition(args.Position))
                                        return;

            MarkBrokerStateDirty();
                        
                                    Position position =
                                        args.Position;
                        
                                    if (!_lifecycleEventGuard.TryBegin(
                                            "POSITION_OPENED",
                                            args.Position.Id))
                                        return;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;
                        
                                    bool boundToActivePlan =
                                        _plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId > 0 &&
                                        _plan.PositionId == position.Id;

                                    if (boundToActivePlan)
                                    {
                                        // The broker event reconciles the already-bound position.
                                        // A pre-trade plan is never rebound to an unrelated
                                        // position sharing the managed label.
                                        _plan.PositionId =
                                            position.Id;
                        
                                        _plan.Entry =
                                            NormalizePrice(
                                                position.EntryPrice);
                        
                                        _plan.IsLivePosition = true;
                        
                                        bool protectionHealthy =
                                            EvaluateBrokerProtection(
                                                position,
                                                out bool brokerStopValid,
                                                out bool brokerTargetValid);

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

                                        _brokerProtectionRecoveryRequired =
                                            !protectionHealthy;

                                        SetLifecycleState(
                                            _brokerProtectionRecoveryRequired
                                                ? LifecycleState.RecoveryRequired
                                                : LifecycleState.LivePosition,
                                            _brokerProtectionRecoveryRequired
                                                ? "POSITION OPENED • PROTECTION MISSING OR INVALID"
                                                : "POSITION OPENED • LIVE");
                                    }
                        
                                    SendUnifiedAlert(
                                        "POSITION-OPEN|" +
                                        position.Id,
                                        "CFIP POSITION OPENED | #" +
                                        position.Id,
                                        direction,
                                        true);
                                }
    }
}
