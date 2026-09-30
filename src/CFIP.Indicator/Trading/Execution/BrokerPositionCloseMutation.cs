// CFIP Indicator — BrokerPositionCloseMutation.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryClosePosition(
                            Position position,
                            string context,
                            double? volumeInUnits = null)
                        {
                            if (position == null)
                                return false;
                
                            try
                            {
                                MarkBrokerStateDirty();

                                TradeResult result =
                                    volumeInUnits.HasValue
                                        ? ClosePosition(
                                            position,
                                            volumeInUnits.Value)
                                        : ClosePosition(position);
                
                                if (!BrokerConfirmationPolicy.IsSuccessfulMutation(
                                        result != null,
                                        result != null &&
                                        result.IsSuccessful))
                                {
                                    Print(
                                        "CFIP close rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP close failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }
    }
}
