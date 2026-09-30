using System;
using System.Collections.Generic;
using StockQuote = Skender.Stock.Indicators.Quote;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class OssQuoteCacheEntry
    {
        public Bars Bars { get; set; }

        public int StableClosedIndex { get; set; } = -1;
        public int RollingFirstIndex { get; set; } = -1;
        public int RollingClosedIndex { get; set; } = -1;
        public int BarCount { get; set; } = -1;
        public bool InvalidationPending { get; set; }

        public DateTime FirstOpenTime { get; set; } = DateTime.MinValue;
        public double FirstOpen { get; set; } = double.NaN;
        public double FirstHigh { get; set; } = double.NaN;
        public double FirstLow { get; set; } = double.NaN;
        public double FirstClose { get; set; } = double.NaN;
        public double FirstTickVolume { get; set; } = double.NaN;

        public DateTime StableLastOpenTime { get; set; } = DateTime.MinValue;
        public double StableLastOpen { get; set; } = double.NaN;
        public double StableLastHigh { get; set; } = double.NaN;
        public double StableLastLow { get; set; } = double.NaN;
        public double StableLastClose { get; set; } = double.NaN;
        public double StableLastTickVolume { get; set; } = double.NaN;

        public List<StockQuote> StableQuotes { get; set; }
        public List<StockQuote> RollingQuotes { get; set; }
    }
}
