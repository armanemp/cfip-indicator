using System;
using System.Globalization;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class CbotExecutionStateBusKey
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string ForIndicatorInstance(string instanceId)
        {
            ulong hash = OffsetBasis;
            string value = instanceId ?? string.Empty;

            unchecked
            {
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= Prime;
                }
            }

            return
                "CFIPCbotState" +
                hash.ToString("X16", CultureInfo.InvariantCulture);
        }
    }

    public static class CbotExecutionStateCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(
            CbotExecutionStateSnapshot snapshot)
        {
            return JsonSerializer.Serialize(
                snapshot,
                Options);
        }

        public static bool TryDeserialize(
            string payload,
            out CbotExecutionStateSnapshot snapshot)
        {
            snapshot = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                snapshot =
                    JsonSerializer.Deserialize<CbotExecutionStateSnapshot>(
                        payload,
                        Options);

                return snapshot != null;
            }
            catch
            {
                snapshot = null;
                return false;
            }
        }
    }
}