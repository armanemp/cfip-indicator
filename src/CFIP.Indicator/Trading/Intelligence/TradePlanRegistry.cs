using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class TradePlanRegistry
    {
        private readonly Dictionary<string, TradeOpportunityCandidate> _entries =
            new Dictionary<string, TradeOpportunityCandidate>(StringComparer.OrdinalIgnoreCase);

        public void Clear()
        {
            _entries.Clear();
        }

        public void Upsert(TradeOpportunityCandidate candidate)
        {
            if (candidate == null ||
                string.IsNullOrWhiteSpace(candidate.Id))
                return;

            _entries[candidate.Id] = candidate;
        }

        public bool TryGet(
            string id,
            out TradeOpportunityCandidate candidate)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                candidate = null;
                return false;
            }

            return _entries.TryGetValue(
                id,
                out candidate);
        }

        public IReadOnlyList<TradeOpportunityCandidate> Snapshot()
        {
            return new List<TradeOpportunityCandidate>(_entries.Values);
        }

        public int Count =>
            _entries.Count;

        public bool Contains(
            string id)
        {
            return !string.IsNullOrWhiteSpace(id) &&
                   _entries.ContainsKey(id);
        }

        public void Remove(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            _entries.Remove(id);
        }
    }
}
