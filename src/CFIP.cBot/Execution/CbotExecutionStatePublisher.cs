using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotExecutionStatePublisher
    {
        private const int MaxPublishIntervalMilliseconds = 1000;

        private DateTime _lastPublishUtc = DateTime.MinValue;

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
                        !string.Equals(
                            position.Label,
                            executionLabel,
                            StringComparison.Ordinal))
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
                        !string.Equals(
                            order.Label,
                            pendingLabel,
                            StringComparison.Ordinal))
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