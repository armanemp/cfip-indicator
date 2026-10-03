using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RefreshPanelHeader()
        {
            // Header content has one canonical realtime owner. This wrapper
            // remains for legacy call sites while preventing stale duplicate state.
            UpdatePanelHeaderLiveState();
        }

        private string ResolvePanelHeaderCbotState()
        {
            if (IsCbotExecutionStateFresh())
            {
                return
                    "CBOT " +
                    (
                        string.IsNullOrWhiteSpace(
                            _cBotExecutionState.ExecutionAccountMode)
                            ? (
                                _cBotExecutionState.DemoAccount
                                    ? "DEMO"
                                    : "LIVE")
                            : _cBotExecutionState.ExecutionAccountMode) +
                    " " +
                    (
                        string.IsNullOrWhiteSpace(
                            _cBotExecutionState.RuntimeState)
                            ? "UNKNOWN"
                            : _cBotExecutionState.RuntimeState);
            }

            if (HasFreshCbotPresence())
                return "CBOT LINK PENDING";

            return "CBOT LINK";
        }

    }
}
