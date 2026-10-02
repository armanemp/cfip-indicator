using System;
using System.Collections.Generic;
using System.Globalization;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private string BuildScenarioBatchFingerprint(
            int closedM5)
        {
            RefreshParallelOpportunityCandidates(closedM5);

            List<string> items =
                new List<string>();

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    _opportunityCandidates[i];

                if (!IsScenarioBatchExecutableCandidate(candidate, closedM5))
                    continue;

                items.Add(
                    string.Join(
                        "|",
                        candidate.ScenarioId ?? "",
                        candidate.Direction.ToString(
                            CultureInfo.InvariantCulture),
                        candidate.CreatedM5.ToString(
                            CultureInfo.InvariantCulture),
                        candidate.Entry.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        candidate.Stop.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        candidate.Tp1.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        candidate.ExecutionMode.ToString(),
                        candidate.RequestedVolume.ToString(
                            "R",
                            CultureInfo.InvariantCulture)));
            }

            items.Sort(StringComparer.Ordinal);
            return string.Join("||", items);
        }

        private SignalScenarioBatch BuildScenarioBatch(
            SignalEnvelope canonicalEnvelope,
            int closedM5,
            DateTime observedUtc)
        {
            RefreshParallelOpportunityCandidates(closedM5);

            List<SignalEnvelope> scenarios =
                new List<SignalEnvelope>();

            if (canonicalEnvelope != null)
                scenarios.Add(canonicalEnvelope);

            string canonicalScenario =
                canonicalEnvelope == null ||
                canonicalEnvelope.Identity == null
                    ? ""
                    : canonicalEnvelope.Identity.ScenarioId ?? "";

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    _opportunityCandidates[i];

                if (!IsScenarioBatchExecutableCandidate(candidate, closedM5))
                    continue;

                if (string.Equals(
                        candidate.ScenarioId ?? "",
                        canonicalScenario,
                        StringComparison.Ordinal))
                    continue;

                SignalEnvelope envelope;
                if (!TryBuildScenarioEnvelope(
                        candidate,
                        observedUtc,
                        out envelope))
                    continue;

                scenarios.Add(envelope);
            }

            return new SignalScenarioBatch(
                ContractVersion.Current,
                InstanceId ?? "",
                SymbolName ?? "",
                observedUtc,
                Math.Max(
                    1,
                    _cfipProviderRevision),
                scenarios.ToArray());
        }

        private bool TryBuildScenarioEnvelope(
            TradeOpportunityCandidate candidate,
            DateTime observedUtc,
            out SignalEnvelope envelope)
        {
            envelope = null;

            if (!IsScenarioBatchExecutableCandidate(
                    candidate,
                    candidate == null
                        ? -1
                        : candidate.CreatedM5))
                return false;

            string signalId =
                ResolveProviderSignalId(
                    candidate.CreatedM5);

            string scenarioId =
                candidate.ScenarioId ?? "";

            if (string.IsNullOrWhiteSpace(signalId) ||
                string.IsNullOrWhiteSpace(scenarioId))
                return false;

            DateTime createdUtc =
                _m5Bars.OpenTimes[candidate.CreatedM5];

            string planId =
                "CFIP-PLAN|" +
                signalId +
                "|SC|" +
                scenarioId;

            string executionLabel =
                ScenarioExecutionIdentityRule.ForScenario(
                    ManagedExecutionLabel(),
                    scenarioId);

            if (string.IsNullOrWhiteSpace(executionLabel))
                return false;

            double target =
                IsFinitePositive(candidate.Tp1)
                    ? candidate.Tp1
                    : candidate.Tp4;

            if (!IsFinitePositive(candidate.Entry) ||
                !IsFinitePositive(candidate.Stop) ||
                !IsFinitePositive(target) ||
                !IsFinitePositive(candidate.RequestedVolume))
                return false;

            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    signalId,
                    scenarioId,
                    planId,
                    SymbolName ?? "",
                    ResolveContractDirection(
                        candidate.Direction),
                    ResolveContractLane(
                        candidate.Lane),
                    ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                        candidate,
                        ProviderScenarioIdentityRule.CanonicalM5),
                    createdUtc,
                    candidate.CreatedM5,
                    null,
                    Math.Max(
                        1,
                        _cfipProviderRevision),
                    signalId,
                    string.Join(
                        "|",
                        "CFIP-SC6M",
                        signalId,
                        scenarioId,
                        planId,
                        candidate.Entry.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        candidate.Stop.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        target.ToString(
                            "R",
                            CultureInfo.InvariantCulture)));

            ExecutionAction action =
                ResolveScenarioExecutionAction(
                    candidate.ExecutionMode);

            if (action == ExecutionAction.None)
                return false;

            DateTime? expiryUtc =
                action == ExecutionAction.PendingStop ||
                action == ExecutionAction.PendingLimit
                    ? observedUtc.AddMinutes(
                        Math.Max(
                            15,
                            PendingOrderExpiryMinutes))
                    : (DateTime?)null;

            CFIP.Contracts.ExecutionIntent intent =
                new CFIP.Contracts.ExecutionIntent(
                    identity,
                    action,
                    candidate.Entry,
                    candidate.Stop,
                    target,
                    candidate.RequestedVolume,
                    SizingMode.ToString(),
                    observedUtc,
                    expiryUtc,
                    candidate.Source ?? "",
                    executionLabel,
                    null)
                {
                    MaxSpreadToStopRiskRatio =
                        MaximumSpreadToStopRiskRatio
                };

            double risk =
                Math.Abs(
                    candidate.Entry -
                    candidate.Stop);

            PlanSnapshot plan =
                new PlanSnapshot(
                    candidate.Entry,
                    candidate.IdealEntry,
                    0,
                    0,
                    candidate.Trigger,
                    candidate.Invalidation,
                    candidate.Stop,
                    candidate.Tp1,
                    candidate.Tp2,
                    candidate.Tp3,
                    candidate.Tp4,
                    risk,
                    candidate.Tp1RR,
                    candidate.Tp2RR,
                    candidate.Tp3RR,
                    candidate.Tp4RR,
                    candidate.EntryDistanceAtr > 0
                        ? candidate.Quality
                        : 0,
                    candidate.Quality,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    candidate.Source ?? "",
                    candidate.Source ?? "",
                    candidate.Source ?? "",
                    candidate.Source ?? "",
                    candidate.Source ?? "",
                    candidate.Source ?? "",
                    candidate.ActionabilityReason ?? "");

            envelope =
                new SignalEnvelope(
                    identity,
                    SignalStage.Confirmed,
                    plan,
                    intent,
                    observedUtc);

            return true;
        }

        private ExecutionAction ResolveScenarioExecutionAction(
            ExecutionMode executionMode)
        {
            switch (executionMode)
            {
                case ExecutionMode.RetestMarket:
                case ExecutionMode.BreakoutMarket:
                    return ExecutionAction.Market;

                case ExecutionMode.ContinuationStop:
                    return ExecutionAction.PendingStop;

                case ExecutionMode.ReversalLimit:
                    return ExecutionAction.PendingLimit;

                default:
                    return ExecutionAction.None;
            }
        }

        private bool IsScenarioBatchExecutableCandidate(
            TradeOpportunityCandidate candidate,
            int closedM5)
        {
            return candidate != null &&
                candidate.CreatedM5 == closedM5 &&
                !candidate.PresentationOnly &&
                candidate.ExecutionPolicyAllowed &&
                candidate.ActionableNow &&
                candidate.Direction != 0 &&
                IsFinitePositive(candidate.Entry) &&
                IsFinitePositive(candidate.Stop) &&
                IsFinitePositive(
                    IsFinitePositive(candidate.Tp1)
                        ? candidate.Tp1
                        : candidate.Tp4) &&
                IsFinitePositive(candidate.RequestedVolume);
        }
    }
}
