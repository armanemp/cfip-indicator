// CFIP Indicator — BrokerTakeProfitMutation.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryModifyTakeProfit(
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
                                TradeResult result =
                                    position.ModifyTakeProfitPrice(normalized);
                
                                if (result == null ||
                                    !result.IsSuccessful)
                                {
                                    Print(
                                        "CFIP TP mutation rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP TP mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }
    }
}
