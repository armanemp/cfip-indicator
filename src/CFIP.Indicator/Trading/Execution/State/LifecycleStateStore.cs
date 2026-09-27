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
                                    _lifecycleState = state;
                                    _lifecycleReason =
                                        string.IsNullOrWhiteSpace(reason)
                                            ? state.ToString().ToUpperInvariant()
                                            : reason;
                                }
    }
}
