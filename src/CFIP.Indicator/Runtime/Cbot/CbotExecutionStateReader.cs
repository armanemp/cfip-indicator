using System;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private CbotExecutionStateSnapshot _cBotExecutionState;
        private CbotPresenceSnapshot _cBotPresence;
        private DateTime _nextCbotStateReadUtc = DateTime.MinValue;
        private string _cBotStateReadError = string.Empty;

        private bool TryFindChartCbot(
            out ChartRobot robot,
            out string reason)
        {
            robot = null;
            reason = "CBOT NOT ATTACHED";

            int count = 0;

            try
            {
                foreach (ChartRobot candidate in ChartRobots)
                {
                    if (candidate == null)
                        continue;

                    bool instanceNameMatches =
                        string.Equals(
                            candidate.Name,
                            CbotIdentity.DisplayName,
                            StringComparison.Ordinal);

                    bool typeNameMatches =
                        candidate.Type != null &&
                        string.Equals(
                            candidate.Type.Name,
                            CbotIdentity.TypeName,
                            StringComparison.Ordinal);

                    if (!instanceNameMatches &&
                        !typeNameMatches)
                        continue;

                    robot = candidate;
                    count++;
                }
            }
            catch (Exception ex)
            {
                reason =
                    "CBOT PRESENCE READ FAILED • " +
                    ex.Message;
                return false;
            }

            if (count == 0)
            {
                reason = "CBOT NOT ATTACHED";
                return false;
            }

            if (count > 1)
            {
                robot = null;
                reason = "CBOT AMBIGUOUS • MULTIPLE INSTANCES";
                return false;
            }

            if (string.IsNullOrWhiteSpace(robot.InstanceId))
            {
                robot = null;
                reason = "CBOT INSTANCE ID UNAVAILABLE";
                return false;
            }

            return true;
        }

        private bool IsChartCbotRunning(
            out ChartRobot robot,
            out string state,
            out string reason)
        {
            state = "UNKNOWN";

            if (!TryFindChartCbot(
                    out robot,
                    out reason))
                return false;

            state =
                robot.State.ToString() ?? "UNKNOWN";

            return string.Equals(
                state,
                "Running",
                StringComparison.OrdinalIgnoreCase);
        }


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

                string presencePayload =
                    LocalStorage.GetString(
                        CbotExecutionStateBusKey.ForSymbol(
                            SymbolName),
                        LocalStorageScope.Device);

                if (CbotExecutionStateCodec.TryDeserializePresence(
                        presencePayload,
                        out CbotPresenceSnapshot presence) &&
                    presence != null &&
                    string.Equals(
                        presence.Symbol,
                        SymbolName,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        presence.CbotTypeName,
                        CbotIdentity.TypeName,
                        StringComparison.Ordinal))
                {
                    _cBotPresence = presence;
                }

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
                        _cBotExecutionState.CbotAutoTradingEnabled !=
                            snapshot.CbotAutoTradingEnabled ||
                        _cBotExecutionState.CbotAutomaticOrdersEnabled !=
                            snapshot.CbotAutomaticOrdersEnabled ||
                        _cBotExecutionState.EffectiveAutoTradingEnabled !=
                            snapshot.EffectiveAutoTradingEnabled ||
                        _cBotExecutionState.EffectiveAutomaticOrdersEnabled !=
                            snapshot.EffectiveAutomaticOrdersEnabled ||
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
                        "CBOT HEARTBEAT PENDING";
                }
                else
                {
                    _cBotStateReadError =
                        "CBOT HEARTBEAT INVALID";
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

            if (HasFreshCbotHeartbeat())
                return
                    "CBOT CONNECTED • HEARTBEAT LIVE • " +
                    CbotExecutionStatePanelText();

            if (HasFreshCbotPresence())
                return
                    "CBOT DETECTED • " +
                    (_cBotPresence.State ?? "RUNNING") +
                    " • INDICATOR BIND " +
                    (string.Equals(
                        _cBotPresence.BoundIndicatorInstanceId,
                        InstanceId,
                        StringComparison.Ordinal)
                        ? "RESOLVED"
                        : string.IsNullOrWhiteSpace(
                            _cBotPresence.BoundIndicatorInstanceId)
                            ? "WAITING"
                            : "OTHER INSTANCE");

            ChartRobot chartRobot;
            string chartState;
            string chartReason;

            if (!IsChartCbotRunning(
                    out chartRobot,
                    out chartState,
                    out chartReason))
            {
                if (chartRobot != null)
                    return
                        "CBOT " +
                        chartState.ToUpperInvariant();

                return
                    chartReason ==
                    "CBOT NOT ATTACHED"
                        ? "CBOT NOT ATTACHED • START cBot ON THIS CHART"
                        : chartReason;
            }

            if (_cBotExecutionState == null)
                return
                    "CBOT CONNECTING • HEARTBEAT PENDING";

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

            return
                "CBOT CONNECTED • " +
                CbotExecutionStatePanelText();
        }

        private bool IsCbotExecutionStateFresh()
        {
            if (_cBotExecutionState == null ||
                !string.Equals(
                    _cBotExecutionState.IndicatorInstanceId,
                    InstanceId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    _cBotExecutionState.Symbol,
                    SymbolName,
                    StringComparison.Ordinal))
                return false;

            double ageSeconds =
                (TimeInUtc -
                 _cBotExecutionState.ObservedUtc).TotalSeconds;

            return
                ageSeconds >= 0 &&
                ageSeconds <= 3.0;
        }

        private bool HasFreshCbotHeartbeat()
        {
            return IsCbotExecutionStateFresh();
        }

        private bool HasFreshCbotPresence()
        {
            if (_cBotPresence == null)
                return false;

            double ageSeconds =
                (TimeInUtc -
                 _cBotPresence.ObservedUtc).TotalSeconds;

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

            string lifecycle =
                string.IsNullOrWhiteSpace(_cBotExecutionState.LifecycleState)
                    ? "UNKNOWN"
                    : _cBotExecutionState.LifecycleState;

            string accountMode =
                string.IsNullOrWhiteSpace(_cBotExecutionState.ExecutionAccountMode)
                    ? (_cBotExecutionState.DemoAccount ? "DEMO" : "LIVE")
                    : _cBotExecutionState.ExecutionAccountMode;

            string protection =
                string.IsNullOrWhiteSpace(_cBotExecutionState.ProtectionState)
                    ? "UNKNOWN"
                    : _cBotExecutionState.ProtectionState;

            return
                "CBOT " +
                accountMode + " • " +
                lifecycle +
                " • " +
                protection +
                (_cBotExecutionState.RecoveryRequired
                    ? " • RECOVERY"
                    : "") +
                " • " +
                modes +
                " • POS " +
                Math.Max(0, _cBotExecutionState.ManagedPositions) +
                " • PEND " +
                Math.Max(0, _cBotExecutionState.ManagedPendingOrders) +
                " • " +
                ageSeconds.ToString("F1") +
                "s" +
                (_cBotExecutionState.RecoveryRequired &&
                 !string.IsNullOrWhiteSpace(_cBotExecutionState.RecoveryReason)
                    ? " • " + _cBotExecutionState.RecoveryReason
                    : "");
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
                HasFreshCbotHeartbeat() &&
                (_cBotExecutionState.MarketExecutionEnabled ||
                 _cBotExecutionState.AggressiveExecutionEnabled);
        }

        private bool CbotCanPendingExecute()
        {
            RefreshCbotExecutionStateIfDue();

            return
                HasFreshCbotHeartbeat() &&
                (_cBotExecutionState.PendingStopExecutionEnabled ||
                 _cBotExecutionState.PendingLimitExecutionEnabled);
        }

        private bool CbotCanManage()
        {
            RefreshCbotExecutionStateIfDue();

            return
                HasFreshCbotHeartbeat() &&
                _cBotExecutionState.ManagementExecutionEnabled;
        }

        private Color CbotExecutionStatePanelColor()
        {
            RefreshCbotExecutionStateIfDue();

            if (!IsCbotExecutionStateFresh())
                return PanelWarningColor;

            if (_cBotExecutionState.RecoveryRequired)
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
