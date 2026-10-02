using System;
using System.Globalization;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class ManagementBusKey
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string CommandKeyForInstance(string instanceId) =>
            "CFIPManagementCommands" + Hash(instanceId);

        public static string ReportKeyForInstance(string instanceId) =>
            "CFIPManagementReports" + Hash(instanceId);

        private static string Hash(string value)
        {
            ulong hash = OffsetBasis;
            value = value ?? "";

            unchecked
            {
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= Prime;
                }
            }

            return hash.ToString("X16", CultureInfo.InvariantCulture);
        }
    }

    public static class ManagementCommandCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(ManagementCommand[] commands) =>
            JsonSerializer.Serialize(commands ?? Array.Empty<ManagementCommand>(), Options);

        public static bool TryDeserialize(string payload, out ManagementCommand[] commands)
        {
            commands = null;
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                commands = JsonSerializer.Deserialize<ManagementCommand[]>(payload, Options);
                return commands != null;
            }
            catch
            {
                commands = null;
                return false;
            }
        }
    }

    public static class BrokerExecutionReportCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(BrokerExecutionReport[] reports) =>
            JsonSerializer.Serialize(reports ?? Array.Empty<BrokerExecutionReport>(), Options);

        public static bool TryDeserialize(
            string payload,
            out BrokerExecutionReport[] reports)
        {
            reports = null;
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                reports = JsonSerializer.Deserialize<BrokerExecutionReport[]>(payload, Options);
                return reports != null;
            }
            catch
            {
                reports = null;
                return false;
            }
        }
    }
}
