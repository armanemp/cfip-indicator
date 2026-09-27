// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PendingOrderLifecycleManager
        {
            private readonly string _managedLabel;
            private readonly Dictionary<string, CFIPClean89PendingOrderRecord> _records =
                new Dictionary<string, CFIPClean89PendingOrderRecord>(
                    StringComparer.Ordinal);
            private readonly List<CFIPClean89PendingOrderAction> _actions =
                new List<CFIPClean89PendingOrderAction>();
            private readonly HashSet<string> _actionKeys =
                new HashSet<string>(StringComparer.Ordinal);
    
            public CFIPClean89PendingOrderLifecycleManager(
                string managedLabel)
            {
                _managedLabel = managedLabel ?? string.Empty;
            }
    
            private static readonly TimeSpan BrokerConfirmationGrace =
                TimeSpan.FromSeconds(5);
    
            public void ReconcileBrokerState(
                IReadOnlyList<CFIPClean89BrokerPendingOrderSnapshot> activeOrders,
                IReadOnlyList<CFIPClean89BrokerPositionSnapshot> activePositions,
                DateTime utc)
            {
                var activeOrderIds =
                    new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0;
                     activeOrders != null && i < activeOrders.Count;
                     i++)
                {
                    if (activeOrders[i] != null)
                        activeOrderIds.Add(
                            activeOrders[i].BrokerOrderId);
                }
    
                var snapshot =
                    new List<CFIPClean89PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    CFIPClean89PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        CFIPClean89PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        CFIPClean89PendingOrderLifecycleState.Reconciled)
                        continue;
    
                    if (activeOrderIds.Contains(record.BrokerOrderId))
                        continue;
    
                    IReadOnlyList<CFIPClean89BrokerPositionSnapshot> matchedPositions =
                        FindMatchingPositions(
                            record,
                            activePositions);
    
                    if (matchedPositions.Count > 0)
                    {
                        for (int positionIndex = 0;
                             positionIndex < matchedPositions.Count;
                             positionIndex++)
                        {
                            record.MarkFilled(
                                utc,
                                matchedPositions[positionIndex].BrokerPositionId);
                        }
    
                        // The consumed PendingOrder is no longer the mutation
                        // target. Position lifecycle owns protection after fill.
                    }
                    else
                    {
                        if (record.CreatedUtc != DateTime.MinValue &&
                            utc >= record.CreatedUtc &&
                            utc - record.CreatedUtc < BrokerConfirmationGrace)
                            continue;
    
                        record.MarkReconciled();
                    }
                }
            }
    
            public IReadOnlyList<CFIPClean89PendingOrderRecord> Records
            {
                get
                {
                    return new ReadOnlyCollection<CFIPClean89PendingOrderRecord>(
                        new List<CFIPClean89PendingOrderRecord>(
                            _records.Values));
                }
            }
    
            public void RegisterExisting(
                PendingOrder order,
                DateTime utc)
            {
                if (!IsManaged(order))
                    return;
    
                string key = order.Id.ToString();
                if (_records.ContainsKey(key))
                    return;
    
                string signalId;
                string planId;
                ParseIdentity(
                    order.Comment,
                    out signalId,
                    out planId);
    
                _records[key] =
                    CreateRecord(
                        order,
                        utc,
                        signalId,
                        planId,
                        null);
            }
    
            public void RegisterSubmitted(
                PendingOrder order,
                CFIPClean89ExecutionIntent intent,
                DateTime utc)
            {
                if (order == null || intent == null)
                    return;
    
                var record =
                    new CFIPClean89PendingOrderRecord(
                        order.Id.ToString(),
                        intent.TradeIdentity != null
                            ? intent.TradeIdentity.SignalId
                            : string.Empty,
                        intent.TradeIdentity != null
                            ? intent.TradeIdentity.PlanId
                            : string.Empty,
                        intent.Direction,
                        intent.Kind,
                        order.TargetPrice,
                        intent.StopLoss != null
                            ? (double?)intent.StopLoss.Price
                            : null,
                        intent.EffectiveTarget != null
                            ? (double?)intent.EffectiveTarget.Price
                            : null,
                        utc,
                        intent.ExpiryUtc);
    
                _records[record.BrokerOrderId] = record;
                QueueProtectionRecoveryIfRequired(order, record);
            }
    
            public void HandleModified(PendingOrder order)
            {
                if (!IsManaged(order))
                    return;
    
                CFIPClean89PendingOrderRecord record;
                if (!_records.TryGetValue(
                        order.Id.ToString(),
                        out record))
                {
                    RegisterExisting(order, DateTime.MinValue);
                    return;
                }
    
                if (!ProtectionMatches(
                        order.StopLoss,
                        order.TakeProfit,
                        record.ExpectedStopLoss,
                        record.ExpectedTakeProfit))
                    QueueProtectionRecoveryIfRequired(
                        order,
                        record,
                        utc);
            }
    
            public void HandleFilled(
                PendingOrder order,
                Position position,
                DateTime utc)
            {
                if (order == null || position == null)
                    return;
    
                CFIPClean89PendingOrderRecord record;
                if (!_records.TryGetValue(
                        order.Id.ToString(),
                        out record))
                {
                    RegisterExisting(order, DateTime.MinValue);
                    _records.TryGetValue(
                        order.Id.ToString(),
                        out record);
                }
    
                if (record != null)
                {
                    record.MarkFilled(utc, position.Id.ToString());
    
                    // Protection recovery belongs to the resulting Position
                    // lifecycle, using its actual broker Position id.
                }
            }
    
            public void HandleActionResult(
                CFIPClean89PendingOrderAction action,
                CFIPClean89ExecutionResult result,
                DateTime utc)
            {
                if (action == null || result == null)
                    return;
    
                CFIPClean89PendingOrderRecord record;
                if (!_records.TryGetValue(
                        action.BrokerOrderId,
                        out record))
                    return;
    
                if (action.Kind ==
                    CFIPClean89PendingOrderActionKind.Cancel)
                {
                    if (result.Accepted)
                        record.MarkCancelActionAccepted(utc);
                    else
                        record.MarkCancelActionFailed(utc);
                    return;
                }
    
                if (action.Kind ==
                    CFIPClean89PendingOrderActionKind.RestoreProtection)
                {
                    if (result.Accepted)
                        record.MarkProtectionActionAccepted();
                    else
                        record.MarkProtectionActionFailed(utc);
                }
            }
    
            public void HandleCancelled(
                PendingOrder order,
                DateTime utc)
            {
                if (order == null)
                    return;
    
                CFIPClean89PendingOrderRecord record;
                if (_records.TryGetValue(
                        order.Id.ToString(),
                        out record))
                    record.MarkCancelled(utc);
            }
    
            private IReadOnlyList<CFIPClean89BrokerPositionSnapshot> FindMatchingPositions(
                CFIPClean89PendingOrderRecord record,
                IReadOnlyList<CFIPClean89BrokerPositionSnapshot> activePositions)
            {
                var matches =
                    new List<CFIPClean89BrokerPositionSnapshot>();
    
                if (record == null ||
                    activePositions == null ||
                    string.IsNullOrWhiteSpace(record.PlanId))
                    return matches;
    
                var known =
                    new HashSet<string>(StringComparer.Ordinal);
    
                for (int i = 0; i < activePositions.Count; i++)
                {
                    CFIPClean89BrokerPositionSnapshot position =
                        activePositions[i];
    
                    if (position == null ||
                        !position.IsOpen ||
                        string.IsNullOrWhiteSpace(position.BrokerPositionId))
                        continue;
    
                    if (known.Contains(position.BrokerPositionId))
                        continue;
    
                    if (position.Comment != null &&
                        position.Comment.IndexOf(
                            record.PlanId,
                            StringComparison.Ordinal) >= 0)
                    {
                        known.Add(position.BrokerPositionId);
                        matches.Add(position);
                    }
                }
    
                return matches;
            }
    
            public void InvalidateSupersededPlan(
                string activePlanId,
                DateTime utc)
            {
                if (string.IsNullOrWhiteSpace(activePlanId))
                    return;
    
                var snapshot =
                    new List<CFIPClean89PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    CFIPClean89PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        CFIPClean89PendingOrderLifecycleState.Filled ||
                        record.State ==
                        CFIPClean89PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        CFIPClean89PendingOrderLifecycleState.Reconciled)
                        continue;
    
                    if (string.IsNullOrWhiteSpace(record.PlanId) ||
                        string.Equals(
                            record.PlanId,
                            activePlanId,
                            StringComparison.Ordinal))
                        continue;
    
                    QueueCancelIfDue(
                        record,
                        "PLAN_SUPERSEDED",
                        utc);
                }
            }
    
            public void Evaluate(
                DateTime utc,
                bool dailyLossBreached)
            {
                var snapshot =
                    new List<CFIPClean89PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    CFIPClean89PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        CFIPClean89PendingOrderLifecycleState.Filled ||
                        record.State ==
                        CFIPClean89PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        CFIPClean89PendingOrderLifecycleState.Reconciled)
                        continue;
    
                    if (dailyLossBreached)
                    {
                        QueueCancelIfDue(
                            record,
                            "DAILY_LOSS_LIMIT",
                            utc);
                        continue;
                    }
    
                    if (record.ExpectedExpiryUtc.HasValue &&
                        record.ExpectedExpiryUtc.Value <= utc)
                    {
                        QueueCancelIfDue(
                            record,
                            "EXPECTED_EXPIRY",
                            utc);
                        continue;
                    }
    
                    if (record.State ==
                        CFIPClean89PendingOrderLifecycleState.ProtectionRecoveryRequired)
                        QueueProtectionRecoveryForRecord(
                            record,
                            utc);
                }
            }
    
            public IReadOnlyList<CFIPClean89PendingOrderAction> DrainActions()
            {
                var result =
                    new ReadOnlyCollection<CFIPClean89PendingOrderAction>(
                        new List<CFIPClean89PendingOrderAction>(
                            _actions));
                _actions.Clear();
                _actionKeys.Clear();
                return result;
            }
    
            private void QueueProtectionRecoveryForRecord(
                CFIPClean89PendingOrderRecord record,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsProtectionRetryDue(utc))
                    return;
    
                if (!record.ExpectedStopLoss.HasValue &&
                    !record.ExpectedTakeProfit.HasValue)
                {
                    record.MarkProtectionRecoveryRequired();
                    return;
                }
    
                Queue(
                    new CFIPClean89PendingOrderAction(
                        record.BrokerOrderId,
                        CFIPClean89PendingOrderActionKind.RestoreProtection,
                        record.ExpectedStopLoss,
                        record.ExpectedTakeProfit,
                        "PENDING_PROTECTION_DRIFT"));
    
                record.MarkProtectionActionQueued();
                record.MarkProtectionRecoveryRequired();
            }
    
            private void QueueProtectionRecoveryIfRequired(
                PendingOrder order,
                CFIPClean89PendingOrderRecord record,
                DateTime utc)
            {
                if (order.StopLoss.HasValue &&
                    order.TakeProfit.HasValue)
                    return;
    
                if (!record.ExpectedStopLoss.HasValue &&
                    !record.ExpectedTakeProfit.HasValue)
                {
                    record.MarkProtectionRecoveryRequired();
                    return;
                }
    
                QueueProtectionRecoveryForRecord(
                    record,
                    utc);
            }
    
            private void QueueCancelIfDue(
                CFIPClean89PendingOrderRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsCancelRetryDue(utc))
                    return;
    
                Queue(
                    new CFIPClean89PendingOrderAction(
                        record.BrokerOrderId,
                        CFIPClean89PendingOrderActionKind.Cancel,
                        null,
                        null,
                        reason));
    
                record.MarkCancelActionQueued();
            }
    
            private static bool ProtectionMatches(
                double? actualStop,
                double? actualTarget,
                double? expectedStop,
                double? expectedTarget)
            {
                if (expectedStop.HasValue &&
                    (!actualStop.HasValue ||
                     !PricesMatch(
                         actualStop.Value,
                         expectedStop.Value)))
                    return false;
    
                if (expectedTarget.HasValue &&
                    (!actualTarget.HasValue ||
                     !PricesMatch(
                         actualTarget.Value,
                         expectedTarget.Value)))
                    return false;
    
                return actualStop.HasValue &&
                       actualTarget.HasValue;
            }
    
            private static bool PricesMatch(
                double actual,
                double expected)
            {
                return Math.Abs(actual - expected) <= 0.000001;
            }
    
            private void Queue(CFIPClean89PendingOrderAction action)
            {
                string key =
                    action.BrokerOrderId +
                    "|" +
                    action.Kind.ToString();
    
                if (_actionKeys.Add(key))
                    _actions.Add(action);
            }
    
            private CFIPClean89PendingOrderRecord CreateRecord(
                PendingOrder order,
                DateTime utc,
                string signalId,
                string planId,
                CFIPClean89ExecutionIntent intent)
            {
                return
                    new CFIPClean89PendingOrderRecord(
                        order.Id.ToString(),
                        signalId,
                        planId,
                        order.TradeType == TradeType.Buy
                            ? CFIPClean89Direction.Buy
                            : CFIPClean89Direction.Sell,
                        order.OrderType == PendingOrderType.Stop
                            ? CFIPClean89ExecutionKind.Stop
                            : CFIPClean89ExecutionKind.Limit,
                        order.TargetPrice,
                        order.StopLoss,
                        order.TakeProfit,
                        utc == DateTime.MinValue
                            ? order.SubmittedTime
                            : utc,
                        intent != null
                            ? intent.ExpiryUtc
                            : order.ExpirationTime);
            }
    
            private void ParseIdentity(
                string comment,
                out string signalId,
                out string planId)
            {
                signalId = string.Empty;
                planId = string.Empty;
    
                if (string.IsNullOrWhiteSpace(comment))
                    return;
    
                const string signalMarker = "CFIP89|SIGNAL|";
                const string planMarker = "CFIP89|PLAN|";
    
                int signalStart =
                    comment.IndexOf(
                        signalMarker,
                        StringComparison.Ordinal);
                int planStart =
                    comment.IndexOf(
                        planMarker,
                        StringComparison.Ordinal);
    
                if (signalStart >= 0)
                {
                    int signalValueStart =
                        signalStart + signalMarker.Length;
    
                    int signalEnd =
                        planStart > signalStart
                            ? planStart
                            : comment.Length;
    
                    signalId =
                        comment.Substring(
                            signalValueStart,
                            Math.Max(
                                0,
                                signalEnd - signalValueStart))
                        .Trim('|');
                }
    
                if (planStart >= 0)
                {
                    int planValueStart =
                        planStart + planMarker.Length;
    
                    planId =
                        comment.Substring(planValueStart)
                        .Trim('|');
                }
            }
    
            private bool IsManaged(PendingOrder order)
            {
                return
                    order != null &&
                    string.Equals(
                        order.Label,
                        _managedLabel,
                        StringComparison.Ordinal) &&
                    order.Comment != null &&
                    order.Comment.IndexOf(
                        "CFIP89|",
                        StringComparison.Ordinal) >= 0;
            }
        }
}
