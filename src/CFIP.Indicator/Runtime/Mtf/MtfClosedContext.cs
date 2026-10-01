using System;

namespace cAlgo
{
    internal sealed class MtfClosedContext
    {
        public DateTime Reference { get; }
        public int M5 { get; }
        public int M1 { get; }
        public int M15 { get; }
        public int M30 { get; }
        public int H1 { get; }
        public int H4 { get; }
        public int D1 { get; }
        public int W1 { get; }

        public MtfClosedContext(
            DateTime reference,
            int m5,
            int m1,
            int m15,
            int m30,
            int h1,
            int h4,
            int d1,
            int w1)
        {
            Reference = reference;
            M5 = m5;
            M1 = m1;
            M15 = m15;
            M30 = m30;
            H1 = h1;
            H4 = h4;
            D1 = d1;
            W1 = w1;
        }

        public MtfClosedContext WithReference(
            DateTime reference)
        {
            return new MtfClosedContext(
                reference,
                M5,
                M1,
                M15,
                M30,
                H1,
                H4,
                D1,
                W1);
        }

        public bool HasPrimaryDecisionHistory
        {
            get
            {
                return
                    M5 >= 30 &&
                    M15 >= 30 &&
                    M30 >= 30 &&
                    H1 >= 30 &&
                    H4 >= 30;
            }
        }
    }
}
