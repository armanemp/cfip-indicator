using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class MarketRegimeFrameCache
    {
        private const int Capacity = 8;

        private readonly Entry[] _entries =
            new Entry[Capacity];

        public bool TryGet(
            Bars bars,
            int index,
            out MarketRegimeSnapshot snapshot)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];

                if (entry == null ||
                    !ReferenceEquals(entry.Bars, bars) ||
                    entry.Index != index)
                    continue;

                if (!IsCompatible(entry, bars))
                {
                    _entries[i] = null;
                    break;
                }

                snapshot = entry.Snapshot;
                return snapshot != null;
            }

            snapshot = null;
            return false;
        }

        public void Set(
            Bars bars,
            int index,
            MarketRegimeSnapshot snapshot)
        {
            if (bars == null ||
                snapshot == null ||
                index < 0 ||
                index >= bars.Count)
                return;

            int slot = -1;

            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i] != null &&
                    ReferenceEquals(
                        _entries[i].Bars,
                        bars))
                {
                    slot = i;
                    break;
                }

                if (_entries[i] == null &&
                    slot < 0)
                    slot = i;
            }

            if (slot < 0)
                slot = 0;

            _entries[slot] =
                new Entry(
                    bars,
                    index,
                    snapshot,
                    bars.Count,
                    Fingerprint(
                        bars,
                        0),
                    Fingerprint(
                        bars,
                        index));
        }

        public void Invalidate()
        {
            for (int i = 0; i < _entries.Length; i++)
                _entries[i] = null;
        }

        private static bool IsCompatible(
            Entry entry,
            Bars bars)
        {
            if (entry == null ||
                bars == null ||
                !ReferenceEquals(entry.Bars, bars) ||
                entry.BarCount > bars.Count)
                return false;

            return
                entry.First == Fingerprint(
                    bars,
                    0) &&
                entry.Last == Fingerprint(
                    bars,
                    entry.Index);
        }

        private static BarFingerprint Fingerprint(
            Bars bars,
            int index)
        {
            return new BarFingerprint(
                bars.OpenTimes[index],
                bars.OpenPrices[index],
                bars.HighPrices[index],
                bars.LowPrices[index],
                bars.ClosePrices[index],
                bars.TickVolumes[index]);
        }

        private sealed class Entry
        {
            public Bars Bars { get; }
            public int Index { get; }
            public MarketRegimeSnapshot Snapshot { get; }
            public int BarCount { get; }
            public BarFingerprint First { get; }
            public BarFingerprint Last { get; }

            public Entry(
                Bars bars,
                int index,
                MarketRegimeSnapshot snapshot,
                int barCount,
                BarFingerprint first,
                BarFingerprint last)
            {
                Bars = bars;
                Index = index;
                Snapshot = snapshot;
                BarCount = barCount;
                First = first;
                Last = last;
            }
        }

        private readonly struct BarFingerprint : IEquatable<BarFingerprint>
        {
            private readonly DateTime _openTime;
            private readonly double _open;
            private readonly double _high;
            private readonly double _low;
            private readonly double _close;
            private readonly double _tickVolume;

            public BarFingerprint(
                DateTime openTime,
                double open,
                double high,
                double low,
                double close,
                double tickVolume)
            {
                _openTime = openTime;
                _open = open;
                _high = high;
                _low = low;
                _close = close;
                _tickVolume = tickVolume;
            }

            public bool Equals(BarFingerprint other)
            {
                return _openTime == other._openTime &&
                       _open == other._open &&
                       _high == other._high &&
                       _low == other._low &&
                       _close == other._close &&
                       _tickVolume == other._tickVolume;
            }

            public override bool Equals(object obj)
            {
                return obj is BarFingerprint other &&
                       Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + _openTime.GetHashCode();
                    hash = hash * 31 + _open.GetHashCode();
                    hash = hash * 31 + _high.GetHashCode();
                    hash = hash * 31 + _low.GetHashCode();
                    hash = hash * 31 + _close.GetHashCode();
                    hash = hash * 31 + _tickVolume.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
