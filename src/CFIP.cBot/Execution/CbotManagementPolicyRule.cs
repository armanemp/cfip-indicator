using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal static class CbotManagementPolicyRule
    {
        public static bool Allows(
            ManagementCommandType command,
            bool enableLiveExitManagement,
            bool enablePartialTakeProfit,
            bool autoBrokerProtection,
            bool autoProtectBrokerPositions,
            bool syncBrokerTakeProfit,
            bool cancelPendingBeforeHighImpactNews,
            bool closeActiveBeforeHighImpactNews,
            string commandReason,
            out string reason)
        {
            reason = "OK";

            switch (command)
            {
                case ManagementCommandType.CancelPending:
                    if (IsHighImpactNews(commandReason) &&
                        !cancelPendingBeforeHighImpactNews)
                    {
                        reason = "NEWS PENDING CANCELLATION DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.FullClose:
                    if (IsHighImpactNews(commandReason) &&
                        !closeActiveBeforeHighImpactNews)
                    {
                        reason = "NEWS POSITION PROTECTION DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.Keep:
                    return true;

                case ManagementCommandType.PartialClose:
                    if (!enablePartialTakeProfit)
                    {
                        reason = "PARTIAL TAKE PROFIT DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.ModifyProtection:
                case ManagementCommandType.BreakEven:
                    if (!autoBrokerProtection &&
                        !autoProtectBrokerPositions)
                    {
                        reason = "BROKER PROTECTION DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.AdvanceTarget:
                    if (!enableLiveExitManagement)
                    {
                        reason = "LIVE EXIT MANAGEMENT DISABLED";
                        return false;
                    }

                    if (!syncBrokerTakeProfit)
                    {
                        reason = "BROKER TAKE PROFIT SYNC DISABLED";
                        return false;
                    }

                    return true;

                default:
                    reason = "UNSUPPORTED MANAGEMENT COMMAND";
                    return false;
            }
        }

        private static bool IsHighImpactNews(string reason)
        {
            return !string.IsNullOrWhiteSpace(reason) &&
                   reason.IndexOf(
                       "HIGH IMPACT NEWS",
                       System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
