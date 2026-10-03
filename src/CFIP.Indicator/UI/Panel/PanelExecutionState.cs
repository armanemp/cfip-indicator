// ============================================================================
// CFIP Indicator — PanelExecutionState.cs
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
        private bool _panelExecutionProtectionStateCached;
        private ExecutionPanelStateKind _panelAutoTradingState;
        private ExecutionPanelStateKind _panelAutoOrdersState;
        private ProtectionPanelStateKind _panelProtectionState;

        private void InvalidatePanelExecutionProtectionStateCache()
        {
            _panelExecutionProtectionStateCached = false;
        }

        private void EnsurePanelExecutionProtectionStateCache()
        {
            RefreshCbotExecutionStateIfDue();

            if (_panelExecutionProtectionStateCached)
                return;

            Position managedPosition =
                GetManagedPosition();

            PendingOrder managedPending =
                GetManagedPendingOrder();

            string executionReason =
                _autoExecutionBlockReason ?? "";

            string ordersReason =
                _autoOrdersBlockReason ?? "";

            _panelAutoTradingState =
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    AutoTradingEnabled,
                    managedPosition != null,
                    string.Equals(
                        executionReason,
                        "READY TO SUBMIT",
                        StringComparison.OrdinalIgnoreCase),
                    IsPanelExecutionBlockedReason(
                        executionReason,
                        "AWAITING EXECUTION"),
                    IsPanelExecutionRecoveryRequired(
                        _autoTradingState,
                        executionReason));

            _panelAutoOrdersState =
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    AutomaticOrdersEnabled,
                    managedPending != null ||
                    string.Equals(
                        ordersReason,
                        "ORDER PLACED",
                        StringComparison.OrdinalIgnoreCase),
                    string.Equals(
                        ordersReason,
                        "READY TO PLACE",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        ordersReason,
                        "READY TO SUBMIT",
                        StringComparison.OrdinalIgnoreCase),
                    IsPanelExecutionBlockedReason(
                        ordersReason,
                        "AWAITING ORDER SETUP"),
                    IsPanelExecutionRecoveryRequired(
                        _autoTradingState,
                        ordersReason));

            bool livePosition =
                managedPosition != null;

            int protectionDirection =
                livePosition &&
                managedPosition.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            bool brokerStopValid =
                livePosition &&
                managedPosition.StopLoss.HasValue &&
                IsExistingManagedStopHealthy(
                    protectionDirection,
                    managedPosition.EntryPrice,
                    managedPosition.StopLoss.Value);

            bool serverLadderActive =
                livePosition &&
                _serverSideTakeProfitLadderActive;

            bool brokerTargetValid =
                livePosition &&
                managedPosition.TakeProfit.HasValue &&
                IsFinitePositive(
                    managedPosition.TakeProfit.Value) &&
                IsValidTarget(
                    protectionDirection,
                    managedPosition.EntryPrice,
                    managedPosition.TakeProfit.Value);

            bool targetRequired =
                livePosition &&
                SyncBrokerTakeProfit &&
                !serverLadderActive;

            _panelProtectionState =
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    AutoBrokerProtection ||
                    AutoProtectBrokerPositions,
                    livePosition,
                    brokerStopValid,
                    targetRequired,
                    brokerTargetValid,
                    serverLadderActive,
                    _brokerProtectionRecoveryRequired);

            _panelExecutionProtectionStateCached = true;
        }

        private string GetAutoTradingPanelState()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return CbotConnectionPanelText();

            if (_cBotExecutionState.RecoveryRequired)
                return
                    "CBOT RECOVERY REQUIRED • " +
                    CbotReasonText();

            if (_cBotExecutionState.EffectiveAutoTradingEnabled)
                return
                    "CBOT AUTO TRADE ON • " +
                    CbotMarketModeText();

            if (!_cBotExecutionState.IndicatorAutoTradingEnabled)
                return "CBOT CONNECTED • AUTO TRADE OFF • INDICATOR SETTING OFF";

            if (!_cBotExecutionState.MarketExecutionEnabled &&
                !_cBotExecutionState.AggressiveExecutionEnabled)
                return "CBOT CONNECTED • AUTO TRADE OFF • MARKET DISARMED";

            return
                "CBOT CONNECTED • AUTO TRADE BLOCKED • " +
                CbotReasonText();
        }

        private Color GetAutoTradingPanelColor()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return PanelWarningColor;

            if (_cBotExecutionState.RecoveryRequired)
                return PanelWarningColor;

            return
                _cBotExecutionState.EffectiveAutoTradingEnabled
                    ? TpLineColor
                    : PanelMutedTextColor;
        }
        
        private string GetAutoOrdersPanelState()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return CbotConnectionPanelText();

            if (_cBotExecutionState.RecoveryRequired)
                return
                    "CBOT RECOVERY REQUIRED • " +
                    CbotReasonText();

            if (_cBotExecutionState.EffectiveAutomaticOrdersEnabled)
                return
                    "CBOT AUTO ORDERS ON • " +
                    CbotPendingModeText();

            if (!_cBotExecutionState.IndicatorAutomaticOrdersEnabled)
                return "CBOT CONNECTED • AUTO ORDERS OFF • INDICATOR SETTING OFF";

            if (!_cBotExecutionState.IndicatorAutoTradingEnabled)
                return "CBOT CONNECTED • AUTO ORDERS BLOCKED • AUTO TRADE OFF";

            if (!_cBotExecutionState.PendingStopExecutionEnabled &&
                !_cBotExecutionState.PendingLimitExecutionEnabled)
                return "CBOT CONNECTED • AUTO ORDERS OFF • PENDING DISARMED";

            return
                "CBOT CONNECTED • AUTO ORDERS BLOCKED • " +
                CbotReasonText();
        }

        private Color GetAutoOrdersPanelColor()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return PanelWarningColor;

            if (_cBotExecutionState.RecoveryRequired)
                return PanelWarningColor;

            return
                _cBotExecutionState.EffectiveAutomaticOrdersEnabled
                    ? TriggerLineColor
                    : PanelMutedTextColor;
        }
        
        private string GetAutoProtectionPanelState()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return CbotConnectionPanelText();

            string runtimeState =
                string.IsNullOrWhiteSpace(
                    _cBotExecutionState.RuntimeState)
                    ? "UNKNOWN"
                    : _cBotExecutionState.RuntimeState.Trim();

            string protection =
                string.IsNullOrWhiteSpace(
                    _cBotExecutionState.ProtectionState)
                    ? "UNKNOWN"
                    : _cBotExecutionState.ProtectionState.Trim();

            return
                "CBOT " +
                runtimeState +
                " • PROTECTION " +
                protection +
                (_cBotExecutionState.RecoveryRequired
                    ? " • RECOVERY REQUIRED"
                    : "");
        }

        private Color GetAutoProtectionPanelColor()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return PanelWarningColor;

            if (_cBotExecutionState.RecoveryRequired)
                return PanelWarningColor;

            return CbotCanManage()
                ? TpLineColor
                : PanelMutedTextColor;
        }

        private string CbotMarketModeText()
        {
            string mode = "";

            if (_cBotExecutionState != null &&
                _cBotExecutionState.MarketExecutionEnabled)
                mode = "MKT";

            if (_cBotExecutionState != null &&
                _cBotExecutionState.AggressiveExecutionEnabled)
                mode += string.IsNullOrWhiteSpace(mode) ? "AGG" : "/AGG";

            return string.IsNullOrWhiteSpace(mode)
                ? "NONE"
                : mode;
        }

        private string CbotPendingModeText()
        {
            string mode = "";

            if (_cBotExecutionState != null &&
                _cBotExecutionState.PendingStopExecutionEnabled)
                mode = "STOP";

            if (_cBotExecutionState != null &&
                _cBotExecutionState.PendingLimitExecutionEnabled)
                mode += string.IsNullOrWhiteSpace(mode) ? "LIMIT" : "/LIMIT";

            return string.IsNullOrWhiteSpace(mode)
                ? "NONE"
                : mode;
        }

        private string CbotReasonText()
        {
            if (_cBotExecutionState == null ||
                string.IsNullOrWhiteSpace(
                    _cBotExecutionState.Reason))
                return "NO REASON";

            return CompactText(
                _cBotExecutionState.Reason,
                70);
        }
        
        private string GetExecutionRelationText(
            ExecutionModel model,
            double entry)
        {
            if (model == null)
                return "NONE";

            if (model.Mode ==
                ExecutionMode.WaitingForTrigger)
                return
                    DirectionText(model.Direction) +
                    " WAIT • ENTRY MUST REACH TRIGGER " +
                    (model.Direction == 1
                        ? "ABOVE"
                        : model.Direction == -1
                            ? "BELOW"
                            : "UNKNOWN");

            if (!IsFinitePositive(entry))
                return "NOT EXECUTABLE";

            if (model.Mode ==
                ExecutionMode.BreakoutMarket)
            {
                double tolerance =
                    Math.Max(
                        Symbol.TickSize * 2,
                        Math.Max(
                            Symbol.PipSize * 0.5,
                            (Symbol.Ask - Symbol.Bid) * 2));

                bool acceptable =
                    model.Direction == 1
                        ? entry >= model.Trigger - tolerance
                        : entry <= model.Trigger + tolerance;

                if (!acceptable)
                    return "BREAKOUT NOT CONFIRMED";

                return IsTriggerReached(
                    model.Direction,
                    entry,
                    model.Trigger)
                    ? "BREAKOUT CONFIRMED"
                    : "BREAKOUT • SLIPPAGE ACCEPTED";
            }

            if (model.Mode ==
                ExecutionMode.RetestMarket)
                return "RETEST INSIDE ZONE";

            return ExecutionModeText(model.Mode);
        }

        private bool IsPanelExecutionRecoveryRequired(
            string runtimeState,
            string reason)
        {
            if (_brokerProtectionRecoveryRequired ||
                _lifecycleState == LifecycleState.RecoveryRequired)
                return true;

            return
                string.Equals(
                    runtimeState,
                    "ERROR",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    runtimeState,
                    "ENTRY_BLOCKED",
                    StringComparison.OrdinalIgnoreCase) ||
                ContainsRecoveryToken(reason);
        }

        private bool ContainsRecoveryToken(
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            return
                reason.IndexOf(
                    "RECOVERY",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                reason.IndexOf(
                    "INITIALIZATION FAULT",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                reason.IndexOf(
                    "RUNTIME FAULT",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                reason.IndexOf(
                    "PROTECTION",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsPanelExecutionBlockedReason(
            string reason,
            string waitingReason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            if (string.Equals(
                    reason,
                    "NOT EVALUATED",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    "DISABLED",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    waitingReason,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    "READY TO SUBMIT",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    "READY TO PLACE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    "ORDER PLACED",
                    StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private string FormatExecutionPanelState(
            ExecutionPanelStateKind state,
            string reason,
            string readyText)
        {
            switch (state)
            {
                case ExecutionPanelStateKind.Disabled:
                    return "OFF";

                case ExecutionPanelStateKind.RecoveryRequired:
                    return
                        "RECOVERY REQUIRED • " +
                        CompactText(
                            reason,
                            62);

                case ExecutionPanelStateKind.Active:
                    return "ACTIVE";

                case ExecutionPanelStateKind.Ready:
                    return readyText;

                case ExecutionPanelStateKind.Blocked:
                    return
                        "BLOCKED • " +
                        CompactText(
                            reason,
                            62);

                default:
                    return "ARMED";
            }
        }

        private Color ExecutionPanelStateColor(
            ExecutionPanelStateKind state)
        {
            switch (state)
            {
                case ExecutionPanelStateKind.Disabled:
                    return PanelMutedTextColor;

                case ExecutionPanelStateKind.RecoveryRequired:
                    return SlLineColor;

                case ExecutionPanelStateKind.Active:
                case ExecutionPanelStateKind.Ready:
                    return TpLineColor;

                case ExecutionPanelStateKind.Blocked:
                    return PanelWarningColor;

                default:
                    return PanelAccentColor;
            }
        }
    }
}
