using System;
using cAlgo.API;
using CFIP.cBot.Execution;

namespace CFIP.cBot.Recovery
{
    internal sealed class CbotBrokerReconciliationResult
    {
        public string LifecycleState { get; }
        public string ProtectionState { get; }
        public bool RecoveryRequired { get; }
        public string Reason { get; }
        public string ExecutionLabel { get; }
        public int ManagedPositions { get; }
        public int ManagedPendingOrders { get; }
        public long? BrokerPositionId { get; }
        public long? BrokerPendingOrderId { get; }

        public CbotBrokerReconciliationResult(
            string lifecycleState,
            string protectionState,
            bool recoveryRequired,
            string reason,
            string executionLabel,
            int managedPositions,
            int managedPendingOrders,
            long? brokerPositionId,
            long? brokerPendingOrderId)
        {
            LifecycleState = string.IsNullOrWhiteSpace(lifecycleState)
                ? "UNKNOWN"
                : lifecycleState;
            ProtectionState = string.IsNullOrWhiteSpace(protectionState)
                ? "UNKNOWN"
                : protectionState;
            RecoveryRequired = recoveryRequired;
            Reason = reason ?? string.Empty;
            ExecutionLabel = executionLabel ?? string.Empty;
            ManagedPositions = Math.Max(0, managedPositions);
            ManagedPendingOrders = Math.Max(0, managedPendingOrders);
            BrokerPositionId = brokerPositionId;
            BrokerPendingOrderId = brokerPendingOrderId;
        }
    }

    internal sealed class CbotBrokerReconciliation
    {
        private const string InstanceMarker = "|CFIP-I:";

        public CbotBrokerReconciliationResult Evaluate(
            Robot robot,
            string indicatorInstanceId,
            string preferredExecutionLabel)
        {
            if (robot == null)
                return Result(
                    "BLOCKED",
                    "UNKNOWN",
                    true,
                    "CBOT HOST UNAVAILABLE",
                    string.Empty,
                    0,
                    0,
                    null,
                    null);

            if (string.IsNullOrWhiteSpace(indicatorInstanceId))
                return Result(
                    "BLOCKED",
                    "UNKNOWN",
                    true,
                    "INDICATOR INSTANCE ID UNAVAILABLE",
                    string.Empty,
                    0,
                    0,
                    null,
                    null);

            string preferred =
                preferredExecutionLabel == null
                    ? string.Empty
                    : preferredExecutionLabel.Trim();

            string scopedSuffix =
                InstanceMarker +
                indicatorInstanceId.Trim();

            Position managedPosition = null;
            PendingOrder managedPending = null;
            string resolvedLabel = preferred;
            int managedPositions = 0;
            int managedPendingOrders = 0;

            foreach (Position position in robot.Positions)
            {
                if (!MatchesScope(
                        position == null
                            ? string.Empty
                            : position.SymbolName,
                        robot.SymbolName) ||
                    !MatchesLabel(
                        position == null
                            ? string.Empty
                            : position.Label,
                        preferred,
                        scopedSuffix))
                    continue;

                managedPositions++;
                if (managedPosition == null)
                    managedPosition = position;

                if (position != null &&
                    (!string.Equals(
                        position.Label,
                        preferred,
                        StringComparison.Ordinal) ||
                     string.IsNullOrWhiteSpace(resolvedLabel)))
                    resolvedLabel =
                        position.Label ?? string.Empty;
            }

            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (!MatchesScope(
                        order == null
                            ? string.Empty
                            : order.SymbolName,
                        robot.SymbolName) ||
                    !MatchesLabel(
                        order == null
                            ? string.Empty
                            : order.Label,
                        string.IsNullOrWhiteSpace(resolvedLabel)
                            ? preferred
                            : resolvedLabel + "-PENDING",
                        scopedSuffix + "-PENDING"))
                    continue;

                managedPendingOrders++;
                if (managedPending == null)
                    managedPending = order;

                if (order != null &&
                    !string.IsNullOrWhiteSpace(order.Label))
                {
                    string label =
                        order.Label;

                    const string pendingSuffix =
                        "-PENDING";

                    string baseLabel =
                        label.EndsWith(
                            pendingSuffix,
                            StringComparison.Ordinal)
                            ? label.Substring(
                                0,
                                label.Length - pendingSuffix.Length)
                            : label;

                    if (string.IsNullOrWhiteSpace(resolvedLabel) ||
                        !string.Equals(
                            resolvedLabel,
                            baseLabel,
                            StringComparison.Ordinal))
                        resolvedLabel = baseLabel;
                }
            }

