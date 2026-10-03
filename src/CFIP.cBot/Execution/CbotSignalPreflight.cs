using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal static class CbotSignalPreflight
    {
        public static bool TryValidate(
            Robot robot,
            SignalEnvelope envelope,
            DateTime nowUtc,
            int staleAfterSeconds,
            out string reason)
        {
            reason = "OK";

            if (robot == null)
            {
                reason = "CBOT ROBOT UNAVAILABLE";
                return false;
            }

            if (envelope == null)
            {
                reason = "SIGNAL ENVELOPE UNAVAILABLE";
                return false;
            }

            if (envelope.Identity == null)
            {
                reason = "SIGNAL IDENTITY UNAVAILABLE";
                return false;
            }

            if (envelope.Identity.ContractVersion !=
                ContractVersion.Current)
            {
                reason = "SIGNAL CONTRACT VERSION MISMATCH";
                return false;
            }

            if (envelope.Plan == null ||
                envelope.Intent == null)
            {
                reason = "SIGNAL PLAN OR INTENT UNAVAILABLE";
                return false;
            }

            if (envelope.Intent.Identity == null ||
                !envelope.Intent.Identity.Equals(
                    envelope.Identity))
            {
                reason = "SIGNAL INTENT IDENTITY MISMATCH";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.Identity.SignalId) ||
                string.IsNullOrWhiteSpace(envelope.Identity.ScenarioId) ||
                string.IsNullOrWhiteSpace(envelope.Identity.PlanId) ||
                string.IsNullOrWhiteSpace(envelope.Identity.IdempotencyKey))
            {
                reason = "SIGNAL EXECUTION IDENTITY INCOMPLETE";
                return false;
            }

            if (envelope.Intent.Action == ExecutionAction.None)
            {
                reason = "SIGNAL EXECUTION ACTION UNAVAILABLE";
                return false;
            }

            if (envelope.ObservedUtc > nowUtc)
            {
                reason = "SIGNAL OBSERVED TIME IS IN THE FUTURE";
                return false;
            }

            double ageSeconds =
                (nowUtc - envelope.ObservedUtc).TotalSeconds;

            if (ageSeconds > Math.Max(1, staleAfterSeconds))
            {
                reason =
                    "SIGNAL ENVELOPE STALE • AGE " +
                    Math.Round(ageSeconds, 1) +
                    "S";
                return false;
            }

            if (!string.Equals(
                    envelope.Identity.Symbol,
                    robot.SymbolName,
                    StringComparison.Ordinal))
            {
                reason = "SIGNAL SYMBOL SCOPE MISMATCH";
                return false;
            }

            return true;
        }
    }
}
