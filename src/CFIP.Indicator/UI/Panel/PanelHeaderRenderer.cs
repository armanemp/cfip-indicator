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

        private static string NormalizePanelHeaderState(
            string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return "WAIT";

            string normalized =
                state.Trim()
                    .Replace(
                        "SMART ACTION",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

            return string.IsNullOrWhiteSpace(normalized)
                ? "WAIT"
                : normalized;
        }

        private Color ResolvePanelHeaderColor(
            int direction,
            string signal)
        {
            if (signal.IndexOf(
                    "BLOCKED",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return PanelWarningColor;

            if (signal.IndexOf(
                    "ACTIONABLE",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return TpLineColor;

            if (direction != 0)
                return PanelAccentColor;

            return PanelSecondaryTextColor;
        }
    }
}
