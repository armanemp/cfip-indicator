using System;
using System.Collections.Generic;
using StockQuote = Skender.Stock.Indicators.Quote;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private sealed class OssQuoteCacheEntry
        {
            public Bars Bars { get; set; }
            public int FirstIndex { get; set; } = -1;
            public int ClosedIndex { get; set; } = -1;
            public int BarCount { get; set; } = -1;
            public List<StockQuote> Quotes { get; set; }
        }

        private readonly List<OssQuoteCacheEntry> _ossQuoteCaches =
            new List<OssQuoteCacheEntry>();

        private IReadOnlyList<StockQuote> GetOssQuotes(
            Bars bars,
            int closedIndex)
        {
            if (bars == null ||
                closedIndex < 0 ||
                closedIndex >= bars.Count)
                return null;

            OssQuoteCacheEntry cache =
                _ossQuoteCaches.Find(
                    entry => ReferenceEquals(entry.Bars, bars));

            int firstIndex =
                Math.Max(
                    0,
                    closedIndex - 800);

            bool stale =
                cache == null ||
                cache.FirstIndex != firstIndex ||
                cache.ClosedIndex != closedIndex ||
                cache.BarCount != bars.Count;

            if (!stale)
                return cache.Quotes;

            List<StockQuote> quotes =
                new List<StockQuote>(
                    Math.Max(
                        0,
                        closedIndex - firstIndex + 1));

            for (int i = firstIndex;
                 i <= closedIndex;
                 i++)
            {
                quotes.Add(
                    new StockQuote
                    {
                        Date = bars.OpenTimes[i],
                        Open = (decimal)bars.OpenPrices[i],
                        High = (decimal)bars.HighPrices[i],
                        Low = (decimal)bars.LowPrices[i],
                        Close = (decimal)bars.ClosePrices[i],
                        Volume = Math.Max(
                            1m,
                            (decimal)bars.TickVolumes[i])
                    });
            }

            if (cache == null)
            {
                cache = new OssQuoteCacheEntry();
                _ossQuoteCaches.Add(cache);
            }

            cache.Bars = bars;
            cache.FirstIndex = firstIndex;
            cache.ClosedIndex = closedIndex;
            cache.BarCount = bars.Count;
            cache.Quotes = quotes;

            return quotes;
        }
    }
}
