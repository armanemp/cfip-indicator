using System;
using System.Linq;
using Skender.Stock.Indicators;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FacioQuoMacdHistogram(
            Bars bars,
            int index)
        {
            IReadOnlyList<Quote> quotes =
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
                return 0;

            var results =
                quotes
                    .GetMacd(
                        fast,
                        slow,
                        9)
                    .ToList();

            return results.Count == 0
                ? 0
                : results[results.Count - 1].Histogram ?? 0;
        }
    }
}
