using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class LifecycleEventIdempotencyGuard
    {
        private readonly HashSet<string> _processedKeys =
            new HashSet<string>(StringComparer.Ordinal);

        public bool TryBegin(
            string eventType,
            long entityId)
        {
            if (string.IsNullOrWhiteSpace(eventType) ||
                entityId <= 0)
                return false;

            return _processedKeys.Add(
                eventType.Trim() + ":" + entityId);
        }
    }
}
