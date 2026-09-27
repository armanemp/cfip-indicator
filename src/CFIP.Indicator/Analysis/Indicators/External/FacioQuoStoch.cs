using System;
using System.Linq;
using Skender.Stock.Indicators;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void FacioQuoStochBias(
            Bars bars,
            int index,
            out double k,
            out double d)
        {
            k = 0;
            d = 0;

            IReadOnlyList<Quote> quotes =
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
