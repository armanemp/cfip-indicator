using System;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private SignalEnvelope _cfipProviderEnvelope;
        private cAlgo.ExecutionIntent _cfipProviderExecutionIntent;
        private int _cfipProviderExecutionIntentM5 = -1;
        private long _cfipProviderRevision;
        private string _cfipProviderFingerprint = "";
        private DateTime _cfipProviderUpdatedUtc = DateTime.MinValue;

        [Output(
            "CFIP Provider Heartbeat",
            PlotType = PlotType.DiscontinuousLine,
            LineColor = "Transparent")]
        public IndicatorDataSeries ProviderHeartbeat { get; set; }

        // Read-only cross-boundary snapshot. The cBot may consume this value,
        // but it can never replace or mutate Indicator state through this API.
        public SignalEnvelope LatestSignalEnvelope =>
            _cfipProviderEnvelope;

        public long ProviderRevision =>
            _cfipProviderRevision;

        public bool ProviderReady =>
            _initializationReady &&
            _cfipProviderEnvelope != null;

        public string ProviderState =>
            ResolveProviderState(_cfipProviderEnvelope);

        public DateTime ProviderUpdatedUtc =>
            _cfipProviderUpdatedUtc;

        private void CaptureProviderExecutionIntent(
            cAlgo.ExecutionIntent intent)
        {
            if (intent == null)
                return;

            _cfipProviderExecutionIntent = intent;
            _cfipProviderExecutionIntentM5 =
                intent.CreatedM5;
        }

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

            DateTime createdUtc =
                _m5Bars.OpenTimes[closedM5];

            CFIP.Contracts.ExecutionIntent canonicalIntent =
                BuildCanonicalExecutionIntent(
                    signalId,
                    scenarioId,
                    planId,
                    lane,
                    direction,
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

            string sourceTimeframe =
                Bars == null
                    ? "UNKNOWN"
                    : Bars.TimeFrame.ToString();

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
                return _activeExecutionScenarioId;

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

        private CFIP.Contracts.ExecutionIntent
            BuildCanonicalExecutionIntent(
                string signalId,
                string scenarioId,
                string planId,
                OpportunityLane lane,
                int direction,
                DateTime createdUtc,
                DateTime observedUtc,
                int closedM5)
        {
            if (_cfipProviderExecutionIntent == null ||
                _cfipProviderExecutionIntentM5 != closedM5)
                return null;

            ExecutionAction action =
                ResolveContractExecutionAction(
                    _cfipProviderExecutionIntent);

            if (action == ExecutionAction.None)
                return null;

            DateTime? expiryUtc =
                action == ExecutionAction.PendingStop ||
                action == ExecutionAction.PendingLimit
                    ? createdUtc.AddMinutes(
                        Math.Max(
                            15,
                            PendingOrderExpiryMinutes))
                    : (DateTime?)null;

            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    signalId,
                    scenarioId,
                    planId,
                    SymbolName ?? "",
                    ResolveContractDirection(direction),
                    ResolveContractLane(lane),
                    Bars == null
                        ? "UNKNOWN"
                        : Bars.TimeFrame.ToString(),
                    createdUtc,
                    closedM5,
                    expiryUtc,
                    Math.Max(
                        1,
                        _cfipProviderRevision),
                    signalId,
                    "");

            return new CFIP.Contracts.ExecutionIntent(
                identity,
                action,
                _cfipProviderExecutionIntent.RequestedEntry,
                _cfipProviderExecutionIntent.Stop,
                _cfipProviderExecutionIntent.Target,
                _cfipProviderExecutionIntent.Volume > 0
                    ? _cfipProviderExecutionIntent.Volume
                    : (double?)null,
                SizingMode.ToString(),
                observedUtc,
                expiryUtc,
                _cfipProviderExecutionIntent.Source ?? "");
        }

        private PlanSnapshot BuildCanonicalPlanSnapshot(
            int direction,
            CFIP.Contracts.ExecutionIntent canonicalIntent)
        {
            if (_plan != null &&
                (direction == 0 ||
                 _plan.Direction == direction))
            {
                return new PlanSnapshot(
                    _plan.Entry,
                    _plan.IdealEntry,
                    _plan.EntryZoneLow,
                    _plan.EntryZoneHigh,
                    _plan.EntryTrigger,
                    _plan.EntryInvalidation,
                    _plan.Stop,
                    _plan.Tp1,
                    _plan.Tp2,
                    _plan.Tp3,
                    _plan.Tp4,
                    _plan.Risk,
                    _plan.Tp1RR,
                    _plan.Tp2RR,
                    _plan.Tp3RR,
                    _plan.Tp4RR,
                    _plan.EntryQuality,
                    ResolvePlanQuality(),
                    _plan.StopQuality,
                    _plan.Tp1Quality,
                    _plan.Tp2Quality,
                    _plan.Tp3Quality,
                    _plan.Tp4Quality,
                    _plan.HtfTargetCount,
                    _plan.EntrySource ?? "",
                    _plan.StopSource ?? "",
                    _plan.Tp1Source ?? "",
                    _plan.Tp2Source ?? "",
                    _plan.Tp3Source ?? "",
                    _plan.Tp4Source ?? "",
                    ResolveProviderAnalyticalReason());
            }

            if (_setupPreview != null &&
                (direction == 0 ||
                 _setupPreview.Direction == direction))
            {
                return new PlanSnapshot(
                    _setupPreview.Entry,
                    _setupPreview.IdealEntry,
                    _setupPreview.ZoneLow,
                    _setupPreview.ZoneHigh,
                    _setupPreview.Trigger,
                    _setupPreview.Invalidation,
                    _setupPreview.Stop,
                    _setupPreview.Tp1,
                    _setupPreview.Tp2,
                    _setupPreview.Tp3,
                    _setupPreview.Tp4,
                    _setupPreview.Risk,
                    0,
                    0,
                    0,
                    0,
                    _setupPreview.Direction == 0
                        ? 0
                        : (_decision == null
                            ? 0
                            : _decision.EntryLocationQuality),
                    ResolvePlanQuality(),
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    _executionModel == null
                        ? ""
                        : _executionModel.Source ?? "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            if (canonicalIntent != null)
            {
                double risk =
                    Math.Abs(
                        canonicalIntent.RequestedEntry -
                        canonicalIntent.Stop);

                return new PlanSnapshot(
                    canonicalIntent.RequestedEntry,
                    canonicalIntent.RequestedEntry,
                    0,
                    0,
                    canonicalIntent.RequestedEntry,
                    0,
                    canonicalIntent.Stop,
                    canonicalIntent.InitialTarget,
                    0,
                    0,
                    0,
                    risk,
                    0,
                    0,
                    0,
                    0,
                    _decision == null
                        ? 0
                        : _decision.EntryLocationQuality,
                    ResolvePlanQuality(),
                    0,
                    _decision == null
                        ? 0
                        : _decision.ActionableTp1RR > 0
                            ? 100
                            : 0,
                    0,
                    0,
                    0,
                    0,
                    "EXECUTION INTENT",
                    "EXECUTION INTENT",
                    "EXECUTION INTENT",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            if (_executionModel != null &&
                (direction == 0 ||
                 _executionModel.Direction == direction))
            {
                return new PlanSnapshot(
                    _executionModel.ActualEntry,
                    _executionModel.IdealEntry,
                    _executionModel.ZoneLow,
                    _executionModel.ZoneHigh,
                    _executionModel.Trigger,
                    _executionModel.Invalidation,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Stop,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp1,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp2,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp3,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp4,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Risk,
                    0,
                    0,
                    0,
                    0,
                    _executionModel.Quality,
                    ResolvePlanQuality(),
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    _executionModel.Source ?? "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            return null;
        }

        private int ResolvePlanQuality()
        {
            if (_decision == null)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    100,
                    _decision.SmartQuality));
        }

        private string ResolveProviderAnalyticalReason()
        {
            if (_decision == null)
                return "NO DECISION";

            if (!string.IsNullOrWhiteSpace(
                    _decision.ActionabilityReason))
                return _decision.ActionabilityReason;

            if (!string.IsNullOrWhiteSpace(
                    _decision.BlockReason))
                return _decision.BlockReason;

            return _decision.Reason ?? "";
        }

        private SignalStage ResolveProviderSignalStage(
            int direction,
            CFIP.Contracts.ExecutionIntent canonicalIntent)
        {
            if (direction == 0)
                return SignalStage.Watch;

            if (_plan != null &&
                _plan.IsLivePosition)
                return SignalStage.Active;

            if (_decision != null &&
                !_decision.EntryAllowed)
                return SignalStage.Blocked;

            if (canonicalIntent != null)
                return SignalStage.Confirmed;

            if (_decision != null &&
                _decision.TriggerReady)
                return SignalStage.Confirmed;

            return
                _decision != null
                    ? SignalStage.Prediction
                    : SignalStage.Unavailable;
        }

        private ExecutionAction ResolveContractExecutionAction(
            cAlgo.ExecutionIntent intent)
        {
            if (intent == null)
                return ExecutionAction.None;

            if (intent.Policy ==
                DecisionPolicyMode.Aggressive)
                return ExecutionAction.Aggressive;

            if (intent.Kind ==
                ExecutionIntentKind.Stop)
                return ExecutionAction.PendingStop;

            if (intent.Kind ==
                ExecutionIntentKind.Limit)
                return ExecutionAction.PendingLimit;

            if (intent.Kind ==
                ExecutionIntentKind.Market)
                return ExecutionAction.Market;

            return ExecutionAction.None;
        }

        private TradeDirection ResolveContractDirection(
            int direction)
        {
            if (direction > 0)
                return TradeDirection.Buy;

            if (direction < 0)
                return TradeDirection.Sell;

            return TradeDirection.None;
        }

        private CFIP.Contracts.OpportunityLane
            ResolveContractLane(OpportunityLane lane)
        {
            switch (lane)
            {
                case OpportunityLane.Strategic:
                    return CFIP.Contracts.OpportunityLane.Strategic;

                case OpportunityLane.CounterHtfTactical:
                    return CFIP.Contracts.OpportunityLane.CounterHtfTactical;

                case OpportunityLane.MicroReaction:
                    return CFIP.Contracts.OpportunityLane.MicroReaction;

                default:
                    return CFIP.Contracts.OpportunityLane.Tactical;
            }
        }

        private string BuildProviderIdempotencyKey(
            string signalId,
            string scenarioId,
            string planId,
            CFIP.Contracts.ExecutionIntent intent,
            long revision)
        {
            return
                string.Join(
                    "|",
                    "CFIP-P2",
                    signalId,
                    scenarioId,
                    planId,
                    intent == null
                        ? "NONE"
                        : intent.Action.ToString(),
                    revision.ToString(
                        CultureInfo.InvariantCulture));
        }

        private string ResolveProviderState(
            SignalEnvelope envelope)
        {
            if (envelope == null)
                return
                    _initializationReady
                        ? "NO SNAPSHOT"
                        : "INITIALIZING";

            if (envelope.Stage == SignalStage.Blocked)
                return "BLOCKED";

            if (envelope.Intent != null)
                return "EXECUTION INTENT";

            return
                envelope.Stage.ToString()
                    .ToUpperInvariant();
        }

        private void PublishProviderHeartbeat(
            DateTime now)
        {
            _cfipProviderUpdatedUtc = now;
        }

        private void PublishProviderHeartbeatValue(
            int index)
        {
            if (ProviderHeartbeat != null &&
                index >= 0)
            {
                ProviderHeartbeat[index] =
                    _cfipProviderRevision;
            }
        }
    }
}
