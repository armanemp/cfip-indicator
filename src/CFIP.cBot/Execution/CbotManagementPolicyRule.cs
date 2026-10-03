using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal static class CbotManagementPolicyRule
    {
        public static bool Allows(
            ManagementCommandType command,
            CbotIndicatorExecutionSettings settings,
            out string reason)
        {
            return Allows(
                command,
                settings != null && settings.EnableLiveExitManagement,
                settings != null && settings.EnablePartialTakeProfit,
                settings != null && settings.AutoBrokerProtection,
                settings != null && settings.AutoProtectBrokerPositions,
                settings != null && settings.SyncBrokerTakeProfit,
                out reason);
        }

        public static bool Allows(
            ManagementCommandType command,
            bool enableLiveExitManagement,
            bool enablePartialTakeProfit,
            bool autoBrokerProtection,
            bool autoProtectBrokerPositions,
            bool syncBrokerTakeProfit,
            out string reason)
        {
            reason = "OK";

            switch (command)
            {
                case ManagementCommandType.CancelPending:
                case ManagementCommandType.FullClose:
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
    }
}
