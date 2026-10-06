// ============================================================================
// CFIP Indicator — AggressiveFlowRuntime.cs
// Single owner for realtime tick-flow lifecycle.
// ============================================================================

using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Ticks _aggressiveFlowTicks;
        private readonly AggressiveFlowAnalyzer _aggressiveFlowAnalyzer =
            new AggressiveFlowAnalyzer();

        private void InitializeAggressiveFlowRuntime()
        {
            try
            {
                _aggressiveFlowTicks =
                    MarketData.GetTicks(Symbol.Name);

                _aggressiveFlowAnalyzer.Attach(
                    _aggressiveFlowTicks);
            }
            catch (System.Exception ex)
            {
                _aggressiveFlowTicks = null;
                Print(
                    "CFIP aggressive flow initialization failed: {0}",
                    ex.Message);
            }
        }

        private AggressiveFlowSnapshot GetAggressiveFlowSnapshot()
        {
            return _aggressiveFlowAnalyzer == null
                ? AggressiveFlowSnapshot.Empty
                : _aggressiveFlowAnalyzer.Snapshot;
        }

        private void StopAggressiveFlowRuntime()
        {
            _aggressiveFlowAnalyzer.Detach();
            _aggressiveFlowTicks = null;
        }
    }
}
