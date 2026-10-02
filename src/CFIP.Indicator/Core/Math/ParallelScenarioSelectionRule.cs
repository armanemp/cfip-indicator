using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal static class ParallelScenarioSelectionRule
    {
        internal static string GetScenarioIdentity(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(candidate.ScenarioId))
                return candidate.ScenarioId.Trim();

            return
                (candidate.SourceTimeframe ?? "NONE") +
                "|" +
                (int)candidate.Lane +
                "|" +
                candidate.Direction;
        }

        internal static string CoverageKey(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return string.Empty;

            string source =
                string.IsNullOrWhiteSpace(candidate.SourceTimeframe)
                    ? "LANE"
                    : candidate.SourceTimeframe.Trim();

            return
                source +
                "|" +
                (int)candidate.Lane +
                "|" +
                candidate.Direction;
        }

        internal static int DisplayPriority(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return int.MinValue;

            int lanePriority;

            if (candidate.IsPrimaryTimeframeSignal)
            {
                lanePriority = 5000;
            }
            else
            {
                switch (candidate.Lane)
                {
                    case OpportunityLane.Strategic:
                        lanePriority = 4000;
                        break;

                    case OpportunityLane.CounterHtfTactical:
                        lanePriority = 3200;
                        break;

                    case OpportunityLane.MicroReaction:
                        lanePriority = 3000;
                        break;

                    default:
                        lanePriority =
                            string.IsNullOrWhiteSpace(candidate.SourceTimeframe)
                                ? 2600
                                : 2800;
                        break;
                }
            }

            int quality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        candidate.Quality));

            int compositeBonus =
                TradeOpportunityQualityRule.CalculateRankBonus(
                    candidate);

            return
                lanePriority +
                quality +
                compositeBonus;
        }

        internal static bool SameIdentity(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            return
                !string.IsNullOrWhiteSpace(GetScenarioIdentity(left)) &&
                string.Equals(
                    GetScenarioIdentity(left),
                    GetScenarioIdentity(right),
                    StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldReplace(
            TradeOpportunityCandidate existing,
            TradeOpportunityCandidate incoming,
            double priceTolerance)
        {
            if (existing == null ||
                incoming == null ||
                !SameIdentity(existing, incoming))
                return false;

            double tolerance =
                Math.Max(
                    0,
                    priceTolerance);

            bool geometryClose =
                IsFinite(existing.Entry) &&
                IsFinite(incoming.Entry) &&
                Math.Abs(existing.Entry - incoming.Entry) <= tolerance;

            if (geometryClose)
            {
                return
                    DisplayPriority(incoming) >
                    DisplayPriority(existing);
            }

            return
                incoming.CreatedM5 >=
                existing.CreatedM5;
        }

        internal static IReadOnlyList<TradeOpportunityCandidate> SelectForDisplay(
            IReadOnlyList<TradeOpportunityCandidate> candidates,
            int maximumVisible)
        {
            int maximum =
                Math.Max(
                    1,
                    maximumVisible);

            List<TradeOpportunityCandidate> ordered =
                new List<TradeOpportunityCandidate>();

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    TradeOpportunityCandidate candidate =
                        candidates[i];

                    if (candidate != null)
                        ordered.Add(candidate);
                }
            }

            ordered.Sort(CompareDisplay);

            if (ordered.Count <= maximum)
                return ordered;

            List<TradeOpportunityCandidate> selected =
                new List<TradeOpportunityCandidate>();

            HashSet<string> covered =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < ordered.Count &&
                 selected.Count < maximum;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    ordered[i];

                string coverage =
                    CoverageKey(candidate);

                if (string.IsNullOrWhiteSpace(coverage) ||
                    covered.Contains(coverage))
                    continue;

                selected.Add(candidate);
                covered.Add(coverage);
            }

            for (int i = 0;
                 i < ordered.Count &&
                 selected.Count < maximum;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    ordered[i];

                if (selected.Contains(candidate))
                    continue;

                selected.Add(candidate);
            }

            return selected;
        }

        private static int CompareDisplay(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            int priority =
                DisplayPriority(right).CompareTo(
                    DisplayPriority(left));

            if (priority != 0)
                return priority;

            int primary =
                right.IsPrimaryTimeframeSignal.CompareTo(
                    left.IsPrimaryTimeframeSignal);

            if (primary != 0)
                return primary;

            int quality =
                right.Quality.CompareTo(
                    left.Quality);

            if (quality != 0)
                return quality;

            int primaryLocation =
                right.PrimaryLocationQuality.CompareTo(
                    left.PrimaryLocationQuality);

            if (primaryLocation != 0)
                return primaryLocation;

            int confluence =
                right.PrimaryLocationConfluence.CompareTo(
                    left.PrimaryLocationConfluence);

            if (confluence != 0)
                return confluence;

            int location =
                right.LocationConfluenceScore.CompareTo(
                    left.LocationConfluenceScore);

            if (location != 0)
                return location;

            int actionable =
                right.ActionableNow.CompareTo(
                    left.ActionableNow);

            if (actionable != 0)
                return actionable;

            int rr =
                right.Tp1RR.CompareTo(
                    left.Tp1RR);

            if (rr != 0)
                return rr;

            int created =
                right.CreatedM5.CompareTo(
                    left.CreatedM5);

            if (created != 0)
                return created;

            return string.CompareOrdinal(
                GetScenarioIdentity(left),
                GetScenarioIdentity(right));
        }

        private static bool IsFinite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
