// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PendingOrderRecord
        {
            public string BrokerOrderId { get; private set; }
            public string SignalId { get; private set; }
            public string PlanId { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89ExecutionKind Kind { get; private set; }
            public double TargetPrice { get; private set; }
            public double? ExpectedStopLoss { get; private set; }
            public double? ExpectedTakeProfit { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpectedExpiryUtc { get; private set; }
            public DateTime? FilledUtc { get; private set; }
            public DateTime? CancelledUtc { get; private set; }
            public CFIPClean89PendingOrderLifecycleState State { get; private set; }
    
            private readonly List<string> _brokerPositionIds =
                new List<string>();
            private int _protectionAttempts;
            private int _cancelAttempts;
            private bool _protectionActionPending;
            private bool _cancelActionPending;
            private bool _cancelRequested;
            private DateTime _nextProtectionRetryUtc = DateTime.MinValue;
            private DateTime _nextCancelRetryUtc = DateTime.MinValue;
    
            public CFIPClean89PendingOrderRecord(
                string brokerOrderId,
                string signalId,
                string planId,
                CFIPClean89Direction direction,
                CFIPClean89ExecutionKind kind,
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
                State = CFIPClean89PendingOrderLifecycleState.Pending;
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
                State = CFIPClean89PendingOrderLifecycleState.Pending;
            }
    
            public void MarkProtectionActionFailed(DateTime utc)
            {
                _protectionActionPending = false;
                _protectionAttempts++;
                _nextProtectionRetryUtc =
                    utc + RetryDelay(_protectionAttempts);
                State =
                    CFIPClean89PendingOrderLifecycleState.ProtectionRecoveryRequired;
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
                State = CFIPClean89PendingOrderLifecycleState.Pending;
            }
    
            public void MarkProtectionRecoveryRequired()
            {
                State =
                    CFIPClean89PendingOrderLifecycleState.ProtectionRecoveryRequired;
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
    
                State = CFIPClean89PendingOrderLifecycleState.Filled;
            }
    
            public void MarkCancelled(DateTime utc)
            {
                CancelledUtc = utc;
                _cancelActionPending = false;
                _cancelRequested = false;
                State = CFIPClean89PendingOrderLifecycleState.Cancelled;
            }
    
            public void MarkReconciled()
            {
                _cancelActionPending = false;
                _cancelRequested = false;
                _protectionActionPending = false;
                State = CFIPClean89PendingOrderLifecycleState.Reconciled;
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
}
