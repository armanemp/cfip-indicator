using System;
using System.Linq;
using Skender.Stock.Indicators;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FacioQuoSuperTrend(
            Bars bars,
            int index)
        {
            IReadOnlyList<Quote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < 60)
                return 0;

            var results =
                quotes
                    .GetSuperTrend(
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
