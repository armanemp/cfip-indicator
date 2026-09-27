using System;
using System.Linq;
using Skender.Stock.Indicators;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FacioQuoMfi(
            Bars bars,
            int index)
        {
            IReadOnlyList<Quote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 40)
                return 0;

            var results =
                StockIndicator.GetMfi(
                    quotes,
                    14)
                    .ToList();

            return results.Count == 0
                ? 0
                : results[results.Count - 1].Mfi ?? 0;
        }
    }
}
