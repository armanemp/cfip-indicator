using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryValidateAutomaticMarketSubmission(
            int closedM5,
            TradeType type,
            double entry,
            double target,
            double volume,
            out string reason)
        {
            reason = "";
            ExecutionIntent marketIntent;

            if (!EnsureTradingPermission())
            {
                reason =
                    "TRADING PERMISSION NOT GRANTED";
                return false;
            }

            if (!PassesAutoTradeSafetyGuards(
                    type,
                    volume,
                    out reason))
                return false;

            marketIntent =
                BuildExecutionIntent(
                    _plan.Direction,
                    ConfirmedSignalsOnly
                        ? DecisionPolicyMode.Confirmed
                        : DecisionPolicyMode.Soft,
                    ExecutionIntentKind.Market,
                    entry,
                    _plan.EntryTrigger,
                    _plan.EntryZoneLow,
                    _plan.EntryZoneHigh,
                    _plan.Stop,
                    target,
                    volume,
                    closedM5,
                    "NORMAL MARKET");

            if (!ValidateExecutionIntent(
                    marketIntent,
                    entry,
                    out reason))
            {
                marketIntent = null;
                return false;
            }

            return true;
        }
    }
}
