using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal static class AlertEventDedupCoordinator
    {
        private const int MaxRememberedEvents = 256;
        private const int ClaimLifetimeSeconds = 300;

        private static readonly object Sync =
            new object();

        private static readonly Dictionary<string, DateTime> Claims =
            new Dictionary<string, DateTime>(
                StringComparer.OrdinalIgnoreCase);

        public static int ClaimLifetime
        {
            get { return ClaimLifetimeSeconds; }
        }

        public static bool TryClaim(
            string eventKey,
            DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(eventKey))
                return false;

            DateTime now =
                NormalizeUtc(nowUtc);

            lock (Sync)
            {
                PruneUnsafe(now);

                DateTime previousUtc;
                if (Claims.TryGetValue(
                        eventKey,
                        out previousUtc) &&
                    (now - previousUtc).TotalSeconds <
                    ClaimLifetimeSeconds)
                {
                    return false;
                }

                Claims[eventKey] =
                    now;

                TrimUnsafe();

                return true;
            }
        }

        public static void Release(
            string eventKey)
        {
            if (string.IsNullOrWhiteSpace(eventKey))
                return;

            lock (Sync)
            {
                Claims.Remove(
                    eventKey);
            }
        }

        private static DateTime NormalizeUtc(
            DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            return value.ToUniversalTime();
        }

        private static void PruneUnsafe(
            DateTime nowUtc)
        {
            List<string> stale =
                new List<string>();

            foreach (KeyValuePair<string, DateTime> entry in
                Claims)
            {
                if ((nowUtc - entry.Value).TotalSeconds >=
                    ClaimLifetimeSeconds)
                {
                    stale.Add(
                        entry.Key);
                }
            }

            foreach (string key in stale)
                Claims.Remove(
                    key);
        }

        private static void TrimUnsafe()
        {
            while (Claims.Count > MaxRememberedEvents)
            {
                string oldestKey = null;
                DateTime oldestUtc =
                    DateTime.MaxValue;

                foreach (KeyValuePair<string, DateTime> entry in
                    Claims)
                {
                    if (entry.Value < oldestUtc)
                    {
                        oldestUtc =
                            entry.Value;
                        oldestKey =
                            entry.Key;
                    }
                }

                if (oldestKey == null)
                    break;

                Claims.Remove(
                    oldestKey);
            }
        }
    }
}