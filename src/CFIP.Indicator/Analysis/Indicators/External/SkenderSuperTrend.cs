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
        private double SkenderSuperTrend(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssStableQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.SuperTrendMinimumHistory)
                return double.NaN;

            var last =
                StockIndicator.GetSuperTrend(
                    quotes,
                    OssIndicatorSettings.Default.SuperTrendPeriod,
                    OssIndicatorSettings.Default.SuperTrendMultiplier)
                    .LastOrDefault();

            return last == null || !last.SuperTrend.HasValue
                ? double.NaN
                : (double)last.SuperTrend.Value;
        }
    }
}
