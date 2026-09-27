// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PositionLifecycleManager
        {
            private readonly string _managedLabel;
            private readonly Dictionary<string, CFIPClean89PositionRecord> _records =
                new Dictionary<string, CFIPClean89PositionRecord>(
                    StringComparer.Ordinal);
            private readonly List<CFIPClean89PositionAction> _actions =
                new List<CFIPClean89PositionAction>();
            private readonly HashSet<string> _actionKeys =
                new HashSet<string>(StringComparer.Ordinal);
    
            public CFIPClean89PositionLifecycleManager(string managedLabel)
            {
                _managedLabel = managedLabel ?? string.Empty;
            }
    
            public IReadOnlyList<CFIPClean89PositionRecord> Records
            {
                get
                {
                    return new ReadOnlyCollection<CFIPClean89PositionRecord>(
                        new List<CFIPClean89PositionRecord>(
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
                CFIPClean89PositionRecord record;
    
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
                    new CFIPClean89PositionRecord(
                        id,
                        _hostStrategyId,
                        signalId,
                        planId,
                        position.TradeType == TradeType.Buy
                            ? CFIPClean89Direction.Buy
                            : CFIPClean89Direction.Sell,
                        position.EntryPrice,
                        position.VolumeInUnits,
                        expectedStopLoss,
                        expectedTakeProfit,
                        utc);
    
                _records[id] = record;
                VerifyProtection(position, record, utc);
            }
    
            public void ReconcileBrokerState(
                IReadOnlyList<CFIPClean89BrokerPositionSnapshot> activePositions,
                DateTime utc)
            {
                var activeIds =
                    new HashSet<string>(StringComparer.Ordinal);
    
                for (int i = 0;
                     activePositions != null &&
                     i < activePositions.Count;
                     i++)
                {
                    CFIPClean89BrokerPositionSnapshot snapshot =
                        activePositions[i];
    
                    if (snapshot == null)
                        continue;
    
                    activeIds.Add(snapshot.BrokerPositionId);
    
                    CFIPClean89PositionRecord record;
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
                    new List<CFIPClean89PositionRecord>(
                        _records.Values);
    
                for (int i = 0; i < records.Count; i++)
                {
                    CFIPClean89PositionRecord record = records[i];
    
                    if (activeIds.Contains(record.BrokerPositionId))
                        continue;
    
                    if (record.State !=
                        CFIPClean89PositionLifecycleState.Closed &&
                        record.State !=
                        CFIPClean89PositionLifecycleState.Reconciled)
                        record.MarkReconciled();
                }
            }
    
            public void HandleModified(
                Position position,
                DateTime utc)
            {
                if (!IsManaged(position))
                    return;
    
                CFIPClean89PositionRecord record;
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
    
                CFIPClean89PositionRecord record;
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
                CFIPClean89PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return false;
    
                if (record.State ==
                        CFIPClean89PositionLifecycleState.Closed ||
                    record.State ==
                        CFIPClean89PositionLifecycleState.Reconciled ||
                    record.State ==
                        CFIPClean89PositionLifecycleState.Orphan)
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
                CFIPClean89PositionAction action,
                CFIPClean89ExecutionResult result,
                DateTime utc)
            {
                if (action == null || result == null)
                    return;
    
                CFIPClean89PositionRecord record;
                if (!_records.TryGetValue(
                        action.BrokerPositionId,
                        out record))
                    return;
    
                if (action.Kind ==
                    CFIPClean89PositionActionKind.Close)
                {
                    if (result.Accepted)
                        record.MarkCloseActionAccepted(utc);
                    else
                        record.MarkCloseActionFailed(utc);
                    return;
                }
    
                if (action.Kind ==
                    CFIPClean89PositionActionKind.RestoreProtection)
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
                CFIPClean89PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return;
    
                if (stopLoss.HasValue)
                    SetExpectedStop(record, stopLoss);
    
                if (takeProfit.HasValue)
                    SetExpectedTarget(record, takeProfit);
    
                if (record.State ==
                        CFIPClean89PositionLifecycleState.ProtectionRecoveryRequired ||
                    record.State ==
                        CFIPClean89PositionLifecycleState.RecoveryRequired)
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
                CFIPClean89PositionRecord record;
    
                if (!_records.TryGetValue(
                        brokerPositionId ?? string.Empty,
                        out record))
                    return false;
    
                if (record.State ==
                        CFIPClean89PositionLifecycleState.Closed ||
                    record.State ==
                        CFIPClean89PositionLifecycleState.Reconciled ||
                    record.State ==
                        CFIPClean89PositionLifecycleState.Orphan ||
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
    
            public IReadOnlyList<CFIPClean89PositionAction> DrainActions()
            {
                var result =
                    new ReadOnlyCollection<CFIPClean89PositionAction>(
                        new List<CFIPClean89PositionAction>(
                            _actions));
                _actions.Clear();
                _actionKeys.Clear();
                return result;
            }
    
            private readonly string _hostStrategyId = "CFIP-PRO-v89";
    
            private void RegisterSnapshot(
                CFIPClean89BrokerPositionSnapshot snapshot,
                DateTime utc)
            {
                string signalId;
                string planId;
                ParseIdentity(
                    snapshot.Comment,
                    out signalId,
                    out planId);
    
                var record =
                    new CFIPClean89PositionRecord(
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
                CFIPClean89PositionRecord record,
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
                        CFIPClean89PositionLifecycleState.ExitRequested)
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
                CFIPClean89BrokerPositionSnapshot snapshot,
                CFIPClean89PositionRecord record,
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
                        CFIPClean89PositionLifecycleState.ExitRequested)
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
                CFIPClean89PositionRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsProtectionRetryDue(utc) ||
                    (!record.ExpectedStopLoss.HasValue &&
                     !record.ExpectedTakeProfit.HasValue))
                    return false;
    
                Queue(
                    new CFIPClean89PositionAction(
                        record.BrokerPositionId,
                        CFIPClean89PositionActionKind.RestoreProtection,
                        record.ExpectedStopLoss,
                        record.ExpectedTakeProfit,
                        reason));
    
                record.MarkProtectionActionQueued();
                record.MarkProtectionRecoveryRequired();
                return true;
            }
    
            private void QueueCloseIfDue(
                CFIPClean89PositionRecord record,
                string reason,
                DateTime utc)
            {
                if (record == null ||
                    !record.IsCloseRetryDue(utc))
                    return;
    
                Queue(
                    new CFIPClean89PositionAction(
                        record.BrokerPositionId,
                        CFIPClean89PositionActionKind.Close,
                        null,
                        null,
                        reason));
    
                record.MarkCloseActionQueued();
            }
    
            public void EvaluateRetryActions(DateTime utc)
            {
                var snapshot =
                    new List<CFIPClean89PositionRecord>(
                        _records.Values);
    
                for (int i = 0; i < snapshot.Count; i++)
                {
                    CFIPClean89PositionRecord record = snapshot[i];
    
                    if (record.State ==
                        CFIPClean89PositionLifecycleState.Orphan ||
                        record.State ==
                        CFIPClean89PositionLifecycleState.Closed ||
                        record.State ==
                        CFIPClean89PositionLifecycleState.Reconciled)
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
                        CFIPClean89PositionLifecycleState.RecoveryRequired ||
                        record.State ==
                        CFIPClean89PositionLifecycleState.ProtectionRecoveryRequired)
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
                            CFIPClean89PositionActionKind.RestoreProtection)
                    {
                        _actions.RemoveAt(i);
                        removed = true;
                    }
                }
    
                string prefix =
                    brokerPositionId +
                    "|" +
                    CFIPClean89PositionActionKind.RestoreProtection.ToString();
    
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
    
            private void Queue(CFIPClean89PositionAction action)
            {
                string key =
                    action.BrokerPositionId +
                    "|" +
                    action.Kind.ToString();
    
                if (_actionKeys.Add(key))
                    _actions.Add(action);
            }
    
            private void SetExpectedStop(
                CFIPClean89PositionRecord record,
                double? value)
            {
                if (record != null && value.HasValue)
                    record.SetExpectedProtection(value, null);
            }
    
            private void SetExpectedTarget(
                CFIPClean89PositionRecord record,
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
                        "CFIP89|",
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
