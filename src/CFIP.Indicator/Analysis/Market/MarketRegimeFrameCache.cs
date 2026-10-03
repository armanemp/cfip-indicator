using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class MarketRegimeFrameCache
    {
        private const int Capacity = 9;

        private readonly MarketRegimeFrameCacheEntry[] _entries =
            new MarketRegimeFrameCacheEntry[Capacity];

        public bool TryGetSnapshot(
            Bars bars,
            int index,
            out MarketRegimeSnapshot snapshot)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                MarketRegimeFrameCacheEntry entry = _entries[i];

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

        public void StoreSnapshot(
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
                new MarketRegimeFrameCacheEntry(
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

        public void InvalidateFrameRegimeSnapshots()
        {
            for (int i = 0; i < _entries.Length; i++)
                _entries[i] = null;
        }

        private static bool IsCompatible(
            MarketRegimeFrameCacheEntry entry,
            Bars bars)
        {
            if (entry == null ||
                bars == null ||
                !ReferenceEquals(entry.Bars, bars) ||
                entry.BarCount > bars.Count)
                return false;

            return
                MatchesFingerprint(
                    entry.First,
                    bars,
                    0) &&
                MatchesFingerprint(
                    entry.Last,
                    bars,
                    entry.Index);
        }

        private static MarketRegimeBarFingerprint Fingerprint(
            Bars bars,
            int index)
        {
            return new MarketRegimeBarFingerprint(
                bars.OpenTimes[index],
                bars.OpenPrices[index],
                bars.HighPrices[index],
                bars.LowPrices[index],
                bars.ClosePrices[index],
                bars.TickVolumes[index]);
        }

        private static bool MatchesFingerprint(
            MarketRegimeBarFingerprint expected,
            Bars bars,
            int index)
        {
            return expected.OpenTime == bars.OpenTimes[index] &&
                   expected.Open == bars.OpenPrices[index] &&
                   expected.High == bars.HighPrices[index] &&
                   expected.Low == bars.LowPrices[index] &&
                   expected.Close == bars.ClosePrices[index] &&
                   expected.TickVolume == bars.TickVolumes[index];
        }
    }
}
