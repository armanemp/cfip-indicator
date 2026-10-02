using System;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private CbotExecutionStateSnapshot _cBotExecutionState;
        private DateTime _nextCbotStateReadUtc = DateTime.MinValue;
        private string _cBotStateReadError = string.Empty;

        private void RefreshCbotExecutionStateIfDue(
            bool force = false)
        {
            DateTime now = TimeInUtc;

            if (!force &&
                now < _nextCbotStateReadUtc)
                return;

            _nextCbotStateReadUtc =
                now.AddMilliseconds(750);

            try
            {
                LocalStorage.Reload(
                    LocalStorageScope.Device);

                string payload =
                    LocalStorage.GetString(
                        CbotExecutionStateBusKey.ForIndicatorInstance(
                            InstanceId),
                        LocalStorageScope.Device);

                if (CbotExecutionStateCodec.TryDeserialize(
                        payload,
                        out CbotExecutionStateSnapshot snapshot) &&
                    snapshot != null &&
                    string.Equals(
                        snapshot.IndicatorInstanceId,
                        InstanceId,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        snapshot.Symbol,
                        SymbolName,
                        StringComparison.Ordinal))
                {
                    _cBotExecutionState = snapshot;
                    _cBotStateReadError = string.Empty;
                }
                else if (string.IsNullOrWhiteSpace(payload))
                {
                    _cBotStateReadError =
                        "CBOT NOT ATTACHED";
                }
                else
                {
                    _cBotStateReadError =
                        "CBOT STATE INVALID";
                }
            }
            catch (Exception ex)
            {
                _cBotStateReadError =
                    "CBOT STATE READ FAILED • " +
                    ex.Message;
            }
        }

        private bool TryReadChartCbot(
            out string instanceId,
            out string state,
            out int matchingCount)
        {
            instanceId = string.Empty;
            state = string.Empty;
            matchingCount = 0;

            foreach (var candidate in ChartRobots)
            {
                if (candidate == null ||
                    !string.Equals(
                        candidate.Name,
                        CbotIdentity.DisplayName,
                        StringComparison.Ordinal))
                    continue;

                matchingCount++;

                if (matchingCount == 1)
                {
                    instanceId =
                        candidate.InstanceId ?? string.Empty;

                    state =
                        candidate.State == null
                            ? string.Empty
                            : candidate.State.ToString();
                }
            }

            return matchingCount == 1 &&
                   !string.IsNullOrWhiteSpace(instanceId);
        }

        private bool IsCbotChartRunning()
        {
            TryReadChartCbot(
                out string instanceId,
                out string state,
                out int matchingCount);

            return
                matchingCount == 1 &&
                string.Equals(
                    state,
                    "Running",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(instanceId);
        }

        private string CbotConnectionPanelText()
        {
            RefreshCbotExecutionStateIfDue();

            TryReadChartCbot(
                out string instanceId,
                out string state,
                out int matchingCount);

            if (matchingCount == 0)
                return
                    "CBOT NOT ATTACHED • ATTACH TO THIS CHART";

            if (matchingCount > 1)
                return
                    "CBOT AMBIGUOUS • " +
                    matchingCount.ToString(
                        CultureInfo.InvariantCulture) +
                    " INSTANCES";

            if (!string.Equals(
                    state,
                    "Running",
                    StringComparison.OrdinalIgnoreCase))
                return
                    "CBOT " +
                    (string.IsNullOrWhiteSpace(state)
                        ? "UNKNOWN"
                        : state.ToUpperInvariant());

            if (string.IsNullOrWhiteSpace(instanceId) ||
                !IsCbotExecutionStateFresh())
                return
                    "CBOT CONNECTING • HEARTBEAT PENDING";

            return
                "CBOT CONNECTED • " +
                CbotExecutionStatePanelText();
        }

        private bool IsCbotExecutionStateFresh()
        {
            if (_cBotExecutionState == null)
                return false;

            double ageSeconds =
                (TimeInUtc -
                 _cBotExecutionState.ObservedUtc).TotalSeconds;

            return
                ageSeconds >= 0 &&
                ageSeconds <= 3.0;
        }

        private string CbotExecutionStatePanelText()
        {
            RefreshCbotExecutionStateIfDue();

            if (_cBotExecutionState == null)
                return
                    string.IsNullOrWhiteSpace(_cBotStateReadError)
                        ? "CBOT HEARTBEAT UNAVAILABLE"
                        : _cBotStateReadError;

            double ageSeconds =
                Math.Max(
                    0,
                    (TimeInUtc -
                     _cBotExecutionState.ObservedUtc).TotalSeconds);

            if (ageSeconds > 3.0)
                return
                    "CBOT STALE • " +
                    ageSeconds.ToString("F1") +
                    "s";

            string modes =
                (_cBotExecutionState.MarketExecutionEnabled ? "MKT" : "") +
                (_cBotExecutionState.AggressiveExecutionEnabled ? "/AGG" : "") +
                (_cBotExecutionState.PendingStopExecutionEnabled ? "/STOP" : "") +
                (_cBotExecutionState.PendingLimitExecutionEnabled ? "/LIMIT" : "") +
                (_cBotExecutionState.ManagementExecutionEnabled ? "/MGMT" : "");

            modes =
                modes.Trim('/');

            if (string.IsNullOrWhiteSpace(modes))
                modes = "NONE";

            return
                "CBOT " +
                (_cBotExecutionState.RuntimeState ?? "UNKNOWN") +
                " • " +
                modes +
                " • POS " +
                Math.Max(0, _cBotExecutionState.ManagedPositions) +
                " • PEND " +
                Math.Max(0, _cBotExecutionState.ManagedPendingOrders) +
                " • " +
                ageSeconds.ToString("F1") +
                "s";
        }

        private string CbotExecutionScenarioPanelText()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return "SCENARIO NONE";

            string scenario =
                _cBotExecutionState.ExecutionScenarioId ?? string.Empty;

            return
                "SCENARIO " +
                (string.IsNullOrWhiteSpace(scenario)
                    ? "NONE"
                    : scenario);
        }

        private bool CbotCanMarketExecute()
        {
            RefreshCbotExecutionStateIfDue();

            return
                IsCbotChartRunning() &&
                IsCbotExecutionStateFresh() &&
                (_cBotExecutionState.MarketExecutionEnabled ||
                 _cBotExecutionState.AggressiveExecutionEnabled);
        }

        private bool CbotCanPendingExecute()
        {
            RefreshCbotExecutionStateIfDue();

            return
                IsCbotChartRunning() &&
                IsCbotExecutionStateFresh() &&
                (_cBotExecutionState.PendingStopExecutionEnabled ||
                 _cBotExecutionState.PendingLimitExecutionEnabled);
        }

        private bool CbotCanManage()
        {
            RefreshCbotExecutionStateIfDue();

            return
                IsCbotChartRunning() &&
                IsCbotExecutionStateFresh() &&
                _cBotExecutionState.ManagementExecutionEnabled;
        }

        private Color CbotExecutionStatePanelColor()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotChartRunning())
                return PanelWarningColor;

            if (!IsCbotExecutionStateFresh())
                return PanelWarningColor;

            string state =
                _cBotExecutionState.RuntimeState ?? string.Empty;

            if (string.Equals(
                    state,
                    "ACTIVE",
                    StringComparison.OrdinalIgnoreCase))
                return TpLineColor;

            if (string.Equals(
                    state,
                    "BLOCKED",
                    StringComparison.OrdinalIgnoreCase))
                return PanelWarningColor;

            return
                _cBotExecutionState.MarketExecutionEnabled ||
                _cBotExecutionState.PendingStopExecutionEnabled ||
                _cBotExecutionState.PendingLimitExecutionEnabled ||
                _cBotExecutionState.AggressiveExecutionEnabled ||
                _cBotExecutionState.ManagementExecutionEnabled
                    ? PanelAccentColor
                    : PanelSecondaryTextColor;
        }
    }
}
