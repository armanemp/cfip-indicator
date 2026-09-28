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

        private void ScheduleNextInitializationStage()
                                {
                                    Timer.Stop();
                                    Timer.Start(
                                        TimeSpan.FromMilliseconds(100));
                                }

        protected override void Initialize()
                                {
                                    _native.Clear();
                                    _historicalDrawn.Clear();
                                    _outcomeDrawn.Clear();

                                    _initializationReady = false;
                                    _initializationStage = 0;
                                    _status = "STARTING";

                                    // Register the panel before any expensive data acquisition.
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

                                    ScheduleNextInitializationStage();
                                }

        protected override void OnTimer()
                                {
                                    try
                                    {
                                        switch (_initializationStage)
                                        {
                                            case 0:
                                                _m1Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Minute);
                                                break;

                                            case 1:
                                                _m5Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Minute5);
                                                break;

                                            case 2:
                                                _m15Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Minute15);
                                                break;

                                            case 3:
                                                _m30Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Minute30);
                                                break;

                                            case 4:
                                                _h1Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Hour);
                                                break;

                                            case 5:
                                                _h4Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Hour4);
                                                break;

                                            case 6:
                                                _d1Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Daily);
                                                break;

                                            case 7:
                                                _w1Bars =
                                                    MarketData.GetBars(
                                                        TimeFrame.Weekly);
                                                break;

                                            case 8:
                                                RegisterAllNative();
                                                break;

                                            case 9:
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
                                                    Print(
                                                        "CFIP trading event hookup failed: {0}",
                                                        ex.ToString());
                                                }
                                                break;

                                            case 10:
                                                InitializeExecutionRuntimeState();

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
                                                Timer.Stop();

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
                                                return;
                                        }

                                        _initializationStage++;
                                        ScheduleNextInitializationStage();
                                    }
                                    catch (Exception ex)
                                    {
                                        Timer.Stop();
                                        SetInitializationFault(
                                            ex,
                                            "STAGE " +
                                            _initializationStage);
                                    }
                                }

        protected override void OnDestroy()
                                {
                                    try
                                    {
                                        Timer.Stop();
                                    }
                                    catch
                                    {
                                    }

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
    }
}
