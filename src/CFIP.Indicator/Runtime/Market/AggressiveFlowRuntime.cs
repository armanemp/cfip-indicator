using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private Ticks _aggressiveFlowTicks;
        private readonly AggressiveFlowAnalyzer _aggressiveFlowAnalyzer =
            new AggressiveFlowAnalyzer();
        private AggressiveFlowSnapshot _aggressiveFlowSnapshot;
        private DateTime _lastAggressiveFlowSnapshotUtc = DateTime.MinValue;

        private void StartAggressiveFlowRuntime()
        {
            if (_aggressiveFlowTicks != null)
                return;

            _aggressiveFlowTicks =
                MarketData.GetTicks(Symbol.Name);

            if (_aggressiveFlowTicks == null)
                return;

            _aggressiveFlowTicks.Tick += OnAggressiveFlowTick;
            RefreshAggressiveFlowSnapshot(TimeInUtc);
        }

        private void OnAggressiveFlowTick(TicksTickEventArgs args)
        {
            try
            {
                if (args == null)
                    return;

                _aggressiveFlowAnalyzer.ProcessTick(
                    args.Ticks.LastTick);

                RefreshAggressiveFlowSnapshot(
                    args.Ticks.LastTick.Time.ToUniversalTime());
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP aggressive-flow tick processing failed: {0}",
                    ex.Message);
            }
        }

        private void RefreshAggressiveFlowSnapshot(DateTime utc)
        {
            _aggressiveFlowSnapshot =
                _aggressiveFlowAnalyzer.Snapshot(
                    utc.ToUniversalTime());
            _lastAggressiveFlowSnapshotUtc =
                utc.ToUniversalTime();
        }

        private AggressiveFlowSnapshot GetAggressiveFlowSnapshot()
        {
            if (_lastAggressiveFlowSnapshotUtc == DateTime.MinValue)
                RefreshAggressiveFlowSnapshot(TimeInUtc);

            return _aggressiveFlowSnapshot;
        }

        private void StopAggressiveFlowRuntime()
        {
            if (_aggressiveFlowTicks == null)
                return;

            try
            {
                _aggressiveFlowTicks.Tick -= OnAggressiveFlowTick;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP aggressive-flow unsubscribe failed: {0}",
                    ex.Message);
            }
            finally
            {
                _aggressiveFlowTicks = null;
                _aggressiveFlowSnapshot =
                    new AggressiveFlowSnapshot(
                        0,
                        0,
                        0,
                        0,
                        0,
                        DateTime.MinValue);
                _lastAggressiveFlowSnapshotUtc =
                    DateTime.MinValue;
            }
        }
    }
}
