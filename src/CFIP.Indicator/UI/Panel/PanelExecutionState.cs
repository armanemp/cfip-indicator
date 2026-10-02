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
            EnsurePanelExecutionProtectionStateCache();

            string state =
                FormatExecutionPanelState(
                    _panelAutoTradingState,
                    _autoExecutionBlockReason,
                    "READY TO SUBMIT");

            return
                state +
                "  •  " +
                CbotConnectionPanelText();
        }
        
        private Color GetAutoTradingPanelColor()
        {
            EnsurePanelExecutionProtectionStateCache();

            if (AutoTradingEnabled &&
                !CbotCanMarketExecute())
                return PanelWarningColor;

            return ExecutionPanelStateColor(
                _panelAutoTradingState);
        }
        
        private string GetAutoOrdersPanelState()
        {
            EnsurePanelExecutionProtectionStateCache();

            string state =
                FormatExecutionPanelState(
                    _panelAutoOrdersState,
                    _autoOrdersBlockReason,
                    "READY TO PLACE");

            return
                state +
                "  •  " +
                CbotConnectionPanelText();
        }
        
        private Color GetAutoOrdersPanelColor()
        {
            EnsurePanelExecutionProtectionStateCache();

            if (AutomaticOrdersEnabled &&
                !CbotCanPendingExecute())
                return PanelWarningColor;

            return ExecutionPanelStateColor(
                _panelAutoOrdersState);
        }
        
        private string GetAutoProtectionPanelState()
        {
            EnsurePanelExecutionProtectionStateCache();

            string state =
                _panelProtectionState ==
                    ProtectionPanelStateKind.Off
                    ? "OFF"
                    : _panelProtectionState ==
                        ProtectionPanelStateKind.NoLivePosition
                        ? "READY • NO LIVE POSITION"
                        : _panelProtectionState ==
                            ProtectionPanelStateKind.Protected
                            ? "PROTECTED"
                            : "RECOVERY REQUIRED";

            RefreshCbotExecutionStateIfDue();

            return
                state +
                " • " +
                CbotConnectionPanelText();
        }
        
        private Color GetAutoProtectionPanelColor()
        {
            EnsurePanelExecutionProtectionStateCache();

            if ((AutoBrokerProtection ||
                 AutoProtectBrokerPositions) &&
                !CbotCanManage())
                return PanelWarningColor;

            return
                _panelProtectionState ==
                    ProtectionPanelStateKind.Protected
                    ? TpLineColor
                    : _panelProtectionState ==
                        ProtectionPanelStateKind.RecoveryRequired
                        ? SlLineColor
                        : _panelProtectionState ==
                            ProtectionPanelStateKind.Off
                            ? PanelMutedTextColor
                            : PanelAccentColor;
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
                    model.Direction == 1
                        ? "BUY WAIT • ENTRY MUST REACH TRIGGER ABOVE"
                        : "SELL WAIT • ENTRY MUST REACH TRIGGER BELOW";

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
