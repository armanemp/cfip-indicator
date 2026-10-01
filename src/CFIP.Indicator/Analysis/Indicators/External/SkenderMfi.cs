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
        private double SkenderMfi(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.MfiMinimumHistory)
                return double.NaN;

            var last =
                StockIndicator.GetMfi(
                    quotes,
                    OssIndicatorSettings.Default.MfiPeriod)
                    .LastOrDefault();

            return last == null || !last.Mfi.HasValue
                ? double.NaN
                : last.Mfi.Value;
        }
    }
}
