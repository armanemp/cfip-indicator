using System;

namespace CFIP.cBot.Execution
{
    internal static class CbotExecutionLifecycleRule
    {
        public static bool AllowsExecution(
            string lifecycleState,
            bool recoveryRequired)
        {
            if (recoveryRequired ||
                string.IsNullOrWhiteSpace(lifecycleState))
                return false;

            string state =
                lifecycleState.Trim();

            return
                StartsWithState(state, "READY") ||
                StartsWithState(state, "ACTIVE") ||
                StartsWithState(state, "PENDING");
        }

        private static bool StartsWithState(
            string state,
            string prefix)
        {
            return
                string.Equals(
                    state,
                    prefix,
                    StringComparison.OrdinalIgnoreCase) ||
                state.StartsWith(
                    prefix + " /",
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
