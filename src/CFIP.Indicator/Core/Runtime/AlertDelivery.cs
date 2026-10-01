using System;

namespace cAlgo
{
    internal struct AlertDelivery
    {
        public AlertDelivery(
            string key,
            string message,
            bool critical,
            DateTime enqueuedUtc,
            bool playSound,
            string soundTypeName,
            string soundFilePath,
            bool showPopup)
        {
            Key = key;
            Message = message;
            Critical = critical;
            EnqueuedUtc = enqueuedUtc;
            PlaySound = playSound;
            SoundTypeName = soundTypeName;
            SoundFilePath = soundFilePath;
            ShowPopup = showPopup;
        }

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
