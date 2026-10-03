using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RefreshPanelHeader()
        {
            if (_panelHeaderTitle == null)
                return;

            int direction =
                GetAuthoritativeDirection();

            string state =
                GetAuthoritativeState(
                    direction);

            string signal =
                GetCanonicalSignalPanelStatus();

            string cbot =
                ResolvePanelHeaderCbotState();

            string header =
                "CFIP SMART  •  " +
                NormalizePanelHeaderState(
                    state) +
                "  •  M15  •  " +
                cbot +
                "  •  " +
                Server.TimeInUtc.ToString(
                    "HH:mm:ss") +
                " UTC";

            if (string.Equals(
                    header,
                    _panelStableHeader,
                    StringComparison.Ordinal))
                return;

            _panelStableHeader = header;
            _panelStableHeaderSinceUtc =
                Server.TimeInUtc;

            _panelHeaderTitle.Text =
                header;

            _panelHeaderTitle.ForegroundColor =
                ResolvePanelHeaderColor(
                    direction,
                    signal);
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
