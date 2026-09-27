// CFIP Indicator — BrokerPendingOrderPlacement.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
    }
}
