// ============================================================================
// CFIP Indicator — RuntimeInitialization.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
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

        private void SetInitializationFault(Exception exception, string stage)
                                {
                                    _initializationReady = false;
                                    _autoTradingEnabledRuntime = false;
                                    _automaticOrdersEnabledRuntime = false;
                                    _autoExecutionBlockReason = "INITIALIZATION FAULT";
                                    _autoOrdersBlockReason = "INITIALIZATION FAULT";
                                    _autoTradingState = "ERROR";
                                    _autoTradingReason = "INITIALIZATION FAULT";
                                    _status = "INIT ERROR";

                                    Print(
                                        "CFIP initialization fault [{0}]: {1}",
                                        stage,
                                        exception == null
                                            ? "UNKNOWN"
                                            : exception.ToString());

                                    try
                                    {
                                        if (_panelHeaderTitle != null)
                                        {
                                            _panelHeaderTitle.Text =
                                                "CFIP SMART  •  INIT ERROR  •  " +
                                                stage;
                                            _panelHeaderTitle.ForegroundColor =
                                                PanelWarningColor;
                                        }

                                        if (_panel != null)
                                            _panel.IsVisible = true;
                                    }
                                    catch (Exception panelException)
                                    {
                                        Print(
                                            "CFIP initialization-fault panel update failed: {0}",
                                            panelException.ToString());
                                    }
                                }

        private void ScheduleInitializationPoll()
        {
            Timer.Stop();
            Timer.Start(
                TimeSpan.FromMilliseconds(250));
        }

        private void StartAsyncBarsInitialization()
        {
            if (_initializationDataRequested)
                return;

            _initializationDataRequested = true;
            _initializationDataReady = false;
            _initializationPendingDataLoads = 0;
            _initializationStartedUtc = TimeInUtc;

            RequestBars(
                TimeFrame.Minute5,
                bars => _m5Bars = bars);

            RequestBars(
                TimeFrame.Minute15,
                bars => _m15Bars = bars);

            RequestBars(
                TimeFrame.Minute30,
                bars => _m30Bars = bars);

            RequestBars(
                TimeFrame.Hour,
                bars => _h1Bars = bars);

            RequestBars(
                TimeFrame.Hour4,
                bars => _h4Bars = bars);

            RequestBars(
                TimeFrame.Minute,
                bars => _m1Bars = bars);

            if (SmartWeeklyContext)
            {
                // D1/W1 refine the top-down context but are not required to
                // start the primary decision engine. Load them independently
                // so a slow higher-timeframe request cannot stall startup.
                RequestOptionalBars(
                    TimeFrame.Daily,
                    bars => _d1Bars = bars);

                RequestOptionalBars(
                    TimeFrame.Weekly,
                    bars => _w1Bars = bars);
            }

            if (_initializationPendingDataLoads == 0)
                _initializationDataReady = true;
        }

        private void RequestBars(
            TimeFrame timeFrame,
            Action<Bars> assign)
        {
            _initializationPendingDataLoads++;

            try
            {
                MarketData.GetBarsAsync(
                    timeFrame,
                    bars =>
                    {
                        try
                        {
                            if (bars != null && assign != null)
                                assign(bars);
                        }
                        finally
                        {
                            _initializationPendingDataLoads =
                                Math.Max(
                                    0,
                                    _initializationPendingDataLoads - 1);

                            if (_initializationPendingDataLoads == 0)
                                _initializationDataReady = true;
                        }
                    });
            }
            catch
            {
                _initializationPendingDataLoads =
                    Math.Max(
                        0,
                        _initializationPendingDataLoads - 1);
                throw;
            }
        }

        private void FinalizeAsyncInitialization()
        {
            if (_initializationReady)
                return;

            if (!HasEnoughData())
            {
                _status = "BUILDING DATA";
                return;
            }

            RegisterNative(_m5Bars);
            RegisterNative(_m15Bars);
            RegisterNative(_m30Bars);
            RegisterNative(_h1Bars);
            RegisterNative(_h4Bars);

            RegisterNative(_m1Bars);

            if (_d1Bars != null)
                RegisterNative(_d1Bars);

            if (_w1Bars != null)
                RegisterNative(_w1Bars);

            try
            {
                Positions.Opened += OnPositionOpened;
                Positions.Closed += OnPositionClosed;
                Positions.Modified += OnPositionModified;
                Account.Switched += OnAccountSwitched;
                Account.Switched += OnOutcomeMemoryAccountSwitched;
                PendingOrders.Created += OnPendingOrderCreated;
                PendingOrders.Modified += OnPendingOrderModified;
                PendingOrders.Filled += OnPendingOrderFilled;
                PendingOrders.Cancelled += OnPendingOrderCancelled;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP trading event hookup failed: {0}",
                    ex.ToString());
            }

            InitializeExecutionRuntimeState();
            HookHistoricalBarsEvents();

            if (!ValidateTradeIdentityConfiguration())
            {
                _autoTradingEnabledRuntime = false;
                _automaticOrdersEnabledRuntime = false;
                _autoExecutionBlockReason =
                    "IDENTITY CONFIGURATION";
                _autoOrdersBlockReason =
                    "IDENTITY CONFIGURATION";
            }

            SetLifecycleState(
                LifecycleState.Flat,
                "READY");

            SetAutoTradingState(
                AutoTradingEnabled
                    ? "ARMED"
                    : "OFF",
                AutoTradingEnabled
                    ? "INITIALIZING"
                    : "DISABLED");

            _status = "READY";
            _initializationReady = true;
            _lastPanelRenderUtc = DateTime.MinValue;

            Timer.Stop();
            Timer.Start(
                TimeSpan.FromMilliseconds(500));

            Print(
                "CFIP ALERT AUDIO | enabled={0} | semanticSounds={1} | configuredCue={2} | customFile={3}",
                EnableSoundAlerts,
                UseSemanticAlertSounds,
                AlertSoundType,
                string.IsNullOrWhiteSpace(SoundFilePath)
                    ? "NONE"
                    : SoundFilePath);

            try
            {
                RenderPanel();
            }
            catch (Exception ex)
            {
                HandleRuntimeFault(
                    ex,
                    -1,
                    "INITIALIZATION RENDER");
            }

            QueueStartupCalculationSeed();
        }

        protected override void Initialize()
        {
            ResetEconomicNewsClientLifecycle();

            _native.Clear();
            _historicalDrawn.Clear();
            _outcomeDrawn.Clear();
            ResetHistoricalRenderingState();

            try
            {
                EnsurePortableMemoryArtifacts();
                RestoreOutcomeHistory();
                PersistPortableMemorySnapshot();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome memory initialization failed: {0}",
                    ex.Message);
            }

            _initializationReady = false;
            _initializationDataRequested = false;
            _initializationDataReady = false;
            _initializationPendingDataLoads = 0;
            _initializationStartedUtc = TimeInUtc;
            _startupCalculationSeedDone = false;
            _startupCalculationSeedQueued = false;
            _status = "STARTING";

            SubscribeCbotChartLifecycleEvents();

            CreatePanel();

            try
            {
                RenderPanel();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP startup panel render failed: {0}",
                    ex.ToString());
            }

            try
            {
                StartAsyncBarsInitialization();
                ScheduleInitializationPoll();
            }
            catch (Exception ex)
            {
                Timer.Stop();
                SetInitializationFault(
                    ex,
                    "ASYNC DATA REQUEST");
            }
        }

        protected override void OnTimer()
        {
            if (_initializationReady)
            {
                HandleRuntimeHeartbeat();
                ProcessQueuedAlertDelivery();
                return;
            }

            if (!_initializationDataRequested)
            {
                try
                {
                    StartAsyncBarsInitialization();
                }
                catch (Exception ex)
                {
                    Timer.Stop();
                    SetInitializationFault(
                        ex,
                        "ASYNC DATA REQUEST");
                    return;
                }

                ScheduleInitializationPoll();
                return;
            }

            if (HasEnoughData())
            {
                try
                {
                    FinalizeAsyncInitialization();
                }
                catch (Exception ex)
                {
                    Timer.Stop();
                    SetInitializationFault(
                        ex,
                        "FINALIZATION");
                    return;
                }

                if (!_initializationReady)
                    ScheduleInitializationPoll();

                return;
            }

            if (!_initializationDataReady)
            {
                TimeSpan elapsed =
                    TimeInUtc -
                    _initializationStartedUtc;

                if (elapsed.TotalSeconds >=
                    30)
                {
                    Timer.Stop();

                    SetInitializationFault(
                        new TimeoutException(
                            "ASYNC MARKET DATA INITIALIZATION TIMEOUT"),
                        "DATA LOAD");
                    return;
                }

                _status =
                    _initializationPendingDataLoads > 0
                        ? "LOADING DATA"
                        : "BUILDING DATA";

                UpdateInitializationPanelStatus();
                RenderPanel();
                ScheduleInitializationPoll();
                return;
            }

            try
            {
                FinalizeAsyncInitialization();
            }
            catch (Exception ex)
            {
                Timer.Stop();
                SetInitializationFault(
                    ex,
                    "FINALIZATION");
            }
            finally
            {
                if (!_initializationReady)
                    ScheduleInitializationPoll();
            }
        }

        private void QueueStartupCalculationSeed()
        {
            if (_startupCalculationSeedDone ||
                _startupCalculationSeedQueued ||
                !_initializationReady)
                return;

            _startupCalculationSeedQueued = true;

            BeginInvokeOnMainThread(
                () =>
                {
                    _startupCalculationSeedQueued = false;

                    if (!_initializationReady ||
                        _startupCalculationSeedDone)
                        return;

                    try
                    {
                        RunStartupCalculationSeed();
                    }
                    catch (Exception ex)
                    {
                        HandleRuntimeFault(
                            ex,
                            -1,
                            "STARTUP CALCULATION SEED");
                    }
                });
        }

        protected override void OnDestroy()
                                {
                                    DisposeEconomicNewsClient();
                                    FlushBufferedPersistenceOnShutdown();

                                    try
                                    {
                                        Timer.Stop();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP OnDestroy timer stop failed: {0}",
                                            ex.ToString());
                                    }

                                    try
                                    {
                                        UnsubscribeCbotChartLifecycleEvents();
                                        UnhookHistoricalBarsEvents();
                                        Positions.Opened -= OnPositionOpened;
                                        Positions.Closed -= OnPositionClosed;
                                        Positions.Modified -= OnPositionModified;
                                        Account.Switched -= OnAccountSwitched;
                                        Account.Switched -= OnOutcomeMemoryAccountSwitched;
                                        PendingOrders.Created -= OnPendingOrderCreated;
                                        PendingOrders.Modified -= OnPendingOrderModified;
                                        PendingOrders.Filled -= OnPendingOrderFilled;
                                        PendingOrders.Cancelled -= OnPendingOrderCancelled;
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP OnDestroy event unsubscription failed: {0}",
                                            ex.ToString());
                                    }

                                    PersistOutcomeHistory();
                                    PersistPortableMemorySnapshot();

                                    RemoveAllChartObjects();
                                    RemovePanel();
                                    _panelAlertHistory.Clear();
                                    _panelAlertMessageRows.Clear();
                                    _panelAlertMessageStack = null;
                                    _alertDeliveryQueue.ClearPendingAlerts();
                                    base.OnDestroy();
                                }
    }
}
