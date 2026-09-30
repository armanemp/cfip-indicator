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
        private double SkenderObvBias(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null || quotes.Count < OssIndicatorParameters.ObvMinimumHistory)
                return double.NaN;

            var results =
                StockIndicator.GetObv(
                    quotes)
                    .ToList();

            if (results.Count < 2)
                return double.NaN;

            double current = results[results.Count - 1].Obv;
            double previous = results[results.Count - 2].Obv;

            if (double.IsNaN(current) ||
                double.IsInfinity(current) ||
                double.IsNaN(previous) ||
                double.IsInfinity(previous))
                return double.NaN;

            if (current > previous)
                return 1;

            if (current < previous)
                return -1;

            return 0;
        }
    }
}
