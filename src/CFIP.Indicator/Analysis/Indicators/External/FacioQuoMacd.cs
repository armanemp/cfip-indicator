using System;
using System.Collections.Generic;
using System.Linq;
using StockQuote = Skender.Stock.Indicators.Quote;
using StockIndicator = Skender.Stock.Indicators.Indicator;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FacioQuoMacdHistogram(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            int fast =
                Math.Max(2, MacdFastPeriod);

            int slow =
                Math.Max(
                    fast + 1,
                    MacdSlowPeriod);

            if (quotes == null ||
                quotes.Count < slow + 20)
                return double.NaN;

            var results =
                StockIndicator.GetMacd(
                    quotes,
                    fast,
                    slow,
                    9)
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].Histogram.HasValue
                ? double.NaN
                : results[results.Count - 1].Histogram.Value;
        }
    }
}
