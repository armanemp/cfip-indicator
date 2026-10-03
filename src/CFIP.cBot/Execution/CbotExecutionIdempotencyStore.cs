using System;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    /// <summary>
    /// Device-persistent execution-attempt memory scoped to one Indicator
    /// instance. Confirmed idempotency keys remain suppressed across cBot
    /// restart; failed attempts are retryable after a bounded cool-down.
    /// </summary>
    internal sealed class CbotExecutionIdempotencyStore
    {
        private const int MaxEntries = 256;
        private const int RetentionHours = 24;
        private const int RetryDelaySeconds = 5;
        private const string KeyPrefix = "CFIPExecIdem";

        private readonly Dictionary<string, Entry> _entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);

        private string _instanceId = string.Empty;

        private sealed class Entry
        {
            public DateTime AttemptedUtc { get; }
            public bool Confirmed { get; }

            public Entry(
                DateTime attemptedUtc,
                bool confirmed)
            {
                AttemptedUtc = attemptedUtc;
                Confirmed = confirmed;
            }
        }

        public void Reload(
            Robot robot,
            string indicatorInstanceId,
            DateTime nowUtc)
        {
            _entries.Clear();
            _instanceId =
                indicatorInstanceId == null
                    ? string.Empty
                    : indicatorInstanceId.Trim();

            if (robot == null ||
                string.IsNullOrWhiteSpace(_instanceId))
                return;

            string payload;
            try
            {
                payload =
                    robot.LocalStorage.GetString(
                        StoreKey(),
                        LocalStorageScope.Device);
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(payload))
                return;

            string[] rows =
                payload.Split(
                    new[] { '
' },
                    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0;
                 i < rows.Length &&
                 _entries.Count < MaxEntries;
                 i++)
            {
                string[] fields =
                    rows[i].Split('	');

                if (fields.Length != 3)
                    continue;

                if (!long.TryParse(
                        fields[0],
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out long ticks))
                    continue;

                if (!bool.TryParse(
                        fields[1],
                        out bool confirmed))
                    continue;

                string key;
                try
                {
                    key =
                        System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(fields[2]));
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                DateTime attemptedUtc;
                try
                {
                    attemptedUtc =
                        new DateTime(
                            ticks,
                            DateTimeKind.Utc);
                }
                catch
                {
                    continue;
                }

                if (attemptedUtc.AddHours(RetentionHours) <= nowUtc)
                    continue;

                _entries[key] =
                    new Entry(
                        attemptedUtc,
                        confirmed);
            }
        }

        public bool CanAttempt(
            string key,
            DateTime nowUtc,
            out string reason)
        {
            reason = "OK";

            if (string.IsNullOrWhiteSpace(key))
            {
                reason = "MISSING IDEMPOTENCY KEY";
                return false;
            }

            Prune(nowUtc);

            if (!_entries.TryGetValue(
                    key,
                    out Entry entry) ||
                entry == null)
                return true;

            if (entry.Confirmed)
            {
                reason =
                    "DUPLICATE IDEMPOTENCY KEY • PERSISTED CONFIRMED";
                return false;
            }

            if (entry.AttemptedUtc.AddSeconds(
                    RetryDelaySeconds) > nowUtc)
            {
                reason =
                    "IDEMPOTENCY RETRY COOLDOWN";
                return false;
            }

            return true;
        }

        public void RecordAttempt(
            Robot robot,
            string key,
            bool confirmed,
            DateTime nowUtc)
        {
            if (robot == null ||
                string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(_instanceId))
                return;

            Prune(nowUtc);
            _entries[key] =
                new Entry(
                    nowUtc,
                    confirmed);

            while (_entries.Count > MaxEntries)
            {
                string oldestKey = null;
                DateTime oldest = DateTime.MaxValue;

                foreach (KeyValuePair<string, Entry> item in _entries)
                {
                    if (item.Value == null ||
                        item.Value.AttemptedUtc < oldest)
                    {
                        oldestKey = item.Key;
                        oldest =
                            item.Value == null
                                ? DateTime.MinValue
                                : item.Value.AttemptedUtc;
                    }
                }

                if (oldestKey == null)
                    break;

                _entries.Remove(oldestKey);
            }

            string[] rows =
                new string[_entries.Count];

            int index = 0;
            foreach (KeyValuePair<string, Entry> item in _entries)
            {
                string encodedKey =
                    Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(
                            item.Key ?? string.Empty));

                rows[index++] =
                    item.Value.AttemptedUtc.Ticks
                        .ToString(
                            CultureInfo.InvariantCulture) +
                    "	" +
                    item.Value.Confirmed.ToString(
                        CultureInfo.InvariantCulture) +
                    "	" +
                    encodedKey;
            }

            try
            {
                robot.LocalStorage.SetString(
                    StoreKey(),
                    string.Join(
                        "
",
                        rows),
                    LocalStorageScope.Device);
                robot.LocalStorage.Flush(
                    LocalStorageScope.Device);
            }
            catch (Exception ex)
            {
                robot.Print(
                    "CFIP IDEMPOTENCY STORE WRITE FAILED | {0}",
                    ex.Message);
            }
        }

        private void Prune(DateTime nowUtc)
        {
            List<string> expired =
                new List<string>();

            foreach (KeyValuePair<string, Entry> item in _entries)
            {
                if (item.Value == null ||
                    item.Value.AttemptedUtc.AddHours(
                        RetentionHours) <= nowUtc)
                    expired.Add(item.Key);
            }

            for (int i = 0; i < expired.Count; i++)
                _entries.Remove(expired[i]);
        }

        private string StoreKey()
        {
            return KeyPrefix +
                   ContractBusKeyHash.Hash(
                       _instanceId);
        }
    }
}
