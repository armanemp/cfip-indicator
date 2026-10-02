#nullable disable
using System;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class SignalScenarioBatchCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(
            SignalScenarioBatch batch) =>
            JsonSerializer.Serialize(
                batch,
                Options);

        public static bool TryDeserialize(
            string payload,
            out SignalScenarioBatch batch)
        {
            batch = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                SignalScenarioBatch parsed =
                    JsonSerializer.Deserialize<SignalScenarioBatch>(
                        payload,
                        Options);

                if (parsed == null)
                    return false;

                batch = parsed;
                return true;
            }
            catch
            {
                batch = null;
                return false;
            }
        }
    }
}
#nullable enable
