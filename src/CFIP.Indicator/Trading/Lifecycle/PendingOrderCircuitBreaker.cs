// CFIP Indicator — PendingOrderCircuitBreaker.cs
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
        private void CancelAllOrders()
                                {
                                    bool allCancelledOrAbsent = true;
                        
                                    foreach (PendingOrder order in PendingOrders)
                                    {
                                        if (!IsManagedPendingOrder(order))
                                            continue;
                        
                                        ManagementCommandRequestStatus cancelStatus =
                                            RequestCancelPendingOrder(
                                                order,
                                                "PENDING CIRCUIT BREAKER");
                                        if (!cancelStatus.IsAccepted())
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
