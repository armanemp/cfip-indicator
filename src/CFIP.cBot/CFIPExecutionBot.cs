using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;
using CFIP.cBot.Binding;
using CFIP.cBot.Execution;
using CFIP.cBot.Shadow;
using CFIP.cBot.Recovery;

namespace CFIP.cBot
{
#pragma warning disable CS0612
    [Robot(
        CbotIdentity.DisplayName,
        DefaultTimeFrame = "M5",
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
#pragma warning restore CS0612
        private const string StartupState = "READY";

        [Parameter(
            "Enable Demo Market Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoMarketExecution { get; set; }

        [Parameter(
            "Enable Demo Pending Stop Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoPendingStopExecution { get; set; }

        [Parameter(
            "Enable Demo Pending Limit Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoPendingLimitExecution { get; set; }

        [Parameter(
            "Enable Demo Aggressive Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoAggressiveExecution { get; set; }

        [Parameter(
            "Enable Demo Management Execution",
            Group = "Execution",
            DefaultValue = false)]
        public bool EnableDemoManagementExecution { get; set; }

        [Parameter(
            "Enable Live Market Execution",
            Group = "Live Execution",
            DefaultValue = false)]
        public bool EnableLiveMarketExecution { get; set; }

        [Parameter(
            "Enable Live Pending Stop Execution",
            Group = "Live Execution",
            DefaultValue = false)]
        public bool EnableLivePendingStopExecution { get; set; }

        [Parameter(
            "Enable Live Pending Limit Execution",
            Group = "Live Execution",
            DefaultValue = false)]
        public bool EnableLivePendingLimitExecution { get; set; }

        [Parameter(
            "Enable Live Aggressive Execution",
            Group = "Live Execution",
            DefaultValue = false)]
        public bool EnableLiveAggressiveExecution { get; set; }

        [Parameter(
            "Enable Live Management Execution",
            Group = "Live Execution",
            DefaultValue = false)]
        public bool EnableLiveManagementExecution { get; set; }

        [Parameter(
            "Max Demo Executions Per Session",
            Group = "Safety",
            DefaultValue = 3,
            MinValue = 1,
            MaxValue = 20)]
        public int MaxDemoExecutionsPerSession { get; set; }

        [Parameter(
            "Max Live Executions Per Session",
            Group = "Safety",
            DefaultValue = 3,
            MinValue = 1,
            MaxValue = 20)]
        public int MaxLiveExecutionsPerSession { get; set; }

        [Parameter(
            "Max Concurrent Scenarios",
            Group = "Safety",
            DefaultValue = 3,
            MinValue = 1,
            MaxValue = 10)]
        public int MaxConcurrentScenarios { get; set; }

        [Parameter(
            "Max Execution Margin Usage %",
            Group = "Safety",
            DefaultValue = 80,
            MinValue = 10,
            MaxValue = 100)]
        public double MaxExecutionMarginUsagePercent { get; set; }

        [Parameter(
            "Execution Margin Buffer %",
            Group = "Safety",
            DefaultValue = 10,
            MinValue = 0,
            MaxValue = 40)]
        public double ExecutionMarginBufferPercent { get; set; }

        [Parameter(
            "Provider Stale After Seconds",
            Group = "Safety",
            DefaultValue = 15,
            MinValue = 1,
            MaxValue = 60)]
        public int ProviderStaleAfterSeconds { get; set; }

        [Parameter(
            "Management Command Max Age Seconds",
            Group = "Safety",
            DefaultValue = 30,
            MinValue = 5,
            MaxValue = 300)]
        public int ManagementCommandMaxAgeSeconds { get; set; }

        private readonly DemoMarketExecutionCoordinator _market =
            new DemoMarketExecutionCoordinator();

        private readonly DemoPendingOrderExecutionCoordinator _pending =
            new DemoPendingOrderExecutionCoordinator();

        private readonly ShadowHostCoordinator _shadow =
            new ShadowHostCoordinator();

        private readonly ManagementExecutionCoordinator _management =
            new ManagementExecutionCoordinator();

        private readonly CbotExecutionStatePublisher _statePublisher =
            new CbotExecutionStatePublisher();

        private readonly CbotBrokerReconciliation _brokerReconciliation =
            new CbotBrokerReconciliation();

        private readonly CbotExecutionIdempotencyStore _idempotencyStore =
            new CbotExecutionIdempotencyStore();

        private readonly CbotExecutionEnvironmentGate _executionEnvironment =
            new CbotExecutionEnvironmentGate();

        private readonly Risk.CbotDailyLossGuard _dailyLossGuard =
            new Risk.CbotDailyLossGuard();

        private CbotIndicatorExecutionSettings _executionSettings;

        private CbotBrokerReconciliationResult _reconciliation;
        private readonly Dictionary<string, SignalEnvelope> _scenarioEnvelopes =
            new Dictionary<string, SignalEnvelope>(StringComparer.Ordinal);
        private readonly Dictionary<string, CbotBrokerReconciliationResult> _scenarioReconciliations =
            new Dictionary<string, CbotBrokerReconciliationResult>(StringComparer.Ordinal);
        private DateTime _nextBrokerReconciliationUtc = DateTime.MinValue;
        private string _lastReconciledExecutionLabel = "";

        private ShadowHostState _state =
            ShadowHostState.Waiting;

        private long _lastLoggedRevision = -1;
        private string _lastLoggedReason = "";
        private long _tickCount;
        private int _sessionExecutions;
        private DateTime _startedUtc;
        private DateTime _nextBindingCheckUtc = DateTime.MinValue;
        private DateTime _nextSignalReloadUtc = DateTime.MinValue;
        private string _boundIndicatorInstanceId = "";
        private string _activeManagedExecutionLabel = "";
        private SignalEnvelope _lastSignalEnvelope;
        private long _stateRevision;

        private bool EffectiveMarketExecutionEnabled =>
            Account.IsLive
                ? EnableLiveMarketExecution
                : EnableDemoMarketExecution;

        private bool EffectivePendingStopExecutionEnabled =>
            Account.IsLive
                ? EnableLivePendingStopExecution
                : EnableDemoPendingStopExecution;

        private bool EffectivePendingLimitExecutionEnabled =>
            Account.IsLive
                ? EnableLivePendingLimitExecution
                : EnableDemoPendingLimitExecution;

        private bool EffectiveAggressiveExecutionEnabled =>
            Account.IsLive
                ? EnableLiveAggressiveExecution
                : EnableDemoAggressiveExecution;

        private bool EffectiveManagementExecutionEnabled =>
            Account.IsLive
                ? EnableLiveManagementExecution
                : EnableDemoManagementExecution;

        private int EffectiveSessionExecutionCap =>
            Math.Max(
                1,
                Account.IsLive
                    ? MaxLiveExecutionsPerSession
                    : MaxDemoExecutionsPerSession);
        private int EffectiveConcurrentScenarioLimit =>
            Math.Max(
                1,
                MaxConcurrentScenarios);

        private string EffectiveAccountMode =>
            Account.IsLive ? "LIVE" : "DEMO";

        protected override void OnStart()
        {
            _startedUtc = Server.TimeInUtc;
            PublishPresence("STARTING");
            _tickCount = 0;
            _sessionExecutions = 0;

            // Chart timeframe is host-only. CFIP execution is driven by the
            // Indicator's internal M15 analysis clock and does not use Bars.TimeFrame.
            Print(
                "CFIP cBot START | account={0} | hostTimeframe={1} | execTimeframe=M15 | state={2} | " +
                "marketExecution={3} | pendingStopExecution={4} | pendingLimitExecution={5} | aggressiveExecution={6} | managementExecution={7} | " +
                "maxSessionExecutions={8} | maxConcurrentScenarios={9} | staleAfter={10}s | managementMaxAge={11}s | contractVersion={12}",
                EffectiveAccountMode,
                Bars == null ? "UNKNOWN" : Bars.TimeFrame.ToString(),
                StartupState,
                EffectiveMarketExecutionEnabled ? "ARMED" : "DISARMED",
                EffectivePendingStopExecutionEnabled ? "ARMED" : "DISARMED",
                EffectivePendingLimitExecutionEnabled ? "ARMED" : "DISARMED",
                EffectiveAggressiveExecutionEnabled ? "ARMED" : "DISARMED",
                EffectiveManagementExecutionEnabled ? "ARMED" : "DISARMED",
                EffectiveSessionExecutionCap,
                EffectiveConcurrentScenarioLimit,
                ProviderStaleAfterSeconds,
                ManagementCommandMaxAgeSeconds,
                ContractVersion.Current);

            SubscribeIndicatorLifecycleEvents();
            SubscribeBrokerLifecycleEvents();
            RefreshIndicatorBinding(true);
            RefreshExecutionSettings(true);
            ReloadSignalStore(true);
            _idempotencyStore.Reload(
                this,
                _boundIndicatorInstanceId,
                Server.TimeInUtc);
            ReconcileBrokerState(true);
            PublishExecutionState("CBOT STARTED", true);
        }

        protected override void OnTick()
        {
            _tickCount++;
            PublishPresence("RUNNING");

            if (!RefreshIndicatorBinding(false))
            {
                LogBlockedState("INDICATOR BINDING BLOCKED");
                return;
            }

            RefreshExecutionSettings(false);
            ReloadSignalStore(false);

            DateTime nowUtc = Server.TimeInUtc;
            ReconcileBrokerState(false);
            PublishExecutionState("HEARTBEAT", false);

            if (EffectiveManagementExecutionEnabled &&
                !string.IsNullOrWhiteSpace(_boundIndicatorInstanceId))
            {
                _management.Process(
                    this,
                    _boundIndicatorInstanceId,
                    nowUtc,
                    ManagementCommandMaxAgeSeconds,
                    _executionSettings,
                    out string managementStatus);

                if (!string.IsNullOrWhiteSpace(managementStatus) &&
                    !string.Equals(
                        managementStatus,
                        "NO MANAGEMENT COMMAND",
                        StringComparison.Ordinal))
                {
                    Print("CFIP MANAGEMENT | {0}", managementStatus);
                }
            }

            bool hasScenarioBatch =
                CfipDeviceSignalTransport.TryReadScenarioBatch(
                    this,
                    _boundIndicatorInstanceId,
                    out SignalScenarioBatch scenarioBatch,
                    out string scenarioBatchReason);

            if (hasScenarioBatch)
            {
                if (scenarioBatch == null ||
                    scenarioBatch.ContractVersion != ContractVersion.Current ||
                    !string.Equals(
                        scenarioBatch.IndicatorInstanceId,
                        _boundIndicatorInstanceId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        scenarioBatch.Symbol,
                        SymbolName,
                        StringComparison.Ordinal))
                {
                    LogBlockedState(
                        "SCENARIO BATCH CONTRACT OR IDENTITY MISMATCH");
                    return;
                }

                SignalEnvelope[] scenarios =
                    scenarioBatch.Scenarios ??
                    Array.Empty<SignalEnvelope>();

                if (scenarios.Length == 0)
                {
                    PublishExecutionState(
                        "NO EXECUTABLE SCENARIOS",
                        false);
                    return;
                }

                for (int scenarioIndex = 0;
                     scenarioIndex < scenarios.Length;
                     scenarioIndex++)
                {
                    ProcessSignalEnvelope(
                        scenarios[scenarioIndex],
                        nowUtc);
                }

                SweepScenarioProtectionStates(nowUtc);
                return;
            }

            if (!CfipDeviceSignalTransport.TryRead(
                    this,
                    _boundIndicatorInstanceId,
                    out SignalEnvelope envelope,
                    out string transportReason))
            {
                LogBlockedState(
                    string.IsNullOrWhiteSpace(scenarioBatchReason)
                        ? transportReason
                        : scenarioBatchReason);
                return;
            }

            ProcessSignalEnvelope(
                envelope,
                nowUtc);

            SweepScenarioProtectionStates(nowUtc);
            return;
        }

        private void ProcessSignalEnvelope(
            SignalEnvelope envelope,
            DateTime nowUtc)
        {
            if (!CbotSignalPreflight.TryValidate(
                    this,
                    envelope,
                    nowUtc,
                    ProviderStaleAfterSeconds,
                    out string signalPreflightReason))
            {
                LogBlockedState(signalPreflightReason);
                PublishExecutionState(
                    "SIGNAL PREFLIGHT BLOCKED • " +
                    signalPreflightReason,
                    true);
                return;
            }

            TrackScenarioEnvelope(envelope);

            _lastSignalEnvelope = envelope;
            _activeManagedExecutionLabel =
                envelope.Intent == null
                    ? ""
                    : envelope.Intent.ExecutionLabel ?? "";

            _reconciliation =
                ReconcileScenarioState(
                    envelope,
                    nowUtc);

            PublishExecutionState("SIGNAL OBSERVED", false);

            ShadowHostResult shadowResult =
                _shadow.Observe(
                    envelope,
                    ReadBrokerSnapshot(),
                    ContractVersion.Current,
                    envelope.Identity.Revision,
                    nowUtc);

            LogStateIfChanged(shadowResult);
            PublishExecutionState(
                shadowResult == null
                    ? "SHADOW STATE UNAVAILABLE"
                    : shadowResult.Reason ?? "SHADOW STATE",
                false);

            if (_reconciliation != null &&
                _reconciliation.RecoveryRequired)
            {
                if (EffectiveManagementExecutionEnabled &&
                    TryRecoverProtection(
                        envelope,
                        nowUtc))
                {
                    _reconciliation =
                        ReconcileScenarioState(
                            envelope,
                            nowUtc);
                }

                if (_reconciliation != null &&
                    _reconciliation.RecoveryRequired)
                {
                    LogBlockedState(
                        "BROKER RECOVERY REQUIRED • " +
                        (_reconciliation.Reason ?? "UNKNOWN"));
                    PublishExecutionState(
                        "RECOVERY REQUIRED",
                        true);
                    return;
                }
            }

            bool executionEnabled =
                EffectiveMarketExecutionEnabled ||
                EffectivePendingStopExecutionEnabled ||
                EffectivePendingLimitExecutionEnabled ||
                EffectiveAggressiveExecutionEnabled;

            bool pendingAction =
                envelope.Intent != null &&
                (envelope.Intent.Action ==
                    ExecutionAction.PendingStop ||
                 envelope.Intent.Action ==
                    ExecutionAction.PendingLimit);

            if (executionEnabled &&
                !_executionEnvironment.Evaluate(
                    this,
                    envelope,
                    MaxExecutionMarginUsagePercent,
                    ExecutionMarginBufferPercent,
                    _executionSettings,
                    _dailyLossGuard,
                    pendingAction,
                    EffectiveConcurrentScenarioLimit,
                    nowUtc,
                    Account.IsLive && executionEnabled,
                    out string environmentReason))
            {
                LogBlockedState(
                    environmentReason);
                PublishExecutionState(
                    "EXECUTION BLOCKED • " +
                    environmentReason,
                    true);
                return;
            }

            bool actionEnabled =
                envelope.Intent != null &&
                (envelope.Intent.Action == ExecutionAction.Aggressive
                    ? EffectiveAggressiveExecutionEnabled
                    : envelope.Intent.Action == ExecutionAction.PendingStop
                        ? EffectivePendingStopExecutionEnabled
                        : envelope.Intent.Action == ExecutionAction.PendingLimit
                            ? EffectivePendingLimitExecutionEnabled
                            : envelope.Intent.Action == ExecutionAction.Market &&
                          EffectiveMarketExecutionEnabled);

            if (!executionEnabled ||
                !actionEnabled ||
                shadowResult == null ||
                shadowResult.State != ShadowHostState.Ready)
                return;

            if (_sessionExecutions >=
                EffectiveSessionExecutionCap)
            {
                LogBlockedState("SESSION EXECUTION CAP REACHED");
                return;
            }

            if (envelope.Intent.Action == ExecutionAction.PendingStop ||
                envelope.Intent.Action == ExecutionAction.PendingLimit)
            {
                if (_pending.TryExecute(
                        this,
                        envelope,
                        Account.IsLive,
                        nowUtc,
                        MaxExecutionMarginUsagePercent,
                        ExecutionMarginBufferPercent,
                        EffectiveConcurrentScenarioLimit,
                        _idempotencyStore,
                        out BrokerExecutionReport pendingReport,
                        out string pendingReason))
                {
                    _sessionExecutions++;
                }

                if (pendingReport != null)
                {
                    Print(
                        "CFIP {0} PENDING | status={1} | action={2} | " +
                        "revision={3} | pending={4} | reason={5}",
                        EffectiveAccountMode,
                        pendingReport.Status,
                        pendingReport.Action,
                        pendingReport.AttemptRevision,
                        pendingReport.BrokerPendingOrderId.HasValue
                            ? pendingReport.BrokerPendingOrderId.Value.ToString()
                            : "",
                        pendingReason);

                    ReconcileBrokerState(true);

                    PublishExecutionState(
                        pendingReport.Status ==
                        BrokerReportStatus.Confirmed
                            ? "BROKER PENDING ORDER CONFIRMED"
                            : "PENDING SUBMISSION RESULT • " +
                              pendingReport.Status,
                        true);
                }

                return;
            }

            if (_market.TryExecute(
                    this,
                    envelope,
                    Account.IsLive,
                    nowUtc,
                    MaxExecutionMarginUsagePercent,
                    ExecutionMarginBufferPercent,
                    EffectiveConcurrentScenarioLimit,
                    _idempotencyStore,
                    out BrokerExecutionReport report,
                    out string executionReason))
            {
                _sessionExecutions++;
            }

            if (report != null)
            {
                Print(
                    "CFIP {0} MARKET | status={1} | action={2} | " +
                    "revision={3} | position={4} | entry={5} | stop={6} | " +
                    "target={7} | reason={8}",
                    EffectiveAccountMode,
                    report.Status,
                    report.Action,
                    report.AttemptRevision,
                    report.BrokerPositionId.HasValue
                        ? report.BrokerPositionId.Value.ToString()
                        : "",
                    report.ConfirmedEntry.HasValue
                        ? report.ConfirmedEntry.Value.ToString(
                            "G17",
                            System.Globalization.CultureInfo.InvariantCulture)
                        : "",
                    report.ConfirmedStop.HasValue
                        ? report.ConfirmedStop.Value.ToString(
                            "G17",
                            System.Globalization.CultureInfo.InvariantCulture)
                        : "",
                    report.ConfirmedTarget.HasValue
                        ? report.ConfirmedTarget.Value.ToString(
                            "G17",
                            System.Globalization.CultureInfo.InvariantCulture)
                        : "",
                    executionReason);

                ReconcileBrokerState(true);

                PublishExecutionState(
                    report.Status ==
                    BrokerReportStatus.Confirmed
                        ? "BROKER POSITION CONFIRMED"
                        : "MARKET SUBMISSION RESULT • " +
                          report.Status,
                    true);
            }
        }

        private void TrackScenarioEnvelope(
            SignalEnvelope envelope)
        {
            if (envelope == null ||
                envelope.Identity == null ||
                string.IsNullOrWhiteSpace(
                    envelope.Identity.ScenarioId) ||
                envelope.Intent == null ||
                string.IsNullOrWhiteSpace(
                    envelope.Intent.ExecutionLabel))
                return;

            string key =
                envelope.Identity.ScenarioId.Trim();

            _scenarioEnvelopes[key] =
                envelope;

            if (_scenarioEnvelopes.Count > 32)
            {
                string removeKey = null;

                foreach (KeyValuePair<string, SignalEnvelope> item in
                         _scenarioEnvelopes)
                {
                    removeKey = item.Key;
                    break;
                }

                if (!string.IsNullOrWhiteSpace(removeKey))
                {
                    _scenarioEnvelopes.Remove(removeKey);
                    _scenarioReconciliations.Remove(removeKey);
                }
            }
        }

        private CbotBrokerReconciliationResult ReconcileScenarioState(
            SignalEnvelope envelope,
            DateTime nowUtc)
        {
            if (envelope == null ||
                envelope.Identity == null ||
                envelope.Intent == null)
                return null;

            string label =
                envelope.Intent.ExecutionLabel ?? "";

            CbotBrokerReconciliationResult result =
                _brokerReconciliation.Evaluate(
                    this,
                    _boundIndicatorInstanceId,
                    label);

            string scenarioId =
                envelope.Identity.ScenarioId ?? "";

            if (!string.IsNullOrWhiteSpace(scenarioId))
            {
                _scenarioReconciliations[scenarioId] =
                    result;
            }

            return result;
        }

        private void SweepScenarioProtectionStates(
            DateTime nowUtc)
        {
            if (_scenarioEnvelopes.Count == 0)
                return;

            foreach (KeyValuePair<string, SignalEnvelope> item in
                     _scenarioEnvelopes)
            {
                SignalEnvelope scenario = item.Value;

                if (scenario == null ||
                    scenario.Identity == null ||
                    scenario.Intent == null ||
                    string.IsNullOrWhiteSpace(
                        scenario.Intent.ExecutionLabel))
                    continue;

                CbotBrokerReconciliationResult result =
                    _brokerReconciliation.Evaluate(
                        this,
                        _boundIndicatorInstanceId,
                        scenario.Intent.ExecutionLabel);

                _scenarioReconciliations[item.Key] =
                    result;

                if (result == null ||
                    !result.RecoveryRequired ||
                    result.ManagedPositions != 1 ||
                    !EffectiveManagementExecutionEnabled)
                    continue;

                _activeManagedExecutionLabel =
                    scenario.Intent.ExecutionLabel;

                _lastSignalEnvelope =
                    scenario;

                _reconciliation =
                    result;

                if (TryRecoverProtection(
                        scenario,
                        nowUtc))
                {
                    _scenarioReconciliations[item.Key] =
                        _brokerReconciliation.Evaluate(
                            this,
                            _boundIndicatorInstanceId,
                            scenario.Intent.ExecutionLabel);
                }
            }
        }

        private bool RefreshIndicatorBinding(bool force)
        {
            DateTime now = Server.TimeInUtc;

            if (!force &&
                now < _nextBindingCheckUtc)
                return !string.IsNullOrWhiteSpace(_boundIndicatorInstanceId);

            _nextBindingCheckUtc = now.AddMilliseconds(500);

            if (!CfipIndicatorChartBinding.TryFind(
                    this,
                    out ChartIndicator indicator,
                    out string reason))
            {
                _boundIndicatorInstanceId = "";
                _activeManagedExecutionLabel = "";
                _executionSettings = null;
                _scenarioEnvelopes.Clear();
                _scenarioReconciliations.Clear();
                _state = ShadowHostState.Blocked;
                LogBlockedState(reason);
                return false;
            }

            string instanceId = indicator.InstanceId ?? "";

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                _boundIndicatorInstanceId = "";
                _activeManagedExecutionLabel = "";
                _executionSettings = null;
                LogBlockedState("CFIP INDICATOR INSTANCE ID UNAVAILABLE");
                return false;
            }

            if (!force &&
                string.Equals(
                    _boundIndicatorInstanceId,
                    instanceId,
                    StringComparison.Ordinal))
                return true;

            _boundIndicatorInstanceId = instanceId;

            _idempotencyStore.Reload(
                this,
                _boundIndicatorInstanceId,
                now);

            _scenarioEnvelopes.Clear();
            _scenarioReconciliations.Clear();

            _executionSettings = null;
            RefreshExecutionSettings(true);

            Print(
                "CFIP ANALYSIS BIND | state=READY | name={0} | instance={1}",
                CfipIndicatorChartBinding.DisplayName,
                _boundIndicatorInstanceId);

            return true;
        }

        private void RefreshExecutionSettings(bool force)
        {
            if (string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
            {
                _executionSettings = null;
                return;
            }

            if (!force &&
                _executionSettings != null)
                return;

            if (!CfipIndicatorChartBinding.TryFind(
                    this,
                    out ChartIndicator indicator,
                    out string reason))
            {
                _executionSettings = null;
                LogBlockedState(reason);
                return;
            }

            if (!CbotIndicatorExecutionSettings.TryRead(
                    indicator,
                    out CbotIndicatorExecutionSettings settings,
                    out reason))
            {
                _executionSettings = null;
                LogBlockedState(reason);
                return;
            }

            _executionSettings = settings;
        }

        private void ReloadSignalStore(bool force)
        {
            DateTime now = Server.TimeInUtc;

            if (!force &&
                now < _nextSignalReloadUtc)
                return;

            _nextSignalReloadUtc =
                now.AddMilliseconds(100);

            try
            {
                CfipDeviceSignalTransport.Reload(this);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SIGNAL STORE RELOAD FAILED | {0}",
                    ex.Message);
            }
        }

        private ShadowBrokerSnapshot ReadBrokerSnapshot()
        {
            string managedLabel =
                _activeManagedExecutionLabel ?? "";

            int managedPositions = 0;
            foreach (Position position in Positions)
            {
                if (position != null &&
                    string.Equals(
                        position.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    CbotManagedObjectIdentityRule.MatchesManagedLabel(
                        position.Label,
                        managedLabel))
                    managedPositions++;
            }

            int managedPendingOrders = 0;
            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(
                        order.Label,
                        managedLabel))
                    managedPendingOrders++;
            }

            return new ShadowBrokerSnapshot(
                managedPositions,
                managedPendingOrders,
                SymbolName ?? "",
                Symbol.Bid,
                Symbol.Ask,
                Symbol.PipSize);
        }

        private void LogBlockedState(string reason)
        {
            reason = reason ?? "UNKNOWN";

            if (_state == ShadowHostState.Blocked &&
                string.Equals(
                    _lastLoggedReason,
                    reason,
                    StringComparison.Ordinal))
                return;

            _state = ShadowHostState.Blocked;
            _lastLoggedReason = reason;

            Print(
                "CFIP cBot STATE | state=BLOCKED | reason={0} | " +
                "indicator={1} | tickCount={2}",
                reason,
                string.IsNullOrWhiteSpace(_boundIndicatorInstanceId)
                    ? "NONE"
                    : _boundIndicatorInstanceId,
                _tickCount);
        }

        private void LogStateIfChanged(ShadowHostResult result)
        {
            if (result == null)
                return;

            if (result.State == _state &&
                result.Revision == _lastLoggedRevision &&
                string.Equals(
                    result.Reason,
                    _lastLoggedReason,
                    StringComparison.Ordinal))
                return;

            _state = result.State;
            _lastLoggedRevision = result.Revision;
            _lastLoggedReason = result.Reason ?? "";

            PublishExecutionState(
                result.Reason ?? "SHADOW STATE",
                true);

            Print(
                "CFIP DEMO SHADOW | state={0} | reason={1} | revision={2} | " +
                "signal={3} | scenario={4} | plan={5} | key={6}",
                result.State,
                result.Reason,
                result.Revision,
                result.SignalId ?? "",
                result.ScenarioId ?? "",
                result.PlanId ?? "",
                result.IdempotencyKey ?? "");
        }


        private void SubscribeIndicatorLifecycleEvents()
        {
            ChartIndicators.IndicatorAdded += OnChartIndicatorAdded;
            ChartIndicators.IndicatorRemoved += OnChartIndicatorRemoved;
            ChartIndicators.IndicatorModified += OnChartIndicatorModified;
        }

        private void UnsubscribeIndicatorLifecycleEvents()
        {
            ChartIndicators.IndicatorAdded -= OnChartIndicatorAdded;
            ChartIndicators.IndicatorRemoved -= OnChartIndicatorRemoved;
            ChartIndicators.IndicatorModified -= OnChartIndicatorModified;
        }

        private void OnChartIndicatorAdded(
            ChartIndicatorAddedEventArgs args)
        {
            RefreshIndicatorBinding(true);

            ReconcileBrokerState(true);

            if (!string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
                PublishExecutionState(
                    "INDICATOR ATTACHED",
                    true);
        }

        private void OnChartIndicatorRemoved(
            ChartIndicatorRemovedEventArgs args)
        {
            string removedInstance =
                args == null ||
                args.Indicator == null
                    ? ""
                    : args.Indicator.InstanceId ?? "";

            bool wasBound =
                !string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId) &&
                string.Equals(
                    removedInstance,
                    _boundIndicatorInstanceId,
                    StringComparison.Ordinal);

            RefreshIndicatorBinding(true);

            ReconcileBrokerState(true);

            if (wasBound &&
                string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
            {
                LogBlockedState(
                    "CFIP SMART INDICATOR REMOVED FROM CHART");
            }
        }

        private void OnChartIndicatorModified(
            ChartIndicatorModifiedEventArgs args)
        {
            RefreshIndicatorBinding(true);

            ReconcileBrokerState(true);
            RefreshExecutionSettings(true);

            if (!string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
                PublishExecutionState(
                    "INDICATOR MODIFIED",
                    true);
        }

        private void SubscribeBrokerLifecycleEvents()
        {
            Positions.Opened +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "POSITION OPENED",
                        true);
                };

            Positions.Modified +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "POSITION MODIFIED",
                        true);
                };

            Positions.Closed +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "POSITION CLOSED",
                        true);
                };

            PendingOrders.Created +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "PENDING CREATED",
                        true);
                };

            PendingOrders.Modified +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "PENDING MODIFIED",
                        true);
                };

            PendingOrders.Filled +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "PENDING FILLED",
                        true);
                };

            PendingOrders.Cancelled +=
                args =>
                {
                    ReconcileBrokerState(true);
                    PublishExecutionState(
                        "PENDING CANCELLED",
                        true);
                };
        }

        private void ReconcileBrokerState(bool force)
        {
            if (string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
                return;

            DateTime now = Server.TimeInUtc;

            bool executionLabelChanged =
                !string.Equals(
                    _lastReconciledExecutionLabel,
                    _activeManagedExecutionLabel,
                    StringComparison.Ordinal);

            if (!force &&
                now < _nextBrokerReconciliationUtc &&
                !executionLabelChanged)
                return;

            _nextBrokerReconciliationUtc =
                now.AddMilliseconds(500);

            _reconciliation =
                _brokerReconciliation.Evaluate(
                    this,
                    _boundIndicatorInstanceId,
                    _activeManagedExecutionLabel);

            if (_reconciliation != null &&
                !string.IsNullOrWhiteSpace(
                    _reconciliation.ExecutionLabel))
            {
                _activeManagedExecutionLabel =
                    _reconciliation.ExecutionLabel;
            }

            _lastReconciledExecutionLabel =
                _activeManagedExecutionLabel ?? "";
        }

        private bool TryRecoverProtection(
            SignalEnvelope envelope,
            DateTime nowUtc)
        {
            if (envelope == null ||
                envelope.Intent == null ||
                _reconciliation == null ||
                !_reconciliation.RecoveryRequired ||
                _reconciliation.ManagedPositions != 1)
                return false;

            if (envelope.Identity == null ||
                envelope.Intent.Identity == null ||
                !envelope.Intent.Identity.Equals(
                    envelope.Identity))
                return false;

            if (string.IsNullOrWhiteSpace(
                    envelope.Intent.ExecutionLabel))
                return false;

            return _management.TryRecoverProtectionFromSignal(
                this,
                _boundIndicatorInstanceId,
                nowUtc,
                envelope,
                out _,
                out _);
        }

        private void PublishPresence(
            string state)
        {
            _statePublisher.PublishPresence(
                this,
                Server.TimeInUtc,
                state,
                _boundIndicatorInstanceId);
        }

        private void PublishExecutionState(
            string reason,
            bool force)
        {
            if (string.IsNullOrWhiteSpace(
                    _boundIndicatorInstanceId))
                return;

            ShadowBrokerSnapshot brokerSnapshot =
                ReadBrokerSnapshot();

            string runtimeState;

            if (_state == ShadowHostState.Blocked)
            {
                runtimeState = "BLOCKED";
            }
            else if (brokerSnapshot != null &&
                     (brokerSnapshot.ManagedPositionCount > 0 ||
                      brokerSnapshot.ManagedPendingOrderCount > 0))
            {
                runtimeState = "ACTIVE";
            }
            else if (EffectiveMarketExecutionEnabled ||
                     EffectivePendingStopExecutionEnabled ||
                     EffectivePendingLimitExecutionEnabled ||
                     EffectiveAggressiveExecutionEnabled ||
                     EffectiveManagementExecutionEnabled)
            {
                runtimeState = "ARMED";
            }
            else
            {
                runtimeState = "DISARMED";
            }

            _statePublisher.Publish(
                this,
                _boundIndicatorInstanceId,
                Server.TimeInUtc,
                ++_stateRevision,
                runtimeState,
                reason ?? string.Empty,
                EffectiveMarketExecutionEnabled,
                EffectivePendingStopExecutionEnabled,
                EffectivePendingLimitExecutionEnabled,
                EffectiveAggressiveExecutionEnabled,
                EffectiveManagementExecutionEnabled,
                _activeManagedExecutionLabel,
                _lastSignalEnvelope == null ||
                _lastSignalEnvelope.Identity == null
                    ? ""
                    : _lastSignalEnvelope.Identity.ScenarioId ?? "",
                _lastSignalEnvelope,
                _reconciliation,
                _executionSettings,
                force);
        }

        protected override void OnStop()
        {
            PublishPresence("STOPPED");
            PublishExecutionState(
                "CBOT STOPPED",
                true);

            UnsubscribeIndicatorLifecycleEvents();

            Print(
                "CFIP cBot STOP | account={0} | state={1} | executions={2} | " +
                "sessionMs={3}",
                EffectiveAccountMode,
                _state,
                _sessionExecutions,
                (Server.TimeInUtc - _startedUtc).TotalMilliseconds);
        }
    }
}