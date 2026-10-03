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
            reason = "OK";

            if (settings == null)
            {
                reason = "CBOT EXECUTION SETTINGS UNAVAILABLE";
                return false;
            }

            switch (command)
            {
                case ManagementCommandType.CancelPending:
                case ManagementCommandType.FullClose:
                case ManagementCommandType.Keep:
                    return true;

                case ManagementCommandType.PartialClose:
                    if (!settings.EnablePartialTakeProfit)
                    {
                        reason = "PARTIAL TAKE PROFIT DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.ModifyProtection:
                case ManagementCommandType.BreakEven:
                    if (!settings.AutoBrokerProtection &&
                        !settings.AutoProtectBrokerPositions)
                    {
                        reason = "BROKER PROTECTION DISABLED";
                        return false;
                    }
                    return true;

                case ManagementCommandType.AdvanceTarget:
                    if (!settings.EnableLiveExitManagement)
                    {
                        reason = "LIVE EXIT MANAGEMENT DISABLED";
                        return false;
                    }

                    if (!settings.SyncBrokerTakeProfit)
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
