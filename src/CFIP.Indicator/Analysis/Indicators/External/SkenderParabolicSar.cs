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
        private double SkenderParabolicSar(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssStableQuotes(
                    bars,
                    index);

            if (quotes == null || quotes.Count < OssIndicatorParameters.ParabolicSarMinimumHistory)
                return double.NaN;

            var last =
                StockIndicator.GetParabolicSar(
                    quotes,
                    OssIndicatorSettings.Default.ParabolicSarAccelerationFactor,
                    OssIndicatorSettings.Default.ParabolicSarMaximumAccelerationFactor)
                    .LastOrDefault();

            return last == null || !last.Sar.HasValue
                ? double.NaN
                : last.Sar.Value;
        }
    }
}
