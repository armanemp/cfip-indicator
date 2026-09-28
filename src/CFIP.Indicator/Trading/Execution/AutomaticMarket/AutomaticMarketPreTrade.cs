using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareAutomaticMarketTrade(
            int closedM5,
            out TradeType type,
            out double entry,
            out double stopPips,
            out double targetPips,
            out double target,
            out double volume)
        {
            if (!PassAutomaticMarketPreTradeEligibility(
                closedM5))
            {
                type = TradeType.Buy;
                entry = 0;
                stopPips = 0;
                targetPips = 0;
                target = 0;
                volume = 0;
                return false;
            }

            return TryPrepareAutomaticMarketExecution(
                closedM5,
                out type,
                out entry,
                out stopPips,
                out targetPips,
                out target,
                out volume);
        }
    }
}
