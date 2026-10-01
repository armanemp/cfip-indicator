using System;

namespace cAlgo
{
    internal sealed class CalculationMarketContext
    {
        public int HostIndex { get; }
        public DateTime SignalReferenceUtc { get; }
        public DateTime QuoteObservedUtc { get; }
        public CanonicalPriceSnapshot Price { get; }
        public MtfClosedContext ClosedBars { get; }

        public int AtrM5Index =>
            ClosedBars == null ? -1 : ClosedBars.M5;

        public int AtrM1Index =>
            ClosedBars == null ? -1 : ClosedBars.M1;

        public CalculationMarketContext(
            int hostIndex,
            DateTime signalReferenceUtc,
            DateTime quoteObservedUtc,
            CanonicalPriceSnapshot price,
            MtfClosedContext closedBars)
        {
            if (price == null)
                throw new ArgumentNullException(nameof(price));

            if (closedBars == null)
                throw new ArgumentNullException(nameof(closedBars));

            HostIndex = hostIndex;
            SignalReferenceUtc = signalReferenceUtc;
            QuoteObservedUtc = quoteObservedUtc;
            Price = price;
            ClosedBars = closedBars;
        }
    }
}
