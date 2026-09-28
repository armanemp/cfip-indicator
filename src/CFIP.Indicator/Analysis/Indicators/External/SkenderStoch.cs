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
        private void SkenderStochBias(
            Bars bars,
            int index,
            out double k,
            out double d)
        {
            k = double.NaN;
            d = double.NaN;

            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 40)
                return;

            var results =
                StockIndicator.GetStoch(
                    quotes,
                    14,
                    3,
                    3)
                    .ToList();

            if (results.Count == 0)
                return;

            k = results[results.Count - 1].K ?? 0;
            d = results[results.Count - 1].D ?? 0;
        }
    }
}
