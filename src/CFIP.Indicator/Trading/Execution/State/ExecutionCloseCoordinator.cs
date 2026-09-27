// ============================================================================
// CFIP Indicator — ExecutionCloseCoordinator.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        
        private Position GetManagedPositionById(long positionId)
                                {
                                    if (positionId <= 0)
                                        return null;
                        
                                    foreach (Position position in Positions)
                                    {
                                        if (position != null &&
                                            position.Id == positionId &&
                                            IsManagedPosition(position))
                                            return position;
                                    }
                        
                                    return null;
                                }
        
        private void CloseAllPositions()
                                {
                                    bool allClosedOrAbsent = true;
                        
                                    foreach (Position position in Positions)
                                    {
                                        if (!IsManagedPosition(position))
                                            continue;
                        
                                        if (!TryClosePosition(
                                                position,
                                                "END OF DAY"))
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
        
        private void CancelAllOrders()
                                {
                                    bool allCancelledOrAbsent = true;
                        
                                    foreach (PendingOrder order in PendingOrders)
                                    {
                                        if (!IsManagedPendingOrder(order))
                                            continue;
                        
                                        if (!TryCancelPendingOrder(
                                                order,
                                                "PENDING CIRCUIT BREAKER"))
                                            allCancelledOrAbsent = false;
                                    }
                        
                                    if (GetManagedPendingOrder() == null)
                                    {
                                        RemoveManagedPendingOrderObjects();
                                    }
                                    else if (!allCancelledOrAbsent)
                                    {
                                        SetLifecycleState(
                                            LifecycleState.RecoveryRequired,
                                            "PENDING CANCEL REJECTED");
                                    }
                                }
    }
}
