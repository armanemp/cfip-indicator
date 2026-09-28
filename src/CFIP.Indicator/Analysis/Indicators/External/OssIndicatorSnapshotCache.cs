using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class OssIndicatorSnapshotCache
    {
        private Bars _bars;
        private int _index = -1;
        private int _barCount = -1;
        private DateTime _openTime = DateTime.MinValue;
        private double _open = double.NaN;
        private double _high = double.NaN;
        private double _low = double.NaN;
        private double _close = double.NaN;
        private double _tickVolume = double.NaN;
        private OssIndicatorSnapshot _snapshot;

        public bool TryGet(
            Bars bars,
            int index,
            out OssIndicatorSnapshot snapshot)
        {
            if (!MatchesCurrentBar(bars, index))
            {
                snapshot = null;
                return false;
            }

            snapshot = _snapshot;
            return true;
        }

        public void Set(
            Bars bars,
            int index,
            OssIndicatorSnapshot snapshot)
        {
            if (bars == null ||
                snapshot == null ||
                index < 0 ||
                index >= bars.Count)
                return;

            _bars = bars;
            _index = index;
            _barCount = bars.Count;
            _openTime = bars.OpenTimes[index];
            _open = bars.OpenPrices[index];
            _high = bars.HighPrices[index];
            _low = bars.LowPrices[index];
            _close = bars.ClosePrices[index];
            _tickVolume = bars.TickVolumes[index];
            _snapshot = snapshot;
        }

        private bool MatchesCurrentBar(
            Bars bars,
            int index)
        {
            return bars != null &&
                   index >= 0 &&
                   index < bars.Count &&
                   _snapshot != null &&
                   ReferenceEquals(_bars, bars) &&
                   _index == index &&
                   _barCount == bars.Count &&
                   _openTime == bars.OpenTimes[index] &&
                   _open == bars.OpenPrices[index] &&
                   _high == bars.HighPrices[index] &&
                   _low == bars.LowPrices[index] &&
                   _close == bars.ClosePrices[index] &&
                   _tickVolume == bars.TickVolumes[index];
        }
    }
}
