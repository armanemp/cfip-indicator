using System;
using cAlgo.API;
using CFIP.Contracts;
using CFIP.cBot.Recovery;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotExecutionStatePublisher
    {
        private const int MaxPublishIntervalMilliseconds = 1000;

        private DateTime _lastPublishUtc = DateTime.MinValue;

        public void PublishPresence(
            Robot robot,
            DateTime nowUtc,
            string state,
            string boundIndicatorInstanceId)
        {
            if (robot == null)
                return;

            try
            {
                CbotPresenceSnapshot snapshot =
                    new CbotPresenceSnapshot(
                        ContractVersion.Current,
                        (robot.GetType().Name ?? string.Empty) + "|" +
                        (robot.SymbolName ?? string.Empty),
                        robot.GetType().Name ?? string.Empty,
                        CbotIdentity.DisplayName,
                        robot.SymbolName ?? string.Empty,
                        nowUtc,
                        string.IsNullOrWhiteSpace(state)
                            ? "UNKNOWN"
                            : state,
                        !robot.Account.IsLive,
                        boundIndicatorInstanceId ?? string.Empty);

                robot.LocalStorage.SetString(
                    CbotExecutionStateBusKey.ForSymbol(
                        robot.SymbolName ?? string.Empty),
                    CbotExecutionStateCodec.SerializePresence(
                        snapshot),
                    LocalStorageScope.Device);

                robot.LocalStorage.Flush(
                    LocalStorageScope.Device);
            }
            catch (Exception ex)
            {
                robot.Print(
                    "CFIP CBOT PRESENCE PUBLISH FAILED | {0}",
                    ex.Message);
            }
        }

        public void Publish(
            Robot robot,
            string indicatorInstanceId,
            DateTime nowUtc,
            long revision,
            string runtimeState,
            string reason,
            bool marketExecutionEnabled,
            bool pendingStopExecutionEnabled,
            bool pendingLimitExecutionEnabled,
            bool aggressiveExecutionEnabled,
            bool managementExecutionEnabled,
            string executionLabel,
            string executionScenarioId,
            SignalEnvelope envelope,
            CbotBrokerReconciliationResult reconciliation,
            CbotIndicatorExecutionSettings executionSettings,
            bool force)
        {
            if (robot == null ||
                string.IsNullOrWhiteSpace(indicatorInstanceId))
                return;

            executionLabel =
                executionLabel ?? string.Empty;

            int managedPositions = 0;
            int managedPendingOrders = 0;
            long? positionId = null;
            long? pendingOrderId = null;
            double? entry = null;
            double? stop = null;
            double? target = null;

            if (!string.IsNullOrWhiteSpace(executionLabel))
            {
                foreach (Position position in robot.Positions)
                {
                    if (position == null ||
                        !string.Equals(
                            position.SymbolName,
                            robot.SymbolName,
                            StringComparison.Ordinal) ||
                        !CbotManagedObjectIdentityRule.MatchesManagedLabel(
                            position.Label,
                            executionLabel))
                        continue;

                    managedPositions++;

                    if (!positionId.HasValue)
                    {
                        positionId = position.Id;
                        entry = position.EntryPrice;
                        stop = position.StopLoss;
                        target = position.TakeProfit;
                    }
                }

                string pendingLabel =
                    executionLabel + "-PENDING";

                foreach (PendingOrder order in robot.PendingOrders)
                {
                    if (order == null ||
                        !string.Equals(
                            order.SymbolName,
                            robot.SymbolName,
                            StringComparison.Ordinal) ||
                        !CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(
                            order.Label,
                            executionLabel))
                        continue;

                    managedPendingOrders++;

                    if (!pendingOrderId.HasValue)
                        pendingOrderId = order.Id;
                }
            }

            long signalRevision =
                envelope == null ||
                envelope.Identity == null
                    ? 0
                    : envelope.Identity.Revision;

            string scenarioId =
                string.IsNullOrWhiteSpace(executionScenarioId)
                    ? envelope == null ||
                      envelope.Identity == null
                        ? string.Empty
                        : envelope.Identity.ScenarioId ?? string.Empty
                    : executionScenarioId;

            CbotExecutionStateSnapshot snapshot =
                new CbotExecutionStateSnapshot(
                    ContractVersion.Current,
                    indicatorInstanceId,
                    robot.SymbolName ?? string.Empty,
                    nowUtc,
                    revision,
                    runtimeState ?? "UNKNOWN",
                    reason ?? string.Empty,
                    !robot.Account.IsLive,
                    marketExecutionEnabled,
                    pendingStopExecutionEnabled,
                    pendingLimitExecutionEnabled,
                    aggressiveExecutionEnabled,
                    managementExecutionEnabled,
                    managedPositions,
                    managedPendingOrders,
                    positionId,
                    pendingOrderId,
                    entry,
                    stop,
                    target,
                    executionLabel,
                    scenarioId,
                    signalRevision);

            bool cbotMarketArmed =
                marketExecutionEnabled ||
                aggressiveExecutionEnabled;

            bool cbotOrdersArmed =
                pendingStopExecutionEnabled ||
                pendingLimitExecutionEnabled;

            bool lifecycleAllowsExecution =
                reconciliation == null ||
                string.Equals(
                    reconciliation.LifecycleState,
                    "READY",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reconciliation.LifecycleState,
                    "ACTIVE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    reconciliation.LifecycleState,
                    "PENDING",
                    StringComparison.OrdinalIgnoreCase);

            snapshot = snapshot with
            {
                IndicatorAutoTradingEnabled =
                    executionSettings != null &&
                    executionSettings.EnableAutoTrading,
                IndicatorAutomaticOrdersEnabled =
                    executionSettings != null &&
                    executionSettings.EnableAutomaticOrders,
                EffectiveAutoTradingEnabled =
                    executionSettings != null &&
                    executionSettings.EnableAutoTrading &&
                    cbotMarketArmed &&
                    lifecycleAllowsExecution &&
                    !string.Equals(
                        runtimeState,
                        "BLOCKED",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        runtimeState,
                        "DISARMED",
                        StringComparison.OrdinalIgnoreCase) &&
                    !(reconciliation != null &&
                      reconciliation.RecoveryRequired),
                EffectiveAutomaticOrdersEnabled =
                    executionSettings != null &&
                    executionSettings.EnableAutoTrading &&
                    executionSettings.EnableAutomaticOrders &&
                    cbotOrdersArmed &&
                    lifecycleAllowsExecution &&
                    !string.Equals(
                        runtimeState,
                        "BLOCKED",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        runtimeState,
                        "DISARMED",
                        StringComparison.OrdinalIgnoreCase) &&
                    !(reconciliation != null &&
                      reconciliation.RecoveryRequired),
                LifecycleState =
                    reconciliation == null
                        ? "UNKNOWN"
                        : reconciliation.LifecycleState,
                ProtectionState =
                    reconciliation == null
                        ? "UNKNOWN"
                        : reconciliation.ProtectionState,
                RecoveryRequired =
                    reconciliation != null &&
                    reconciliation.RecoveryRequired,
                RecoveryReason =
                    reconciliation == null
                        ? string.Empty
                        : reconciliation.Reason
            };

            if (!force &&
                _lastPublishUtc != DateTime.MinValue &&
                (nowUtc - _lastPublishUtc).TotalMilliseconds <
                    MaxPublishIntervalMilliseconds)
                return;

            string payload =
                CbotExecutionStateCodec.Serialize(snapshot);

            try
            {
                robot.LocalStorage.SetString(
                    CbotExecutionStateBusKey.ForIndicatorInstance(
                        indicatorInstanceId),
                    payload,
                    LocalStorageScope.Device);

                robot.LocalStorage.Flush(
                    LocalStorageScope.Device);

                _lastPublishUtc = nowUtc;
            }
            catch (Exception ex)
            {
                robot.Print(
                    "CFIP CBOT STATE PUBLISH FAILED | {0}",
                    ex.Message);
            }
        }
    }
}