// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PositionRecord
        {
            public string BrokerPositionId { get; private set; }
            public string StrategyId { get; private set; }
            public string SignalId { get; private set; }
            public string PlanId { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public double OriginalEntryPrice { get; private set; }
            public double? ExpectedStopLoss { get; private set; }
            public double? ExpectedTakeProfit { get; private set; }
            public double ActualEntryPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public DateTime AdoptedUtc { get; private set; }
            public DateTime? ClosedUtc { get; private set; }
            public CFIPClean89PositionLifecycleState State { get; private set; }
    
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
    
            public CFIPClean89PositionRecord(
                string brokerPositionId,
                string strategyId,
                string signalId,
                string planId,
                CFIPClean89Direction direction,
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
                State = CFIPClean89PositionLifecycleState.Adopted;
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
                State = CFIPClean89PositionLifecycleState.Protected;
            }
    
            public void MarkProtectionRecoveryRequired()
            {
                State =
                    CFIPClean89PositionLifecycleState.ProtectionRecoveryRequired;
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
                    State = CFIPClean89PositionLifecycleState.Protected;
            }
    
            public void MarkProtectionActionFailed(DateTime utc)
            {
                _protectionActionPending = false;
                _protectionAttempts++;
                _nextProtectionRetryUtc =
                    utc + RetryDelay(_protectionAttempts);
                State =
                    CFIPClean89PositionLifecycleState.RecoveryRequired;
            }
    
            public void RequestClose(DateTime utc)
            {
                _closeRequested = true;
                State = CFIPClean89PositionLifecycleState.ExitRequested;
    
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
                State = CFIPClean89PositionLifecycleState.ExitRequested;
            }
    
            public void MarkCloseActionAccepted(DateTime utc)
            {
                _closeActionPending = false;
                State = CFIPClean89PositionLifecycleState.ExitRequested;
                _nextCloseRetryUtc = utc + TimeSpan.FromSeconds(5);
            }
    
            public void MarkCloseActionFailed(DateTime utc)
            {
                _closeActionPending = false;
                _closeAttempts++;
                _nextCloseRetryUtc =
                    utc + RetryDelay(_closeAttempts);
                State =
                    CFIPClean89PositionLifecycleState.RecoveryRequired;
            }
    
            public void MarkClosed(DateTime utc)
            {
                ClosedUtc = utc;
                _closeRequested = false;
                _closeActionPending = false;
                State = CFIPClean89PositionLifecycleState.Closed;
            }
    
            public void MarkReconciled()
            {
                _closeRequested = false;
                _closeActionPending = false;
                State = CFIPClean89PositionLifecycleState.Reconciled;
            }
    
            public void MarkOrphan()
            {
                _closeRequested = false;
                _closeActionPending = false;
                State = CFIPClean89PositionLifecycleState.Orphan;
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
