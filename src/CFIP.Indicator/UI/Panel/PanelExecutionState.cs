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
        private string GetAutoTradingPanelState()
        {
            Position managedPosition =
                GetManagedPosition();

            string runtimeState =
                _autoTradingState ?? "";

            string runtimeReason =
                _autoExecutionBlockReason ?? "";

            bool recoveryRequired =
                IsPanelExecutionRecoveryRequired(
                    runtimeState,
                    runtimeReason);

            bool readyToSubmit =
                string.Equals(
                    runtimeReason,
                    "READY TO SUBMIT",
                    StringComparison.OrdinalIgnoreCase);

            bool blocked =
                IsPanelExecutionBlockedReason(
                    runtimeReason,
                    "AWAITING EXECUTION");

            ExecutionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    AutoTradingEnabled,
                    managedPosition != null,
                    readyToSubmit,
                    blocked,
                    recoveryRequired);

            return FormatExecutionPanelState(
                state,
                runtimeReason,
                "READY TO SUBMIT");
        }

        private Color GetAutoTradingPanelColor()
        {
            Position managedPosition =
                GetManagedPosition();

            ExecutionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    AutoTradingEnabled,
                    managedPosition != null,
                    string.Equals(
                        _autoExecutionBlockReason,
                        "READY TO SUBMIT",
                        StringComparison.OrdinalIgnoreCase),
                    IsPanelExecutionBlockedReason(
                        _autoExecutionBlockReason,
                        "AWAITING EXECUTION"),
                    IsPanelExecutionRecoveryRequired(
                        _autoTradingState,
                        _autoExecutionBlockReason));

            return ExecutionPanelStateColor(state);
        }

        private string GetAutoOrdersPanelState()
        {
            PendingOrder managedPending =
                GetManagedPendingOrder();

            string reason =
                _autoOrdersBlockReason ?? "";

            bool recoveryRequired =
                IsPanelExecutionRecoveryRequired(
                    _autoTradingState,
                    reason);

            bool readyToPlace =
                string.Equals(
                    reason,
                    "READY TO PLACE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reason,
                    "READY TO SUBMIT",
                    StringComparison.OrdinalIgnoreCase);

            bool blocked =
                IsPanelExecutionBlockedReason(
                    reason,
                    "AWAITING ORDER SETUP");

            ExecutionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    AutomaticOrdersEnabled,
                    managedPending != null,
                    readyToPlace,
                    blocked,
                    recoveryRequired);

            return FormatExecutionPanelState(
                state,
                reason,
                "READY TO PLACE");
        }

        private Color GetAutoOrdersPanelColor()
        {
            PendingOrder managedPending =
                GetManagedPendingOrder();

            ExecutionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    AutomaticOrdersEnabled,
                    managedPending != null ||
                    string.Equals(
                        _autoOrdersBlockReason,
                        "ORDER PLACED",
                        StringComparison.OrdinalIgnoreCase),
                    string.Equals(
                        _autoOrdersBlockReason,
                        "READY TO PLACE",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        _autoOrdersBlockReason,
                        "READY TO SUBMIT",
                        StringComparison.OrdinalIgnoreCase),
                    IsPanelExecutionBlockedReason(
                        _autoOrdersBlockReason,
                        "AWAITING ORDER SETUP"),
                    IsPanelExecutionRecoveryRequired(
                        _autoTradingState,
                        _autoOrdersBlockReason));

            return ExecutionPanelStateColor(state);
        }

        private string GetAutoProtectionPanelState()
        {
            Position managedPosition =
                GetManagedPosition();

            bool livePosition =
                managedPosition != null;

            bool brokerStopValid =
                livePosition &&
                managedPosition.StopLoss.HasValue &&
                IsExistingManagedStopHealthy(
                    managedPosition.TradeType == TradeType.Buy
                        ? 1
                        : -1,
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
                    managedPosition.TradeType == TradeType.Buy
                        ? 1
                        : -1,
                    managedPosition.EntryPrice,
                    managedPosition.TakeProfit.Value);

            bool targetRequired =
                livePosition &&
                SyncBrokerTakeProfit &&
                !serverLadderActive;

            bool monitoringConfigured =
                AutoBrokerProtection ||
                AutoProtectBrokerPositions;

            ProtectionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    monitoringConfigured,
                    livePosition,
                    brokerStopValid,
                    targetRequired,
                    brokerTargetValid,
                    serverLadderActive,
                    _brokerProtectionRecoveryRequired);

            return
                state == ProtectionPanelStateKind.Off
                    ? "OFF"
                    : state == ProtectionPanelStateKind.NoLivePosition
                        ? "READY • NO LIVE POSITION"
                        : state == ProtectionPanelStateKind.Protected
                            ? "PROTECTED"
                            : "RECOVERY REQUIRED";
        }

        private Color GetAutoProtectionPanelColor()
        {
            Position managedPosition =
                GetManagedPosition();

            bool livePosition =
                managedPosition != null;

            bool brokerStopValid =
                livePosition &&
                managedPosition.StopLoss.HasValue &&
                IsExistingManagedStopHealthy(
                    managedPosition.TradeType == TradeType.Buy
                        ? 1
                        : -1,
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
                    managedPosition.TradeType == TradeType.Buy
                        ? 1
                        : -1,
                    managedPosition.EntryPrice,
                    managedPosition.TakeProfit.Value);

            ProtectionPanelStateKind state =
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    AutoBrokerProtection ||
                    AutoProtectBrokerPositions,
                    livePosition,
                    brokerStopValid,
                    livePosition &&
                    SyncBrokerTakeProfit &&
                    !serverLadderActive,
                    brokerTargetValid,
                    serverLadderActive,
                    _brokerProtectionRecoveryRequired);

            return state == ProtectionPanelStateKind.Protected
                ? TpLineColor
                : state == ProtectionPanelStateKind.RecoveryRequired
                    ? SlLineColor
                    : state == ProtectionPanelStateKind.Off
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