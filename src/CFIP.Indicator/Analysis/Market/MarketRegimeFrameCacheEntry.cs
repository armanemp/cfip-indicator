using System;
using cAlgo.API;
using cAlgo;

namespace cAlgo
{
    internal sealed class MarketRegimeFrameCacheEntry
    {
        public Bars Bars { get; }
        public int Index { get; }
        public MarketRegimeSnapshot Snapshot { get; }
        public int BarCount { get; }
        public MarketRegimeBarFingerprint First { get; }
        public MarketRegimeBarFingerprint Last { get; }

        public MarketRegimeFrameCacheEntry(
            Bars bars,
            int index,
            MarketRegimeSnapshot snapshot,
            int barCount,
            MarketRegimeBarFingerprint first,
            MarketRegimeBarFingerprint last)
        {
            Bars = bars;
            Index = index;
            Snapshot = snapshot;
            BarCount = barCount;
            First = first;
            Last = last;
        }
    }
}
