using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareAutomaticMarketExecution(
            int closedM5,
            out TradeType type,
            out double entry,
            out double stopPips,
            out double targetPips,
            out double target,
            out double volume)
        {
            type = TradeType.Buy;
            entry = 0;
            stopPips = 0;
            targetPips = 0;
            target = 0;
            volume = 0;

            double plannedEntry =
                _plan.Entry;

            entry =
                NormalizePrice(
                    _plan.Direction == 1
                        ? Symbol.Ask
                        : Symbol.Bid);

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "ATR UNAVAILABLE");
                return false;
            }

            if (Math.Abs(
                    entry -
                    plannedEntry) >
                atr *
                MaximumEntryExtensionAtr)
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "ENTRY EXTENSION");
                return false;
            }

            if (!TryPrepareExecutablePlan(
                    closedM5,
                    entry,
                    out stopPips,
                    out targetPips,
                    out target))
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "INVALID SMART EXECUTION PLAN");
                return false;
            }

            string executableEntryReason;

            if (!IsExecutableMarketEntry(
                    _plan,
                    entry,
                    out executableEntryReason))
            {
                _autoExecutionBlockReason =
                    executableEntryReason;
                SetAutoTradingState(
                    "ARMED",
                    executableEntryReason);
                return false;
            }

            double effectiveStopPips =
                EffectiveRiskStopPips(
                    stopPips);

            volume =
                CalculateVolume(
                    effectiveStopPips);

            volume =
                AdjustVolumeForMargin(
                    _plan.Direction == 1
                        ? TradeType.Buy
                        : TradeType.Sell,
                    volume);

            if (volume <
                Symbol.VolumeInUnitsMin)
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "VOLUME BELOW MINIMUM");
                return false;
            }

            type =
                _plan.Direction == 1
                    ? TradeType.Buy
                    : TradeType.Sell;

            return true;
        }
    }
}
