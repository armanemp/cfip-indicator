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
                                    UpdateRuntimeBootstrapVisual(
                                        "INIT ERROR  •  " +
                                        (string.IsNullOrWhiteSpace(stage)
                                            ? "UNKNOWN"
                                            : stage));
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
        
        protected override void Initialize()
                                {
                                    // Create a visible, correctly sized bootstrap surface before
                                    // accessing secondary data feeds or native indicators. This makes
                                    // initialization failures diagnosable instead of leaving an
                                    // apparently empty indicator instance on the chart.
                                    _native.Clear();
                                    _historicalDrawn.Clear();
                                    _outcomeDrawn.Clear();

                                    _status = "STARTING";
                                    CreateRuntimeBootstrapVisual();
                                    CreatePanel();

                                    try
                                    {
                                        _m1Bars = MarketData.GetBars(TimeFrame.Minute);
                                        _m5Bars = MarketData.GetBars(TimeFrame.Minute5);
                                        _m15Bars = MarketData.GetBars(TimeFrame.Minute15);
                                        _m30Bars = MarketData.GetBars(TimeFrame.Minute30);
                                        _h1Bars = MarketData.GetBars(TimeFrame.Hour);
                                        _h4Bars = MarketData.GetBars(TimeFrame.Hour4);
                                        _d1Bars = MarketData.GetBars(TimeFrame.Daily);
                                        _w1Bars = MarketData.GetBars(TimeFrame.Weekly);
                                    }
                                    catch (Exception ex)
                                    {
                                        SetInitializationFault(ex, "MARKET DATA");
                                        return;
                                    }

                                    try
                                    {
                                        RegisterAllNative();
                                    }
                                    catch (Exception ex)
                                    {
                                        SetInitializationFault(ex, "NATIVE INDICATORS");
                                        return;
                                    }
                        
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
                                        Print("CFIP trading event hookup failed: {0}", ex.ToString());
                                    }
                        
                                    try
                                    {
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

                                        RenderPanel();
                                    }
                                    catch (Exception ex)
                                    {
                                        SetInitializationFault(ex, "RUNTIME STATE");
                                    }
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
                                    RemoveRuntimeBootstrapVisual();
                                    base.OnDestroy();
                                }
    }
}
