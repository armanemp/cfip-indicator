using System;
using System.Collections.Generic;
using cAlgo.API;

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
            SoundType soundType,
            string soundFilePath,
            bool showPopup)
        {
            Key = key;
            Message = message;
            Critical = critical;
            EnqueuedUtc = enqueuedUtc;
            PlaySound = playSound;
            SoundType = soundType;
            SoundFilePath = soundFilePath;
            ShowPopup = showPopup;
        }

        public string Key { get; }
        public string Message { get; }
        public bool Critical { get; }
        public DateTime EnqueuedUtc { get; }
        public bool PlaySound { get; }
        public SoundType SoundType { get; }
        public string SoundFilePath { get; }
        public bool ShowPopup { get; }
    }

    internal sealed class AlertDeliveryQueue
    {
        private readonly int _capacity;
        private readonly Queue<AlertDelivery> _critical =
            new Queue<AlertDelivery>();
        private readonly Queue<AlertDelivery> _normal =
            new Queue<AlertDelivery>();

        public AlertDeliveryQueue(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public int Count
        {
            get { return _critical.Count + _normal.Count; }
        }

        public bool Enqueue(AlertDelivery delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery.Message))
                return false;

            if (Count >= _capacity)
            {
                if (delivery.Critical)
                {
                    if (_normal.Count > 0)
                        _normal.Dequeue();
                    else if (_critical.Count > 0)
                        _critical.Dequeue();
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            if (delivery.Critical)
                _critical.Enqueue(delivery);
            else
                _normal.Enqueue(delivery);

            return true;
        }

        public bool TryPeek(out AlertDelivery delivery)
        {
            if (_critical.Count > 0)
            {
                delivery = _critical.Peek();
                return true;
            }

            if (_normal.Count > 0)
            {
                delivery = _normal.Peek();
                return true;
            }

            delivery = default(AlertDelivery);
            return false;
        }

        public bool TryDequeue(out AlertDelivery delivery)
        {
            if (_critical.Count > 0)
            {
                delivery = _critical.Dequeue();
                return true;
            }

            if (_normal.Count > 0)
            {
                delivery = _normal.Dequeue();
                return true;
            }

            delivery = default(AlertDelivery);
            return false;
        }

        public void ClearPendingAlerts()
        {
            _critical.Clear();
            _normal.Clear();
        }
    }
}
