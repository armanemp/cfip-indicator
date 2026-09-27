using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class LifecycleTransition
        {
            public LifecycleState From { get; private set; }
            public LifecycleState To { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public string Reason { get; private set; }
    
            public LifecycleTransition(
                LifecycleState from,
                LifecycleState to,
                DateTime timeUtc,
                string reason)
            {
                From = from;
                To = to;
                TimeUtc = timeUtc;
                Reason = reason ?? string.Empty;
            }
        }
    
        public sealed class LifecycleManager
        {
            private LifecycleState _state;
    
            public LifecycleState State
            {
                get { return _state; }
            }
    
            public LifecycleManager()
            {
                _state = LifecycleState.Flat;
            }
    
            public bool TryTransition(
                LifecycleState target,
                DateTime timeUtc,
                string reason)
            {
                if (!IsAllowed(_state, target))
                    return false;
    
                _state = target;
                return true;
            }
    
            private static bool IsAllowed(
                LifecycleState from,
                LifecycleState to)
            {
                if (from == to)
                    return true;
    
                switch (from)
                {
                    case LifecycleState.Flat:
                        return
                            to == LifecycleState.SignalDetected ||
                            to == LifecycleState.PendingOrder ||
                            to == LifecycleState.LivePosition ||
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Closed;
    
                    case LifecycleState.SignalDetected:
                        return
                            to == LifecycleState.PlanReady ||
                            to == LifecycleState.Rejected ||
                            to == LifecycleState.Flat;
    
                    case LifecycleState.PlanReady:
                        return
                            to == LifecycleState.ExecutionReady ||
                            to == LifecycleState.Rejected ||
                            to == LifecycleState.Flat;
    
                    case LifecycleState.ExecutionReady:
                        return
                            to == LifecycleState.PendingOrder ||
                            to == LifecycleState.LivePosition ||
                            to == LifecycleState.Rejected ||
                            to == LifecycleState.Error;
    
                    case LifecycleState.PendingOrder:
                        return
                            to == LifecycleState.LivePosition ||
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Flat ||
                            to == LifecycleState.Error;
    
                    case LifecycleState.LivePosition:
                        return
                            to == LifecycleState.ExitRequested ||
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Closed ||
                            to == LifecycleState.Error;
    
                    case LifecycleState.ExitRequested:
                        return
                            to == LifecycleState.Closed ||
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Error;
    
                    case LifecycleState.RecoveryRequired:
                        return
                            to == LifecycleState.PendingOrder ||
                            to == LifecycleState.LivePosition ||
                            to == LifecycleState.Closed ||
                            to == LifecycleState.Error ||
                            to == LifecycleState.Flat;
    
                    case LifecycleState.Closed:
                        return
                            to == LifecycleState.SignalDetected ||
                            to == LifecycleState.PendingOrder ||
                            to == LifecycleState.LivePosition ||
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Flat;
    
                    case LifecycleState.Rejected:
                        return
                            to == LifecycleState.Flat ||
                            to == LifecycleState.SignalDetected;
    
                    case LifecycleState.Error:
                        return
                            to == LifecycleState.RecoveryRequired ||
                            to == LifecycleState.Flat;
    
                    default:
                        return false;
                }
            }
        }
    
        public enum PendingOrderLifecycleState
        {
            Unknown = 0,
            Pending = 1,
            ProtectionRecoveryRequired = 2,
            Filled = 3,
            Cancelled = 4,
            Reconciled = 5
        }
    
        public enum PendingOrderActionKind
        {
            None = 0,
            Cancel = 1,
            RestoreProtection = 2
        }
    
        public sealed class PendingOrderRecord
        {
            public string BrokerOrderId { get; private set; }
            public string SignalId { get; private set; }
            public string PlanId { get; private set; }
            public Direction Direction { get; private set; }
            public ExecutionKind Kind { get; private set; }
            public double TargetPrice { get; private set; }
            public double? ExpectedStopLoss { get; private set; }
            public double? ExpectedTakeProfit { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpectedExpiryUtc { get; private set; }
            public DateTime? FilledUtc { get; private set; }
            public DateTime? CancelledUtc { get; private set; }
            public PendingOrderLifecycleState State { get; private set; }
    
            private readonly List<string> _brokerPositionIds =
                new List<string>();
            private int _protectionAttempts;
            private int _cancelAttempts;
            private bool _protectionActionPending;
            private bool _cancelActionPending;
            private bool _cancelRequested;
            private DateTime _nextProtectionRetryUtc = DateTime.MinValue;
            private DateTime _nextCancelRetryUtc = DateTime.MinValue;
    
            public PendingOrderRecord(
                string brokerOrderId,
                string signalId,
                string planId,
                Direction direction,
                ExecutionKind kind,
                double targetPrice,
                double? expectedStopLoss,
                double? expectedTakeProfit,
                DateTime createdUtc,
                DateTime? expectedExpiryUtc)
            {
                BrokerOrderId = brokerOrderId ?? string.Empty;
                SignalId = signalId ?? string.Empty;
                PlanId = planId ?? string.Empty;
                Direction = direction;
                Kind = kind;
                TargetPrice = targetPrice;
                ExpectedStopLoss = expectedStopLoss;
                ExpectedTakeProfit = expectedTakeProfit;
                CreatedUtc = createdUtc;
                ExpectedExpiryUtc = expectedExpiryUtc;
                State = PendingOrderLifecycleState.Pending;
            }
    
            public bool IsProtectionRetryDue(DateTime utc)
            {
                return
                    !_protectionActionPending &&
                    utc >= _nextProtectionRetryUtc;
            }
    
            public void MarkProtectionActionQueued()
            {
                _protectionActionPending = true;
            }
    
            public void ResetProtectionActionQueue()
            {
                _protectionActionPending = false;
            }
    
            public void MarkProtectionActionAccepted()
            {
                _protectionActionPending = false;
                _protectionAttempts = 0;
                _nextProtectionRetryUtc = DateTime.MinValue;
                State = PendingOrderLifecycleState.Pending;
            }
    
            public void MarkProtectionActionFailed(DateTime utc)
            {
                _protectionActionPending = false;
                _protectionAttempts++;
                _nextProtectionRetryUtc =
                    utc + RetryDelay(_protectionAttempts);
                State =
                    PendingOrderLifecycleState.ProtectionRecoveryRequired;
            }
    
            public bool IsCancelRetryDue(DateTime utc)
            {
                return
                    !_cancelActionPending &&
                    utc >= _nextCancelRetryUtc;
            }
    
            public void MarkCancelActionQueued()
            {
                _cancelActionPending = true;
                _cancelRequested = true;
            }
    
            public void MarkCancelActionAccepted(DateTime utc)
            {
                _cancelActionPending = false;
                _cancelAttempts = 0;
                _cancelRequested = true;
                _nextCancelRetryUtc =
                    utc + TimeSpan.FromSeconds(5);
            }
    
            public void MarkCancelActionFailed(DateTime utc)
            {
                _cancelActionPending = false;
                _cancelAttempts++;
                _cancelRequested = true;
                _nextCancelRetryUtc =
                    utc + RetryDelay(_cancelAttempts);
                State = PendingOrderLifecycleState.Pending;
            }
    
            public void MarkProtectionRecoveryRequired()
            {
                State =
                    PendingOrderLifecycleState.ProtectionRecoveryRequired;
            }
    
            public string BrokerPositionId
            {
                get
                {
                    return
                        _brokerPositionIds.Count > 0
                            ? _brokerPositionIds[0]
                            : string.Empty;
                }
            }
    
            public IReadOnlyList<string> BrokerPositionIds
            {
                get
                {
                    return new ReadOnlyCollection<string>(
                        new List<string>(_brokerPositionIds));
                }
            }
    
            public void MarkFilled(
                DateTime utc,
                string brokerPositionId)
            {
                FilledUtc = utc;
    
                string id = brokerPositionId ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(id) &&
                    !_brokerPositionIds.Contains(id))
                    _brokerPositionIds.Add(id);
    
                State = PendingOrderLifecycleState.Filled;
            }
    
            public void MarkCancelled(DateTime utc)
            {
                CancelledUtc = utc;
                _cancelActionPending = false;
                _cancelRequested = false;
                State = PendingOrderLifecycleState.Cancelled;
            }
    
            public void MarkReconciled()
            {
                _cancelActionPending = false;
                _cancelRequested = false;
                _protectionActionPending = false;
                State = PendingOrderLifecycleState.Reconciled;
            }
    
            private static TimeSpan RetryDelay(int attempts)
            {
                int normalized = Math.Max(1, attempts);
                int exponent = Math.Min(6, normalized - 1);
                int seconds = 2;
    
                for (int i = 0; i < exponent; i++)
                    seconds = Math.Min(60, seconds * 2);
    
                return TimeSpan.FromSeconds(seconds);
            }
        }
    
        public sealed class PendingOrderAction
        {
            public string BrokerOrderId { get; private set; }
            public PendingOrderActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public string Reason { get; private set; }
    
            public PendingOrderAction(
                string brokerOrderId,
                PendingOrderActionKind kind,
                double? stopLoss,
                double? takeProfit,
                string reason)
            {
                BrokerOrderId = brokerOrderId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                Reason = reason ?? string.Empty;
            }
        }
    
        public sealed class PendingOrderLifecycleManager
        {
            private readonly string _managedLabel;
            private readonly Dictionary<string, PendingOrderRecord> _records =
                new Dictionary<string, PendingOrderRecord>(
                    StringComparer.Ordinal);
            private readonly List<PendingOrderAction> _actions =
                new List<PendingOrderAction>();
            private readonly HashSet<string> _actionKeys =
                new HashSet<string>(StringComparer.Ordinal);
    
            public PendingOrderLifecycleManager(
                string managedLabel)
            {
                _managedLabel = managedLabel ?? string.Empty;
            }
    
            private static readonly TimeSpan BrokerConfirmationGrace =
                TimeSpan.FromSeconds(5);
    
            public void ReconcileBrokerState(
                IReadOnlyList<BrokerPendingOrderSnapshot> activeOrders,
                IReadOnlyList<BrokerPositionSnapshot> activePositions,
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
                    new List<PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        PendingOrderLifecycleState.Reconciled)
                        continue;
    
                    if (activeOrderIds.Contains(record.BrokerOrderId))
                        continue;
    
                    IReadOnlyList<BrokerPositionSnapshot> matchedPositions =
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
    
            public IReadOnlyList<PendingOrderRecord> Records
            {
                get
                {
                    return new ReadOnlyCollection<PendingOrderRecord>(
                        new List<PendingOrderRecord>(
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
                ExecutionIntent intent,
                DateTime utc)
            {
                if (order == null || intent == null)
                    return;
    
                var record =
                    new PendingOrderRecord(
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
    
                PendingOrderRecord record;
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
    
                PendingOrderRecord record;
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
                PendingOrderAction action,
                ExecutionResult result,
                DateTime utc)
            {
                if (action == null || result == null)
                    return;
    
                PendingOrderRecord record;
                if (!_records.TryGetValue(
                        action.BrokerOrderId,
                        out record))
                    return;
    
                if (action.Kind ==
                    PendingOrderActionKind.Cancel)
                {
                    if (result.Accepted)
                        record.MarkCancelActionAccepted(utc);
                    else
                        record.MarkCancelActionFailed(utc);
                    return;
                }
    
                if (action.Kind ==
                    PendingOrderActionKind.RestoreProtection)
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
    
                PendingOrderRecord record;
                if (_records.TryGetValue(
                        order.Id.ToString(),
                        out record))
                    record.MarkCancelled(utc);
            }
    
            private IReadOnlyList<BrokerPositionSnapshot> FindMatchingPositions(
                PendingOrderRecord record,
                IReadOnlyList<BrokerPositionSnapshot> activePositions)
            {
                var matches =
                    new List<BrokerPositionSnapshot>();
    
                if (record == null ||
                    activePositions == null ||
                    string.IsNullOrWhiteSpace(record.PlanId))
                    return matches;
    
                var known =
                    new HashSet<string>(StringComparer.Ordinal);
    
                for (int i = 0; i < activePositions.Count; i++)
                {
                    BrokerPositionSnapshot position =
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
                    new List<PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        PendingOrderLifecycleState.Filled ||
                        record.State ==
                        PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        PendingOrderLifecycleState.Reconciled)
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
                    new List<PendingOrderRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    PendingOrderRecord record =
                        snapshot[i];
    
                    if (record.State ==
                        PendingOrderLifecycleState.Filled ||
                        record.State ==
                        PendingOrderLifecycleState.Cancelled ||
                        record.State ==
                        PendingOrderLifecycleState.Reconciled)
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
                        PendingOrderLifecycleState.ProtectionRecoveryRequired)
                        QueueProtectionRecoveryForRecord(
                            record,
                            utc);
                }
            }
    
            public IReadOnlyList<PendingOrderAction> DrainActions()
            {
                var result =
                    new ReadOnlyCollection<PendingOrderAction>(
                        new List<PendingOrderAction>(
                            _actions));
                _actions.Clear();
                _actionKeys.Clear();
                return result;
            }
    
            private void QueueProtectionRecoveryForRecord(
                PendingOrderRecord record,
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
                    new PendingOrderAction(
                        record.BrokerOrderId,
                        PendingOrderActionKind.RestoreProtection,
                        record.ExpectedStopLoss,
                        record.ExpectedTakeProfit,
                        "PENDING_PROTECTION_DRIFT"));
    
                record.MarkProtectionActionQueued();
                record.MarkProtectionRecoveryRequired();
            }
    
            private void QueueProtectionRecoveryIfRequired(
                PendingOrder order,
                PendingOrderRecord record,
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
                PendingOrderRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsCancelRetryDue(utc))
                    return;
    
                Queue(
                    new PendingOrderAction(
                        record.BrokerOrderId,
                        PendingOrderActionKind.Cancel,
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
    
            private void Queue(PendingOrderAction action)
            {
                string key =
                    action.BrokerOrderId +
                    "|" +
                    action.Kind.ToString();
    
                if (_actionKeys.Add(key))
                    _actions.Add(action);
            }
    
            private PendingOrderRecord CreateRecord(
                PendingOrder order,
                DateTime utc,
                string signalId,
                string planId,
                ExecutionIntent intent)
            {
                return
                    new PendingOrderRecord(
                        order.Id.ToString(),
                        signalId,
                        planId,
                        order.TradeType == TradeType.Buy
                            ? Direction.Buy
                            : Direction.Sell,
                        order.OrderType == PendingOrderType.Stop
                            ? ExecutionKind.Stop
                            : ExecutionKind.Limit,
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
    
                const string signalMarker = "CFIP|SIGNAL|";
                const string planMarker = "CFIP|PLAN|";
    
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
                        "CFIP|",
                        StringComparison.Ordinal) >= 0;
            }
        }
    
        public enum PositionLifecycleState
        {
            Unknown = 0,
            Adopted = 1,
            Protected = 2,
            ProtectionRecoveryRequired = 3,
            ExitRequested = 4,
            Closed = 5,
            Reconciled = 6,
            Orphan = 7,
            RecoveryRequired = 8
        }
    
        public enum PositionActionKind
        {
            None = 0,
            RestoreProtection = 1,
            Close = 2
        }
    
        public sealed class PositionRecord
        {
            public string BrokerPositionId { get; private set; }
            public string StrategyId { get; private set; }
            public string SignalId { get; private set; }
            public string PlanId { get; private set; }
            public Direction Direction { get; private set; }
            public double OriginalEntryPrice { get; private set; }
            public double? ExpectedStopLoss { get; private set; }
            public double? ExpectedTakeProfit { get; private set; }
            public double ActualEntryPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public DateTime AdoptedUtc { get; private set; }
            public DateTime? ClosedUtc { get; private set; }
            public PositionLifecycleState State { get; private set; }
    
            private int _protectionAttempts;
            private int _closeAttempts;
            private bool _protectionActionPending;
            private bool _closeActionPending;
            private bool _closeRequested;
            private DateTime _nextProtectionRetryUtc = DateTime.MinValue;
            private DateTime _nextCloseRetryUtc = DateTime.MinValue;
    
            public int ProtectionAttempts
            {
                get { return _protectionAttempts; }
            }
    
            public int CloseAttempts
            {
                get { return _closeAttempts; }
            }
    
            public bool CloseRequested
            {
                get { return _closeRequested; }
            }
    
            public PositionRecord(
                string brokerPositionId,
                string strategyId,
                string signalId,
                string planId,
                Direction direction,
                double entryPrice,
                double volumeInUnits,
                double? expectedStopLoss,
                double? expectedTakeProfit,
                DateTime adoptedUtc)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                StrategyId = strategyId ?? string.Empty;
                SignalId = signalId ?? string.Empty;
                PlanId = planId ?? string.Empty;
                Direction = direction;
                OriginalEntryPrice = entryPrice;
                ActualEntryPrice = entryPrice;
                VolumeInUnits = Math.Max(0, volumeInUnits);
                ExpectedStopLoss = expectedStopLoss;
                ExpectedTakeProfit = expectedTakeProfit;
                AdoptedUtc = adoptedUtc;
                State = PositionLifecycleState.Adopted;
            }
    
            public void RebaseActualEntry(double actualEntryPrice)
            {
                if (actualEntryPrice > 0)
                    ActualEntryPrice = actualEntryPrice;
            }
    
            public void SetExpectedProtection(
                double? stopLoss,
                double? takeProfit)
            {
                if (stopLoss.HasValue)
                    ExpectedStopLoss = stopLoss;
    
                if (takeProfit.HasValue)
                    ExpectedTakeProfit = takeProfit;
            }
    
            public void MarkProtected()
            {
                State = PositionLifecycleState.Protected;
            }
    
            public void MarkProtectionRecoveryRequired()
            {
                State =
                    PositionLifecycleState.ProtectionRecoveryRequired;
            }
    
            public bool IsProtectionRetryDue(DateTime utc)
            {
                return
                    !_protectionActionPending &&
                    utc >= _nextProtectionRetryUtc;
            }
    
            public void MarkProtectionActionQueued()
            {
                _protectionActionPending = true;
            }
    
            public void MarkProtectionActionAccepted()
            {
                _protectionActionPending = false;
                _protectionAttempts = 0;
                _nextProtectionRetryUtc = DateTime.MinValue;
    
                if (!_closeRequested)
                    State = PositionLifecycleState.Protected;
            }
    
            public void MarkProtectionActionFailed(DateTime utc)
            {
                _protectionActionPending = false;
                _protectionAttempts++;
                _nextProtectionRetryUtc =
                    utc + RetryDelay(_protectionAttempts);
                State =
                    PositionLifecycleState.RecoveryRequired;
            }
    
            public void RequestClose(DateTime utc)
            {
                _closeRequested = true;
                State = PositionLifecycleState.ExitRequested;
    
                if (_nextCloseRetryUtc == DateTime.MinValue)
                    _nextCloseRetryUtc = utc;
            }
    
            public bool IsCloseRetryDue(DateTime utc)
            {
                return
                    _closeRequested &&
                    !_closeActionPending &&
                    utc >= _nextCloseRetryUtc;
            }
    
            public void MarkCloseActionQueued()
            {
                _closeActionPending = true;
                State = PositionLifecycleState.ExitRequested;
            }
    
            public void MarkCloseActionAccepted(DateTime utc)
            {
                _closeActionPending = false;
                State = PositionLifecycleState.ExitRequested;
                _nextCloseRetryUtc = utc + TimeSpan.FromSeconds(5);
            }
    
            public void MarkCloseActionFailed(DateTime utc)
            {
                _closeActionPending = false;
                _closeAttempts++;
                _nextCloseRetryUtc =
                    utc + RetryDelay(_closeAttempts);
                State =
                    PositionLifecycleState.RecoveryRequired;
            }
    
            public void MarkClosed(DateTime utc)
            {
                ClosedUtc = utc;
                _closeRequested = false;
                _closeActionPending = false;
                State = PositionLifecycleState.Closed;
            }
    
            public void MarkReconciled()
            {
                _closeRequested = false;
                _closeActionPending = false;
                State = PositionLifecycleState.Reconciled;
            }
    
            public void MarkOrphan()
            {
                _closeRequested = false;
                _closeActionPending = false;
                State = PositionLifecycleState.Orphan;
            }
    
            private static TimeSpan RetryDelay(int attempts)
            {
                int normalized = Math.Max(1, attempts);
                int exponent = Math.Min(6, normalized - 1);
                int seconds = 2;
    
                for (int i = 0; i < exponent; i++)
                    seconds = Math.Min(60, seconds * 2);
    
                return TimeSpan.FromSeconds(seconds);
            }
        }
    
        public sealed class PositionAction
        {
            public string BrokerPositionId { get; private set; }
            public PositionActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public string Reason { get; private set; }
    
            public PositionAction(
                string brokerPositionId,
                PositionActionKind kind,
                double? stopLoss,
                double? takeProfit,
                string reason)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                Reason = reason ?? string.Empty;
            }
        }
    
        public sealed class PositionLifecycleManager
        {
            private readonly string _managedLabel;
            private readonly Dictionary<string, PositionRecord> _records =
                new Dictionary<string, PositionRecord>(
                    StringComparer.Ordinal);
            private readonly List<PositionAction> _actions =
                new List<PositionAction>();
            private readonly HashSet<string> _actionKeys =
                new HashSet<string>(StringComparer.Ordinal);
    
            public PositionLifecycleManager(string managedLabel)
            {
                _managedLabel = managedLabel ?? string.Empty;
            }
    
            public IReadOnlyList<PositionRecord> Records
            {
                get
                {
                    return new ReadOnlyCollection<PositionRecord>(
                        new List<PositionRecord>(
                            _records.Values));
                }
            }
    
            public void RegisterOpened(
                Position position,
                DateTime utc,
                double? expectedStopLoss,
                double? expectedTakeProfit)
            {
                if (!IsManaged(position))
                    return;
    
                string id = position.Id.ToString();
                PositionRecord record;
    
                if (_records.TryGetValue(id, out record))
                {
                    record.RebaseActualEntry(position.EntryPrice);
                    record.SetExpectedProtection(
                        expectedStopLoss,
                        expectedTakeProfit);
                    VerifyProtection(
                        position,
                        record,
                        utc);
                    return;
                }
    
                string signalId;
                string planId;
                ParseIdentity(
                    position.Comment,
                    out signalId,
                    out planId);
    
                record =
                    new PositionRecord(
                        id,
                        _hostStrategyId,
                        signalId,
                        planId,
                        position.TradeType == TradeType.Buy
                            ? Direction.Buy
                            : Direction.Sell,
                        position.EntryPrice,
                        position.VolumeInUnits,
                        expectedStopLoss,
                        expectedTakeProfit,
                        utc);
    
                _records[id] = record;
                VerifyProtection(position, record, utc);
            }
    
            public void ReconcileBrokerState(
                IReadOnlyList<BrokerPositionSnapshot> activePositions,
                DateTime utc)
            {
                var activeIds =
                    new HashSet<string>(StringComparer.Ordinal);
    
                for (int i = 0;
                     activePositions != null &&
                     i < activePositions.Count;
                     i++)
                {
                    BrokerPositionSnapshot snapshot =
                        activePositions[i];
    
                    if (snapshot == null)
                        continue;
    
                    activeIds.Add(snapshot.BrokerPositionId);
    
                    PositionRecord record;
                    if (!_records.TryGetValue(
                            snapshot.BrokerPositionId,
                            out record))
                    {
                        RegisterSnapshot(snapshot, utc);
                        _records.TryGetValue(
                            snapshot.BrokerPositionId,
                            out record);
                    }
    
                    if (record == null)
                        continue;
    
                    record.RebaseActualEntry(snapshot.EntryPrice);
    
                    if (string.IsNullOrWhiteSpace(record.PlanId))
                    {
                        record.MarkOrphan();
                        continue;
                    }
    
                    VerifyProtectionSnapshot(
                        snapshot,
                        record);
                }
    
                var records =
                    new List<PositionRecord>(
                        _records.Values);
    
                for (int i = 0; i < records.Count; i++)
                {
                    PositionRecord record = records[i];
    
                    if (activeIds.Contains(record.BrokerPositionId))
                        continue;
    
                    if (record.State !=
                        PositionLifecycleState.Closed &&
                        record.State !=
                        PositionLifecycleState.Reconciled)
                        record.MarkReconciled();
                }
            }
    
            public void HandleModified(
                Position position,
                DateTime utc)
            {
                if (!IsManaged(position))
                    return;
    
                PositionRecord record;
                if (!_records.TryGetValue(
                        position.Id.ToString(),
                        out record))
                {
                    RegisterOpened(
                        position,
                        utc,
                        position.StopLoss,
                        position.TakeProfit);
                    return;
                }
    
                record.RebaseActualEntry(position.EntryPrice);
                VerifyProtection(position, record, utc);
            }
    
            public void HandleClosed(
                Position position,
                DateTime utc)
            {
                if (position == null)
                    return;
    
                PositionRecord record;
                if (_records.TryGetValue(
                        position.Id.ToString(),
                        out record))
                    record.MarkClosed(utc);
            }
    
            public bool RequestClose(
                string brokerPositionId,
                DateTime utc,
                string reason)
            {
                PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return false;
    
                if (record.State ==
                        PositionLifecycleState.Closed ||
                    record.State ==
                        PositionLifecycleState.Reconciled ||
                    record.State ==
                        PositionLifecycleState.Orphan)
                    return false;
    
                record.RequestClose(utc);
    
                RemoveQueuedProtectionAction(
                    record.BrokerPositionId);
    
                QueueCloseIfDue(
                    record,
                    reason ?? "CLOSE_REQUESTED",
                    utc);
    
                return true;
            }
    
            public void HandleActionResult(
                PositionAction action,
                ExecutionResult result,
                DateTime utc)
            {
                if (action == null || result == null)
                    return;
    
                PositionRecord record;
                if (!_records.TryGetValue(
                        action.BrokerPositionId,
                        out record))
                    return;
    
                if (action.Kind ==
                    PositionActionKind.Close)
                {
                    if (result.Accepted)
                        record.MarkCloseActionAccepted(utc);
                    else
                        record.MarkCloseActionFailed(utc);
                    return;
                }
    
                if (action.Kind ==
                    PositionActionKind.RestoreProtection)
                {
                    if (result.Accepted)
                        record.MarkProtectionActionAccepted();
                    else
                        record.MarkProtectionActionFailed(utc);
                }
            }
    
            public void RegisterExpectedProtection(
                string brokerPositionId,
                double? stopLoss,
                double? takeProfit,
                DateTime utc)
            {
                PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return;
    
                if (stopLoss.HasValue)
                    SetExpectedStop(record, stopLoss);
    
                if (takeProfit.HasValue)
                    SetExpectedTarget(record, takeProfit);
    
                if (record.State ==
                        PositionLifecycleState.ProtectionRecoveryRequired ||
                    record.State ==
                        PositionLifecycleState.RecoveryRequired)
                {
                    QueueProtectionIfDue(
                        record,
                        "PENDING_FILL_EXPECTED_PROTECTION",
                        utc);
                }
            }
    
            public bool RequestProtectionMutation(
                string brokerPositionId,
                double? stopLoss,
                double? takeProfit,
                DateTime utc,
                string reason)
            {
                PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return false;
    
                if (record.State ==
                        PositionLifecycleState.Closed ||
                    record.State ==
                        PositionLifecycleState.Reconciled ||
                    record.State ==
                        PositionLifecycleState.Orphan ||
                    record.CloseRequested ||
                    !stopLoss.HasValue && !takeProfit.HasValue)
                    return false;
    
                record.SetExpectedProtection(
                    stopLoss,
                    takeProfit);
    
                if (RemoveQueuedProtectionAction(
                        record.BrokerPositionId))
                    record.ResetProtectionActionQueue();
    
                return QueueProtectionIfDue(
                    record,
                    reason ?? "LIVE_PROTECTION_UPDATE",
                    utc);
            }
    
            public IReadOnlyList<PositionAction> DrainActions()
            {
                var result =
                    new ReadOnlyCollection<PositionAction>(
                        new List<PositionAction>(
                            _actions));
                _actions.Clear();
                _actionKeys.Clear();
                return result;
            }
    
            private readonly string _hostStrategyId = "CFIP-PRO-";
    
            private void RegisterSnapshot(
                BrokerPositionSnapshot snapshot,
                DateTime utc)
            {
                string signalId;
                string planId;
                ParseIdentity(
                    snapshot.Comment,
                    out signalId,
                    out planId);
    
                var record =
                    new PositionRecord(
                        snapshot.BrokerPositionId,
                        _hostStrategyId,
                        signalId,
                        planId,
                        snapshot.Direction,
                        snapshot.EntryPrice,
                        snapshot.VolumeInUnits,
                        snapshot.StopLoss,
                        snapshot.TakeProfit,
                        utc);
    
                _records[snapshot.BrokerPositionId] = record;
                VerifyProtectionSnapshot(snapshot, record, utc);
            }
    
            private void VerifyProtection(
                Position position,
                PositionRecord record,
                DateTime utc)
            {
                bool hasStop = position.StopLoss.HasValue;
                bool hasTarget = position.TakeProfit.HasValue;
    
                bool stopDrift =
                    record.ExpectedStopLoss.HasValue &&
                    (!hasStop ||
                     !PricesMatch(
                         position.StopLoss.Value,
                         record.ExpectedStopLoss.Value));
    
                bool targetDrift =
                    record.ExpectedTakeProfit.HasValue &&
                    (!hasTarget ||
                     !PricesMatch(
                         position.TakeProfit.Value,
                         record.ExpectedTakeProfit.Value));
    
                if (hasStop && hasTarget &&
                    !stopDrift &&
                    !targetDrift)
                {
                    if (!record.CloseRequested &&
                        record.State !=
                        PositionLifecycleState.ExitRequested)
                        record.MarkProtected();
                    return;
                }
    
                if (!record.ExpectedStopLoss.HasValue &&
                    !record.ExpectedTakeProfit.HasValue)
                {
                    record.MarkProtectionRecoveryRequired();
                    return;
                }
    
                QueueProtectionIfDue(
                    record,
                    stopDrift || targetDrift
                        ? "POSITION_PROTECTION_DRIFT"
                        : "POSITION_PROTECTION_MISSING",
                    utc);
            }
    
            private void VerifyProtectionSnapshot(
                BrokerPositionSnapshot snapshot,
                PositionRecord record,
                DateTime utc)
            {
                bool hasStop = snapshot.StopLoss.HasValue;
                bool hasTarget = snapshot.TakeProfit.HasValue;
    
                bool stopDrift =
                    record.ExpectedStopLoss.HasValue &&
                    (!hasStop ||
                     !PricesMatch(
                         snapshot.StopLoss.Value,
                         record.ExpectedStopLoss.Value));
    
                bool targetDrift =
                    record.ExpectedTakeProfit.HasValue &&
                    (!hasTarget ||
                     !PricesMatch(
                         snapshot.TakeProfit.Value,
                         record.ExpectedTakeProfit.Value));
    
                if (hasStop && hasTarget &&
                    !stopDrift &&
                    !targetDrift)
                {
                    if (!record.CloseRequested &&
                        record.State !=
                        PositionLifecycleState.ExitRequested)
                        record.MarkProtected();
                    return;
                }
    
                if (record.ExpectedStopLoss.HasValue ||
                    record.ExpectedTakeProfit.HasValue)
                {
                    QueueProtectionIfDue(
                        record,
                        stopDrift || targetDrift
                            ? "BROKER_PROTECTION_DRIFT"
                            : "BROKER_PROTECTION_MISSING",
                        utc);
                }
                else
                {
                    record.MarkProtectionRecoveryRequired();
                }
            }
    
            private static bool PricesMatch(
                double actual,
                double expected)
            {
                return Math.Abs(actual - expected) <= 0.000001;
            }
    
            private bool QueueProtectionIfDue(
                PositionRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsProtectionRetryDue(utc) ||
                    (!record.ExpectedStopLoss.HasValue &&
                     !record.ExpectedTakeProfit.HasValue))
                    return false;
    
                Queue(
                    new PositionAction(
                        record.BrokerPositionId,
                        PositionActionKind.RestoreProtection,
                        record.ExpectedStopLoss,
                        record.ExpectedTakeProfit,
                        reason));
    
                record.MarkProtectionActionQueued();
                record.MarkProtectionRecoveryRequired();
                return true;
            }
    
            private void QueueCloseIfDue(
                PositionRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsCloseRetryDue(utc))
                    return;
    
                Queue(
                    new PositionAction(
                        record.BrokerPositionId,
                        PositionActionKind.Close,
                        null,
                        null,
                        reason));
    
                record.MarkCloseActionQueued();
            }
    
            public void EvaluateRetryActions(DateTime utc)
            {
                var snapshot =
                    new List<PositionRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    PositionRecord record = snapshot[i];
    
                    if (record.State ==
                        PositionLifecycleState.Orphan ||
                        record.State ==
                        PositionLifecycleState.Closed ||
                        record.State ==
                        PositionLifecycleState.Reconciled)
                        continue;
    
                    if (record.CloseRequested)
                    {
                        QueueCloseIfDue(
                            record,
                            "CLOSE_RETRY",
                            utc);
                        continue;
                    }
    
                    if (record.State ==
                        PositionLifecycleState.RecoveryRequired ||
                        record.State ==
                        PositionLifecycleState.ProtectionRecoveryRequired)
                    {
                        QueueProtectionIfDue(
                            record,
                            "PROTECTION_RETRY",
                            utc);
                    }
                }
            }
    
            private bool RemoveQueuedProtectionAction(
                string brokerPositionId)
            {
                if (string.IsNullOrWhiteSpace(brokerPositionId))
                    return false;
    
                bool removed = false;
    
                for (int i = _actions.Count - 1;
                     i >= 0;
                     i--)
                {
                    if (_actions[i] != null &&
                        string.Equals(
                            _actions[i].BrokerPositionId,
                            brokerPositionId,
                            StringComparison.Ordinal) &&
                        _actions[i].Kind ==
                            PositionActionKind.RestoreProtection)
                    {
                        _actions.RemoveAt(i);
                        removed = true;
                    }
                }
    
                string prefix =
                    brokerPositionId +
                    "|" +
                    PositionActionKind.RestoreProtection.ToString();
    
                var staleKeys =
                    new List<string>();
    
                foreach (string key in _actionKeys)
                {
                    if (key.StartsWith(
                            prefix,
                            StringComparison.Ordinal))
                        staleKeys.Add(key);
                }
    
                for (int i = 0; i < staleKeys.Count; i++)
                {
                    _actionKeys.Remove(staleKeys[i]);
                    removed = true;
                }
    
                return removed;
            }
    
            private void Queue(PositionAction action)
            {
                string key =
                    action.BrokerPositionId +
                    "|" +
                    action.Kind.ToString();
    
                if (_actionKeys.Add(key))
                    _actions.Add(action);
            }
    
            private void SetExpectedStop(
                PositionRecord record,
                double? value)
            {
                if (record != null && value.HasValue)
                    record.SetExpectedProtection(value, null);
            }
    
            private void SetExpectedTarget(
                PositionRecord record,
                double? value)
            {
                if (record != null && value.HasValue)
                    record.SetExpectedProtection(null, value);
            }
    
            private bool IsManaged(Position position)
            {
                return
                    position != null &&
                    string.Equals(
                        position.Label,
                        _managedLabel,
                        StringComparison.Ordinal) &&
                    position.Comment != null &&
                    position.Comment.IndexOf(
                        "CFIP|",
                        StringComparison.Ordinal) >= 0;
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
    
                const string signalMarker = "CFIP|SIGNAL|";
                const string planMarker = "CFIP|PLAN|";
    
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
                    int start =
                        signalStart + signalMarker.Length;
                    int end =
                        planStart > signalStart
                            ? planStart
                            : comment.Length;
    
                    signalId =
                        comment.Substring(
                            start,
                            Math.Max(0, end - start))
                        .Trim('|');
                }
    
                if (planStart >= 0)
                {
                    planId =
                        comment.Substring(
                            planStart + planMarker.Length)
                        .Trim('|');
                }
            }
        }
    
    
}
