using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int _runtimeFaultCount;
        private DateTime _lastRuntimeFaultUtc = DateTime.MinValue;
        private string _lastRuntimeFaultMessage = string.Empty;
        private readonly RuntimeFaultStateMachine _runtimeFaultStateMachine =
            new RuntimeFaultStateMachine();

        private RuntimeFaultState CurrentRuntimeFaultState
        {
            get { return _runtimeFaultStateMachine.State; }
        }

        private void BeginRuntimeFaultCycle()
        {
            _runtimeFaultStateMachine.BeginCycle();
            ApplyRuntimeFaultState();
        }

        private void MarkRuntimeManagementReadyForRecovery()
        {
            _runtimeFaultStateMachine.MarkManagementReadyForRecovery();
            ApplyRuntimeFaultState();
        }

        private void CompleteRuntimeFaultCycle()
        {
            _runtimeFaultStateMachine.CompleteCycle();
            ApplyRuntimeFaultState();
        }

        private void ApplyRuntimeFaultState()
        {
            RuntimeFaultState state =
                CurrentRuntimeFaultState;

            if (state == RuntimeFaultState.Healthy)
                return;

            if (state == RuntimeFaultState.Recovering)
            {
                _autoExecutionBlockReason =
                    "RUNTIME RECOVERY";
                _autoOrdersBlockReason =
                    "RUNTIME RECOVERY";

                SetAutoTradingState(
                    "RECOVERING",
                    "RUNTIME RECOVERY");
                _autoTradingReason =
                    "RUNTIME RECOVERY";
                _status =
                    "RUNTIME RECOVERING";
                return;
            }

            if (state == RuntimeFaultState.Degraded)
            {
                _autoExecutionBlockReason =
                    "RUNTIME DEGRADED";
                _autoOrdersBlockReason =
                    "RUNTIME DEGRADED";

                SetAutoTradingState(
                    "DEGRADED",
                    "RUNTIME DEGRADED");
                _autoTradingReason =
                    "RUNTIME DEGRADED";
                _status =
                    "RUNTIME DEGRADED";
                return;
            }

            _autoExecutionBlockReason =
                "RUNTIME ENTRY BLOCKED";
            _autoOrdersBlockReason =
                "RUNTIME ENTRY BLOCKED";

            SetAutoTradingState(
                "ENTRY_BLOCKED",
                "RUNTIME FAULT • EXPLICIT RE-ARM REQUIRED");
            _autoTradingReason =
                "RUNTIME FAULT • EXPLICIT RE-ARM REQUIRED";
            _status =
                "RUNTIME ENTRY BLOCKED";
        }

        private void HandleRuntimeFault(
            Exception exception,
            int index,
            string source)
        {
            if (exception == null)
                return;

            _runtimeFaultStateMachine.RecordRecoverableFault();
            _runtimeFaultStateMachine.BlockAutomaticEntry();
            _runtimeFaultCount++;
            _lastRuntimeFaultUtc = TimeInUtc;
            _lastRuntimeFaultMessage =
                exception.GetType().Name +
                ": " +
                exception.Message;

            ApplyRuntimeFaultState();

            Print(
                "CFIP runtime fault #{0} [{1}] at index {2}: {3}",
                _runtimeFaultCount,
                source,
                index,
                exception.ToString());

            try
            {
                RenderPanel();
            }
            catch (Exception panelException)
            {
                Print(
                    "CFIP runtime-fault panel update failed: {0}",
                    panelException.ToString());
            }
        }

    }
}
