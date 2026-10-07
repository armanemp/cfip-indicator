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
                // Runtime faults are cycle-local. A recoverable fault blocks
                // automatic entry for the affected calculation cycle only.
                // The next clean cycle can resume without a hidden global
                // trading switch or manual UI re-arm.
                return _state == RuntimeFaultState.Healthy &&
                       !_cycleFaulted &&
                       _entryArmed;
            }
        }

        public void BeginCycle()
        {
            _cycleFaulted = false;

            // A recoverable degraded state is cycle-scoped, but an explicit
            // ENTRY_BLOCKED state remains fail-closed until management confirms
            // recovery. This prevents a new calculation cycle from re-arming
            // entry before broker/runtime state has been reconciled.
            if (_state == RuntimeFaultState.Degraded)
                _state = RuntimeFaultState.Healthy;
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
                _state == RuntimeFaultState.Healthy)
            {
                _entryArmed = true;
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
            // Block the current runtime cycle only. The execution master policy
            // belongs to the cBot and must never be latched off by this
            // Indicator-side recoverable fault boundary.
            _cycleFaulted = true;
            _state = RuntimeFaultState.EntryBlocked;
        }

        public bool RequestExplicitRearm()
        {
            if (_state != RuntimeFaultState.Healthy)
                return false;

            _entryArmed = true;
            _cycleFaulted = false;
            return true;
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
            // Management/protection may complete after a recoverable fault, but
            // the affected cycle remains entry-blocked until its boundary closes.
            if (_state == RuntimeFaultState.EntryBlocked)
                _state = RuntimeFaultState.Recovering;
        }

        public void CompleteCycle()
        {
            if (!_cycleFaulted &&
                _state == RuntimeFaultState.Recovering)
            {
                _state = RuntimeFaultState.Healthy;
                return;
            }

            if (_cycleFaulted &&
                _state == RuntimeFaultState.Recovering)
            {
                // Preserve a diagnostic degraded state for the completed faulted
                // cycle. BeginCycle() clears it before the next decision cycle.
                _state = RuntimeFaultState.Degraded;
            }
        }
    }
}
