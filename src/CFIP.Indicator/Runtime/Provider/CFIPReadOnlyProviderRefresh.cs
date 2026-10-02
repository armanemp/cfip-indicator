using System;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void RefreshReadOnlyProvider(
            int closedM5)
        {
            if (!_initializationReady ||
                _m5Bars == null ||
                closedM5 < 0 ||
                closedM5 >= _m5Bars.Count)
                return;

            DateTime observedUtc =
                Server.TimeInUtc;

            string signalId =
                ResolveProviderSignalId(closedM5);

            if (string.IsNullOrWhiteSpace(signalId))
                return;

            int direction =
                _decision != null
                    ? _decision.Direction
                    : (_plan != null ? _plan.Direction : 0);

            OpportunityLane lane =
                ResolveProviderLane();

            string scenarioId =
                ResolveProviderScenarioId(
                    signalId,
                    lane,
                    direction);

            string planId =
                ResolveProviderPlanId(
                    signalId);

            TradeOpportunityCandidate providerScenario;
            _tradePlanRegistry.TryGetCandidate(
                scenarioId,
                out providerScenario);

            string sourceTimeframe =
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    providerScenario,
                    ProviderScenarioIdentityRule.CanonicalM5);

            DateTime createdUtc =
                _m5Bars.OpenTimes[closedM5];

            CFIP.Contracts.ExecutionIntent canonicalIntent =
                BuildCanonicalExecutionIntent(
                    signalId,
                    scenarioId,
                    planId,
                    lane,
                    direction,
                    sourceTimeframe,
                    createdUtc,
                    observedUtc,
                    closedM5);

            PlanSnapshot planSnapshot =
                BuildCanonicalPlanSnapshot(
                    direction,
                    canonicalIntent);

            SignalStage stage =
                ResolveProviderSignalStage(
                    direction,
                    canonicalIntent);

            string expiryKey =
                canonicalIntent == null ||
                !canonicalIntent.ExpiryUtc.HasValue
                    ? ""
                    : canonicalIntent.ExpiryUtc.Value
                        .Ticks
                        .ToString(
                            CultureInfo.InvariantCulture);

            string fingerprint =
                string.Join(
                    "|",
                    signalId,
                    scenarioId,
                    planId,
                    direction.ToString(
                        CultureInfo.InvariantCulture),
                    lane.ToString(),
                    sourceTimeframe,
                    closedM5.ToString(
                        CultureInfo.InvariantCulture),
                    stage.ToString(),
                    canonicalIntent == null
                        ? "NONE"
                        : canonicalIntent.Action.ToString(),
                    canonicalIntent == null
                        ? "0"
                        : canonicalIntent.RequestedEntry
                            .ToString("R", CultureInfo.InvariantCulture),
                    canonicalIntent == null
                        ? "0"
                        : canonicalIntent.Stop
                            .ToString("R", CultureInfo.InvariantCulture),
                    canonicalIntent == null
                        ? "0"
                        : canonicalIntent.InitialTarget
                            .ToString("R", CultureInfo.InvariantCulture),
                    expiryKey,
                    planSnapshot == null
                        ? "NO-PLAN"
                        : planSnapshot.PlanRisk
                            .ToString("R", CultureInfo.InvariantCulture),
                    _lifecycleState.ToString(),
                    _autoTradingState ?? "",
                    _autoTradingReason ?? "");

            if (string.Equals(
                    fingerprint,
                    _cfipProviderFingerprint,
                    StringComparison.Ordinal))
            {
                PublishProviderHeartbeat(
                    observedUtc);

                return;
            }

            _cfipProviderRevision =
                Math.Max(
                    1,
                    _cfipProviderRevision + 1);

            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    signalId,
                    scenarioId,
                    planId,
                    SymbolName ?? "",
                    ResolveContractDirection(direction),
                    ResolveContractLane(lane),
                    sourceTimeframe,
                    createdUtc,
                    closedM5,
                    canonicalIntent == null
                        ? null
                        : canonicalIntent.ExpiryUtc,
                    _cfipProviderRevision,
                    signalId,
                    BuildProviderIdempotencyKey(
                        signalId,
                        scenarioId,
                        planId,
                        canonicalIntent,
                        _cfipProviderRevision));

            _cfipProviderEnvelope =
                new SignalEnvelope(
                    identity,
                    stage,
                    planSnapshot,
                    canonicalIntent,
                    observedUtc);

            _cfipProviderFingerprint =
                fingerprint;

            _cfipProviderUpdatedUtc =
                observedUtc;

            PublishProviderHeartbeat(
                observedUtc);
        }

        private string ResolveProviderSignalId(
            int closedM5)
        {
            if (_plan != null &&
                !string.IsNullOrWhiteSpace(
                    _plan.SignalTraceId))
                return _plan.SignalTraceId;

            return SignalTraceIdentityRule.CreateTraceId(
                SymbolName,
                Bars == null
                    ? "UNKNOWN"
                    : Bars.TimeFrame.ToString(),
                MemoryAccountScopeToken(),
                MemoryConfigurationFingerprint(),
                GetSignalBarOpenTimeUtcTicks(closedM5));
        }

        private OpportunityLane ResolveProviderLane()
        {
            if (_plan != null)
                return _plan.Lane;

            if (_decision != null)
            {
                if (_decision.TopDownEligible &&
                    string.Equals(
                        _decision.TopDownStage,
                        "ENTRY CALIBRATED",
                        StringComparison.OrdinalIgnoreCase))
                    return OpportunityLane.Strategic;

                return _decision.TacticalOpportunityLane;
            }

            return OpportunityLane.Tactical;
        }

        private string ResolveProviderScenarioId(
            string signalId,
            OpportunityLane lane,
            int direction)
        {
            if (!string.IsNullOrWhiteSpace(
                    _activeExecutionScenarioId))
                return ProviderScenarioIdentityRule.ResolveScenarioId(
                    null,
                    _activeExecutionScenarioId);

            if (_plan != null &&
                _decision != null)
            {
                string reason;
                TradeOpportunityCandidate selected = null;

                if (ScenarioExecutionPolicyRule.TryResolvePlanScenario(
                        _tradePlanRegistry.Snapshot(),
                        _plan,
                        _decision,
                        Math.Max(
                            Symbol.TickSize * 2,
                            Symbol.PipSize * 0.10),
                        out selected,
                        out reason) &&
                    selected != null)
                {
                    return ProviderScenarioIdentityRule.ResolveScenarioId(
                        selected,
                        string.Empty);
                }
            }

            return
                "CFIP-SC1|" +
                signalId +
                "|" +
                lane.ToString() +
                "|" +
                direction.ToString(
                    CultureInfo.InvariantCulture);
        }

        private string ResolveProviderPlanId(
            string signalId)
        {
            if (_plan != null &&
                !string.IsNullOrWhiteSpace(
                    _plan.SignalTraceId))
            {
                return
                    "CFIP-PLAN|" +
                    _plan.SignalTraceId;
            }

            return
                "CFIP-PLAN|" +
                signalId;
        }


    }
}
