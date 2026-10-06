using System;
using System.Collections.Generic;
using StockQuote = Skender.Stock.Indicators.Quote;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class OssQuoteCacheEntry
    {
        internal Bars Bars { get; set; }

        internal int StableFirstIndex { get; set; } = -1;
        internal int StableClosedIndex { get; set; } = -1;
        internal int RollingFirstIndex { get; set; } = -1;
        internal int RollingClosedIndex { get; set; } = -1;
        internal int BarCount { get; set; } = -1;
        internal bool InvalidationPending { get; set; }

        internal DateTime FirstOpenTime { get; set; } = DateTime.MinValue;
        internal double FirstOpen { get; set; } = double.NaN;
        internal double FirstHigh { get; set; } = double.NaN;
        internal double FirstLow { get; set; } = double.NaN;
        internal double FirstClose { get; set; } = double.NaN;
        internal double FirstTickVolume { get; set; } = double.NaN;

        internal DateTime StableFirstOpenTime { get; set; } = DateTime.MinValue;
        internal double StableFirstOpen { get; set; } = double.NaN;
        internal double StableFirstHigh { get; set; } = double.NaN;
        internal double StableFirstLow { get; set; } = double.NaN;
        internal double StableFirstClose { get; set; } = double.NaN;
        internal double StableFirstTickVolume { get; set; } = double.NaN;

        internal DateTime StableLastOpenTime { get; set; } = DateTime.MinValue;
        internal double StableLastOpen { get; set; } = double.NaN;
        internal double StableLastHigh { get; set; } = double.NaN;
        internal double StableLastLow { get; set; } = double.NaN;
        internal double StableLastClose { get; set; } = double.NaN;
        internal double StableLastTickVolume { get; set; } = double.NaN;

        internal List<StockQuote> StableQuotes { get; set; }
        internal List<StockQuote> RollingQuotes { get; set; }
    }
}
