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

            var results =
                StockIndicator.GetParabolicSar(
                    quotes,
                    OssIndicatorParameters.ParabolicSarAccelerationFactor,
                    OssIndicatorParameters.ParabolicSarMaximumAccelerationFactor)
                    .ToList();

            return results.Count == 0 ||
                   !results[results.Count - 1].Sar.HasValue
                ? double.NaN
                : results[results.Count - 1].Sar.Value;
        }
    }
}
