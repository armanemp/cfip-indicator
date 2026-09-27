// ============================================================================
// CFIP Indicator — PendingCreatedHandler.cs
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
        private void OnPendingOrderCreated(
                                    PendingOrderCreatedEventArgs args)
                                {
                                    if (args == null ||
                                        args.PendingOrder == null ||
                                        !IsManagedPendingOrder(args.PendingOrder))
                                        return;
                        
                                    SetLifecycleState(
                                        LifecycleState.PendingOrder,
                                        "PENDING ORDER CREATED #" +
                                        args.PendingOrder.Id);
                                }
    }
}
