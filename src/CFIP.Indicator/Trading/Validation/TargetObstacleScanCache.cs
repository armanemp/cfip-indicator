using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    internal readonly struct TargetObstacleSwingPoint
    {
        public int Index { get; }
        public double Level { get; }

        public TargetObstacleSwingPoint(int index, double level)
        {
            Index = index;
            Level = level;
        }
    }

    internal readonly struct TargetObstacleEqualPair
    {
        public double FirstLevel { get; }
        public double SecondLevel { get; }
        public double ResolvedLevel { get; }

        public TargetObstacleEqualPair(
            double firstLevel,
            double secondLevel,
            double resolvedLevel)
        {
            FirstLevel = firstLevel;
            SecondLevel = secondLevel;
            ResolvedLevel = resolvedLevel;
        }
    }

    internal sealed class TargetObstacleScanSnapshot
    {
        public TargetObstacleSwingPoint[] SwingPoints { get; }
        public TargetObstacleEqualPair[] EqualPairs { get; }

        public TargetObstacleScanSnapshot(
            TargetObstacleSwingPoint[] swingPoints,
            TargetObstacleEqualPair[] equalPairs)
        {
            SwingPoints = swingPoints ?? Array.Empty<TargetObstacleSwingPoint>();
            EqualPairs = equalPairs ?? Array.Empty<TargetObstacleEqualPair>();
        }

        public static TargetObstacleScanSnapshot Empty
        {
            get
            {
                return new TargetObstacleScanSnapshot(
                    Array.Empty<TargetObstacleSwingPoint>(),
                    Array.Empty<TargetObstacleEqualPair>());
            }
        }
    }

    internal sealed class TargetObstacleScanCacheEntry
    {
        public Bars Bars;
        public TargetObstacleCacheKey Key;
        public TargetObstacleScanSnapshot Snapshot;
        public long UseStamp;
    }

    internal sealed class TargetObstacleScanCache
    {
        private readonly TargetObstacleScanCacheEntry[] _entries =
            new TargetObstacleScanCacheEntry[TargetObstacleCachePolicy.MaximumEntries];

        private readonly List<Bars> _subscribedBars =
            new List<Bars>();

        private long _useStamp;

        public int Hits { get; private set; }
        public int Misses { get; private set; }
        public int Builds { get; private set; }
        public int Evictions { get; private set; }

        public bool TryGetSnapshot(
            Bars bars,
            TargetObstacleCacheKey key,
            out TargetObstacleScanSnapshot snapshot)
        {
            snapshot = null;

            if (bars == null ||
                !TargetObstacleCachePolicy.IsSupportedDirection(key.Direction) ||
                key.Index < 0 ||
                key.Index >= bars.Count)
                return false;

            EnsureSubscribed(bars);
            InvalidateObsoleteSameBars(bars, key);

            for (int i = 0; i < _entries.Length; i++)
            {
                TargetObstacleScanCacheEntry entry = _entries[i];

                if (entry == null ||
                    !ReferenceEquals(entry.Bars, bars) ||
                    !entry.Key.Equals(key))
                    continue;

                if (entry.Snapshot == null)
                {
                    _entries[i] = null;
                    break;
                }

                entry.UseStamp = ++_useStamp;
                Hits++;
                snapshot = entry.Snapshot;
                return true;
            }

            Misses++;
            return false;
        }

        public void StoreSnapshot(
            Bars bars,
            TargetObstacleCacheKey key,
            TargetObstacleScanSnapshot snapshot)
        {
            if (bars == null ||
                snapshot == null ||
                !TargetObstacleCachePolicy.IsSupportedDirection(key.Direction) ||
                key.Index < 0 ||
                key.Index >= bars.Count)
                return;

            EnsureSubscribed(bars);
            InvalidateObsoleteSameBars(bars, key);

            for (int i = 0; i < _entries.Length; i++)
            {
                TargetObstacleScanCacheEntry existing = _entries[i];

                if (existing == null ||
                    !ReferenceEquals(existing.Bars, bars) ||
                    !existing.Key.Equals(key))
                    continue;

                existing.Snapshot = snapshot;
                existing.UseStamp = ++_useStamp;
                return;
            }

            int slot = -1;
            long oldestStamp = long.MaxValue;

            for (int i = 0; i < _entries.Length; i++)
            {
                TargetObstacleScanCacheEntry entry = _entries[i];

                if (entry == null)
                {
                    slot = i;
                    break;
                }

                if (entry.UseStamp < oldestStamp)
                {
                    oldestStamp = entry.UseStamp;
                    slot = i;
                }
            }

            if (slot < 0)
                return;

            if (_entries[slot] != null)
                Evictions++;

            _entries[slot] = new Entry
            {
                Bars = bars,
                Key = key,
                Snapshot = snapshot,
                UseStamp = ++_useStamp
            };

            Builds++;
        }

        public void Clear()
        {
            for (int i = 0; i < _entries.Length; i++)
                _entries[i] = null;
        }

        private void EnsureSubscribed(Bars bars)
        {
            for (int i = 0; i < _subscribedBars.Count; i++)
            {
                if (ReferenceEquals(_subscribedBars[i], bars))
                    return;
            }

            _subscribedBars.Add(bars);
            bars.HistoryLoaded += TargetObstacleCache_BarsHistoryLoaded;
            bars.Reloaded += TargetObstacleCache_BarsReloaded;
        }

        private void TargetObstacleCache_BarsHistoryLoaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateBars(args == null ? null : args.Bars);
        }

        private void TargetObstacleCache_BarsReloaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateBars(args == null ? null : args.Bars);
        }

        private void InvalidateBars(Bars bars)
        {
            if (bars == null)
                return;

            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i] != null &&
                    ReferenceEquals(_entries[i].Bars, bars))
                    _entries[i] = null;
            }
        }

        private void InvalidateObsoleteSameBars(
            Bars bars,
            TargetObstacleCacheKey current)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                TargetObstacleScanCacheEntry entry = _entries[i];

                if (entry != null &&
                    ReferenceEquals(entry.Bars, bars) &&
                    TargetObstacleCachePolicy.IsObsoleteSameBars(entry.Key, current))
                    _entries[i] = null;
            }
        }
    }
}