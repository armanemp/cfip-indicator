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
                    bool changed =
                        _cBotExecutionState == null ||
                        _cBotExecutionState.Revision != snapshot.Revision ||
                        !string.Equals(
                            _cBotExecutionState.RuntimeState,
                            snapshot.RuntimeState,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            _cBotExecutionState.Reason,
                            snapshot.Reason,
                            StringComparison.Ordinal) ||
                        _cBotExecutionState.MarketExecutionEnabled !=
                            snapshot.MarketExecutionEnabled ||
                        _cBotExecutionState.PendingStopExecutionEnabled !=
                            snapshot.PendingStopExecutionEnabled ||
                        _cBotExecutionState.PendingLimitExecutionEnabled !=
                            snapshot.PendingLimitExecutionEnabled ||
                        _cBotExecutionState.AggressiveExecutionEnabled !=
                            snapshot.AggressiveExecutionEnabled ||
                        _cBotExecutionState.ManagementExecutionEnabled !=
                            snapshot.ManagementExecutionEnabled ||
                        _cBotExecutionState.ManagedPositions !=
                            snapshot.ManagedPositions ||
                        _cBotExecutionState.ManagedPendingOrders !=
                            snapshot.ManagedPendingOrders;

                    _cBotExecutionState = snapshot;
                    _cBotStateReadError = string.Empty;

                    if (changed)
                        InvalidatePanelExecutionProtectionStateCache();
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

        private string CbotConnectionPanelText()
        {
            RefreshCbotExecutionStateIfDue();

            if (_cBotExecutionState == null)
                return
                    "CBOT NOT CONNECTED • START cBot ON THIS CHART";

            double ageSeconds =
                Math.Max(
                    0,
                    (TimeInUtc -
                     _cBotExecutionState.ObservedUtc).TotalSeconds);

            if (ageSeconds > 3.0)
                return
                    "CBOT RECONNECTING • HEARTBEAT STALE • " +
                    ageSeconds.ToString("F1") +
                    "s";

            if (string.Equals(
                    _cBotExecutionState.Reason,
                    "CBOT STOPPED",
                    StringComparison.OrdinalIgnoreCase))
                return "CBOT STOPPED";

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
                IsCbotExecutionStateFresh() &&
                (_cBotExecutionState.MarketExecutionEnabled ||
                 _cBotExecutionState.AggressiveExecutionEnabled);
        }

        private bool CbotCanPendingExecute()
        {
            RefreshCbotExecutionStateIfDue();

            return
                IsCbotExecutionStateFresh() &&
                (_cBotExecutionState.PendingStopExecutionEnabled ||
                 _cBotExecutionState.PendingLimitExecutionEnabled);
        }

        private bool CbotCanManage()
        {
            RefreshCbotExecutionStateIfDue();

            return
                IsCbotExecutionStateFresh() &&
                _cBotExecutionState.ManagementExecutionEnabled;
        }

        private Color CbotExecutionStatePanelColor()
        {
            RefreshCbotExecutionStateIfDue();

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
