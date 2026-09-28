using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareAggressiveTrade(
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
            if (!PassAggressivePreTradeEligibility(
                    closedM5))
            {
                type = TradeType.Buy;
                entry = 0;
                atr = 0;
                stop = 0;
                target = 0;
                stopPips = 0;
                tpPips = 0;
                volume = 0;
                return false;
            }

            return TryPrepareAggressiveExecution(
                closedM5,
                out type,
                out entry,
                out atr,
                out stop,
                out target,
                out stopPips,
                out tpPips,
                out volume);
        }
    }
}
