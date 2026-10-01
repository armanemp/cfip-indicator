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
        private double SkenderCci(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null || quotes.Count < OssIndicatorParameters.CciMinimumHistory)
                return double.NaN;

            var last =
                StockIndicator.GetCci(
                    quotes,
                    OssIndicatorSettings.Default.CciPeriod)
                    .LastOrDefault();

            return last == null || !last.Cci.HasValue
                ? double.NaN
                : last.Cci.Value;
        }
    }
}
