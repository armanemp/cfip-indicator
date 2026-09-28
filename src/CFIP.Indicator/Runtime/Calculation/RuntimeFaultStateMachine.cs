namespace cAlgo
{
    public sealed class RuntimeFaultStateMachine
    {
        private RuntimeFaultState _state =
            RuntimeFaultState.Healthy;

        private bool _entryArmed = true;
        private bool _hasObservedAutoTradingSetting;
        private bool _lastAutoTradingEnabled;
        private bool _cycleFaulted;

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
            _entryArmed = false;
            _state = RuntimeFaultState.EntryBlocked;
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
