// ============================================================================
// CFIP Indicator — PositionClosedHandler.cs
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
        private void OnPositionClosed(PositionClosedEventArgs args)
                                {

                                    if (args == null ||
                                        !IsManagedPosition(args.Position))
                                        return;

            MarkBrokerStateDirty();
                        

                                    if (!_lifecycleEventGuard.TryBegin(
                                            "POSITION_CLOSED",
                                            args.Position.Id))
                                        return;
                        
                                    _lastExitM5 =
                                        Math.Max(
                                            _lastExitM5,
                                            _lastEvaluatedM5);
                        
                                    int direction =
                                        args.Position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;
                        
                                    if (!_outcomeRegistered)
                                    {
                                        bool profitable =
                                            args.Position.NetProfit > 0;

                                        Plan outcomePlan =
                                            _plan != null &&
                                            _plan.IsLivePosition &&
                                            _plan.PositionId ==
                                            args.Position.Id
                                                ? _plan
                                                : null;

                                        if (EnableOutcomeTelemetry)
                                        {
                                            RecordManagedOutcome(
                                                outcomePlan,
                                                args.Position,
                                                _lastEvaluatedM5);
                                        }

                                        _outcomeRegistered = true;

                                        if (profitable)
                                            _wins++;
                                        else
                                            _losses++;
                                    }
                        
                                    _brokerProtectionRecoveryRequired = false;
                        
                                    if (_plan != null &&
                                        _plan.IsLivePosition &&
                                        _plan.PositionId ==
                                        args.Position.Id)
                                    {
                                        _plan = null;
                                        _activeBrokerStop = 0;
                                        _activeBrokerTarget = 0;
                                        _executionModel = null;
                        
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            args.Position.NetProfit > 0
                                                ? "POSITION CLOSED • PROFIT"
                                                : "POSITION CLOSED • LOSS");
                        
                                        RemovePlanObjects();
                                    }
                                }
    }
}
