// ============================================================================
// CFIP Indicator — PendingCancelledHandler.cs
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
        private void OnPendingOrderCancelled(
                                    PendingOrderCancelledEventArgs args)
                                {

                                    if (args == null ||
                                        args.PendingOrder == null ||
                                        !IsManagedPendingOrder(args.PendingOrder))
                                        return;

                                    MarkBrokerStateDirty();
                        

                                    if (!_lifecycleEventGuard.TryBegin(
                                            "PENDING_CANCELLED",
                                            args.PendingOrder.Id))
                                        return;
                        
                                    PendingOrder remaining =
                                        GetManagedPendingOrder();
                        
                                    if (remaining == null &&
                                        GetManagedPosition() == null &&
                                        _plan == null)
                                    {
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            "PENDING ORDER CANCELLED #" +
                                            args.PendingOrder.Id);
                                    }
                                    else if (remaining == null &&
                                             GetManagedPosition() != null)
                                    {
                                        SetLifecycleState(
                                            LifecycleState.LivePosition,
                                            "PENDING ORDER CANCELLED • LIVE POSITION EXISTS");
                                    }
                                }
    }
}
