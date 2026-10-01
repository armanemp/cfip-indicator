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
        private void SkenderStochBias(
            Bars bars,
            int index,
            out double k,
            out double d)
        {
            k = double.NaN;
            d = double.NaN;

            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.StochMinimumHistory)
                return;

            var last =
                StockIndicator.GetStoch(
                    quotes,
                    OssIndicatorSettings.Default.StochLookbackPeriod,
                    OssIndicatorSettings.Default.StochSignalPeriod,
                    OssIndicatorSettings.Default.StochSmoothPeriod)
                    .LastOrDefault();

            if (last == null)
                return;

            k = last.K ?? 0;
            d = last.D ?? 0;
        }
    }
}
