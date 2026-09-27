// CFIP Indicator — BrokerStateSnapshot.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SynchronizeLiveBrokerState()
                                {
                                    _activeBrokerStop = 0;
                                    _activeBrokerTarget = 0;
                        
                                    if (_plan == null ||
                                        !_plan.IsLivePosition)
                                        return;
                        
                                    Position position =
                                        GetManagedLivePositionForPlan();
                        
                                    if (position == null)
                                        return;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;
                        
                                    if (IsFinitePositive(position.EntryPrice))
                                        _plan.Entry =
                                            NormalizePrice(position.EntryPrice);
                        
                                    if (position.StopLoss.HasValue &&
                                        IsFinitePositive(position.StopLoss.Value) &&
                                        IsValidStop(
                                            direction,
                                            position.EntryPrice,
                                            position.StopLoss.Value))
                                    {
                                        _activeBrokerStop =
                                            NormalizePrice(position.StopLoss.Value);
                                    }
                        
                                    if (position.TakeProfit.HasValue &&
                                        IsFinitePositive(position.TakeProfit.Value))
                                    {
                                        _activeBrokerTarget =
                                            NormalizePrice(position.TakeProfit.Value);
                                    }
                                }
    }
}
