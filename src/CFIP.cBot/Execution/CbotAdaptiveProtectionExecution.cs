using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal static class CbotAdaptiveProtectionExecution
    {
        public static bool TryExecute(
            Robot robot,
            SignalEnvelope envelope,
            TradeType tradeType,
            double volume,
            string label,
            double stopPips,
            double targetPips,
            double marketRangePips,
            string comment,
            out TradeResult result)
        {
            result = null;
            MarketExecutionProfile profile =
                envelope == null || envelope.Intent == null
                    ? null
                    : envelope.Intent.MarketProfile;

            if (profile == null ||
                !profile.UseServerTakeProfitLadder ||
                !Positive(profile.Tp1Pips) ||
                !Positive(profile.FinalTpPips) ||
                !Positive(profile.Tp1Volume))
                return false;

            double firstVolume =
                robot.Symbol.NormalizeVolumeInUnits(
                    profile.Tp1Volume,
                    RoundingMode.Down);

            if (!Positive(firstVolume) || firstVolume >= volume)
                return false;

            bool hasSecond =
                Positive(profile.Tp2Pips) &&
                Positive(profile.Tp2Volume);

            double secondVolume = 0;

            if (hasSecond)
            {
                secondVolume =
                    robot.Symbol.NormalizeVolumeInUnits(
                        profile.Tp2Volume,
                        RoundingMode.Down);

                if (!Positive(secondVolume) ||
                    firstVolume + secondVolume >= volume)
                    return false;
            }

            try
            {
                RelativeStopLossProtection stopProtection =
                    new RelativeStopLossProtection(stopPips);

                RelativeTakeProfitProtections takeProfits =
                    hasSecond
                        ? new RelativeTakeProfitProtections(
                            new RelativeTakeProfitProtection(
                                firstVolume,
                                profile.Tp1Pips),
                            new RelativeTakeProfitProtection(
                                secondVolume,
                                profile.Tp2Pips),
                            new RelativeTakeProfitLastProtection(
                                profile.FinalTpPips))
                        : new RelativeTakeProfitProtections(
                            new RelativeTakeProfitProtection(
                                firstVolume,
                                profile.Tp1Pips),
                            new RelativeTakeProfitLastProtection(
                                profile.FinalTpPips));

                StopLossBreakEven breakEven =
                    profile.BreakEvenTriggerPips.HasValue &&
                    profile.BreakEvenOffsetPips.HasValue &&
                    Positive(profile.BreakEvenTriggerPips.Value)
                        ? new StopLossBreakEven(
                            profile.BreakEvenTriggerPips.Value,
                            Math.Max(
                                0,
                                profile.BreakEvenOffsetPips.Value))
                        : null;

                result =
                    marketRangePips > 0
                        ? robot.ExecuteMarketRangeOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            marketRangePips,
                            envelope.Intent.RequestedEntry,
                            label,
                            stopProtection,
                            takeProfits,
                            comment,
                            false,
                            StopTriggerMethod.Trade,
                            breakEven)
                        : robot.ExecuteMarketOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            label,
                            stopProtection,
                            takeProfits,
                            comment,
                            false,
                            StopTriggerMethod.Trade,
                            breakEven);

                return result != null;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static TradeResult ExecuteLegacy(
            Robot robot,
            bool marketAction,
            TradeType tradeType,
            double volume,
            double marketRangePips,
            double requestedEntry,
            string label,
            double stopPips,
            double targetPips,
            string comment)
        {
            return
                marketAction && marketRangePips > 0
                    ? robot.ExecuteMarketRangeOrder(
                        tradeType,
                        robot.SymbolName,
                        volume,
                        marketRangePips,
                        requestedEntry,
                        label,
                        stopPips,
                        targetPips,
                        comment,
                        false)
                    : robot.ExecuteMarketOrder(
                        tradeType,
                        robot.SymbolName,
                        volume,
                        label,
                        stopPips,
                        targetPips,
                        comment,
                        false);
        }

        private static bool Positive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
