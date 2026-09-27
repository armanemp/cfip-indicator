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
        private void InitializeExecutionRuntimeState()
                {
                    _autoTradingEnabledRuntime =
                        EnableAutoTrading;
        
                    _automaticOrdersEnabledRuntime =
                        EnableAutomaticOrders;
        
                    _lastConfiguredAutoTrading =
                        EnableAutoTrading;
        
                    _lastConfiguredAutomaticOrders =
                        EnableAutomaticOrders;
        
                    _outcomeTelemetryTimedOut =
                        false;
        
                    _executionRuntimeInitialized =
                        true;
        
                    _autoExecutionBlockReason =
                        _autoTradingEnabledRuntime
                            ? "NOT EVALUATED"
                            : "DISABLED";
        
                    _autoOrdersBlockReason =
                        _automaticOrdersEnabledRuntime
                            ? "NOT EVALUATED"
                            : "DISABLED";
                }
        
                private void EnsureExecutionRuntimeState()
                {
                    if (!_executionRuntimeInitialized)
                    {
                        InitializeExecutionRuntimeState();
                        return;
                    }
        
                    // Parameters are configuration inputs. Runtime quick controls own
                    // the live execution state. Only an actual external/configuration
                    // change in a parameter is allowed to alter that runtime state.
                    if (EnableAutoTrading != _lastConfiguredAutoTrading)
                    {
                        _lastConfiguredAutoTrading =
                            EnableAutoTrading;
        
                        _autoTradingEnabledRuntime =
                            EnableAutoTrading;
        
                        _autoExecutionBlockReason =
                            EnableAutoTrading
                                ? "AWAITING EXECUTION"
                                : "DISABLED";
                    }
        
                    if (EnableAutomaticOrders !=
                        _lastConfiguredAutomaticOrders)
                    {
                        _lastConfiguredAutomaticOrders =
                            EnableAutomaticOrders;
        
                        _automaticOrdersEnabledRuntime =
                            EnableAutomaticOrders;
        
                        _autoOrdersBlockReason =
                            EnableAutomaticOrders
                                ? "AWAITING ORDER SETUP"
                                : "DISABLED";
                    }
                }
        
                protected override void Initialize()
                {
                    _native.Clear();
                    _historicalDrawn.Clear();
                    _outcomeDrawn.Clear();
        
                    _m1Bars = MarketData.GetBars(TimeFrame.Minute);
                    _m5Bars = MarketData.GetBars(TimeFrame.Minute5);
                    _m15Bars = MarketData.GetBars(TimeFrame.Minute15);
                    _m30Bars = MarketData.GetBars(TimeFrame.Minute30);
                    _h1Bars = MarketData.GetBars(TimeFrame.Hour);
                    _h4Bars = MarketData.GetBars(TimeFrame.Hour4);
                    _d1Bars = MarketData.GetBars(TimeFrame.Daily);
                    _w1Bars = MarketData.GetBars(TimeFrame.Weekly);
        
                    RegisterAllNative();
        
                    try
                    {
                        Positions.Opened += OnPositionOpened;
                        Positions.Closed += OnPositionClosed;
                        Positions.Modified += OnPositionModified;
                        PendingOrders.Created += OnPendingOrderCreated;
                        PendingOrders.Modified += OnPendingOrderModified;
                        PendingOrders.Filled += OnPendingOrderFilled;
                        PendingOrders.Cancelled += OnPendingOrderCancelled;
                    }
                    catch (Exception ex)
                    {
                        Print("CFIP trading event hookup failed: {0}", ex.Message);
                    }
        
                    InitializeExecutionRuntimeState();
        
                    SetLifecycleState(
                        LifecycleState.Flat,
                        "READY");
        
                    CreatePanel();
        
                    SetAutoTradingState(
                        AutoTradingEnabled
                            ? "ARMED"
                            : "OFF",
                        AutoTradingEnabled
                            ? "INITIALIZING"
                            : "DISABLED");
        
                    _status = "READY";
                }
        
                protected override void OnDestroy()
                {
                    try
                    {
                        Positions.Opened -= OnPositionOpened;
                        Positions.Closed -= OnPositionClosed;
                        Positions.Modified -= OnPositionModified;
                        PendingOrders.Created -= OnPendingOrderCreated;
                        PendingOrders.Modified -= OnPendingOrderModified;
                        PendingOrders.Filled -= OnPendingOrderFilled;
                        PendingOrders.Cancelled -= OnPendingOrderCancelled;
                    }
                    catch
                    {
                    }
        
                    RemoveAllChartObjects();
                    RemovePanel();
                    RemovePopup();
                    base.OnDestroy();
                }
        
                private MtfClosedContext BuildMtfClosedContext(
                    DateTime reference)
                {
                    if (_m5Bars == null)
                        return new MtfClosedContext(
                            reference,
                            -1,
                            -1,
                            -1,
                            -1,
                            -1,
                            -1,
                            -1,
                            -1);
        
                    return new MtfClosedContext(
                        reference,
                        ClosedIndex(_m5Bars, reference),
                        ClosedIndex(_m1Bars, reference),
                        ClosedIndex(_m15Bars, reference),
                        ClosedIndex(_m30Bars, reference),
                        ClosedIndex(_h1Bars, reference),
                        ClosedIndex(_h4Bars, reference),
                        ClosedIndex(_d1Bars, reference),
                        ClosedIndex(_w1Bars, reference));
                }
        
                public override void Calculate(int index)
                {
                    if (!IsLastBar ||
                        Bars == null)
                        return;
        
                    RemoveExpiredPopup();
        
                    if (!HasEnoughData())
                    {
                        _status = "BUILDING DATA";
                        RenderPanel();
                        return;
                    }
        
                    DateTime reference =
                        _m5Bars.OpenTimes[
                            _m5Bars.Count - 1];
        
                    MtfClosedContext mtf =
                        BuildMtfClosedContext(
                            reference);
        
                    int closedM5 =
                        mtf.M5;
        
                    if (!mtf.HasPrimaryDecisionHistory)
                    {
                        _status =
                            "WAITING FOR CLOSED M5";
                        RenderPanel();
                        return;
                    }
        
                    bool newClosedBar =
                        closedM5 !=
                        _lastEvaluatedM5;
        
                    // Expensive MTF/structure calculation runs exactly once per newly
                    // closed M5 bar. Live price management remains tick responsive.
                    if (newClosedBar)
                    {
                        int m1Index = mtf.M1;
        
                        int m15Index = mtf.M15;
        
                        int m30Index = mtf.M30;
        
                        int h1Index = mtf.H1;
        
                        int h4Index = mtf.H4;
        
                        int d1Index = mtf.D1;
        
                        int w1Index = mtf.W1;
        
                        if (m15Index < 30 ||
                            m30Index < 30 ||
                            h1Index < 30 ||
                            h4Index < 30)
                        {
                            _status =
                                "WAITING FOR MTF DATA";
                            RenderPanel();
                            return;
                        }
        
                        _m1Frame =
                            m1Index >= 30
                                ? AnalyzeFrame(
                                    _m1Bars,
                                    m1Index)
                                : null;
        
                        _m5Frame =
                            AnalyzeFrame(
                                _m5Bars,
                                closedM5);
        
                        _m15Frame =
                            AnalyzeFrame(
                                _m15Bars,
                                m15Index);
        
                        _m30Frame =
                            AnalyzeFrame(
                                _m30Bars,
                                m30Index);
        
                        _h1Frame =
                            AnalyzeFrame(
                                _h1Bars,
                                h1Index);
        
                        _h4Frame =
                            AnalyzeFrame(
                                _h4Bars,
                                h4Index);
        
                        _d1Frame =
                            d1Index >= 10
                                ? AnalyzeFrame(
                                    _d1Bars,
                                    d1Index)
                                : null;
        
                        _w1Frame =
                            w1Index >= 10
                                ? AnalyzeFrame(
                                    _w1Bars,
                                    w1Index)
                                : null;
        
                        int decisionChartIndex =
                            MapM5ToClosedChart(
                                closedM5,
                                index);
        
                        _decision =
                            BuildDecision(
                                decisionChartIndex,
                                closedM5,
                                reference);
        
                        RefreshMarketSuitability(
                            closedM5,
                            _decision == null
                                ? 0
                                : _decision.Direction,
                            true);
        
                        _prediction =
                            BuildEarlyPrediction(
                                closedM5);
        
                        RenderPredictionObjects(
                            _prediction,
                            closedM5);
        
                        EmitContextAlerts(
                            closedM5);
        
                        if (_decision != null)
                        {
                            if (AlertOnHighConfidenceEntry &&
                                _decision.Confidence >=
                                HighConfidenceThreshold &&
                                _lastHighConfidenceM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "HIGH|" +
                                    closedM5,
                                    "CFIP HIGH CONFIDENCE | " +
                                    (_decision.Direction == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " | CONF " +
                                    _decision.Confidence,
                                    _decision.Direction,
                                    true);
        
                                _lastHighConfidenceM5 =
                                    closedM5;
                            }
        
                            if (!_decision.EntryAllowed &&
                                RestrictionAlertEnabled(
                                    _decision.BlockReason) &&
                                !string.IsNullOrWhiteSpace(
                                    _decision.BlockReason))
                            {
                                string restrictionMessage =
                                    _decision.BlockReason.Trim();
        
                                bool restrictionChanged =
                                    !string.Equals(
                                        _lastRestrictionMessage,
                                        restrictionMessage,
                                        StringComparison.OrdinalIgnoreCase);
        
                                if (restrictionChanged)
                                {
                                    SendUnifiedAlert(
                                        "RESTRICT|" +
                                        restrictionMessage,
                                        "CFIP ENTRY BLOCKED | " +
                                        restrictionMessage,
                                        _decision.Direction,
                                        false);
        
                                    _lastRestrictionMessage =
                                        restrictionMessage;
        
                                    _lastRestrictionAlertUtc =
                                        TimeInUtc;
        
                                    _lastRestrictionM5 =
                                        closedM5;
                                }
                            }
                            else if (_decision.EntryAllowed)
                            {
                                _lastRestrictionMessage = "";
                                _lastRestrictionAlertUtc =
                                    DateTime.MinValue;
                                _lastRestrictionM5 = -1;
                            }
        
                            if (AlertOnSmartDecision &&
                                _decision.EntryAllowed &&
                                _decision.SmartQuality >=
                                SmartStrongSetupQuality &&
                                _decision.Edge >=
                                SmartStrongSetupEdge &&
                                _lastSmartDecisionAlertM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "SMART|" +
                                    closedM5,
                                    "CFIP SMART DECISION | " +
                                    (_decision.Direction == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " | Q " +
                                    _decision.SmartQuality +
                                    " | CONF " +
                                    _decision.Confidence,
                                    _decision.Direction,
                                    true);
        
                                _lastSmartDecisionAlertM5 =
                                    closedM5;
                            }
                        }
        
                        ReconcilePreTradePlanDirection(
                            closedM5);
        
                        EnsureSignalPlan(
                            closedM5,
                            AutoTradingEnabled &&
                            !ConfirmedSignalsOnly
                                ? DecisionPolicyMode.Soft
                                : DecisionPolicyMode.Confirmed);
        
                        _lastEvaluatedM5 =
                            closedM5;
        
                        if (ShowHistoricalSignals)
                        {
                            int hostBar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        index));
        
                            if (_lastHistoricalHostBar !=
                                hostBar)
                            {
                                RenderHistoricalSignals();
                                _lastHistoricalHostBar =
                                    hostBar;
                            }
                        }
                        else
                        {
                            RemoveHistoricalObjects();
                            _lastHistoricalHostBar =
                                -1;
                        }
                    }
        
                    // Tick-level path: reaction and active plan management remain live.
                    _reaction =
                        BuildReaction();
        
                    RecoverManagedLivePlan(closedM5);
        
                    if (!AutoTradingEnabled &&
                        AutoTradingReminder)
                        CheckAutoTradingDisabledReminder(closedM5);
        
                    SyncQuickExecutionControls();
        
                    if (AutoTradingEnabled &&
                        _plan == null &&
                        _decision != null &&
                        _decision.Direction != 0 &&
                        _lastAutoPlanAttemptM5 !=
                        closedM5)
                    {
                        _lastAutoPlanAttemptM5 =
                            closedM5;
        
                        EnsureSignalPlan(
                            closedM5,
                            ConfirmedSignalsOnly
                                ? DecisionPolicyMode.Confirmed
                                : DecisionPolicyMode.Soft);
                    }
        
                    if (_decision != null &&
                        _decision.Direction != 0 &&
                        _plan == null)
                    {
                        _executionModel =
                            BuildExecutionModel(
                                closedM5,
                                _decision.Direction);
                    }
                    else if (_plan == null)
                    {
                        _executionModel = null;
                    }
        
                    SynchronizeLiveBrokerState();
        
                    EvaluateActivePlan(
                        closedM5);
        
                    // Complete all execution, broker-protection and lifecycle state
                    // changes before rendering. This prevents the chart/panel from
                    // displaying the state from before a same-cycle execution event.
                    TryAutoTrade(
                        closedM5);
        
                    TryAggressiveAutoTrade(
                        closedM5);
        
                    TrySmartPendingOrders(
                        closedM5);
        
                    ProtectBrokerPositions(
                        closedM5);
        
                    MonitorOutcome(
                        closedM5);
        
                    CheckEndOfDayAlert(
                        TimeInUtc);
        
                    CheckReversalProtection();
        
                    SynchronizeLiveBrokerState();
        
                    if (_plan != null)
                        RenderPlan();
                    else
                        RenderWatchAndReaction(
                            index,
                            closedM5);
        
                    RenderManagedPendingOrder();
                    RenderPanel();
                }
        
                private bool HasEnoughData()
                {
                    return _m5Bars != null &&
                           _m15Bars != null &&
                           _m30Bars != null &&
                           _h1Bars != null &&
                           _h4Bars != null &&
                           _m5Bars.Count >= 100 &&
                           _m15Bars.Count >= 100 &&
                           _m30Bars.Count >= 80 &&
                           _h1Bars.Count >= 80 &&
                           _h4Bars.Count >= 60;
                }
    }
}
