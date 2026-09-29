using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePendingSubmission(
            ExecutionIntent intent,
            TradeType tradeType,
            double marketReference,
            double volume,
            string prefix,
            out string reason)
        {
            reason = "";

            string intentReason;

            if (!ValidateExecutionIntent(
                    intent,
                    marketReference,
                    out intentReason))
            {
                reason =
                    prefix +
                    intentReason;
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    intent.CreatedM5,
                    intent.Direction,
                    out suitabilityReason,
                    true))
            {
                reason =
                    prefix +
                    "SUITABILITY " +
                    suitabilityReason;
                return false;
            }

            string safetyReason;

            if (!PassesAutoTradeSafetyGuards(
                    tradeType,
                    volume,
                    out safetyReason))
            {
                reason =
                    prefix +
                    safetyReason;
                return false;
            }

            return true;
        }

        private DateTime PendingExpiration()
        {
            return
                TimeInUtc.AddMinutes(
                    Math.Max(
                        15,
                        PendingOrderExpiryMinutes));
        }
    }
}
