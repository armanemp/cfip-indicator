using System;
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
                return 0;

            var results =
                StockIndicator.GetSuperTrend(
                    quotes,
                    10,
                    3)
                    .ToList();

            return results.Count == 0
                ? 0
                : (double)(
                    results[results.Count - 1].SuperTrend ?? 0);
        }
    }
}
