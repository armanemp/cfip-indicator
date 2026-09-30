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
        private bool TryGetSkenderBollingerMetrics(
            Bars bars,
            int index,
            out double percentB,
            out double width)
        {
            percentB = double.NaN;
            width = double.NaN;

            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null ||
                quotes.Count < OssIndicatorParameters.BollingerMinimumHistory)
                return false;

            var results =
                StockIndicator.GetBollingerBands(
                    quotes,
                    OssIndicatorParameters.BollingerPeriod,
                    OssIndicatorParameters.BollingerStandardDeviations)
                    .ToList();

            if (results.Count == 0)
                return false;

            var last = results[results.Count - 1];

            if (last.PercentB.HasValue)
                percentB = last.PercentB.Value;

            if (last.Width.HasValue)
                width = last.Width.Value;

            return !double.IsNaN(percentB) ||
                   !double.IsNaN(width);
        }

        private double SkenderBollingerPercentB(
            Bars bars,
            int index)
        {
            double percentB;
            double width;

            return TryGetSkenderBollingerMetrics(
                       bars,
                       index,
                       out percentB,
                       out width)
                ? percentB
                : double.NaN;
        }
    }
}
