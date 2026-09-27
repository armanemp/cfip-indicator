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
                        
                                    _activeBrokerStop =
                                        position.StopLoss.HasValue
                                            ? NormalizePrice(position.StopLoss.Value)
                                            : 0;
                        
                                    _activeBrokerTarget =
                                        position.TakeProfit.HasValue
                                            ? NormalizePrice(position.TakeProfit.Value)
                                            : 0;
                        
                                    if (_plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId == position.Id &&
                                        _lifecycleState !=
                                            LifecycleState.ExitRequested)
                                    {
                                        _brokerProtectionRecoveryRequired =
                                            !position.StopLoss.HasValue ||
                                            !position.TakeProfit.HasValue;
                        
                                        SetLifecycleState(
                                            _brokerProtectionRecoveryRequired
                                                ? LifecycleState.RecoveryRequired
                                                : LifecycleState.LivePosition,
                                            _brokerProtectionRecoveryRequired
                                                ? "BROKER POSITION MODIFIED • PROTECTION MISSING"
                                                : "BROKER POSITION MODIFIED");
                                    }
                                }
    }
}
