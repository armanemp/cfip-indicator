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
        private double FacioQuoBollingerPercentB(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 40)
                return 0;

            var results =
                StockIndicator.GetBollingerBands(
                    quotes,
                    20,
                    2)
                    .ToList();

            return results.Count == 0
                ? 0
                : results[results.Count - 1].PercentB ?? 0;
        }
    }
}
