// CFIP Indicator — LivePlanExitCoordinator.cs
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
        private bool RequestLivePlanExit(
                                    int closedM5,
                                    string reason)
                                {
                                    if (_plan == null ||
                                        !_plan.IsLivePosition)
                                        return false;
                        
                                    Position position =
                                        GetManagedLivePositionForPlan();
                        
                                    if (position == null)
                                    {
                                        _plan = null;
                                        _activeBrokerStop = 0;
                                        _activeBrokerTarget = 0;
                                        _executionModel = null;
                        
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            reason +
                                            " • POSITION ALREADY CLOSED");
                        
                                        RemovePlanObjects();
                                        return true;
                                    }
                        
                                    SetLifecycleState(
                                        LifecycleState.ExitRequested,
                                        reason);
                        
                                    if (!TryClosePosition(
                                            position,
                                            reason))
                                    {
                                        SetLifecycleState(
                                            LifecycleState.RecoveryRequired,
                                            reason +
                                            " • EXIT REJECTED");
                        
                                        _autoExecutionBlockReason =
                                            reason +
                                            " • EXIT REJECTED";
                        
                                        return false;
                                    }
                        
                                    _lastExitM5 =
                                        Math.Max(
                                            _lastExitM5,
                                            closedM5);
                        
                                    return true;
                                }
    }
}
