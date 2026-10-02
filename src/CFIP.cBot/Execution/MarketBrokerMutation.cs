using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal static class MarketBrokerMutation
    {
        private const string DefaultComment = "CFIP AUTO";

        public static bool TrySubmit(
            Robot robot,
            ExecutionIntent intent,
            bool useMarketRange,
            out TradeResult result,
            out string reason)
        {
            result = null;
            reason = "OK";

            if (robot == null)
            {
                reason = "CBOT UNAVAILABLE";
                return false;
            }

            if (!MarketExecutionIntentRule.Validate(
                    intent,
                    out reason))
                return false;

            MarketExecutionProfile profile =
                intent.MarketProfile;

            TradeType tradeType =
                intent.Identity.Direction == TradeDirection.Buy
                    ? TradeType.Buy
                    : TradeType.Sell;

            double volume =
                intent.RequestedVolume.Value;

            double stopPips =
                profile.StopPips;

            double targetPips =
                profile.TargetPips;

            bool rangeEnabled =
                intent.Action == ExecutionAction.Market &&
                useMarketRange &&
                profile.MarketRangePips > 0;

            bool ladderEnabled =
                profile.UseServerTakeProfitLadder;

            if (ladderEnabled)
            {
                RelativeTakeProfitProtections takeProfits =
                    new RelativeTakeProfitProtections(
                        new RelativeTakeProfitProtection(
                            profile.Tp1Volume,
                            profile.Tp1Pips),
                        new RelativeTakeProfitProtection(
                            profile.Tp2Volume,
                            profile.Tp2Pips),
                        new RelativeTakeProfitLastProtection(
                            profile.FinalTpPips));

                StopLossBreakEven breakEven = null;

                if (profile.BreakEvenTriggerPips.HasValue)
                {
                    breakEven =
                        new StopLossBreakEven(
                            profile.BreakEvenTriggerPips.Value,
                            profile.BreakEvenOffsetPips ?? 0);
                }

                if (rangeEnabled)
                {
                    result =
                        robot.ExecuteMarketRangeOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            profile.MarketRangePips,
                            intent.RequestedEntry,
                            intent.ExecutionLabel,
                            new RelativeStopLossProtection(stopPips),
                            takeProfits,
                            DefaultComment,
                            false,
                            StopTriggerMethod.Trade,
                            breakEven);
                }
                else
                {
                    result =
                        robot.ExecuteMarketOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            intent.ExecutionLabel,
                            new RelativeStopLossProtection(stopPips),
                            takeProfits,
                            DefaultComment,
                            false,
                            StopTriggerMethod.Trade,
                            breakEven);
                }
            }
            else if (rangeEnabled)
            {
                result =
                    robot.ExecuteMarketRangeOrder(
                        tradeType,
                        robot.SymbolName,
                        volume,
                        profile.MarketRangePips,
                        intent.RequestedEntry,
                        intent.ExecutionLabel,
                        stopPips,
                        targetPips,
                        DefaultComment,
                        false);
            }
            else
            {
                result =
                    robot.ExecuteMarketOrder(
                        tradeType,
                        robot.SymbolName,
                        volume,
                        intent.ExecutionLabel,
                        stopPips,
                        targetPips,
                        DefaultComment,
                        false);
            }

            if (result == null)
            {
                reason = "NULL TRADE RESULT";
                return false;
            }

            if (!result.IsSuccessful)
            {
                reason =
                    result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED";
                return false;
            }

            if (result.Position == null)
            {
                reason = "BROKER ACCEPTED WITHOUT POSITION";
                return false;
            }

            return true;
        }
    }
}
