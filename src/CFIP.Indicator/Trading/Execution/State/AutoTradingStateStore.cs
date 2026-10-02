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

                                    SyncQuickExecutionControls();
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

                                    SyncQuickExecutionControls();
                                }
    }
}
