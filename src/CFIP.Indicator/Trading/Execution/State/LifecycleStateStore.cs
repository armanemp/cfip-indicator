// CFIP Indicator — LifecycleStateStore.cs
// Single-responsibility execution state module.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SetLifecycleState(
                                    LifecycleState state,
                                    string reason)
                                {
                                    LifecycleState previous =
                                        _lifecycleState;

                                    string nextReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? state.ToString().ToUpperInvariant()
                                            : reason;

                                    bool changed =
                                        previous != state ||
                                        !string.Equals(
                                            _lifecycleReason,
                                            nextReason,
                                            StringComparison.Ordinal);

                                    _lifecycleState = state;
                                    _lifecycleReason = nextReason;

                                    if (changed)
                                        InvalidatePanelExecutionProtectionStateCache();

                                    if (previous != state &&
                                        (state == LifecycleState.RecoveryRequired ||
                                         (previous == LifecycleState.RecoveryRequired &&
                                          (state == LifecycleState.LivePosition ||
                                           state == LifecycleState.Closed ||
                                           state == LifecycleState.PlanReady))))
                                    {
                                        RecordLifecycleTelemetry(
                                            state,
                                            _lifecycleReason);
                                    }
                                }
    }
}
