using System;
using System.Diagnostics.CodeAnalysis;
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

        public static string Serialize(SignalEnvelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            return JsonSerializer.Serialize(envelope, Options);
        }

        public static bool TryDeserialize(
            string payload,
            out SignalEnvelope? envelope)
        {
            envelope = null;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                SignalEnvelope? parsed =
                    JsonSerializer.Deserialize<SignalEnvelope>(
                    JsonSerializer.Deserialize<SignalEnvelope>(
                        payload,
                        Options);

                if (parsed == null ||
                    parsed.Identity == null ||
                    !ContractVersion.IsSupported(
                        parsed.Identity.ContractVersion))
                    return false;

                envelope = parsed;
                return true;
            }
            catch
            {
                envelope = null;
                return false;
            }
        }
    }
}
