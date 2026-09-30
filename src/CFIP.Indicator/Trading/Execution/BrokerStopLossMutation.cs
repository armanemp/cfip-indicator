// CFIP Indicator — BrokerStopLossMutation.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryModifyStopLoss(
                            Position position,
                            double price,
                            string context)
                        {
                            if (position == null ||
                                !IsFinitePositive(price))
                                return false;
                
                            double normalized =
                                NormalizePrice(price);
                
                            if (!IsFinitePositive(normalized))
                                return false;
                
                            try
                            {
                                MarkBrokerStateDirty();

                                TradeResult result =
                                    position.ModifyStopLossPrice(normalized);
                
                                if (!BrokerConfirmationPolicy.IsSuccessfulMutation(
                                        result != null,
                                        result != null &&
                                        result.IsSuccessful))
                                {
                                    Print(
                                        "CFIP SL mutation rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP SL mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }
    }
}
