using System;
using System.Collections.Generic;
using StockQuote = Skender.Stock.Indicators.Quote;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly List<OssQuoteCacheEntry> _ossQuoteCaches =
            new List<OssQuoteCacheEntry>();

        private IReadOnlyList<StockQuote> GetOssQuotes(
            Bars bars,
            int closedIndex)
        {
            return GetQuotes(
                bars,
                closedIndex,
                false);
        }

        private IReadOnlyList<StockQuote> GetOssStableQuotes(
            Bars bars,
            int closedIndex)
        {
            return GetQuotes(
                bars,
                closedIndex,
                true);
        }

        private IReadOnlyList<StockQuote> GetQuotes(
            Bars bars,
            int closedIndex,
            bool stablePrefix)
        {
            if (bars == null ||
                bars.Count == 0 ||
                closedIndex < 0 ||
                closedIndex >= bars.Count)
                return null;

            OssQuoteCacheEntry cache =
                _ossQuoteCaches.Find(
                    entry => ReferenceEquals(entry.Bars, bars));

            if (cache == null)
            {
                cache = new OssQuoteCacheEntry();
                _ossQuoteCaches.Add(cache);

                bars.HistoryLoaded +=
                    Bars_HistoryLoaded;
                bars.Reloaded +=
                    Bars_Reloaded;

                ResetCache(cache, bars);
            }
            else if (cache.InvalidationPending ||
                     !IsCacheCompatible(cache, bars))
            {
                ResetCache(cache, bars);
            }

            if (stablePrefix)
            {
                EnsureStableWindow(
                    cache,
                    bars,
                    closedIndex);

                return cache.StableQuotes;
            }

            EnsureRollingWindow(
                cache,
                bars,
                closedIndex);

            return cache.RollingQuotes;
        }

        private static bool IsCacheCompatible(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            if (cache == null ||
                bars == null ||
                !ReferenceEquals(cache.Bars, bars) ||
                cache.BarCount < 0 ||
                bars.Count < cache.BarCount)
                return false;

            if (bars.Count == 0)
                return true;

            if (!MatchesFirstBar(cache, bars))
                return false;

            return cache.StableClosedIndex < 0 ||
                   MatchesStableWindowBoundaries(
                       cache,
                       bars);
        }

        private static bool MatchesFirstBar(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            return bars.Count > 0 &&
                   cache.FirstOpenTime == bars.OpenTimes[0] &&
                   cache.FirstOpen == bars.OpenPrices[0] &&
                   cache.FirstHigh == bars.HighPrices[0] &&
                   cache.FirstLow == bars.LowPrices[0] &&
                   cache.FirstClose == bars.ClosePrices[0] &&
                   cache.FirstTickVolume == bars.TickVolumes[0];
        }

        private static bool MatchesStableWindowBoundaries(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            int firstIndex = cache.StableFirstIndex;
            int lastIndex = cache.StableClosedIndex;

            if (firstIndex < 0 ||
                lastIndex < firstIndex ||
                lastIndex >= bars.Count)
                return false;

            return cache.StableFirstOpenTime == bars.OpenTimes[firstIndex] &&
                   cache.StableFirstOpen == bars.OpenPrices[firstIndex] &&
                   cache.StableFirstHigh == bars.HighPrices[firstIndex] &&
                   cache.StableFirstLow == bars.LowPrices[firstIndex] &&
                   cache.StableFirstClose == bars.ClosePrices[firstIndex] &&
                   cache.StableFirstTickVolume == bars.TickVolumes[firstIndex] &&
                   cache.StableLastOpenTime == bars.OpenTimes[lastIndex] &&
                   cache.StableLastOpen == bars.OpenPrices[lastIndex] &&
                   cache.StableLastHigh == bars.HighPrices[lastIndex] &&
                   cache.StableLastLow == bars.LowPrices[lastIndex] &&
                   cache.StableLastClose == bars.ClosePrices[lastIndex] &&
                   cache.StableLastTickVolume == bars.TickVolumes[lastIndex];
        }

        private static void ResetCache(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            cache.Bars = bars;
            cache.InvalidationPending = false;
            cache.StableFirstIndex = -1;
            cache.StableClosedIndex = -1;
            cache.RollingFirstIndex = -1;
            cache.RollingClosedIndex = -1;
            cache.BarCount = bars.Count;

            cache.FirstOpenTime = DateTime.MinValue;
            cache.FirstOpen = double.NaN;
            cache.FirstHigh = double.NaN;
            cache.FirstLow = double.NaN;
            cache.FirstClose = double.NaN;
            cache.FirstTickVolume = double.NaN;

            cache.StableFirstOpenTime = DateTime.MinValue;
            cache.StableFirstOpen = double.NaN;
            cache.StableFirstHigh = double.NaN;
            cache.StableFirstLow = double.NaN;
            cache.StableFirstClose = double.NaN;
            cache.StableFirstTickVolume = double.NaN;

            cache.StableLastOpenTime = DateTime.MinValue;
            cache.StableLastOpen = double.NaN;
            cache.StableLastHigh = double.NaN;
            cache.StableLastLow = double.NaN;
            cache.StableLastClose = double.NaN;
            cache.StableLastTickVolume = double.NaN;

            cache.StableQuotes =
                new List<StockQuote>(
                    OssIndicatorWarmupPolicy.StableQuoteWindowSize);

            cache.RollingQuotes =
                new List<StockQuote>(
                    OssIndicatorParameters.RollingQuoteWindowSize);

            if (bars.Count > 0)
                CaptureFirstBar(
                    cache,
                    bars);
        }

        private void Bars_HistoryLoaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateBars(
                args == null
                    ? null
                    : args.Bars);
        }

        private void Bars_Reloaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateBars(
                args == null
                    ? null
                    : args.Bars);
        }

        private void InvalidateBars(
            Bars bars)
        {
            if (bars == null)
                return;

            OssQuoteCacheEntry cache =
                _ossQuoteCaches.Find(
                    entry => ReferenceEquals(entry.Bars, bars));

            if (cache != null)
                cache.InvalidationPending = true;
        }

        private static void EnsureStableWindow(
            OssQuoteCacheEntry cache,
            Bars bars,
            int closedIndex)
        {
            if (cache.StableQuotes == null)
                cache.StableQuotes =
                    new List<StockQuote>(
                        OssIndicatorWarmupPolicy.StableQuoteWindowSize);

            int expectedFirstIndex =
                OssQuoteWindowRule.ResolveFirstIndex(
                    closedIndex,
                    OssIndicatorWarmupPolicy.StableQuoteWindowSize);

            bool rebuild =
                OssQuoteWindowRule.RequiresRebuild(
                    closedIndex,
                    expectedFirstIndex,
                    cache.StableClosedIndex,
                    cache.StableFirstIndex);

            if (rebuild)
            {
                cache.StableQuotes.Clear();
                cache.StableFirstIndex = expectedFirstIndex;
                cache.StableClosedIndex = expectedFirstIndex - 1;
            }

            int startIndex =
                Math.Max(
                    expectedFirstIndex,
                    cache.StableClosedIndex + 1);

            for (int i = startIndex;
                 i <= closedIndex;
                 i++)
            {
                cache.StableQuotes.Add(
                    CreateQuote(
                        bars,
                        i));

                cache.StableClosedIndex = i;

                while (cache.StableQuotes.Count >
                       OssIndicatorWarmupPolicy.StableQuoteWindowSize)
                {
                    cache.StableQuotes.RemoveAt(0);
                    cache.StableFirstIndex++;
                }
            }

            if (cache.StableQuotes.Count == 0)
            {
                cache.StableFirstIndex = -1;
                cache.StableClosedIndex = -1;
            }
            else
            {
                cache.StableFirstIndex =
                    cache.StableClosedIndex -
                    cache.StableQuotes.Count + 1;

                CaptureStableFirstBar(
                    cache,
                    bars);

                CaptureStableLastBar(
                    cache,
                    bars);
            }

            cache.BarCount = bars.Count;
        }

        private static void EnsureRollingWindow(
            OssQuoteCacheEntry cache,
            Bars bars,
            int closedIndex)
        {
            if (cache.RollingQuotes == null)
                cache.RollingQuotes =
                    new List<StockQuote>(
                        OssIndicatorParameters.RollingQuoteWindowSize);

            int expectedFirstIndex =
                OssQuoteWindowRule.ResolveFirstIndex(
                    closedIndex,
                    OssIndicatorParameters.RollingQuoteWindowSize);

            if (OssQuoteWindowRule.RequiresRebuild(
                    closedIndex,
                    expectedFirstIndex,
                    cache.RollingClosedIndex,
                    cache.RollingFirstIndex))
            {
                cache.RollingQuotes.Clear();
                cache.RollingFirstIndex = expectedFirstIndex;
                cache.RollingClosedIndex = expectedFirstIndex - 1;
            }

            int startIndex =
                Math.Max(
                    expectedFirstIndex,
                    cache.RollingClosedIndex + 1);

            for (int i = startIndex;
                 i <= closedIndex;
                 i++)
            {
                cache.RollingQuotes.Add(
                    CreateQuote(
                        bars,
                        i));

                cache.RollingClosedIndex = i;
            }

            while (cache.RollingQuotes.Count >
                   OssIndicatorParameters.RollingQuoteWindowSize)
            {
                cache.RollingQuotes.RemoveAt(0);
                cache.RollingFirstIndex++;
            }

            cache.BarCount = bars.Count;
        }

        private static StockQuote CreateQuote(
            Bars bars,
            int index)
        {
            return new StockQuote
            {
                Date = bars.OpenTimes[index],
                Open = (decimal)bars.OpenPrices[index],
                High = (decimal)bars.HighPrices[index],
                Low = (decimal)bars.LowPrices[index],
                Close = (decimal)bars.ClosePrices[index],
                Volume = Math.Max(
                    0m,
                    (decimal)bars.TickVolumes[index])
            };
        }

        private static void CaptureStableFirstBar(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            int index = cache.StableFirstIndex;

            if (index < 0 ||
                index >= bars.Count)
                return;

            cache.StableFirstOpenTime = bars.OpenTimes[index];
            cache.StableFirstOpen = bars.OpenPrices[index];
            cache.StableFirstHigh = bars.HighPrices[index];
            cache.StableFirstLow = bars.LowPrices[index];
            cache.StableFirstClose = bars.ClosePrices[index];
            cache.StableFirstTickVolume = bars.TickVolumes[index];
        }

        private static void CaptureStableLastBar(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            int index = cache.StableClosedIndex;

            if (index < 0 ||
                index >= bars.Count)
                return;

            cache.StableLastOpenTime = bars.OpenTimes[index];
            cache.StableLastOpen = bars.OpenPrices[index];
            cache.StableLastHigh = bars.HighPrices[index];
            cache.StableLastLow = bars.LowPrices[index];
            cache.StableLastClose = bars.ClosePrices[index];
            cache.StableLastTickVolume = bars.TickVolumes[index];
        }

        private static void CaptureFirstBar(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            cache.FirstOpenTime = bars.OpenTimes[0];
            cache.FirstOpen = bars.OpenPrices[0];
            cache.FirstHigh = bars.HighPrices[0];
            cache.FirstLow = bars.LowPrices[0];
            cache.FirstClose = bars.ClosePrices[0];
            cache.FirstTickVolume = bars.TickVolumes[0];
        }
    }
}
