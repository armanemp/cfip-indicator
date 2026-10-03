using System;
using System.Collections.Generic;
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

            if (string.IsNullOrWhiteSpace(preferred))
            {
                return EvaluateAggregate(
                    robot,
                    scopedSuffix);
            }

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

        private static CbotBrokerReconciliationResult EvaluateAggregate(
            Robot robot,
            string scopedSuffix)
        {
            int managedPositions = 0;
            int managedPendingOrders = 0;
            Position firstPosition = null;
            PendingOrder firstPending = null;
            string firstLabel = string.Empty;
            bool allProtectionHealthy = true;
            string protectionReason = "OK";

            HashSet<string> positionLabels =
                new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> pendingLabels =
                new HashSet<string>(StringComparer.Ordinal);

            foreach (Position position in robot.Positions)
            {
                if (position == null ||
                    !MatchesScope(
                        position.SymbolName,
                        robot.SymbolName) ||
                    !CbotManagedObjectIdentityRule.MatchesInstanceScope(
                        position.Label,
                        scopedSuffix))
                    continue;

                string logicalLabel =
                    position.Label ?? string.Empty;

                if (!positionLabels.Add(logicalLabel))
                {
                    return Result(
                        "RECOVERY REQUIRED",
                        "AMBIGUOUS",
                        true,
                        "DUPLICATE MANAGED POSITION SCENARIO",
                        logicalLabel,
                        managedPositions + 1,
                        managedPendingOrders,
                        position.Id,
                        firstPending == null
                            ? (long?)null
                            : firstPending.Id);
                }

                managedPositions++;

                if (firstPosition == null)
                {
                    firstPosition = position;
                    firstLabel = logicalLabel;
                }

                if (!HasHealthyProtection(
                        robot,
                        position,
                        out string currentProtectionReason))
                {
                    allProtectionHealthy = false;
                    protectionReason =
                        currentProtectionReason;
                }
            }

            string pendingScope =
                scopedSuffix + "-PENDING";

            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order == null ||
                    !MatchesScope(
                        order.SymbolName,
                        robot.SymbolName) ||
                    !CbotManagedObjectIdentityRule.MatchesInstanceScope(
                        order.Label,
                        pendingScope))
                    continue;

                string label =
                    order.Label ?? string.Empty;

                const string pendingSuffix = "-PENDING";
                string logicalLabel =
                    label.EndsWith(
                        pendingSuffix,
                        StringComparison.Ordinal)
                        ? label.Substring(
                            0,
                            label.Length - pendingSuffix.Length)
                        : label;

                if (!pendingLabels.Add(logicalLabel))
                {
                    return Result(
                        "RECOVERY REQUIRED",
                        "AMBIGUOUS",
                        true,
                        "DUPLICATE MANAGED PENDING SCENARIO",
                        logicalLabel,
                        managedPositions,
                        managedPendingOrders + 1,
                        firstPosition == null
                            ? (long?)null
                            : firstPosition.Id,
                        order.Id);
                }

                managedPendingOrders++;

                if (firstPending == null)
                {
                    firstPending = order;
                    if (string.IsNullOrWhiteSpace(firstLabel))
                        firstLabel = logicalLabel;
                }
            }

            foreach (string positionLabel in positionLabels)
            {
                if (!pendingLabels.Contains(positionLabel))
                    continue;

                return Result(
                    "RECOVERY REQUIRED",
                    "AMBIGUOUS",
                    true,
                    "POSITION AND PENDING STATE COEXIST FOR SAME SCENARIO",
                    positionLabel,
                    managedPositions,
                    managedPendingOrders,
                    firstPosition == null
                        ? (long?)null
                        : firstPosition.Id,
                    firstPending == null
                        ? (long?)null
                        : firstPending.Id);
            }

            if (managedPositions == 0 &&
                managedPendingOrders == 0)
            {
                return Result(
                    "READY / RECONCILED",
                    "N/A",
                    false,
                    "NO MANAGED BROKER OBJECT",
                    string.Empty,
                    0,
                    0,
                    null,
                    null);
            }

            if (!allProtectionHealthy)
            {
                return Result(
                    "RECOVERY REQUIRED",
                    "MISSING / INVALID",
                    true,
                    protectionReason,
                    firstLabel,
                    managedPositions,
                    managedPendingOrders,
                    firstPosition == null
                        ? (long?)null
                        : firstPosition.Id,
                    firstPending == null
                        ? (long?)null
                        : firstPending.Id);
            }

            int scenarioCount =
                new HashSet<string>(
                    positionLabels,
                    StringComparer.Ordinal).Count;

            scenarioCount +=
                managedPendingOrders -
                CountIntersection(
                    positionLabels,
                    pendingLabels);

            string lifecycle;
            if (managedPositions > 0)
            {
                lifecycle =
                    scenarioCount > 1
                        ? "ACTIVE / MULTI-SCENARIO"
                        : "ACTIVE / RECONCILED";
            }
            else
            {
                lifecycle =
                    scenarioCount > 1
                        ? "PENDING / MULTI-SCENARIO"
                        : "PENDING / RECONCILED";
            }

            string protection =
                managedPositions > 0
                    ? managedPendingOrders > 0
                        ? "PROTECTED / PENDING"
                        : "PROTECTED"
                    : "N/A";

            return Result(
                lifecycle,
                protection,
                false,
                scenarioCount > 1
                    ? "BROKER MULTI-SCENARIO STATE RECONCILED"
                    : "BROKER STATE RECONCILED",
                firstLabel,
                managedPositions,
                managedPendingOrders,
                firstPosition == null
                    ? (long?)null
                    : firstPosition.Id,
                firstPending == null
                    ? (long?)null
                    : firstPending.Id);
        }

        private static int CountIntersection(
            HashSet<string> first,
            HashSet<string> second)
        {
            int count = 0;

            foreach (string value in first)
            {
                if (second.Contains(value))
                    count++;
            }

            return count;
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
