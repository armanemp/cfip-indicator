// CFIP Indicator — PositionCircuitBreaker.cs
// Single-responsibility lifecycle module.

using System;
using CFIP.Contracts;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CloseAllPositions()
                                {
                                    bool allClosedOrAbsent = true;
                        
                                    foreach (Position position in Positions)
                                    {
                                        if (!IsManagedPosition(position))
                                            continue;
                        
                                        ManagementCommandRequestStatus closeStatus =
                                            TryClosePosition(
                                                position,
                                                "END OF DAY");
                                        if (!closeStatus.IsAccepted())
                                            allClosedOrAbsent = false;
                                    }
                        
                                    if (GetManagedPosition() == null)
                                    {
                                        _plan = null;
                                        _executionModel = null;
                                        _activeBrokerStop = 0;
                                        _activeBrokerTarget = 0;
                        
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            "END OF DAY • CLOSED");
                        
                                        RemovePlanObjects();
                                    }
                                    else if (!allClosedOrAbsent)
                                    {
                                        SetLifecycleState(
                                            LifecycleState.RecoveryRequired,
                                            "END OF DAY • CLOSE REJECTED");
                                    }
                                    else
                                    {
                                        SetLifecycleState(
                                            LifecycleState.ExitRequested,
                                            "END OF DAY • EXIT REQUESTED");
                                    }
                                }
    }
}
