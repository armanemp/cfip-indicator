// ============================================================================
// CFIP Indicator — PanelRenderer.cs
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
        private void RenderPanel()
                        {
                            if (!ShowUnifiedPanel)
                            {
                                RemovePanel();
                                return;
                            }
                
                            if (_panel == null)
                                CreatePanel();
                
                            if (_panel == null ||
                                _panelStack == null ||
                                _panelHeaderStack == null ||
                                _panelHeaderTitle == null ||
                                _panelRowsStack == null ||
                                _panelScroll == null ||
                                _buttonStack == null ||
                                _panelRows.Count != PanelRowCount)
                                return;
                
                            if (_panelToggleButton == null)
                                CreatePanelToggleButton();
                
                            if (_panelRestoreButton == null)
                                CreatePanelRestoreButton();
                
                            _panel.IsVisible =
                                !_panelHidden;
                
                            int padding =
                                Math.Max(
                                    0,
                                    PanelPadding);
                
                            int border =
                                Math.Max(
                                    0,
                                    PanelBorderThickness);
                
                            int effectivePanelWidth =
                                Math.Max(
                                    260,
                                    PanelWidth);
                
                            int contentWidth =
                                Math.Max(
                                    200,
                                    effectivePanelWidth -
                                    padding * 2 -
                                    border * 2);
                
                            bool showSafetyButtons =
                                ShowTradeActionButtons ||
                                AlwaysShowSafetyButtons;
                
                            bool showButtonRow =
                                showSafetyButtons ||
                                ShowPanelToggleButton;
                
                            bool buttons =
                                showButtonRow;
                
                            int buttonHeight =
                                Math.Max(
                                    26,
                                    ActionButtonHeight);
                
                            int buttonGap =
                                Math.Max(
                                    0,
                                    PanelButtonGap);
                
                            int buttonMargin =
                                Math.Max(
                                    0,
                                    ActionButtonMargin);
                
                            int toggleSideForLayout =
                                Math.Max(
                                    22,
                                    Math.Min(
                                        40,
                                        Math.Min(
                                            PanelToggleWidth,
                                            PanelToggleHeight)));
                
                            int buttonContentHeight =
                                Math.Max(
                                    buttonHeight,
                                    ShowPanelToggleButton
                                        ? toggleSideForLayout
                                        : 0);
                
                            int buttonAreaHeight =
                                showButtonRow
                                    ? buttonContentHeight +
                                      buttonMargin * 2
                                    : 0;
                
                            int configuredMaxHeight =
                                Math.Max(
                                    240,
                                    PanelMaxHeight);
                
                            int availableChartHeight =
                                0;
                
                            try
                            {
                                availableChartHeight =
                                    (int)Math.Round(
                                        Math.Max(
                                            0,
                                            Chart.Height -
                                            Math.Max(
                                                0,
                                                PanelMargin) * 2 -
                                            8));
                            }
                            catch
                            {
                                availableChartHeight = 0;
                            }
                
                            int maxHeight =
                                availableChartHeight > 0
                                    ? Math.Max(
                                        180,
                                        Math.Min(
                                            configuredMaxHeight,
                                            availableChartHeight))
                                    : configuredMaxHeight;
                
                            int quickExecutionHeight =
                                QuickExecutionRowHeight;
                
                            int fixedHeight =
                                PanelHeaderHeight +
                                quickExecutionHeight +
                                buttonAreaHeight +
                                padding * 2 +
                                border * 2;
                
                            int maximumScrollHeight =
                                Math.Max(
                                    120,
                                    maxHeight -
                                    fixedHeight);
                
                            for (int i = 0;
                                 i < _panelRows.Count;
                                 i++)
                            {
                                TextBlock row =
                                    _panelRows[i];
                
                                row.Margin =
                                    new Thickness(
                                        Math.Max(
                                            0,
                                            PanelRowPadding),
                                        i == 0
                                            ? 0
                                            : Math.Max(
                                                1,
                                                PanelRowGap),
                                        Math.Max(
                                            0,
                                            PanelRowPadding),
                                        Math.Max(
                                            0,
                                            PanelRowPadding));
                
                                row.IsVisible =
                                    false;
                            }
                
                            RenderPanelRows(
                                contentWidth);
                
                            int scrollHeight =
                                EstimatePanelScrollHeight(
                                    contentWidth,
                                    maximumScrollHeight);
                
                            ApplyPanelVisualSettings(
                                contentWidth,
                                scrollHeight,
                                maxHeight,
                                buttons,
                                buttonHeight,
                                buttonGap);
                
                            SyncQuickExecutionControls();
                        }
        
        private void RenderPanelRows(
                            int contentWidth)
                        {
                            int slot = 0;
                
                            int authoritativeDirection =
                                GetAuthoritativeDirection();
                
                            _authoritativeDirection =
                                authoritativeDirection;
                
                            _authoritativeState =
                                GetAuthoritativeState(
                                    authoritativeDirection);
                
                            string stableState =
                                GetStablePanelState(
                                    _authoritativeState);
                
                            int stateDirection =
                                stableState.StartsWith(
                                    "BUY",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? 1
                                    : stableState.StartsWith(
                                        "SELL",
                                        StringComparison.OrdinalIgnoreCase)
                                        ? -1
                                        : 0;
                
                            AddPanelRow(
                                ref slot,
                                "CFIP SMART   •  " +
                                stableState,
                                PanelDirectionColor(
                                    stateDirection),
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                AutoTradingPanelLine(),
                                AutoTradingPanelColor(),
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "SMART ACTION  •  " +
                                _authoritativeState,
                                PanelDirectionColor(
                                    authoritativeDirection),
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "SYNC  " +
                                GetSignalSynchronizationText(),
                                GetSignalSynchronizationColor(),
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                SymbolName +
                                "  •  " +
                                Bars.TimeFrame +
                                "  •  " +
                                TimeInUtc.ToString(
                                    "HH:mm:ss") +
                                " UTC",
                                PanelMutedTextColor,
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                GetSessionPanelText(),
                                GetSessionPanelColor(),
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "SUITABILITY  " +
                                _marketSuitabilityScore +
                                "/100  •  " +
                                _marketSuitabilityState +
                                "  •  " +
                                CompactText(
                                    _marketSuitabilityReason,
                                    54),
                                _marketSuitabilityScore >=
                                    Math.Max(
                                        50,
                                        MinimumMarketSuitability)
                                    ? TpLineColor
                                    : PanelWarningColor,
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "SMART RISK  " +
                                EffectiveAutoRiskPercent().ToString("F2") +
                                "%  •  " +
                                SuitabilityRiskMultiplier().ToString("F2") +
                                "x BASE",
                                PanelSecondaryTextColor,
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "AUTO EXEC  " +
                                (AutoTradingEnabled
                                    ? "ON"
                                    : "OFF") +
                                "  •  " +
                                CompactText(
                                    _autoExecutionBlockReason,
                                    72),
                                AutoTradingEnabled &&
                                string.Equals(
                                    _autoExecutionBlockReason,
                                    "READY TO SUBMIT",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? TpLineColor
                                    : PanelSecondaryTextColor,
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "EXEC MODE  " +
                                (_executionModel == null
                                    ? "NONE"
                                    : ExecutionModeText(
                                        _executionModel.Mode)) +
                                "  •  ENTRY " +
                                (_executionModel != null &&
                                 IsFinitePositive(
                                     _executionModel.ActualEntry)
                                    ? Price(
                                        _executionModel.ActualEntry)
                                    : "WAIT") +
                                "  •  TRIGGER " +
                                (_executionModel != null
                                    ? Price(
                                        _executionModel.Trigger)
                                    : "-"),
                                _executionModel != null &&
                                _executionModel.Mode ==
                                    ExecutionMode.BreakoutMarket
                                    ? EntryLineColor
                                    : TriggerLineColor,
                                true,
                                contentWidth);
                
                            if (_executionModel != null &&
                                _executionModel.Direction != 0)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "ENTRY RELATION  " +
                                    GetExecutionRelationText(
                                        _executionModel,
                                        _executionModel.ActualEntry),
                                    PanelSecondaryTextColor,
                                    false,
                                    contentWidth);
                            }
                
                            AddPanelRow(
                                ref slot,
                                "AUTO ORDERS  " +
                                (AutomaticOrdersEnabled
                                    ? "ON"
                                    : "OFF") +
                                "  •  " +
                                CompactText(
                                    _autoOrdersBlockReason,
                                    72),
                                AutomaticOrdersEnabled &&
                                string.Equals(
                                    _autoOrdersBlockReason,
                                    "ORDER PLACED",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? TpLineColor
                                    : PanelSecondaryTextColor,
                                false,
                                contentWidth);
                
                            if (UseDailyPivots)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "PIVOT  " + DailyPivotPanelText(),
                                    PanelAccentColor,
                                    false,
                                    contentWidth);
                            }
                
                            bool tradingPermission =
                                HasTradingPermission();
                
                            AddPanelRow(
                                ref slot,
                                "PERMISSION  •  " +
                                (tradingPermission
                                    ? "TRADING ALLOWED"
                                    : "TRADING NOT GRANTED"),
                                tradingPermission
                                    ? TpLineColor
                                    : PanelWarningColor,
                                true,
                                contentWidth);
                
                            if (ShowSpreadDiagnostics)
                            {
                                double spreadPips =
                                    Math.Max(
                                        0,
                                        (Symbol.Ask - Symbol.Bid) /
                                        Math.Max(
                                            Symbol.PipSize,
                                            1e-9));
                
                                double spreadAtrRatio = 0;
                
                                if (_m5Frame != null &&
                                    _m5Frame.Atr > 0)
                                {
                                    spreadAtrRatio =
                                        (Symbol.Ask - Symbol.Bid) /
                                        _m5Frame.Atr;
                                }
                
                                AddPanelRow(
                                    ref slot,
                                    "SPREAD  " +
                                    spreadPips.ToString("F1") +
                                    " pips  •  ATR " +
                                    spreadAtrRatio.ToString("F3"),
                                    spreadPips > 0 &&
                                    UseSpreadFilter &&
                                    _m5Frame != null &&
                                    _m5Frame.Atr > 0 &&
                                    (Symbol.Ask - Symbol.Bid) /
                                    _m5Frame.Atr >
                                    MaximumSpreadAtr
                                        ? PanelWarningColor
                                        : PanelMutedTextColor,
                                    false,
                                    contentWidth);
                            }
                
                            if (ShowEngineStatus)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "ENGINE  " +
                                    _status,
                                    _status.IndexOf(
                                        "WAIT",
                                        StringComparison.OrdinalIgnoreCase) >= 0
                                        ? PanelWarningColor
                                        : PanelMutedTextColor,
                                    false,
                                    contentWidth);
                            }
                
                            if (_decision != null)
                            {
                                int direction =
                                    _decision.Direction;
                
                                string decisionState =
                                    direction == 1
                                        ? "BUY"
                                        : direction == -1
                                            ? "SELL"
                                            : "NEUTRAL";
                
                                AddPanelRow(
                                    ref slot,
                                    "DECISION  •  " +
                                    decisionState +
                                    "  •  " +
                                    (_decision.EntryAllowed
                                        ? "READY"
                                        : "WATCH / BLOCKED"),
                                    PanelDirectionColor(
                                        direction),
                                    true,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "CONF " +
                                    _decision.Confidence +
                                    "  •  EDGE " +
                                    _decision.Edge +
                                    "  •  SMART " +
                                    _decision.SmartQuality,
                                    PanelDirectionColor(
                                        direction),
                                    true,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "MTF " +
                                    _decision.TimeframeAgreement +
                                    "  •  EVID " +
                                    _decision.IndependentEvidence +
                                    "  •  STRUCT " +
                                    _decision.StructuralConfirmations,
                                    PanelSecondaryTextColor,
                                    false,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "REGIME " +
                                    _decision.Regime +
                                    "  •  Q" +
                                    _decision.RegimeQuality +
                                    "  •  RETEST " +
                                    _decision.RetestQuality +
                                    "  •  SHARE " +
                                    _decision.BuyShare +
                                    "/" +
                                    _decision.SellShare,
                                    PanelSecondaryTextColor,
                                    false,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "CONFLUENCE  " +
                                    ConfluenceText(
                                        _m5Frame),
                                    PanelAccentColor,
                                    false,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    _decision.TriggerReady
                                        ? "TRIGGER  CONFIRMED"
                                        : "TRIGGER  WAITING",
                                    _decision.TriggerReady
                                        ? TriggerLineColor
                                        : PanelWarningColor,
                                    true,
                                    contentWidth);
                
                                if (!string.IsNullOrWhiteSpace(
                                        _decision.BlockReason))
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "BLOCK  " +
                                        _decision.BlockReason,
                                        SlLineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (_prediction != null &&
                                    _prediction.Direction != 0 &&
                                    _prediction.Confidence >=
                                    Math.Max(
                                        MinimumEarlyConfidence,
                                        EarlySetupConfidence))
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "EARLY ANALYSIS  •  " +
                                        (_prediction.Direction == 1
                                            ? "BUY"
                                            : "SELL") +
                                        "  •  CONF " +
                                        _prediction.Confidence,
                                        PanelDirectionColor(
                                            _prediction.Direction),
                                        true,
                                        contentWidth);
                
                                    AddPanelRow(
                                        ref slot,
                                        "PREDICTION  " +
                                        ExecutionModeText(
                                            _prediction.Mode) +
                                        "  •  ENTRY " +
                                        Price(_prediction.Entry) +
                                        "  •  TRIGGER " +
                                        Price(_prediction.Trigger),
                                        PanelDirectionColor(
                                            _prediction.Direction),
                                        false,
                                        contentWidth);
                
                                    AddPanelRow(
                                        ref slot,
                                        "PRED TARGETS  " +
                                        Price(_prediction.Target1) +
                                        "  /  " +
                                        Price(_prediction.Target2) +
                                        "  /  " +
                                        Price(_prediction.Target3) +
                                        "  /  " +
                                        Price(_prediction.Target4),
                                        PanelSecondaryTextColor,
                                        false,
                                        contentWidth);
                                }
                            }
                
                            AddPanelRow(
                                ref slot,
                                "READINESS  " +
                                PredictionReadinessText(),
                                PredictionReadinessColor(),
                                true,
                                contentWidth);
                
                            if (_executionModel != null &&
                                _executionModel.Direction != 0)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "ENTRY MODEL  •  " +
                                    ExecutionModeText(
                                        _executionModel.Mode) +
                                    "  •  " +
                                    _executionModel.Source +
                                    "  •  Q" +
                                    _executionModel.Quality,
                                    PanelDirectionColor(
                                        _executionModel.Direction),
                                    true,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "IDEAL ENTRY  " +
                                    Price(
                                        _executionModel.IdealEntry) +
                                    "  •  ZONE " +
                                    Price(
                                        _executionModel.ZoneLow) +
                                    " → " +
                                    Price(
                                        _executionModel.ZoneHigh),
                                    PanelSecondaryTextColor,
                                    false,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "ENTRY TRIGGER  " +
                                    Price(
                                        _executionModel.Trigger) +
                                    "  •  INVALIDATION " +
                                    Price(
                                        _executionModel.Invalidation),
                                    _executionModel.Ready
                                        ? TriggerLineColor
                                        : PanelWarningColor,
                                    false,
                                    contentWidth);
                            }
                
                            if (_plan != null &&
                                ShowTradePlanPanel)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "TRADE PLAN  •  " +
                                    (_plan.Direction == 1
                                        ? "BUY ACTIVE"
                                        : "SELL ACTIVE"),
                                    PanelDirectionColor(
                                        _plan.Direction),
                                    true,
                                    contentWidth);
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowEntry)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "ENTRY  " +
                                        Price(_plan.Entry) +
                                        "  •  IDEAL " +
                                        Price(_plan.IdealEntry) +
                                        "  •  Q" +
                                        _plan.EntryQuality +
                                        "  •  " +
                                        _plan.EntrySource,
                                        EntryLineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowSL)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "STOP LOSS  " +
                                        Price(_plan.Stop) +
                                        "  •  " +
                                        _plan.StopSource +
                                        "  •  Q" +
                                        _plan.StopQuality +
                                        "  •  RISK " +
                                        (_plan.Risk /
                                         Math.Max(
                                             Symbol.PipSize,
                                             1e-9)).ToString("F1") +
                                        "p",
                                        SlLineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowTP1)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "TAKE PROFIT 1  " +
                                        Price(_plan.Tp1) +
                                        "  •  RR " +
                                        _plan.Tp1RR.ToString(
                                            "F2") +
                                        "  •  " +
                                        _plan.Tp1Source +
                                        "  •  Q" +
                                        _plan.Tp1Quality +
                                        (_tp1Hit != 0
                                            ? "  •  HIT"
                                            : ""),
                                        TpLineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowTP2 &&
                                    _plan.Tp2 > 0)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "TAKE PROFIT 2  " +
                                        Price(_plan.Tp2) +
                                        "  •  RR " +
                                        _plan.Tp2RR.ToString(
                                            "F2") +
                                        "  •  " +
                                        _plan.Tp2Source +
                                        "  •  Q" +
                                        _plan.Tp2Quality +
                                        (_tp2Hit != 0
                                            ? "  •  HIT"
                                            : ""),
                                        Tp2LineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowTP3 &&
                                    _plan.Tp3 > 0)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "TAKE PROFIT 3  " +
                                        Price(_plan.Tp3) +
                                        "  •  RR " +
                                        _plan.Tp3RR.ToString(
                                            "F2") +
                                        "  •  " +
                                        _plan.Tp3Source +
                                        "  •  Q" +
                                        _plan.Tp3Quality +
                                        (_tp3Hit != 0
                                            ? "  •  HIT"
                                            : ""),
                                        Tp3LineColor,
                                        true,
                                        contentWidth);
                                }
                
                                if (ShowLevelPricesInUnifiedPanel &&
                                    ShowTP4 &&
                                    _plan.Tp4 > 0)
                                {
                                    AddPanelRow(
                                        ref slot,
                                        "TAKE PROFIT 4  " +
                                        Price(_plan.Tp4) +
                                        "  •  RR " +
                                        _plan.Tp4RR.ToString(
                                            "F2") +
                                        "  •  " +
                                        _plan.Tp4Source +
                                        "  •  Q" +
                                        _plan.Tp4Quality +
                                        (_tp4Hit != 0
                                            ? "  •  HIT"
                                            : ""),
                                        Tp4LineColor,
                                        true,
                                        contentWidth);
                                }
                
                                AddPanelRow(
                                    ref slot,
                                    "REWARD MODEL  •  HTF TARGETS " +
                                    _plan.HtfTargetCount +
                                    "  •  MAX RR " +
                                    Math.Max(
                                        0,
                                        MaximumRewardRR).ToString("F2"),
                                    PanelAccentColor,
                                    false,
                                    contentWidth);
                
                                double liveRR =
                                    _plan.Risk > 0
                                        ? (_plan.Direction == 1
                                            ? _lastMarket - _plan.Entry
                                            : _plan.Entry - _lastMarket) /
                                          _plan.Risk
                                        : 0;
                
                                int exitPressure =
                                    CalculateSmartExitPressure(
                                        _lastMarket,
                                        liveRR);
                
                                AddPanelRow(
                                    ref slot,
                                    "LIVE  RR " +
                                    liveRR.ToString(
                                        "F2") +
                                    "  •  TP HIT " +
                                    _tp1Hit +
                                    "/" +
                                    _tp2Hit +
                                    "/" +
                                    _tp3Hit +
                                    "/" +
                                    _tp4Hit,
                                    liveRR >= 0
                                        ? TpLineColor
                                        : SlLineColor,
                                    true,
                                    contentWidth);
                
                                 Position managedPosition =
                                     GetManagedPosition();
                
                                 if (managedPosition != null)
                                 {
                                     AddPanelRow(
                                         ref slot,
                                         "POSITION  •  " +
                                         (managedPosition.TradeType ==
                                              TradeType.Buy
                                             ? "BUY"
                                             : "SELL") +
                                         "  #" +
                                         managedPosition.Id +
                                         "  •  VOL " +
                                         managedPosition.VolumeInUnits.ToString("F0") +
                                         "  •  P/L " +
                                         managedPosition.NetProfit.ToString("F2") +
                                         "  •  SL " +
                                         (managedPosition.StopLoss.HasValue
                                             ? Price(
                                                 managedPosition.StopLoss.Value)
                                             : "-") +
                                         "  •  TP " +
                                         (managedPosition.TakeProfit.HasValue
                                             ? Price(
                                                 managedPosition.TakeProfit.Value)
                                             : "-") +
                                         "  •  " +
                                         BrokerTargetStageText(
                                             managedPosition.TakeProfit.HasValue
                                                 ? managedPosition.TakeProfit.Value
                                                 : 0),
                                         managedPosition.NetProfit >= 0
                                             ? TpLineColor
                                             : SlLineColor,
                                         true,
                                         contentWidth);
                                 }
                
                                AddPanelRow(
                                    ref slot,
                                    "SMART EXIT  " +
                                    GetSmartExitMode() +
                                    "  •  PRESSURE " +
                                    exitPressure,
                                    exitPressure >=
                                        SmartExitPressureThreshold
                                        ? SlLineColor
                                        : exitPressure >=
                                          LiveReactionWatchThreshold
                                            ? PanelWarningColor
                                            : TpLineColor,
                                    true,
                                    contentWidth);
                            }
                
                            if (_reaction != null &&
                                _reaction.Direction != 0)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "LIVE REACTION  •  " +
                                    (_reaction.Direction == 1
                                        ? "BUY"
                                        : "SELL") +
                                    "  •  Q" +
                                    _reaction.Confidence +
                                    "  •  EVID " +
                                    _reaction.IndependentEvidence +
                                    (_reaction.EntryAllowed
                                        ? "  •  READY"
                                        : "  •  WATCH"),
                                    PanelDirectionColor(
                                        _reaction.Direction),
                                    true,
                                    contentWidth);
                            }
                
                            AddPanelRow(
                                ref slot,
                                "MTF ALIGNMENT",
                                PanelSectionColor,
                                true,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "M1   " +
                                FrameText(_m1Frame),
                                PanelDirectionColor(
                                    FrameDirection(_m1Frame)),
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "M5   " +
                                FrameText(_m5Frame),
                                PanelDirectionColor(
                                    FrameDirection(_m5Frame)),
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "M15  " +
                                FrameText(_m15Frame),
                                PanelDirectionColor(
                                    FrameDirection(_m15Frame)),
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "M30  " +
                                FrameText(_m30Frame),
                                PanelDirectionColor(
                                    FrameDirection(_m30Frame)),
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "H1   " +
                                FrameText(_h1Frame),
                                PanelDirectionColor(
                                    FrameDirection(_h1Frame)),
                                false,
                                contentWidth);
                
                            AddPanelRow(
                                ref slot,
                                "H4   " +
                                FrameText(_h4Frame),
                                PanelDirectionColor(
                                    FrameDirection(_h4Frame)),
                                false,
                                contentWidth);
                
                            if (SmartWeeklyContext)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "D1   " +
                                    FrameText(_d1Frame),
                                    PanelDirectionColor(
                                        FrameDirection(_d1Frame)),
                                    false,
                                    contentWidth);
                
                                AddPanelRow(
                                    ref slot,
                                    "W1   " +
                                    FrameText(_w1Frame),
                                    PanelDirectionColor(
                                        FrameDirection(_w1Frame)),
                                    false,
                                    contentWidth);
                            }
                
                            if (ShowOutcomeDiagnostics)
                            {
                                AddPanelRow(
                                    ref slot,
                                    "OUTCOME  W" +
                                    _wins +
                                    "  •  L" +
                                    _losses +
                                    "  •  CAL " +
                                    CalibrationText(),
                                    PanelAccentColor,
                                    false,
                                    contentWidth);
                            }
                
                            AddPanelRow(
                                ref slot,
                                "AUTO TRADING  •  " +
                                GetAutoTradingPanelState(),
                                GetAutoTradingPanelColor(),
                                true,
                                contentWidth);
                
                            PendingOrder managedPending =
                                GetManagedPendingOrder();
                
                            if (managedPending != null)
                            {
                                string pendingType =
                                    managedPending.OrderType ==
                                        PendingOrderType.Stop
                                        ? "STOP"
                                        : managedPending.OrderType ==
                                          PendingOrderType.Limit
                                            ? "LIMIT"
                                            : "PENDING";
                
                                AddPanelRow(
                                    ref slot,
                                    "AUTO ORDER  •  " +
                                    pendingType +
                                    "  •  ENTRY " +
                                    Price(
                                        managedPending.TargetPrice) +
                                    (managedPending.StopLoss.HasValue
                                        ? "  •  SL " +
                                          Price(
                                              managedPending.StopLoss.Value)
                                        : "") +
                                    (managedPending.TakeProfit.HasValue
                                        ? "  •  TP " +
                                          Price(
                                              managedPending.TakeProfit.Value)
                                        : ""),
                                    TriggerLineColor,
                                    true,
                                    contentWidth);
                            }
                
                            AddPanelRow(
                                ref slot,
                                "AUTO CONFIG  •  " +
                                (ConfirmedSignalsOnly
                                    ? "CONFIRMED ONLY"
                                    : "PLAN ELIGIBLE") +
                                "  •  " +
                                (SizingMode ==
                                    SizingMode.FixedLots
                                    ? "FIXED " +
                                      FixedLots.ToString("F2") +
                                      " LOT"
                                    : "RISK " +
                                      RiskPercentEquity.ToString("F2") +
                                      "%"),
                                PanelSecondaryTextColor,
                                false,
                                contentWidth);
                
                            if (AutoTradingEnabled &&
                                (AutoBrokerProtection ||
                                 AutoProtectBrokerPositions))
                            {
                                AddPanelRow(
                                    ref slot,
                                    "AUTO PROTECTION  •  " +
                                    GetAutoProtectionPanelState(),
                                    TpLineColor,
                                    false,
                                    contentWidth);
                            }
                
                            if (!string.IsNullOrWhiteSpace(
                                    _lastAlertMessage))
                            {
                                AddPanelRow(
                                    ref slot,
                                    "LAST ALERT  •  " +
                                    CompactText(
                                        _lastAlertMessage,
                                        120),
                                    _lastAlertCritical
                                        ? (_lastAlertDirection == 1
                                            ? BuyArrowColor
                                            : _lastAlertDirection == -1
                                                ? SellArrowColor
                                                : PanelWarningColor)
                                        : PanelSecondaryTextColor,
                                    false,
                                    contentWidth);
                            }
                
                            while (slot < _panelRows.Count)
                            {
                                _panelRows[slot].IsVisible =
                                    false;
                                slot++;
                            }
                        }
        
        private void SetPanelRow(
                            int index,
                            string text,
                            Color color,
                            bool bold,
                            int width)
                        {
                            if (index < 0 ||
                                index >= _panelRows.Count)
                                return;
                
                            TextBlock row =
                                _panelRows[index];
                
                            row.Text =
                                text ?? "";
                
                            row.Width =
                                Math.Max(
                                    190,
                                    width);
                
                            row.ForegroundColor =
                                color;
                
                            row.TextAlignment =
                                TextAlignment.Left;
                
                            row.TextWrapping =
                                TextWrapping.Wrap;
                
                            row.TextTrimming =
                                TextTrimming.None;
                
                            row.LineHeight =
                                Math.Max(
                                    14,
                                    PanelFontSize + 3);
                
                            row.FontWeight =
                                bold || PanelBold
                                    ? FontWeight.Bold
                                    : FontWeight.Normal;
                
                            row.IsVisible =
                                !string.IsNullOrWhiteSpace(
                                    text);
                        }
        
        private void AddPanelRow(
                            ref int slot,
                            string text,
                            Color color,
                            bool bold,
                            int width)
                        {
                            if (slot >= _panelRows.Count)
                                return;
                
                            SetPanelRow(
                                slot,
                                text,
                                color,
                                bold,
                                width);
                
                            slot++;
                        }
    }
}
