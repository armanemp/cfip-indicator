using System;

namespace cAlgo
{
    internal sealed class MarketStateSnapshot
    {
        public DateTime Reference { get; }
        public MarketStateFrameSnapshot M1 { get; }
        public MarketStateFrameSnapshot M5 { get; }
        public MarketStateFrameSnapshot M15 { get; }
        public MarketStateFrameSnapshot M30 { get; }
        public MarketStateFrameSnapshot H1 { get; }
        public MarketStateFrameSnapshot H4 { get; }
        public MarketStateFrameSnapshot D1 { get; }
        public MarketStateFrameSnapshot W1 { get; }
        public int PremiumDiscountBias { get; }
        public bool SessionOpen { get; }

        public MarketStateSnapshot(
            DateTime reference,
            MarketStateFrameSnapshot m1,
            MarketStateFrameSnapshot m5,
            MarketStateFrameSnapshot m15,
            MarketStateFrameSnapshot m30,
            MarketStateFrameSnapshot h1,
            MarketStateFrameSnapshot h4,
            MarketStateFrameSnapshot d1,
            MarketStateFrameSnapshot w1,
            int premiumDiscountBias,
            bool sessionOpen)
        {
            Reference = reference;
            M1 = Require(m1);
            M5 = Require(m5);
            M15 = Require(m15);
            M30 = Require(m30);
            H1 = Require(h1);
            H4 = Require(h4);
            D1 = Require(d1);
            W1 = Require(w1);
            PremiumDiscountBias =
                premiumDiscountBias == 1 || premiumDiscountBias == -1
                    ? premiumDiscountBias
                    : 0;
            SessionOpen = sessionOpen;
        }

        public bool MatchesReference(
            DateTime reference)
        {
            return Reference == reference;
        }

        public bool IsAlignedWithClosedIndices(
            int m1,
            int m5,
            int m15,
            int m30,
            int h1,
            int h4,
            int d1,
            int w1)
        {
            return
                M1.ClosedIndex == m1 &&
                M5.ClosedIndex == m5 &&
                M15.ClosedIndex == m15 &&
                M30.ClosedIndex == m30 &&
                H1.ClosedIndex == h1 &&
                H4.ClosedIndex == h4 &&
                D1.ClosedIndex == d1 &&
                W1.ClosedIndex == w1;
        }

        private static MarketStateFrameSnapshot Require(
            MarketStateFrameSnapshot frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            return frame;
        }
    }
}
