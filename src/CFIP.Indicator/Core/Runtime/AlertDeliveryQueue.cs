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
                // Sound-bearing alerts are user-visible signal events too.
                // Preserve them under burst conditions by evicting the oldest
                // normal diagnostic message before allowing the audio event to
                // enter the bounded transport.
                if (delivery.Critical || delivery.PlaySound)
                {
                    if (_normal.Count > 0)
                        _normal.Dequeue();
                    else if (delivery.Critical && _critical.Count > 0)
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
