// ============================================================================
// CFIP Indicator — RuntimeInitialization.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
    }
}
