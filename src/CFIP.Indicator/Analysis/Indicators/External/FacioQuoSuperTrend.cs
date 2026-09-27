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
        private double FacioQuoSuperTrend(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 60)
                return double.NaN;

            var results =
                StockIndicator.GetSuperTrend(
                    quotes,
                    10,
                    3)
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].SuperTrend.HasValue
                ? double.NaN
                : (double)results[results.Count - 1].SuperTrend.Value;
        }
    }
}
