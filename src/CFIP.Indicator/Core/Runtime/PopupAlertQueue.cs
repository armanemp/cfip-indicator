using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal struct PopupAlert
    {
        public PopupAlert(string message, bool critical, DateTime enqueuedUtc)
        {
            Message = message;
            Critical = critical;
            EnqueuedUtc = enqueuedUtc;
        }

        public string Message { get; }
        public bool Critical { get; }
        public DateTime EnqueuedUtc { get; }
    }

    internal sealed class PopupAlertQueue
    {
        private readonly int _capacity;
        private readonly Queue<PopupAlert> _critical = new Queue<PopupAlert>();
        private readonly Queue<PopupAlert> _normal = new Queue<PopupAlert>();

        public PopupAlertQueue(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public int Count
        {
            get { return _critical.Count + _normal.Count; }
        }

        public bool Enqueue(
            string message,
            bool critical,
            DateTime enqueuedUtc)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            if (Count >= _capacity)
            {
                if (critical)
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

            PopupAlert alert =
                new PopupAlert(
                    message,
                    critical,
                    enqueuedUtc);

            if (critical)
                _critical.Enqueue(alert);
            else
                _normal.Enqueue(alert);

            return true;
        }

        public bool TryPeek(out PopupAlert alert)
        {
            if (_critical.Count > 0)
            {
                alert = _critical.Peek();
                return true;
            }

            if (_normal.Count > 0)
            {
                alert = _normal.Peek();
                return true;
            }

            alert = default(PopupAlert);
            return false;
        }

        public bool TryDequeue(out PopupAlert alert)
        {
            if (_critical.Count > 0)
            {
                alert = _critical.Dequeue();
                return true;
            }

            if (_normal.Count > 0)
            {
                alert = _normal.Dequeue();
                return true;
            }

            alert = default(PopupAlert);
            return false;
        }

        public void Clear()
        {
            _critical.Clear();
            _normal.Clear();
        }
    }
}
