using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int _runtimeFaultCount;
        private DateTime _lastRuntimeFaultUtc = DateTime.MinValue;
        private string _lastRuntimeFaultMessage = string.Empty;

        private void HandleRuntimeFault(
            Exception exception,
            int index,
            string source)
        {
            if (exception == null)
                return;

            _runtimeFaultCount++;
            _lastRuntimeFaultUtc = TimeInUtc;
            _lastRuntimeFaultMessage =
                exception.GetType().Name +
                ": " +
                exception.Message;

            // A stage fault must fail closed for automatic order creation while
            // allowing independent management, protection and reconciliation stages
            // to continue in the same calculation cycle. Broker-confirmed state
            // remains authoritative.
            _autoTradingEnabledRuntime = false;
            _automaticOrdersEnabledRuntime = false;
            _autoExecutionBlockReason = "RUNTIME FAULT";
            _autoOrdersBlockReason = "RUNTIME FAULT";
            _autoTradingState = "ERROR";
            _autoTradingReason = "RUNTIME FAULT";
            _status = "RUNTIME ERROR";

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
