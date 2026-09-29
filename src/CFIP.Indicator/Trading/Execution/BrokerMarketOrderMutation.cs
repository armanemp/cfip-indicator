// CFIP Indicator — BrokerMarketOrderMutation.cs
// Single-responsibility broker mutation module.

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

        private TradeResult TryExecuteMarketRangeOrderWithTakeProfitLadder(
            TradeType tradeType,
            string symbolName,
            double volume,
            double marketRangePips,
            double basePrice,
            string label,
            double stopPips,
            RelativeTakeProfitProtections takeProfits,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                return ExecuteMarketRangeOrder(
                    tradeType,
                    symbolName,
                    volume,
                    marketRangePips,
                    basePrice,
                    label,
                    new RelativeStopLossProtection(stopPips),
                    takeProfits,
                    comment,
                    hasTrailingStop);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP advanced market-range mutation failed ({0}): {1}",
                    context,
                    ex.Message);
                return null;
            }
        }

        private TradeResult TryExecuteMarketRangeOrder(
            TradeType tradeType,
            string symbolName,
            double volume,
            double marketRangePips,
            double basePrice,
            string label,
            double stopPips,
            double targetPips,
            string comment,
            bool hasTrailingStop,
            string context)
        {
            try
            {
                return ExecuteMarketRangeOrder(
                    tradeType,
                    symbolName,
                    volume,
                    marketRangePips,
                    basePrice,
                    label,
                    stopPips,
                    targetPips,
                    comment,
                    hasTrailingStop);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP market-range mutation failed ({0}): {1}",
                    context,
                    ex.Message);
                return null;
            }
        }
    }
}
