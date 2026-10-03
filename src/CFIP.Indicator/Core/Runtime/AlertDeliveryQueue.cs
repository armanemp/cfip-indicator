using System;
using System.Collections.Generic;

namespace cAlgo
{
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
                // Only critical events may displace queued normal work.
                // PlaySound by itself does not raise priority; actionable
                // signal events already pass through this queue as critical.
                if (!delivery.Critical)
                    return false;

                if (_normal.Count > 0)
                    _normal.Dequeue();
                else
                {
                    // Keep already-buffered critical alerts intact. Under a burst
                    // of critical/sound events, reject the new event rather than
                    // silently dropping an existing signal/audio delivery.
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
