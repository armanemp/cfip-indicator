using System;
using CFIP.Contracts;

namespace cAlgo
{
    internal struct AlertDelivery
    {
        public AlertDelivery(
            AlertEnvelope envelope,
            bool playSound,
            string soundTypeName,
            string soundFilePath,
            bool showPopup)
        {
            Envelope = envelope;
            Key = envelope == null ? "" : envelope.AlertKey;
            Message = envelope == null ? "" : envelope.Message;
            Critical = envelope != null && envelope.Critical;
            EnqueuedUtc = envelope == null
                ? DateTime.MinValue
                : envelope.EventUtc;
            PlaySound = playSound;
            SoundTypeName = soundTypeName;
            SoundFilePath = soundFilePath;
            ShowPopup = showPopup;
        }

        public AlertEnvelope Envelope { get; }
        public string Key { get; }
        public string Message { get; }
        public bool Critical { get; }
        public DateTime EnqueuedUtc { get; }
        public bool PlaySound { get; }
        public string SoundTypeName { get; }
        public string SoundFilePath { get; }
        public bool ShowPopup { get; }
    }
}
