using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareAggressiveExecution(
            int closedM5,
            out TradeType type,
            out double entry,
            out double atr,
            out double stop,
            out double target,
            out double stopPips,
            out double tpPips,
            out double volume)
        {
            type = TradeType.Buy;
            entry = 0;
            atr = 0;
            stop = 0;
            target = 0;
            stopPips = 0;
            tpPips = 0;
            volume = 0;

            CanonicalPriceSnapshot priceSnapshot =
                GetCanonicalPriceSnapshot();

            if (priceSnapshot == null ||
                !priceSnapshot.IsQuoteValid)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • INVALID MARKET QUOTE";
                return false;
            }

            entry =
                NormalizePrice(
                    priceSnapshot.GetExecutablePrice(
                        _reaction.Direction));

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • ATR UNAVAILABLE";
                return false;
            }

            string source;
            int quality;

            stop =
                BuildStructuralStop(
                    closedM5,
                    _reaction.Direction,
                    entry,
                    atr,
                    out source,
                    out quality);

            if (!IsFinitePositive(stop))
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • INVALID SL";
                return false;
            }

            target =
                SelectStructuralAutoTarget(
                    closedM5,
                    _reaction.Direction,
                    entry,
                    stop,
                    atr,
                    AggressiveTpStage);

            if (!IsAutoPlanValid(
                    _reaction.Direction,
                    entry,
                    stop,
                    target))
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "NO VALID STRUCTURAL TARGET");
                return false;
            }

            stopPips =
                Math.Abs(
                    entry -
                    stop) /
                Math.Max(
                    priceSnapshot.PipSize,
                    1e-9);

            tpPips =
                Math.Abs(
                    target -
                    entry) /
                Symbol.PipSize;

            double effectiveStopPips =
                stopPips;

            if (IncludeSpreadInRiskSizing)
                effectiveStopPips +=
                    Math.Max(
                        0,
                        (Symbol.Ask -
                         Symbol.Bid) /
                        Math.Max(
                            Symbol.PipSize,
                            1e-9));

            volume =
                CalculateAggressiveVolume(
                    effectiveStopPips);

            volume =
                AdjustVolumeForMargin(
                    _reaction.Direction == 1
                        ? TradeType.Buy
                        : TradeType.Sell,
                    volume);

            if (volume <
                Symbol.VolumeInUnitsMin)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • VOLUME BELOW MINIMUM";
                return false;
            }

            type =
                _reaction.Direction == 1
                    ? TradeType.Buy
                    : TradeType.Sell;

            return true;
        }
    }
}
