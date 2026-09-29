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

                                    _lifecycleState = state;
                                    _lifecycleReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? state.ToString().ToUpperInvariant()
                                            : reason;

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
