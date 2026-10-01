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
        private double SkenderAroonOscillator(
            Bars bars,
            int index)
        {
            IReadOnlyList<StockQuote> quotes =
                GetOssQuotes(
                    bars,
                    index);

            if (quotes == null || quotes.Count < OssIndicatorParameters.AroonMinimumHistory)
                return double.NaN;

            var last =
                StockIndicator.GetAroon(
                    quotes,
                    OssIndicatorSettings.Default.AroonPeriod)
                    .LastOrDefault();

            return last == null || !last.Oscillator.HasValue
                ? double.NaN
                : last.Oscillator.Value;
        }
    }
}
