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
        private double FacioQuoMfi(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 40)
                return double.NaN;

            var results =
                StockIndicator.GetMfi(
                    quotes,
                    14)
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].Mfi.HasValue
                ? double.NaN
                : results[results.Count - 1].Mfi.Value;
        }
    }
}
