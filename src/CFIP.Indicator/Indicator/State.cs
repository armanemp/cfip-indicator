using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Bars _m1Bars;
                private Bars _m5Bars;
                private Bars _m15Bars;
                private Bars _m30Bars;
                private Bars _h1Bars;
                private Bars _h4Bars;
                private Bars _d1Bars;
                private Bars _w1Bars;
        
                private MarketRegimeSnapshot _m5RegimeSnapshot;
                private int _m5RegimeSnapshotIndex = -1;
                private readonly M5RegimeCoreCache _m5RegimeCoreCache = new M5RegimeCoreCache();

                private Frame _m1Frame;
                private Frame _m5Frame;
                private Frame _m15Frame;
                private Frame _m30Frame;
                private Frame _h1Frame;
                private Frame _h4Frame;
                private Frame _d1Frame;
                private Frame _w1Frame;
        
                private readonly Dictionary<Bars, Native> _native =
                    new Dictionary<Bars, Native>(
                        NativeBarsReferenceComparer.Instance);
                private readonly SubmissionGate _submissionGate =
                    new SubmissionGate();
                private readonly HashSet<string> _historicalDrawn = new HashSet<string>();
                private readonly HashSet<string> _outcomeDrawn = new HashSet<string>();
                private int _outcomeSequence;
        
                private Plan _plan;
                private Decision _decision;
                private Decision _reaction;
                private Prediction _prediction;
                private ExecutionModel _executionModel;
                private TradeSetupPreview _setupPreview;
                private readonly TriggerRuntimeState _triggerRuntime = new TriggerRuntimeState();
                private readonly Dictionary<Bars, WaveTrendEngine> _waveTrendEngines =
                    new Dictionary<Bars, WaveTrendEngine>();
                private readonly List<TradeOpportunityCandidate> _opportunityCandidates =
                    new List<TradeOpportunityCandidate>();
                private readonly TradePlanRegistry _tradePlanRegistry =
                    new TradePlanRegistry();
                private int _lastOpportunityCandidatesM5 = -1;
                private int _parallelGeometryCacheM5 = -1;
                private readonly Dictionary<int, ParallelScenarioGeometry> _parallelGeometryCache =
                    new Dictionary<int, ParallelScenarioGeometry>();
                private readonly Dictionary<int, ExecutionModel> _parallelExecutionModelCache =
                    new Dictionary<int, ExecutionModel>();
                private readonly Dictionary<int, TradeSetupPreview> _parallelPreviewCache =
                    new Dictionary<int, TradeSetupPreview>();

                private VolumeProfileSnapshot _m15VolumeProfile =
                    VolumeProfileSnapshot.Empty;
                private int _m15VolumeProfileClosedIndex = -1;

                private DateTime _lastLiveOpportunityRefreshUtc = DateTime.MinValue;
                private int _lastLiveOpportunityRefreshM5 = -1;
                private bool _suppressProviderIntentCapture;
                // Target-level construction is shared by all same-M5 scenario
                // evaluations for a direction. Cache the deterministic closed-bar
                // result and return a shallow copy to protect callers from mutation.
                private int _targetLevelCacheM5 = -1;
                private int _targetLevelCacheDirection = 0;
                private double _targetLevelCacheEntry = 0;
                private double _targetLevelCacheAtr = 0;
                private List<Level> _targetLevelCache;

                private readonly HashSet<string> _opportunityVisualIds =
                    new HashSet<string>();

        
                private int _lastStructuralStopUpdateM5 = -1;
                private int _lastTargetRepriceM5 = -1;
        
                private int _lastEvaluatedM5 = -1;
                private int _lastSignalM5 = -1;
                private int _lastConfirmedM5 = -1;
                private int _lastConfirmedDirection = 0;
                private int _lastExitM5 = -1;
                private bool _outcomeRegistered;
                private int _lastAutoM5 = -1;
                private int _lastEarlyAlertM5 = -1;
                private int _lastActionableEntryAlertM5 = -1;
                private int _lastActionableEntryAlertDirection = 0;
                private int _tp1Hit;
                private int _tp2Hit;
                private int _tp3Hit;
                private int _tp4Hit;
                private int _lastPartialTp1AttemptM5 = -1;
                private int _lastPartialTp2AttemptM5 = -1;
                private int _lastServerPartialObservationDealCount = -1;
                private int _lastServerTpLadderMutationM5 = -1;
                private string _lastServerTpLadderMutationKind = "";
                private bool _serverSideTakeProfitLadderActiveValue;
                private bool _serverSideTakeProfitLadderOwned;
                private bool _serverSideBreakEvenActive;
                private bool _slHit;
                private double _peakPrice;
                private double _lastMarket;
                private int _wins;
                private int _losses;
        
                private string _status = "INITIALIZING";
                private bool _initializationReady;
                private string _autoTradingState = "OFF";
                private string _autoTradingReason = "DISABLED";
                private string _autoExecutionBlockReasonValue = "NOT EVALUATED";
                private string _autoOrdersBlockReasonValue = "NOT EVALUATED";
                private DateTime _lastAutoTradeAttemptUtc = DateTime.MinValue;
                private DateTime _lastAutoOrderAttemptUtc = DateTime.MinValue;
                private bool _autoTradingEnabledRuntime;
                private bool _automaticOrdersEnabledRuntime;
                private bool _executionRuntimeInitialized;
                private bool _executionToggleSyncing;
                private CalculationMarketContext _calculationMarketContext;

                private string _autoExecutionBlockReason
                {
                    get => _autoExecutionBlockReasonValue;
                    set
                    {
                        if (string.Equals(
                                _autoExecutionBlockReasonValue,
                                value,
                                StringComparison.Ordinal))
                            return;

                        _autoExecutionBlockReasonValue = value;
                        InvalidatePanelExecutionProtectionStateCache();
                    }
                }

                private string _autoOrdersBlockReason
                {
                    get => _autoOrdersBlockReasonValue;
                    set
                    {
                        if (string.Equals(
                                _autoOrdersBlockReasonValue,
                                value,
                                StringComparison.Ordinal))
                            return;

                        _autoOrdersBlockReasonValue = value;
                        InvalidatePanelExecutionProtectionStateCache();
                    }
                }

                private bool _serverSideTakeProfitLadderActive
                {
                    get => _serverSideTakeProfitLadderActiveValue;
                    set
                    {
                        if (_serverSideTakeProfitLadderActiveValue == value)
                            return;

                        _serverSideTakeProfitLadderActiveValue = value;
                        InvalidatePanelExecutionProtectionStateCache();
                    }
                }

                private bool _brokerProtectionRecoveryRequired
                {
                    get => _brokerProtectionRecoveryRequiredValue;
                    set
                    {
                        if (_brokerProtectionRecoveryRequiredValue == value)
                            return;

                        _brokerProtectionRecoveryRequiredValue = value;
                        InvalidatePanelExecutionProtectionStateCache();
                    }
                }
                private bool _lastConfiguredAutoTrading;
                private bool _lastConfiguredAutomaticOrders;
                private bool _outcomeTelemetryTimedOut;
                private string _lastExecutionTelemetryPath = "";
                private string _lastExecutionTelemetryState = "IDLE";
                private string _lastExecutionTelemetryReason = "";
                private string _activeExecutionScenarioId = "";
                private int _lastExecutionTelemetryM5 = -1;
                private DateTime _lastExecutionTelemetryUtc = DateTime.MinValue;
        
                private LifecycleState _lifecycleState =
                    LifecycleState.Flat;
        
                private string _lifecycleReason =
                    "INITIALIZING";
        
                private bool _brokerProtectionRecoveryRequiredValue;
        
                // Public cTrader parameters are configuration inputs. These private
                // flags are the single runtime authority used by execution, panel
                // state and quick controls so UI state cannot become execution state
                // by accident.
                private bool AutoTradingEnabled =>
                    _autoTradingEnabledRuntime;
        
                private bool AutomaticOrdersEnabled =>
                    _automaticOrdersEnabledRuntime;
        
                // Active broker protection is kept separate from the structural plan
                // so chart/panel presentation can distinguish what the engine wants
                // from what the broker actually holds.
                private double _activeBrokerStop;
                private double _activeBrokerTarget;
                // Candidate generated by live protection logic. It becomes plan state
                // only after the broker confirms the corresponding SL mutation.
                private double _pendingProtectedStopCandidate;
                private string _lastBreakEvenDiagnostic = "NOT EVALUATED";
        
                // FIX (CFIP-BUG-ALERT-DEDUP): the cooldown used to compare against a
                // single shared "last alert key", so any two different alert kinds
                // firing back-to-back (e.g. a TP1 alert then a smart/reaction alert)
                // reset that tracker and silently defeated the per-key cooldown for
                // everything else — the same key could then re-fire immediately.
                // Cooldowns are now tracked per key.
                private readonly Dictionary<string, DateTime> _alertCooldowns =
                    new Dictionary<string, DateTime>();
        
                private string _lastRestrictionMessage = "";
        
                private DateTime _lastEndOfDayAlertDate = DateTime.MinValue;
        
                private DateTime _lastEndOfDayCloseDate = DateTime.MinValue;
        
                private DateTime _dailyLossBaselineDate = DateTime.MinValue;
        
        
                private bool _dailyLossLimitAlerted = false;

                private double _dailyLossBaselineUnrealizedNetProfit = 0;

                private double _dailyLossStartEquity = 0;

                private double _dailyLossRealizedNetProfit = 0;

                private double _dailyLossNetCashFlow = 0;

                private int _dailyLossHistoryCount = -1;

                private int _dailyLossTransactionCount = -1;

                private bool _dailyLossDataReady;

                private bool _dailyLossHistoryAvailable;

                private bool _dailyLossTransactionsAvailable;

                private DateTime _lastDailyLossSharedStateReloadUtc =
                    DateTime.MinValue;

                private bool _dailyLossLocked;

                private DailyLossEvaluation _dailyLossEvaluation;

                private DateTime _lastDailyLossPersistUtc = DateTime.MinValue;

                private string _dailyLossStateReason = "NOT EVALUATED";

                private DateTime _lastRestrictionAlertUtc = DateTime.MinValue;
                private int _lastPendingCleanupM5 = -1;
                private int _lastAutoTradingReminderM5 = -1;
                private int _lastVisualDirection = 0;
                private string _lastAlertMessage = "";
                private int _lastAlertDirection;
                private bool _lastAlertCritical;
                private DateTime _lastAlertUtc = DateTime.MinValue;
                private int _authoritativeDirection;
                private string _authoritativeState = "WAITING";
        
                private Border _panel;
                private StackPanel _panelStack;
                private StackPanel _panelHeaderStack;
                private TextBlock _panelHeaderTitle;
                private StackPanel _panelRowsStack;
                private ScrollViewer _panelScroll;
                private readonly List<TextBlock> _panelRows =
                    new List<TextBlock>();
                private StackPanel _buttonStack;
                private Button _panelRestoreButton;
                private StackPanel _quickExecutionStack;
                private ToggleButton _autoTradingQuickToggle;
                private ToggleButton _automaticOrdersQuickToggle;
        
                private readonly AlertDeliveryQueue _alertDeliveryQueue =
                    new AlertDeliveryQueue(16);
                private readonly AlertDeliveryQueue _alertSoundDeliveryQueue =
                    new AlertDeliveryQueue(8);

                private Queue<AlertDelivery> _panelAlertHistory =
                    new Queue<AlertDelivery>(5);
                private StackPanel _panelAlertMessageStack;
                private readonly List<TextBlock> _panelAlertMessageRows =
                    new List<TextBlock>(5);
                private long _panelAlertRevision;
        
                private const string P = "CFIP_";
                private const string H = "CFIP_H_";
        
                private int _lastContextM5 = -1;
                private int _lastInvalidationAlertM5 = -1;
                private int _lastInvalidationEvaluationM5 = -1;
                private bool _panelHidden;
                private Button _panelToggleButton;
                private string _panelStableHeader = "";
                private DateTime _panelStableHeaderSinceUtc = DateTime.MinValue;
        private int _runtimeTpStageIndex = -1;
        private int _runtimeTpStagePlanCreatedM5 = -1;
        private int _lastReactionAlertBar = -1;

        private readonly Dictionary<int, int> _directionSamples =
                    new Dictionary<int, int>();
        
                private readonly Dictionary<int, int> _directionWins =
                    new Dictionary<int, int>();

                private readonly Dictionary<ConfidenceCalibrationKey, int> _calibrationSamples =
                    new Dictionary<ConfidenceCalibrationKey, int>();

                private readonly Dictionary<ConfidenceCalibrationKey, int> _calibrationWins =
                    new Dictionary<ConfidenceCalibrationKey, int>();

                private readonly List<OutcomeObservation> _outcomeHistory =
                    new List<OutcomeObservation>();

                // Long-term learning aggregates are populated from the immutable
                // 90-day archive files. They are intentionally separate from the
                // bounded recent window used for fast contextual calibration.
                private readonly Dictionary<ConfidenceCalibrationKey, int> _archiveCalibrationSamples =
                    new Dictionary<ConfidenceCalibrationKey, int>();
                private readonly Dictionary<ConfidenceCalibrationKey, int> _archiveCalibrationWins =
                    new Dictionary<ConfidenceCalibrationKey, int>();
                private int _archiveLearningOutcomeCount;
                private bool _outcomeArchiveImportQueued;
                private bool _outcomeArchiveImported;

                private readonly List<ExecutionTelemetryRecord> _executionTelemetryHistory =
                    new List<ExecutionTelemetryRecord>();
        
                private DateTime _lastBrokerModifyUtc = DateTime.MinValue;
                private int _lastRestrictionM5 = -1;
                private int _lastHistoricalHostBar = -1;
                private int _lastAutoPlanAttemptM5 = -1;
                private DateTime _lastTradingPermissionRequestUtc = DateTime.MinValue;
        
                private int _marketSuitabilityM5 = -1;
                private int _marketSuitabilityDirection = 0;
                private int _marketSuitabilityScore = 0;
                private string _marketSuitabilityState = "UNKNOWN";
                private string _marketSuitabilityReason = "NOT EVALUATED";
                private DateTime _lastMarketSuitabilityUtc = DateTime.MinValue;
                private MtfClosedContext _lastMtfClosedContext;
                private MarketStateSnapshot _marketStateSnapshot;
                private readonly MtfClosedContextCache _mtfClosedContextCache = new MtfClosedContextCache();
                private readonly AggressiveEntryPolicy _aggressiveEntryPolicy =
                    new AggressiveEntryPolicy();
                private DateTime _lastPanelHeartbeatUtc = DateTime.MinValue;

                // CI-11 timing state: one measurement per causal decision/M1
                // confirmation identity. This is observability only and does not
                // participate in trading eligibility.
                private int _entryTimingM5 = -1;
                private int _entryTimingDirection = 0;
                private EntrySignalTiming _entrySignalTiming =
                    EntrySignalTiming.NotMeasured();
        private string _lastPanelPresentationKey = "";
        private int _panelLiveRow = -1;
        private int _panelPositionRow = -1;
        private int _panelExitRow = -1;
        private DateTime _lastLiveStructuralPulseUtc = DateTime.MinValue;
                private DateTime _lastSafetySupervisorUtc = DateTime.MinValue;
                private SignalVisualSnapshot _renderSignalVisualSnapshot;
                private DateTime _initializationStartedUtc = DateTime.MinValue;
                private int _initializationPendingDataLoads;
                private bool _initializationDataRequested;
                private bool _initializationDataReady;
                private DateTime _lastPanelRenderUtc = DateTime.MinValue;
                private DateTime _lastPanelContentRefreshUtc = DateTime.MinValue;
                private int _panelContentRefreshSequence;
                private int _panelOverflowCount;
                private bool _panelOverflowReported;
                private DateTime _lastCalculationCompletedUtc = DateTime.MinValue;
                private DateTime _lastReactionCalcUtc = DateTime.MinValue;
                private int _lastReactionM5 = -1;
                private double _lastReactionMarket;
                private DateTime _lastExecutionModelBuildUtc = DateTime.MinValue;
                private int _lastExecutionModelM5 = -1;
                private double _lastExecutionModelMarket;
                private int _lastPanelM1ClosedIndex = -1;
                private int _panelClockRow = -1;
                private bool _runtimeTimerBusy;
                private bool _calculationBusy;
                private bool _startupCalculationSeedDone;
                private bool _startupCalculationSeedQueued;
                private bool _panelRenderBusy;
    }
}