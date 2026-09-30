using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class LifecycleEventIdempotencyGuard
    {
        private const int MaxRememberedEvents = 512;

        private readonly HashSet<string> _processedKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private readonly Queue<string> _processedOrder =
            new Queue<string>();

        public bool TryBegin(
            string eventType,
            long entityId)
        {
            if (string.IsNullOrWhiteSpace(eventType) ||
                entityId <= 0)
                return false;

            string key =
                eventType.Trim() +
                ":" +
                entityId;

            if (!_processedKeys.Add(key))
                return false;

            _processedOrder.Enqueue(key);
            Trim();

            return true;
        }

        internal int RememberedEventCount
        {
            get { return _processedOrder.Count; }
        }

        private void Trim()
        {
            while (_processedOrder.Count >
                   MaxRememberedEvents)
            {
                string oldest =
                    _processedOrder.Dequeue();

                _processedKeys.Remove(oldest);
            }
        }
    }
}