            if (managedPositions > 1 ||
                managedPendingOrders > 1)
            {
                return Result(
                    "RECOVERY REQUIRED",
                    "AMBIGUOUS",
                    true,
                    "MULTIPLE MANAGED BROKER OBJECTS",
                    resolvedLabel,
                    managedPositions,
                    managedPendingOrders,
                    managedPosition == null
                        ? (long?)null
                        : managedPosition.Id,
                    managedPending == null
                        ? (long?)null
                        : managedPending.Id);
            }

            if (managedPositions == 1 &&
                managedPendingOrders == 1)
            {
                return Result(
                    "RECOVERY REQUIRED",
                    "AMBIGUOUS",
                    true,
                    "POSITION AND PENDING STATE COEXIST",
                    resolvedLabel,
                    managedPositions,
                    managedPendingOrders,
                    managedPosition.Id,
                    managedPending.Id);
            }

            if (managedPositions == 1)
            {
                string protectionReason;
                bool protectionHealthy =
                    HasHealthyProtection(
                        robot,
                        managedPosition,
                        out protectionReason);

                return Result(
                    protectionHealthy
                        ? "ACTIVE / RECONCILED"
                        : "RECOVERY REQUIRED",
                    protectionHealthy
                        ? "PROTECTED"
                        : "MISSING / INVALID",
                    !protectionHealthy,
                    protectionHealthy
                        ? "BROKER POSITION RECONCILED"
                        : protectionReason,
                    resolvedLabel,
                    managedPositions,
                    managedPendingOrders,
                    managedPosition.Id,
                    null);
            }

            if (managedPendingOrders == 1)
            {
                return Result(
                    "PENDING / RECONCILED",
                    "N/A",
                    false,
                    "BROKER PENDING ORDER RECONCILED",
                    resolvedLabel,
                    0,
                    1,
                    null,
                    managedPending.Id);
            }

            return Result(
                "READY / RECONCILED",
                "N/A",
                false,
                "NO MANAGED BROKER OBJECT",
                resolvedLabel,
                0,
                0,
                null,
                null);
        }

        private static bool HasHealthyProtection(
            Robot robot,
            Position position,
            out string reason)
        {
            reason = "OK";

            if (robot == null ||
                position == null)
            {
                reason = "POSITION UNAVAILABLE";
                return false;
            }

            if (!position.StopLoss.HasValue ||
                !Finite(position.StopLoss.Value))
            {
                reason = "STOP LOSS MISSING";
                return false;
            }

            if (!position.TakeProfit.HasValue ||
                !Finite(position.TakeProfit.Value))
            {
                reason = "TAKE PROFIT MISSING";
                return false;
            }

            if (!Finite(position.EntryPrice) ||
                !Finite(robot.Symbol.Bid) ||
                !Finite(robot.Symbol.Ask))
            {
                reason = "PROTECTION PRICE INPUT INVALID";
                return false;
            }

            if (position.TradeType == TradeType.Buy)
            {
                if (position.StopLoss.Value >= position.EntryPrice)
                {
                    reason = "BUY STOP IS NOT PROTECTIVE";
                    return false;
                }

                if (position.TakeProfit.Value <= position.EntryPrice)
                {
                    reason = "BUY TARGET IS NOT FORWARD";
                    return false;
                }
            }
            else if (position.TradeType == TradeType.Sell)
            {
                if (position.StopLoss.Value <= position.EntryPrice)
                {
                    reason = "SELL STOP IS NOT PROTECTIVE";
                    return false;
                }

                if (position.TakeProfit.Value >= position.EntryPrice)
                {
                    reason = "SELL TARGET IS NOT FORWARD";
                    return false;
                }
            }
            else
            {
                reason = "POSITION DIRECTION INVALID";
                return false;
            }

            return true;
        }

        private static bool MatchesScope(
            string actualSymbol,
            string expectedSymbol)
        {
            return string.Equals(
                actualSymbol ?? string.Empty,
                expectedSymbol ?? string.Empty,
                StringComparison.Ordinal);
        }

        private static bool MatchesLabel(
            string actual,
            string preferred,
            string scopedSuffix)
        {
            if (string.IsNullOrWhiteSpace(actual))
                return false;

            if (!string.IsNullOrWhiteSpace(preferred) &&
                CbotManagedObjectIdentityRule.MatchesManagedLabel(
                    actual,
                    preferred))
                return true;

            return CbotManagedObjectIdentityRule.MatchesInstanceScope(
                actual,
                scopedSuffix);
        }

        private static bool Finite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }

        private static CbotBrokerReconciliationResult Result(
            string lifecycleState,
            string protectionState,
            bool recoveryRequired,
            string reason,
            string executionLabel,
            int managedPositions,
            int managedPendingOrders,
            long? positionId,
            long? pendingOrderId)
        {
            return new CbotBrokerReconciliationResult(
                lifecycleState,
                protectionState,
                recoveryRequired,
                reason,
                executionLabel,
                managedPositions,
                managedPendingOrders,
                positionId,
                pendingOrderId);
        }
    }
}
