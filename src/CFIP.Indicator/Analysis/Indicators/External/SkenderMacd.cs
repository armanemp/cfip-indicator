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
        private double SkenderMacdHistogram(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssStableQuotes(
                    bars,
                    index);

            int fast =
                OssIndicatorParameters.SafeMacdFastPeriod(MacdFastPeriod);

            int slow =
                OssIndicatorParameters.SafeMacdSlowPeriod(
                    MacdFastPeriod,
                    MacdSlowPeriod);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.MacdHistoryRequired(MacdFastPeriod, MacdSlowPeriod))
                return double.NaN;

            var results =
                StockIndicator.GetMacd(
                    quotes,
                    fast,
                    slow,
                    OssIndicatorParameters.MacdSignalPeriod)
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].Histogram.HasValue
                ? double.NaN
                : results[results.Count - 1].Histogram.Value;
        }
    }
}
