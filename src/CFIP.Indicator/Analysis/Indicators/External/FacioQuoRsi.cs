using System;
using System.Linq;
using Skender.Stock.Indicators;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FacioQuoRsi(
            Bars bars,
            int index)
        {
            IReadOnlyList<Quote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < Math.Max(20, RsiPeriod + 5))
                return 0;

            var results =
                StockIndicator.GetRsi(
                    quotes,Math.Max(2, RsiPeriod))
                    .ToList();

            return results.Count == 0
                ? 0
                : results[results.Count - 1].Rsi ?? 0;
        }
    }
}
