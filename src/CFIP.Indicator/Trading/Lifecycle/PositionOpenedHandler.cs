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
                        
                                    if (_plan != null &&
                                        position.SymbolName == SymbolName &&
                                        _plan.Direction == direction)
                                    {
                                        // The broker event is the authoritative fill boundary. If
                                        // execution code has not associated the position yet, bind it
                                        // here using the managed symbol/side and the actual fill.
                                        _plan.PositionId =
                                            position.Id;
                        
                                        _plan.Entry =
                                            NormalizePrice(
                                                position.EntryPrice);
                        
                                        _plan.IsLivePosition = true;
                        
                                        _activeBrokerStop =
                                            position.StopLoss.HasValue
                                                ? NormalizePrice(
                                                    position.StopLoss.Value)
                                                : 0;
                        
                                        _activeBrokerTarget =
                                            position.TakeProfit.HasValue
                                                ? NormalizePrice(
                                                    position.TakeProfit.Value)
                                                : 0;
                        
                                        _brokerProtectionRecoveryRequired =
                                            !position.StopLoss.HasValue ||
                                            !position.TakeProfit.HasValue;
                        
                                        SetLifecycleState(
                                            _brokerProtectionRecoveryRequired
                                                ? LifecycleState.RecoveryRequired
                                                : LifecycleState.LivePosition,
                                            _brokerProtectionRecoveryRequired
                                                ? "POSITION OPENED • PROTECTION MISSING"
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
