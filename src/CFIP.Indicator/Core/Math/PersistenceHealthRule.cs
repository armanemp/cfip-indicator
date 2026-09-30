using System;

namespace cAlgo
{
    internal enum PersistenceHealthState
    {
        Unknown = 0,
        Healthy = 1,
        Degraded = 2,
        Failed = 3
    }

    internal static class PersistenceHealthRule
    {
        internal static PersistenceHealthState Resolve(
            bool probeSucceeded,
            int writeFailures,
            int readFailures,
            int pendingLines)
        {
            if (writeFailures < 0)
                writeFailures = 0;

            if (readFailures < 0)
                readFailures = 0;

            if (pendingLines < 0)
                pendingLines = 0;

            if (probeSucceeded &&
                writeFailures == 0 &&
                readFailures == 0)
                return PersistenceHealthState.Healthy;

            if (writeFailures > 0 ||
                readFailures > 0)
                return PersistenceHealthState.Degraded;

            if (pendingLines > 0)
                return PersistenceHealthState.Degraded;

            return PersistenceHealthState.Unknown;
        }

        internal static string ToText(
            PersistenceHealthState state)
        {
            switch (state)
            {
                case PersistenceHealthState.Healthy:
                    return "READY";

                case PersistenceHealthState.Degraded:
                    return "DEGRADED";

                case PersistenceHealthState.Failed:
                    return "FAILED";

                default:
                    return "UNVERIFIED";
            }
        }

        internal static bool IsRelativeHistoryPath(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string normalized =
                path.Replace(
                    '\\',
                    '/');

            if (normalized.StartsWith(
                    "/",
                    StringComparison.Ordinal) ||
                normalized.Contains(
                    ":",
                    StringComparison.Ordinal))
                return false;

            string[] segments =
                normalized.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i++)
            {
                if (string.Equals(
                        segments[i],
                        "..",
                        StringComparison.Ordinal))
                    return false;
            }

            return segments.Length > 0 &&
                string.Equals(
                    segments[0],
                    "History",
                    StringComparison.Ordinal);
        }
    }
}
