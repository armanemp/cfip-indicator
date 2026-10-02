using System;
using System.Text.Json;

namespace CFIP.Contracts
{
    public static class SignalEnvelopeCodec
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

        public static string Serialize(
            SignalEnvelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            return JsonSerializer.Serialize(
                envelope,
                Options);
        }

        public static bool TryDeserialize(
            string payload,
            out SignalEnvelope envelope)
        {
            envelope = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                envelope =
                    JsonSerializer.Deserialize<SignalEnvelope>(
                        payload,
                        Options);

                return envelope != null;
            }
            catch
            {
                envelope = null;
                return false;
            }
        }
    }
}
