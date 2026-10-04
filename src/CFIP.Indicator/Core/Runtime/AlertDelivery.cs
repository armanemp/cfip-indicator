using System;
using CFIP.Contracts;

namespace cAlgo
{
    internal struct AlertDelivery
    {
        public AlertDelivery(
            AlertEnvelope envelope,
            int direction,
            bool playSound,
            string soundTypeName,
            string soundFilePath,
            string soundGroupKey = null)
        {
            Envelope = envelope;
            Key = envelope == null ? "" : envelope.AlertKey;
            Message = envelope == null ? "" : envelope.Message;
            Direction = direction;
            Critical = envelope != null && envelope.Critical;
            EnqueuedUtc = envelope == null
                ? DateTime.MinValue
                : envelope.EventUtc;
            PlaySound = playSound;
            SoundTypeName = soundTypeName;
            SoundFilePath = soundFilePath;
            SoundGroupKey = soundGroupKey;
        }

        public AlertEnvelope Envelope { get; }
        public string Key { get; }
        public string Message { get; }
        public int Direction { get; }
        public bool Critical { get; }
        public DateTime EnqueuedUtc { get; }
        public bool PlaySound { get; }
        public string SoundTypeName { get; }
        public string SoundFilePath { get; }
        public string SoundGroupKey { get; }
    }
}
