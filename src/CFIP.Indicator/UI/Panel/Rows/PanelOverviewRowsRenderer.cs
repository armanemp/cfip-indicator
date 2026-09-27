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
        private void RenderPanelOverviewRows(
            ref int slot,
            int contentWidth)
        {
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
                                    
            
        }
    }
}
