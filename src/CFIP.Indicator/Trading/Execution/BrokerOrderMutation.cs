// CFIP Indicator — BrokerOrderMutation.cs
// The sole broker order mutation boundary.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeResult TryExecuteMarketOrder(
            TradeType tradeType,
            string symbolName,
            double volume,
            string label,
            double stopPips,
            double targetPips,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                return ExecuteMarketOrder(
                    tradeType,
                    symbolName,
                    volume,
                    label,
                    stopPips,
                    targetPips,
                    comment,
                    hasTrailingStop);
            }
            catch (Exception ex)
            {
                Print("CFIP market mutation failed ({0}): {1}", context, ex.Message);
                return null;
            }
        }

        private TradeResult TryPlaceStopOrder(
            TradeType tradeType,
            string symbolName,
            double volume,
            double targetPrice,
            string label,
            double stopPips,
            double targetPips,
            ProtectionType protectionType,
            DateTime? expiration,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                return PlaceStopOrder(
                    tradeType,
                    symbolName,
                    volume,
                    targetPrice,
                    label,
                    stopPips,
                    targetPips,
                    protectionType,
                    expiration,
                    comment,
                    hasTrailingStop);
            }
            catch (Exception ex)
            {
                Print("CFIP stop-order mutation failed ({0}): {1}", context, ex.Message);
                return null;
            }
        }

        private TradeResult TryPlaceLimitOrder(
            TradeType tradeType,
            string symbolName,
            double volume,
            double targetPrice,
            string label,
            double stopPips,
            double targetPips,
            ProtectionType protectionType,
            DateTime? expiration,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                return PlaceLimitOrder(
                    tradeType,
                    symbolName,
                    volume,
                    targetPrice,
                    label,
                    stopPips,
                    targetPips,
                    protectionType,
                    expiration,
                    comment,
                    hasTrailingStop);
            }
            catch (Exception ex)
            {
                Print("CFIP limit-order mutation failed ({0}): {1}", context, ex.Message);
                return null;
            }
        }

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
