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

                private Frame _m1Frame;
                private Frame _m5Frame;
                private Frame _m15Frame;
                private Frame _m30Frame;
                private Frame _h1Frame;
                private Frame _h4Frame;
                private Frame _d1Frame;
                private Frame _w1Frame;
        
                private readonly List<Native> _native = new List<Native>();
                private readonly List<ZoneCandidateCache> _zoneCandidateCaches =
                    new List<ZoneCandidateCache>();
                private readonly HashSet<string> _historicalDrawn = new HashSet<string>();
                private readonly HashSet<string> _outcomeDrawn = new HashSet<string>();
                private int _outcomeSequence;
        
                private Plan _plan;
                private Decision _decision;
                private Decision _reaction;
                private Prediction _prediction;
                private ExecutionModel _executionModel;
        
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
                private int _lastHighConfidenceM5 = -1;
                private int _tp1Hit;
                private int _tp2Hit;
                private int _tp3Hit;
                private int _tp4Hit;
                private bool _slHit;
                private double _peakPrice;
                private double _lastMarket;
                private int _wins;
                private int _losses;
        
                private string _status = "INITIALIZING";
                private bool _initializationReady;
                private int _initializationStage;
                private string _autoTradingState = "OFF";
                private string _autoTradingReason = "DISABLED";
                private string _autoExecutionBlockReason = "NOT EVALUATED";
                private string _autoOrdersBlockReason = "NOT EVALUATED";
                private DateTime _lastAutoTradeAttemptUtc = DateTime.MinValue;
                private DateTime _lastAutoOrderAttemptUtc = DateTime.MinValue;
                private bool _executionToggleSyncing;
                private bool _autoTradingEnabledRuntime;
                private bool _automaticOrdersEnabledRuntime;
                private bool _executionRuntimeInitialized;
                private bool _lastConfiguredAutoTrading;
                private bool _lastConfiguredAutomaticOrders;
                private bool _outcomeTelemetryTimedOut;
        
                private LifecycleState _lifecycleState =
                    LifecycleState.Flat;
        
                private string _lifecycleReason =
                    "INITIALIZING";
        
                private bool _brokerProtectionRecoveryRequired;
        
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
        
                private double _dailyStartEquity = 0;
        
                private bool _dailyLossLimitAlerted = false;
                private DateTime _lastRestrictionAlertUtc = DateTime.MinValue;
                private int _lastPendingSignalM5 = -1;
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
                private Button _closeButton;
                private Button _cancelButton;
                private Button _panelRestoreButton;
                private StackPanel _quickExecutionStack;
                private ToggleButton _autoTradingQuickToggle;
                private ToggleButton _automaticOrdersQuickToggle;
        
                private Border _popup;
                private TextBlock _popupText;
                private DateTime _popupUntilUtc = DateTime.MinValue;
        
                private const string P = "CFIP_";
                private const string H = "CFIP_H_";
        
                private int _lastContextM5 = -1;
                private int _lastInvalidationAlertM5 = -1;
                private bool _panelHidden;
                private Button _panelToggleButton;
                private string _panelStableHeader = "";
                private DateTime _panelStableHeaderSinceUtc = DateTime.MinValue;
                private Button _popupCloseButton;
        private int _runtimeTpStageIndex = -1;
        private int _runtimeTpStagePlanCreatedM5 = -1;
        private int _lastReactionAlertBar = -1;

        private readonly Dictionary<int, int> _directionSamples =
                    new Dictionary<int, int>();
        
                private readonly Dictionary<int, int> _directionWins =
                    new Dictionary<int, int>();
        
                private DateTime _lastBrokerModifyUtc = DateTime.MinValue;
                private int _lastRestrictionM5 = -1;
                private int _lastSmartDecisionAlertM5 = -1;
                private int _lastHistoricalHostBar = -1;
                private int _lastAutoPlanAttemptM5 = -1;
                private DateTime _lastTradingPermissionRequestUtc = DateTime.MinValue;
        
                private int _marketSuitabilityM5 = -1;
                private int _marketSuitabilityDirection = 0;
                private int _marketSuitabilityScore = 0;
                private string _marketSuitabilityState = "UNKNOWN";
                private string _marketSuitabilityReason = "NOT EVALUATED";
                private DateTime _lastMarketSuitabilityUtc = DateTime.MinValue;

        private DateTime _lastLiveReactionCalcUtc = DateTime.MinValue;
        private int _lastLiveReactionM5 = -1;
        private double _lastLiveReactionMarket;

        private DateTime _lastExecutionModelBuildUtc = DateTime.MinValue;
        private int _lastExecutionModelM5 = -1;
        private double _lastExecutionModelMarket;

        private DateTime _lastPanelRenderUtc = DateTime.MinValue;
        private DateTime _lastPlanRenderUtc = DateTime.MinValue;
        private DateTime _lastQuickControlSyncUtc = DateTime.MinValue;
        private int _lastLiveM1FrameIndex = -1;

        private int _lastSignalRenderBar = -1;
        private int _lastSignalRenderDirection;
        private string _lastSignalRenderState = "";
        private bool _lastSignalRenderVisible;
        private int _lastSignalMarkerClearM5 = -1;
    }
}
