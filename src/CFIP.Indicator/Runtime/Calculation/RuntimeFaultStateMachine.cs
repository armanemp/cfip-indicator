using System;

namespace cAlgo
{
    public sealed class RuntimeFaultStateMachine
    {
        private const int ClosedBarFailureCircuitThreshold = 4;
        private const int ClosedBarCircuitCooldownSeconds = 60;
        private const int MaximumClosedBarBackoffSeconds = 30;

        private RuntimeFaultState _state =
            RuntimeFaultState.Healthy;

        private bool _entryArmed = true;
        private bool _hasObservedAutoTradingSetting;
        private bool _lastAutoTradingEnabled;
        private bool _cycleFaulted;

        private int _closedBarFailureKey = -1;
        private int _closedBarFailureCount;
        private DateTime _closedBarLastFailureUtc = DateTime.MinValue;
        private DateTime _closedBarNextRetryUtc = DateTime.MinValue;
        private DateTime _closedBarCircuitOpenUntilUtc = DateTime.MinValue;

        public RuntimeFaultState State
        {
            get { return _state; }
        }

        public bool EntryArmed
        {
            get { return _entryArmed; }
        }

        public bool CycleFaulted
        {
            get { return _cycleFaulted; }
        }

        public int ClosedBarFailureCount
        {
            get { return _closedBarFailureCount; }
        }

        public DateTime ClosedBarLastFailureUtc
        {
            get { return _closedBarLastFailureUtc; }
        }

        public DateTime ClosedBarNextRetryUtc
        {
            get { return _closedBarNextRetryUtc; }
        }

        public bool CanAutomaticEntryProceed
        {
            get
            {
                return _state == RuntimeFaultState.Healthy &&
                       _entryArmed;
            }
        }

        public void BeginCycle()
        {
            _cycleFaulted = false;
        }

        public void ObserveAutoTradingSetting(
            bool enabled)
        {
            if (!_hasObservedAutoTradingSetting)
            {
                _hasObservedAutoTradingSetting = true;
                _lastAutoTradingEnabled = enabled;
                _entryArmed = enabled;
                return;
            }

            if (!enabled)
            {
                _entryArmed = false;
                _lastAutoTradingEnabled = false;
                return;
            }

            bool explicitEnableTransition =
                !_lastAutoTradingEnabled &&
                enabled;

            if (explicitEnableTransition &&
                (_state == RuntimeFaultState.Healthy ||
                 _state == RuntimeFaultState.Degraded ||
                 _state == RuntimeFaultState.EntryBlocked ||
                 _state == RuntimeFaultState.Recovering))
            {
                // Turning Auto Trading on is the explicit operator re-arm
                // action after a runtime entry block. A healthy next cycle
                // will still re-detect any newly occurring fault and block
                // execution again.
                _entryArmed = true;
                _state = RuntimeFaultState.Healthy;
                _cycleFaulted = false;
            }

            _lastAutoTradingEnabled = true;
        }

        public void RecordRecoverableFault()
        {
            _cycleFaulted = true;

            if (_state != RuntimeFaultState.EntryBlocked)
                _state = RuntimeFaultState.Degraded;
        }

        public void BlockAutomaticEntry()
        {
            _entryArmed = false;
            _state = RuntimeFaultState.EntryBlocked;
        }

        public bool CanAttemptClosedBarAnalysis(
            int closedBarKey,
            DateTime nowUtc,
            out string reason)
        {
            reason = string.Empty;

            if (closedBarKey < 0)
            {
                reason = "NO CLOSED BAR";
                return false;
            }

            if (_closedBarFailureKey != closedBarKey)
                return true;

            if (nowUtc < _closedBarCircuitOpenUntilUtc)
            {
                reason = "CLOSED-BAR RETRY CIRCUIT";
                return false;
            }

            if (nowUtc < _closedBarNextRetryUtc)
            {
                reason = "CLOSED-BAR RETRY BACKOFF";
                return false;
            }

            return true;
        }

        public void RecordClosedBarAnalysisFailure(
            int closedBarKey,
            DateTime nowUtc)
        {
            _closedBarFailureKey = closedBarKey;
            _closedBarFailureCount = Math.Max(
                1,
                _closedBarFailureCount + 1);
            _closedBarLastFailureUtc = nowUtc;

            int exponent = Math.Min(
                5,
                _closedBarFailureCount - 1);

            int delaySeconds =
                1 << exponent;

            delaySeconds = Math.Min(
                MaximumClosedBarBackoffSeconds,
                delaySeconds);

            _closedBarNextRetryUtc =
                nowUtc.AddSeconds(delaySeconds);

            if (_closedBarFailureCount >=
                ClosedBarFailureCircuitThreshold)
            {
                _closedBarCircuitOpenUntilUtc =
                    nowUtc.AddSeconds(
                        ClosedBarCircuitCooldownSeconds);
            }
        }

        public void RecordClosedBarAnalysisSuccess(
            int closedBarKey)
        {
            if (_closedBarFailureKey != closedBarKey)
                return;

            _closedBarFailureKey = -1;
            _closedBarFailureCount = 0;
            _closedBarLastFailureUtc = DateTime.MinValue;
            _closedBarNextRetryUtc = DateTime.MinValue;
            _closedBarCircuitOpenUntilUtc = DateTime.MinValue;
        }

        public bool HasStaleClosedBarFailure(
            int currentClosedBarKey)
        {
            return _closedBarFailureKey >= 0 &&
                   currentClosedBarKey > _closedBarFailureKey;
        }

        public void MarkManagementReadyForRecovery()
        {
            if (!_cycleFaulted &&
                _state == RuntimeFaultState.EntryBlocked)
            {
                _state = RuntimeFaultState.Recovering;
            }
        }

        public void CompleteCycle()
        {
            if (!_cycleFaulted &&
                _state == RuntimeFaultState.Recovering)
            {
                _state = RuntimeFaultState.Healthy;
            }
        }
    }
}
