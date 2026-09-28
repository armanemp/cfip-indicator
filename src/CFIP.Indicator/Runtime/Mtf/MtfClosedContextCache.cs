using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class MtfClosedContextCache
    {
        private Bars _m1;
        private Bars _m5;
        private Bars _m15;
        private Bars _m30;
        private Bars _h1;
        private Bars _h4;
        private Bars _d1;
        private Bars _w1;

        private int _m1Count = -1;
        private int _m5Count = -1;
        private int _m15Count = -1;
        private int _m30Count = -1;
        private int _h1Count = -1;
        private int _h4Count = -1;
        private int _d1Count = -1;
        private int _w1Count = -1;

        private MtfClosedContext _context;

        public bool TryGet(
            Bars m1,
            Bars m5,
            Bars m15,
            Bars m30,
            Bars h1,
            Bars h4,
            Bars d1,
            Bars w1,
            out MtfClosedContext context)
        {
            if (_context != null &&
                ReferenceEquals(_m1, m1) &&
                ReferenceEquals(_m5, m5) &&
                ReferenceEquals(_m15, m15) &&
                ReferenceEquals(_m30, m30) &&
                ReferenceEquals(_h1, h1) &&
                ReferenceEquals(_h4, h4) &&
                ReferenceEquals(_d1, d1) &&
                ReferenceEquals(_w1, w1) &&
                _m1Count == Count(m1) &&
                _m5Count == Count(m5) &&
                _m15Count == Count(m15) &&
                _m30Count == Count(m30) &&
                _h1Count == Count(h1) &&
                _h4Count == Count(h4) &&
                _d1Count == Count(d1) &&
                _w1Count == Count(w1))
            {
                context = _context;
                return true;
            }

            context = null;
            return false;
        }

        public void Set(
            Bars m1,
            Bars m5,
            Bars m15,
            Bars m30,
            Bars h1,
            Bars h4,
            Bars d1,
            Bars w1,
            MtfClosedContext context)
        {
            if (context == null)
                return;

            _m1 = m1;
            _m5 = m5;
            _m15 = m15;
            _m30 = m30;
            _h1 = h1;
            _h4 = h4;
            _d1 = d1;
            _w1 = w1;

            _m1Count = Count(m1);
            _m5Count = Count(m5);
            _m15Count = Count(m15);
            _m30Count = Count(m30);
            _h1Count = Count(h1);
            _h4Count = Count(h4);
            _d1Count = Count(d1);
            _w1Count = Count(w1);

            _context = context;
        }

        private static int Count(Bars bars)
        {
            return bars == null ? -1 : bars.Count;
        }
    }
}
