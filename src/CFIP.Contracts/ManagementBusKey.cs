using System;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class ManagementBusKey
    {
        public static string CommandKeyForInstance(
            string instanceId) =>
            "CFIPManagementCommands" +
            ContractBusKeyHash.Hash(instanceId);

        public static string ReportKeyForInstance(
            string instanceId) =>
            "CFIPManagementReports" +
            ContractBusKeyHash.Hash(instanceId);
    }

    public static class ManagementCommandCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(
            ManagementCommand[] commands) =>
            JsonSerializer.Serialize(
                commands ?? Array.Empty<ManagementCommand>(),
                Options);

        public static bool TryDeserialize(
            string payload,
            out ManagementCommand[] commands)
        {
            commands = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                ManagementCommand[] parsed =
                    JsonSerializer.Deserialize<ManagementCommand[]>(
                        payload,
                        Options);

                commands = parsed;
                return parsed != null;
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

        public static string Serialize(
            BrokerExecutionReport[] reports) =>
            JsonSerializer.Serialize(
                reports ?? Array.Empty<BrokerExecutionReport>(),
                Options);

        public static bool TryDeserialize(
            string payload,
            out BrokerExecutionReport[] reports)
        {
            reports = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                BrokerExecutionReport[] parsed =
                    JsonSerializer.Deserialize<BrokerExecutionReport[]>(
                        payload,
                        Options);

                reports = parsed;
                return parsed != null;
            }
            catch
            {
                reports = null;
                return false;
            }
        }
    }
}
