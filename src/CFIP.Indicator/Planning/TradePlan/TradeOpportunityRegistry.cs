using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class TradeOpportunityRegistry
    {
        private readonly List<TradeOpportunityCandidate> _items =
            new List<TradeOpportunityCandidate>();

        internal int Count
        {
            get { return _items.Count; }
        }

        internal TradeOpportunityCandidate this[int index]
        {
            get { return _items[index]; }
        }

        internal void Clear()
        {
            _items.Clear();
        }

        internal void Upsert(
            TradeOpportunityCandidate candidate,
            double proximity)
        {
            if (candidate == null)
                return;

            double tolerance =
                Math.Max(
                    0,
                    proximity);

            for (int i = 0;
                 i < _items.Count;
                 i++)
            {
                TradeOpportunityCandidate existing =
                    _items[i];

                if (existing == null)
                    continue;

                if (string.Equals(
                        existing.Id,
                        candidate.Id,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _items[i] =
                        candidate;
                    Sort();
                    return;
                }

                if (existing.Direction ==
                    candidate.Direction &&
                    Math.Abs(
                        existing.Entry -
                        candidate.Entry) <=
                    tolerance)
                {
                    if (Compare(candidate, existing) > 0)
                        _items[i] = candidate;

                    Sort();
                    return;
                }
            }

            _items.Add(candidate);
            Sort();
        }

        internal void Limit(
            int maximum)
        {
            int keep =
                Math.Max(
                    1,
                    maximum);

            while (_items.Count > keep)
                _items.RemoveAt(
                    _items.Count - 1);
        }

        private void Sort()
        {
            _items.Sort(
                Compare);
        }

        private static int Compare(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            if (left == null)
                return right == null ? 0 : -1;

            if (right == null)
                return 1;

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

            return string.CompareOrdinal(
                right.Id,
                left.Id);
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
    }
}
