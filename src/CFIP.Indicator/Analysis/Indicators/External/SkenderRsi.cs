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

            var last =
                StockIndicator.GetRsi(
                    quotes,
                    OssIndicatorParameters.SafeRsiPeriod(RsiPeriod))
                    .LastOrDefault();

            return last == null || !last.Rsi.HasValue
                ? double.NaN
                : last.Rsi.Value;
        }
    }
}
