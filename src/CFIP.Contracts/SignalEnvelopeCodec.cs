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
            envelope = default!;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                SignalEnvelope? parsed =
                    JsonSerializer.Deserialize<SignalEnvelope>(
                        payload,
                        Options);

                if (parsed == null)
                    return false;

                envelope = parsed;
                return true;
            }
            catch
            {
                envelope = default!;
                return false;
            }
        }
    }
}
