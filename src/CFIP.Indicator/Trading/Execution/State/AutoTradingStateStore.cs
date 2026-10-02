// CFIP Indicator — AutoTradingStateStore.cs
// Single-responsibility execution state module.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SetAutoTradingState(
                                    string state,
                                    string reason)
                                {
                                    string previousState =
                                        _autoTradingState;

                                    string previousReason =
                                        _autoTradingReason;

                                    _autoTradingState =
                                        string.IsNullOrWhiteSpace(state)
                                            ? "WAIT"
                                            : state.Trim();
                        
                                    _autoTradingReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? ""
                                            : reason.Trim();

                                    if (!string.Equals(
                                            previousState,
                                            _autoTradingState,
                                            StringComparison.OrdinalIgnoreCase) ||
                                        !string.Equals(
                                            previousReason,
                                            _autoTradingReason,
                                            StringComparison.OrdinalIgnoreCase))
                                    {
                                        InvalidatePanelExecutionProtectionStateCache();

                                        ArchiveRuntimeExecution(
                                            "AUTO_STATE",
                                            Math.Max(
                                                -1,
                                                _lastEvaluatedM5),
                                            _autoTradingState,
                                            _autoTradingReason);
                                    }
                                }

        private string AutoTradingPanelLine()
                                {
                                    if (!AutoTradingEnabled)
                                        return
                                            "AUTO TRADING  •  OFF  •  MANUAL REVIEW" +
                                            (AutomaticOrdersEnabled
                                                ? "  •  ORDERS ON"
                                                : "  •  ORDERS OFF");
                        
                                    string state =
                                        string.IsNullOrWhiteSpace(_autoTradingState)
                                            ? "ARMED"
                                            : _autoTradingState;
                        
                                    string trace =
                                        string.IsNullOrWhiteSpace(
                                            _lastExecutionTelemetryPath)
                                            ? "TRACE IDLE"
                                            : "TRACE " +
                                              _lastExecutionTelemetryState +
                                              " • " +
                                              _lastExecutionTelemetryPath;

                                    return
                                        "AUTO TRADING  •  ON  •  " +
                                        state +
                                        "  •  " +
                                        (HasTradingPermission() ? "PERM OK" : "PERM OFF") +
                                        "  •  " +
                                        (AutomaticOrdersEnabled ? "ORDERS ON" : "ORDERS OFF") +
                                        "  •  SMART EXEC " +
                                        _marketSuitabilityScore +
                                        "/100  • SCENARIO " +
                                        (string.IsNullOrWhiteSpace(_activeExecutionScenarioId)
                                            ? "NONE"
                                            : _activeExecutionScenarioId) +
                                        "  •  " +
                                        trace +
                                        (ShowNewsRiskStatus
                                            ? "  •  " +
                                              NewsRiskPanelLine()
                                            : "");
                                }

        private Color AutoTradingPanelColor()
                                {
                                    if (!AutoTradingEnabled)
                                        return PanelMutedTextColor;
                        
                                    if (string.Equals(
                                            _autoTradingState,
                                            "EXECUTED",
                                            StringComparison.OrdinalIgnoreCase))
                                        return TpLineColor;

                                    if (string.Equals(
                                            _autoTradingState,
                                            "RECOVERY",
                                            StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(
                                            _autoTradingState,
                                            "BLOCKED",
                                            StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(
                                            _autoTradingState,
                                            "ERROR",
                                            StringComparison.OrdinalIgnoreCase))
                                        return PanelWarningColor;
                        
                                    return PanelAccentColor;
                                }

                                private void SetAutoTradingRuntimeState(bool enabled, string reason)
                                {
                                    bool changed =
                                        _autoTradingEnabledRuntime != enabled;

                                    _autoTradingEnabledRuntime = enabled;
                                    _autoExecutionBlockReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                            : reason;

                                    if (changed)
                                        InvalidatePanelExecutionProtectionStateCache();

                                }

        private void SetAutomaticOrdersRuntimeState(bool enabled, string reason)
                                {
                                    bool changed =
                                        _automaticOrdersEnabledRuntime != enabled;

                                    _automaticOrdersEnabledRuntime = enabled;
                                    _autoOrdersBlockReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                            : reason;

                                    if (changed)
                                        InvalidatePanelExecutionProtectionStateCache();

                                }
    }
}
