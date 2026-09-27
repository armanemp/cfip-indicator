using System.Collections.Generic;
using StockQuote = Skender.Stock.Indicators.Quote;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class OssQuoteCacheEntry
    {
        public Bars Bars { get; set; }
        public int FirstIndex { get; set; } = -1;
        public int ClosedIndex { get; set; } = -1;
        public int BarCount { get; set; } = -1;
        public List<StockQuote> Quotes { get; set; }
    }
}
