using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class MtfClosedContextCache
    {
        private Bars _m1;
        private Bars _m2;
        private Bars _m5;
        private Bars _m15;
        private Bars _m30;
        private Bars _h1;
        private Bars _h4;
        private Bars _d1;
        private Bars _w1;

        private int _m1Count = -1;
        private int _m2Count = -1;
        private int _m5Count = -1;
        private int _m15Count = -1;
        private int _m30Count = -1;
        private int _h1Count = -1;
        private int _h4Count = -1;
        private int _d1Count = -1;
        private int _w1Count = -1;

        private MtfClosedContext _context;

        public bool TryGetStableContext(
            Bars m1,
            Bars m2,
            Bars m5,
            Bars m15,
            Bars m30,
            Bars h1,
            Bars h4,
            Bars d1,
            Bars w1,
            DateTime reference,
            out MtfClosedContext context)
        {
            if (_context != null &&
                ReferenceEquals(_m1, m1) &&
                ReferenceEquals(_m2, m2) &&
                ReferenceEquals(_m5, m5) &&
                ReferenceEquals(_m15, m15) &&
                ReferenceEquals(_m30, m30) &&
                ReferenceEquals(_h1, h1) &&
                ReferenceEquals(_h4, h4) &&
                ReferenceEquals(_d1, d1) &&
                ReferenceEquals(_w1, w1) &&
                _m1Count == Count(m1) &&
                _m2Count == Count(m2) &&
                _m5Count == Count(m5) &&
                _m15Count == Count(m15) &&
                _m30Count == Count(m30) &&
                _h1Count == Count(h1) &&
                _h4Count == Count(h4) &&
                _d1Count == Count(d1) &&
                _w1Count == Count(w1) &&
                reference >= _context.Reference &&
                IsReferenceStable(_m1, _m1Count, _context.M1, reference) &&
                IsReferenceStable(_m2, _m2Count, _context.M2, reference) &&
                IsReferenceStable(_m5, _m5Count, _context.M5, reference) &&
                IsReferenceStable(_m15, _m15Count, _context.M15, reference) &&
                IsReferenceStable(_m30, _m30Count, _context.M30, reference) &&
                IsReferenceStable(_h1, _h1Count, _context.H1, reference) &&
                IsReferenceStable(_h4, _h4Count, _context.H4, reference) &&
                IsReferenceStable(_d1, _d1Count, _context.D1, reference) &&
                IsReferenceStable(_w1, _w1Count, _context.W1, reference))
            {
                context = _context.WithReference(reference);
                return true;
            }

            context = null;
            return false;
        }

        public void StoreStableContext(
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
            _m2 = m2;
            _m5 = m5;
            _m15 = m15;
            _m30 = m30;
            _h1 = h1;
            _h4 = h4;
            _d1 = d1;
            _w1 = w1;

            _m1Count = Count(m1);
            _m2Count = Count(m2);
            _m5Count = Count(m5);
            _m15Count = Count(m15);
            _m30Count = Count(m30);
            _h1Count = Count(h1);
            _h4Count = Count(h4);
            _d1Count = Count(d1);
            _w1Count = Count(w1);

            _context = context;
        }

        public void Invalidate()
        {
            _m1 = null;
            _m2 = null;
            _m5 = null;
            _m15 = null;
            _m30 = null;
            _h1 = null;
            _h4 = null;
            _d1 = null;
            _w1 = null;
            _m1Count = -1;
            _m2Count = -1;
            _m5Count = -1;
            _m15Count = -1;
            _m30Count = -1;
            _h1Count = -1;
            _h4Count = -1;
            _d1Count = -1;
            _w1Count = -1;
            _context = null;
        }

        private static bool IsReferenceStable(
            Bars bars,
            int count,
            int closedIndex,
            DateTime reference)
        {
            if (closedIndex < 0)
                return true;

            if (bars == null ||
                count < 2 ||
                closedIndex >= count - 1)
                return false;

            return bars.OpenTimes[closedIndex + 1] > reference;
        }

        private static int Count(Bars bars)
        {
            return bars == null ? -1 : bars.Count;
        }
    }
}
