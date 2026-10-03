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

        public bool UpsertScenario(
            TradeOpportunityCandidate candidate,
            double basePriceTolerance)
        {
            if (candidate == null ||
                string.IsNullOrWhiteSpace(candidate.Id))
                return false;

            string existingKey = null;
            TradeOpportunityCandidate existing = null;

            foreach (KeyValuePair<string, TradeOpportunityCandidate> pair
                     in _entries)
            {
                if (ParallelScenarioSelectionRule.SameIdentity(
                        pair.Value,
                        candidate))
                {
                    existingKey = pair.Key;
                    existing = pair.Value;
                    break;
                }
            }

            if (existing != null &&
                !ParallelScenarioSelectionRule.ShouldReplace(
                    existing,
                    candidate,
                    Math.Max(
                        0,
                        Math.Max(
                            basePriceTolerance,
                            Math.Min(
                                existing.Risk,
                                candidate.Risk) *
                            0.10))))
                return false;

            if (existingKey != null &&
                !string.Equals(
                    existingKey,
                    candidate.Id,
                    StringComparison.OrdinalIgnoreCase))
                _entries.Remove(existingKey);

            _entries[candidate.Id] = candidate;
            return true;
        }

        public bool TryGetCandidate(
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
            List<TradeOpportunityCandidate> items =
                new List<TradeOpportunityCandidate>(
                    _entries.Values);

            items.Sort(
                CompareCandidates);

            return items;
        }

        public bool TryGetBest(
            out TradeOpportunityCandidate candidate)
        {
            candidate = null;

            foreach (TradeOpportunityCandidate item
                     in _entries.Values)
            {
                if (item == null)
                    continue;

                if (candidate == null ||
                    CompareCandidates(item, candidate) < 0)
                    candidate = item;
            }

            return candidate != null;
        }

        public int Count =>
            _entries.Count;

        private static int CompareCandidates(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            if (left == null)
                return right == null ? 0 : 1;

            if (right == null)
                return -1;

            int actionable =
                right.ActionableNow.CompareTo(
                    left.ActionableNow);

            if (actionable != 0)
                return actionable;

            int quality =
                right.Quality.CompareTo(
                    left.Quality);

            if (quality != 0)
                return quality;

            int rr =
                right.Tp1RR.CompareTo(
                    left.Tp1RR);

            if (rr != 0)
                return rr;

            int lane =
                LaneRank(right.Lane).CompareTo(
                    LaneRank(left.Lane));

            if (lane != 0)
                return lane;

            int created =
                right.CreatedM5.CompareTo(
                    left.CreatedM5);

            if (created != 0)
                return created;

            return string.CompareOrdinal(
                left.Id,
                right.Id);
        }

        private static int LaneRank(
            OpportunityLane lane)
        {
            switch (lane)
            {
                case OpportunityLane.Strategic:
                    return 4;
                case OpportunityLane.CounterHtfTactical:
                    return 3;
                case OpportunityLane.Tactical:
                    return 2;
                case OpportunityLane.MicroReaction:
                    return 1;
                default:
                    return 0;
            }
        }

        public IReadOnlyList<TradeOpportunityCandidate> SelectScenariosForDisplay(
            int maximumVisible)
        {
            return ParallelScenarioSelectionRule.SelectForDisplay(
                Snapshot(),
                maximumVisible);
        }

        public IReadOnlyList<TradeOpportunityCandidate> SelectScenariosForExecution()
        {
            List<TradeOpportunityCandidate> items =
                new List<TradeOpportunityCandidate>();

            foreach (TradeOpportunityCandidate candidate in _entries.Values)
            {
                if (candidate == null ||
                    candidate.PresentationOnly ||
                    !candidate.ExecutionPolicyAllowed)
                    continue;

                bool current =
                    candidate.ActionableNow;

                bool future =
                    candidate.FutureOrderReady &&
                    (candidate.ExecutionMode ==
                        ExecutionMode.ContinuationStop ||
                     candidate.ExecutionMode ==
                        ExecutionMode.ReversalLimit);

                if (!current && !future)
                    continue;

                items.Add(candidate);
            }

            items.Sort(CompareCandidates);
            return items;
        }

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
