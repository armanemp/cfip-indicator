using System;

namespace CFIP.Contracts
{
    /// <summary>
    /// Stable, platform-neutral broker label identity for non-canonical scenarios.
    /// The base instance-scoped label remains the ownership root.
    /// </summary>
    public static class ScenarioExecutionIdentityRule
    {
        private const string ScenarioMarker = "|CFIP-S:";

        public static string ForScenario(
            string managedInstanceLabel,
            string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(managedInstanceLabel) ||
                string.IsNullOrWhiteSpace(scenarioId))
                return string.Empty;

            string root =
                managedInstanceLabel.Trim();
            string hash =
                ContractBusKeyHash.Hash(
                    scenarioId.Trim());

            if (string.IsNullOrWhiteSpace(hash))
                return string.Empty;

            return root +
                   ScenarioMarker +
                   hash;
        }
    }
}
