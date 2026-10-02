using System;
using System.Globalization;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private AlertEnvelope BuildCanonicalAlertEnvelope(
            string key,
            string message,
            int direction,
            bool critical,
            DateTime now)
        {
            int closedM5 =
                ExtractVisualAlertM5(
                    key,
                    _lastEvaluatedM5);

            if (closedM5 < 0 &&
                _m5Bars != null &&
                _m5Bars.Count > 1)
            {
                closedM5 =
                    _m5Bars.Count - 2;
            }

            if (direction == 0)
            {
                direction =
                    GetAuthoritativeDirection();
            }

            OpportunityLane lane =
                ResolveProviderLane();

            string signalId =
                ResolveProviderSignalId(
                    closedM5);

            string scenarioId =
                ResolveProviderScenarioId(
                    signalId,
                    lane,
                    direction);

            string planId =
                ResolveProviderPlanId(
                    signalId);

            TradeOpportunityCandidate scenario = null;
            _tradePlanRegistry.TryGetCandidate(
                scenarioId,
                out scenario);

            string sourceTimeframe =
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    scenario,
                    ProviderScenarioIdentityRule.CanonicalM5);

            DateTime createdUtc =
                _m5Bars != null &&
                closedM5 >= 0 &&
                closedM5 < _m5Bars.Count
                    ? _m5Bars.OpenTimes[closedM5]
                    : now;

            long revision =
                Math.Max(
                    1,
                    _cfipProviderRevision);

            bool blockedCandidate =
                IsBlockedCandidateAlert(
                    key,
                    message);

            SignalStage stage =
                ResolveAlertSignalStage(
                    key,
                    blockedCandidate);

            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    signalId,
                    scenarioId,
                    planId,
                    SymbolName ?? "",
                    ResolveContractDirection(
                        direction),
                    ResolveContractLane(
                        lane),
                    sourceTimeframe,
                    createdUtc,
                    Math.Max(0, closedM5),
                    null,
                    revision,
                    signalId,
                    "CFIP-ALERT|" +
                    key +
                    "|" +
                    revision.ToString(
                        CultureInfo.InvariantCulture));

            string alertId =
                "CFIP-ALERT|" +
                signalId +
                "|" +
                scenarioId +
                "|" +
                planId +
                "|" +
                revision.ToString(
                    CultureInfo.InvariantCulture) +
                "|" +
                key;

            return new AlertEnvelope(
                identity,
                stage,
                alertId,
                key ?? "",
                message ?? "",
                critical,
                !blockedCandidate &&
                IsVisualSignalAlertKey(key),
                now);
        }

        private bool IsBlockedCandidateAlert(
            string key,
            string message)
        {
            return
                (key ?? "").StartsWith(
                    "RESTRICT|",
                    StringComparison.OrdinalIgnoreCase) ||
                (message ?? "").StartsWith(
                    "CFIP ENTRY BLOCKED",
                    StringComparison.OrdinalIgnoreCase);
        }

        private SignalStage ResolveAlertSignalStage(
            string key,
            bool blockedCandidate)
        {
            if (blockedCandidate)
            {
                return SignalStage.Blocked;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return SignalStage.Watch;
            }

            if (key.StartsWith(
                    "ACTION|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "HIGH|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "SMART|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "PENDING-",
                    StringComparison.OrdinalIgnoreCase))
            {
                return SignalStage.Confirmed;
            }

            if (key.StartsWith(
                    "EARLY|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "WATCH|",
                    StringComparison.OrdinalIgnoreCase))
            {
                return SignalStage.Watch;
            }

            if (key.StartsWith(
                    "REACTION|",
                    StringComparison.OrdinalIgnoreCase))
            {
                return SignalStage.Prediction;
            }

            if (key.StartsWith(
                    "REVERSAL|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "TP",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "SL|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "PROTECTION",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "POSITION-OPEN|",
                    StringComparison.OrdinalIgnoreCase))
            {
                return SignalStage.Active;
            }

            return SignalStage.Confirmed;
        }
    }
}
