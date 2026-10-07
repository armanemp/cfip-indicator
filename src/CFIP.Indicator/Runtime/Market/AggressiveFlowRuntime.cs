using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private static readonly TimeSpan AggressiveFlowFreshness = TimeSpan.FromSeconds(5);

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
            _aggressiveFlowTicks.HistoryLoaded += OnAggressiveFlowHistoryLoaded;
            _aggressiveFlowTicks.Reloaded += OnAggressiveFlowReloaded;

            SeedAggressiveFlowHistory();

            try
            {
                _aggressiveFlowTicks.LoadMoreHistoryAsync();
            }
            catch (Exception ex)
            {
                Print("CFIP aggressive-flow history request failed: {0}", ex.Message);
            }
        }

        private void OnAggressiveFlowTick(TicksTickEventArgs args)
        {
            try
            {
                if (args == null || args.Ticks == null)
                    return;

                Tick tick = args.Ticks.LastTick;
                _aggressiveFlowAnalyzer.ProcessTick(tick);
                RefreshAggressiveFlowSnapshot(tick.Time.ToUniversalTime());
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP aggressive-flow tick processing failed: {0}",
                    ex.Message);
            }
        }

        private void OnAggressiveFlowHistoryLoaded(TicksHistoryLoadedEventArgs args)
        {
            try { SeedAggressiveFlowHistory(); }
            catch (Exception ex) { Print("CFIP aggressive-flow history reseed failed: {0}", ex.Message); }
        }

        private void OnAggressiveFlowReloaded(TicksHistoryLoadedEventArgs args)
        {
            try { SeedAggressiveFlowHistory(); }
            catch (Exception ex) { Print("CFIP aggressive-flow reconnect reseed failed: {0}", ex.Message); }
        }

        private void SeedAggressiveFlowHistory()
        {
            if (_aggressiveFlowTicks == null || _aggressiveFlowTicks.Count <= 0)
            {
                _aggressiveFlowAnalyzer.Reset();
                RefreshAggressiveFlowSnapshot(TimeInUtc);
                return;
            }

            DateTime nowUtc = TimeInUtc.ToUniversalTime();
            DateTime cutoff = nowUtc - TimeSpan.FromSeconds(30);
            List<Tick> recent = new List<Tick>(AggressiveFlowAnalyzer.MaxSamples);

            for (int offset = 0;
                 offset < _aggressiveFlowTicks.Count && offset < AggressiveFlowAnalyzer.MaxSamples;
                 offset++)
            {
                Tick tick = _aggressiveFlowTicks.Last(offset);
                if (tick.Time.ToUniversalTime() < cutoff)
                    break;
                recent.Add(tick);
            }

            _aggressiveFlowAnalyzer.Reset();
            for (int i = recent.Count - 1; i >= 0; i--)
                _aggressiveFlowAnalyzer.ProcessTick(recent[i]);

            RefreshAggressiveFlowSnapshot(nowUtc);
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

        private bool TryGetFreshAggressiveFlowSnapshot(out AggressiveFlowSnapshot snapshot)
        {
            snapshot = GetAggressiveFlowSnapshot();
            return snapshot.IsFresh(TimeInUtc, AggressiveFlowFreshness);
        }

        private void StopAggressiveFlowRuntime()
        {
            if (_aggressiveFlowTicks == null)
                return;

            try
            {
                _aggressiveFlowTicks.Tick -= OnAggressiveFlowTick;
                _aggressiveFlowTicks.HistoryLoaded -= OnAggressiveFlowHistoryLoaded;
                _aggressiveFlowTicks.Reloaded -= OnAggressiveFlowReloaded;
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
                _aggressiveFlowAnalyzer.Reset();
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
