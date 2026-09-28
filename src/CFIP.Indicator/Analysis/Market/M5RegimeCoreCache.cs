using cAlgo.API;

namespace cAlgo
{
    internal sealed class M5RegimeCoreCache
    {
        private Bars _bars;
        private int _index1 = -1;
        private int _index2 = -1;
        private int _index3 = -1;

        private MarketRegimeSnapshot _snapshot1;
        private MarketRegimeSnapshot _snapshot2;
        private MarketRegimeSnapshot _snapshot3;

        public bool TryGetCached(
            Bars bars,
            int index,
            out MarketRegimeSnapshot snapshot)
        {
            if (!ReferenceEquals(_bars, bars))
            {
                snapshot = null;
                return false;
            }

            if (index == _index1 && _snapshot1 != null)
            {
                snapshot = _snapshot1;
                return true;
            }

            if (index == _index2 && _snapshot2 != null)
            {
                snapshot = _snapshot2;
                return true;
            }

            if (index == _index3 && _snapshot3 != null)
            {
                snapshot = _snapshot3;
                return true;
            }

            snapshot = null;
            return false;
        }

        public void Store(
            Bars bars,
            int index,
            MarketRegimeSnapshot snapshot)
        {
            if (bars == null ||
                snapshot == null)
                return;

            if (!IsCurrentSeries(bars))
            {
                _bars = bars;
                    _index1 = -1;
                _index2 = -1;
                _index3 = -1;
                _snapshot1 = null;
                _snapshot2 = null;
                _snapshot3 = null;
            }
            else if (_index1 == index)
            {
                _snapshot1 = snapshot;
                return;
            }
            else if (_index2 == index)
            {
                _snapshot2 = snapshot;
                return;
            }
            else if (_index3 == index)
            {
                _snapshot3 = snapshot;
                return;
            }

            _index3 = _index2;
            _snapshot3 = _snapshot2;
            _index2 = _index1;
            _snapshot2 = _snapshot1;
            _index1 = index;
            _snapshot1 = snapshot;
        }

        private bool IsCurrentSeries(Bars bars)
        {
            return ReferenceEquals(_bars, bars);
        }
    }
}
