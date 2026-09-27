// CFIP Indicator — BrokerPendingOrderCancellation.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryCancelPendingOrder(
                            PendingOrder order,
                            string context)
                        {
                            if (order == null)
                                return false;
                
                            try
                            {
                                TradeResult result =
                                    CancelPendingOrder(order);
                
                                if (result == null ||
                                    !result.IsSuccessful)
                                {
                                    Print(
                                        "CFIP pending cancel rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP pending cancel failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }
    }
}
