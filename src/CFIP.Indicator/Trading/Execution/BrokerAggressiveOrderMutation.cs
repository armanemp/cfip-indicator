// CFIP Indicator — Aggressive broker mutation compatibility owner.
// CBOT-P4A deliberately leaves Aggressive market mutation in Indicator until
// CBOT-P4B migrates this exact owner and its confirmation/protection callers.

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
                MarkBrokerStateDirty();
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
                Print(
                    "CFIP market mutation failed ({0}): {1}",
                    context,
                    ex.Message);
                return null;
            }
        }

        private TradeResult TryExecuteMarketOrderWithTakeProfitLadder(
            TradeType tradeType,
            string symbolName,
            double volume,
            string label,
            double stopPips,
            RelativeTakeProfitProtections takeProfits,
            StopLossBreakEven stopLossBreakEven,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                MarkBrokerStateDirty();
                return ExecuteMarketOrder(
                    tradeType,
                    symbolName,
                    volume,
                    label,
                    new RelativeStopLossProtection(stopPips),
                    takeProfits,
                    comment,
                    hasTrailingStop,
                    StopTriggerMethod.Trade,
                    stopLossBreakEven);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP advanced market mutation failed ({0}): {1}",
                    context,
                    ex.Message);
                return null;
            }
        }
    }
}
