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
        private double SkenderRsi(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssStableQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.RsiHistoryRequired(RsiPeriod))
                return double.NaN;

            var results =
                StockIndicator.GetRsi(
                    quotes,
                    OssIndicatorParameters.SafeRsiPeriod(RsiPeriod))
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].Rsi.HasValue
                ? double.NaN
                : results[results.Count - 1].Rsi.Value;
        }
    }
}
