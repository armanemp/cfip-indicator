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
                                    _autoTradingState =
                                        string.IsNullOrWhiteSpace(state)
                                            ? "WAIT"
                                            : state.Trim();
                        
                                    _autoTradingReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? ""
                                            : reason.Trim();
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
                                        "/100  •  " +
                                        trace;
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
                                    _autoTradingEnabledRuntime = enabled;
                                    _autoExecutionBlockReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                            : reason;
                                    SyncQuickExecutionControls();
                                }

        private void SetAutomaticOrdersRuntimeState(bool enabled, string reason)
                                {
                                    _automaticOrdersEnabledRuntime = enabled;
                                    _autoOrdersBlockReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                            : reason;
                                    SyncQuickExecutionControls();
                                }
    }
}
