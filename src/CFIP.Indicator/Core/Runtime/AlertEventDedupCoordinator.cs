using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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

        public static void Import(
            string payload,
            DateTime nowUtc)
        {
            DateTime now =
                NormalizeUtc(nowUtc);

            if (string.IsNullOrWhiteSpace(payload))
                return;

            string[] lines =
                payload.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

            lock (Sync)
            {
                PruneUnsafe(now);

                foreach (string line in lines)
                {
                    string[] parts =
                        line.Split(
                            '|');

                    if (parts.Length != 3 ||
                        !string.Equals(
                            parts[0],
                            "CFIP-ADEDUP,1",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    long ticks;
                    if (!long.TryParse(
                            parts[1],
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out ticks) ||
                        ticks <= 0)
                    {
                        continue;
                    }

                    string eventKey;
                    try
                    {
                        eventKey =
                            Encoding.UTF8.GetString(
                                Convert.FromBase64String(
                                    parts[2]));
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(eventKey))
                        continue;

                    DateTime claimedUtc =
                        new DateTime(
                            ticks,
                            DateTimeKind.Utc);

                    if ((now - claimedUtc).TotalSeconds >=
                        ClaimLifetimeSeconds)
                        continue;

                    DateTime existingUtc;
                    if (!Claims.TryGetValue(
                            eventKey,
                            out existingUtc) ||
                        claimedUtc > existingUtc)
                    {
                        Claims[eventKey] =
                            claimedUtc;
                    }
                }

                TrimUnsafe();
            }
        }

        public static string Serialize(
            DateTime nowUtc)
        {
            DateTime now =
                NormalizeUtc(nowUtc);

            lock (Sync)
            {
                PruneUnsafe(now);

                StringBuilder builder =
                    new StringBuilder(
                        "CFIP Alert Event Dedup\n");

                foreach (KeyValuePair<string, DateTime> entry in
                    Claims)
                {
                    string encoded =
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(
                                entry.Key));

                    builder.Append(
                        "CFIP-ADEDUP,1|")
                        .Append(
                            entry.Value.Ticks.ToString(
                                CultureInfo.InvariantCulture))
                        .Append('|')
                        .Append(encoded)
                        .Append('\n');
                }

                return builder.ToString();
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