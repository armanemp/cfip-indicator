using System;
using System.Globalization;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class CbotExecutionStateBusKey
    {
        public static string ForSymbol(
            string symbol)
        {
            return
                "CFIPCbotPresence" +
                ContractBusKeyHash.Hash(
                    symbol ?? string.Empty);
        }

        public static string ForSymbol(
            string symbol)
        {
            return
                "CFIPCbotPresence" +
                ContractBusKeyHash.Hash(
                    symbol ?? string.Empty);
        }

        public static string ForIndicatorInstance(string instanceId)
        {
            return
                "CFIPCbotState" +
                ContractBusKeyHash.Hash(instanceId);
        }
    }

    public sealed record CbotPresenceSnapshot(
        int ContractVersion,
        string CbotInstanceId,
        string CbotTypeName,
        string CbotDisplayName,
        string Symbol,
        DateTime ObservedUtc,
        string State,
        bool DemoAccount,
        string BoundIndicatorInstanceId);

    public sealed record CbotPresenceSnapshot(
        int ContractVersion,
        string CbotInstanceId,
        string CbotTypeName,
        string CbotDisplayName,
        string Symbol,
        DateTime ObservedUtc,
        string State,
        bool DemoAccount,
        string BoundIndicatorInstanceId);

    public static class CbotExecutionStateCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string SerializePresence(
            CbotPresenceSnapshot snapshot)
        {
            return JsonSerializer.Serialize(
                snapshot,
                Options);
        }

        public static bool TryDeserializePresence(
            string payload,
            out CbotPresenceSnapshot? snapshot)
        {
            snapshot = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                snapshot =
                    JsonSerializer.Deserialize<CbotPresenceSnapshot>(
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

        public static string SerializePresence(
            CbotPresenceSnapshot snapshot)
        {
            return JsonSerializer.Serialize(
                snapshot,
                Options);
        }

        public static bool TryDeserializePresence(
            string payload,
            out CbotPresenceSnapshot? snapshot)
        {
            snapshot = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                snapshot =
                    JsonSerializer.Deserialize<CbotPresenceSnapshot>(
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

        public static string Serialize(
            CbotExecutionStateSnapshot snapshot)
        {
            return JsonSerializer.Serialize(
                snapshot,
                Options);
        }

        public static bool TryDeserialize(
            string payload,
            out CbotExecutionStateSnapshot? snapshot)
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