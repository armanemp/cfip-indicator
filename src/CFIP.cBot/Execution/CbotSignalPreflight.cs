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
