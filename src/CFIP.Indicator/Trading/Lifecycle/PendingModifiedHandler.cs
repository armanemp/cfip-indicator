// ============================================================================
// CFIP Indicator — PendingModifiedHandler.cs
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
        private void OnPendingOrderModified(
                                    PendingOrderModifiedEventArgs args)
                                {
                                    if (args == null ||
                                        args.PendingOrder == null ||
                                        !IsManagedPendingOrder(args.PendingOrder))
                                        return;
                        
                                    SetLifecycleState(
                                        LifecycleState.PendingOrder,
                                        "PENDING ORDER MODIFIED #" +
                                        args.PendingOrder.Id);
                                }
    }
}
