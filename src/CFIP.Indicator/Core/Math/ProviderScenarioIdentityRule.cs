using System;

namespace cAlgo
{
    internal static class ProviderScenarioIdentityRule
    {
        internal const string CanonicalM5 = "M5";

        internal static string ResolveSourceTimeframe(
            TradeOpportunityCandidate candidate,
            string fallback = CanonicalM5)
        {
            if (candidate != null)
            {
                string source =
                    NormalizeTimeframe(candidate.SourceTimeframe);

                if (!string.IsNullOrWhiteSpace(source))
                    return source;

                string basePlan =
                    NormalizeTimeframe(candidate.BasePlanTimeframe);

                if (!string.IsNullOrWhiteSpace(basePlan))
                    return basePlan;
            }

            string normalizedFallback =
                NormalizeTimeframe(fallback);

            return string.IsNullOrWhiteSpace(normalizedFallback)
                ? CanonicalM5
                : normalizedFallback;
        }

        internal static string ResolveScenarioId(
            TradeOpportunityCandidate candidate,
            string fallback)
        {
            if (candidate != null &&
                !string.IsNullOrWhiteSpace(candidate.ScenarioId))
                return candidate.ScenarioId.Trim();

            return
                string.IsNullOrWhiteSpace(fallback)
                    ? "CANONICAL-NONE"
                    : fallback.Trim();
        }

        private static string NormalizeTimeframe(string value)
        {
            return
                string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value.Trim().ToUpperInvariant();
        }
    }
}
