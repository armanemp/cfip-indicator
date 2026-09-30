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
                EnsureStablePrefix(
                    cache,
                    bars,
                    closedIndex);

                if (closedIndex == cache.StableClosedIndex)
                    return cache.StableQuotes;

                return cache.StableQuotes.GetRange(
                    0,
                    closedIndex + 1);
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
                   cache.StableClosedIndex >= bars.Count ||
                   MatchesStableLastBar(cache, bars);
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

        private static bool MatchesStableLastBar(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            int index = cache.StableClosedIndex;

            if (index < 0 ||
                index >= bars.Count)
                return false;

            return cache.StableLastOpenTime == bars.OpenTimes[index] &&
                   cache.StableLastOpen == bars.OpenPrices[index] &&
                   cache.StableLastHigh == bars.HighPrices[index] &&
                   cache.StableLastLow == bars.LowPrices[index] &&
                   cache.StableLastClose == bars.ClosePrices[index] &&
                   cache.StableLastTickVolume == bars.TickVolumes[index];
        }

        private static void ResetCache(
            OssQuoteCacheEntry cache,
            Bars bars)
        {
            cache.Bars = bars;
            cache.InvalidationPending = false;
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

            cache.StableLastOpenTime = DateTime.MinValue;
            cache.StableLastOpen = double.NaN;
            cache.StableLastHigh = double.NaN;
            cache.StableLastLow = double.NaN;
            cache.StableLastClose = double.NaN;
            cache.StableLastTickVolume = double.NaN;

            cache.StableQuotes =
                new List<StockQuote>();

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

        private static void EnsureStablePrefix(
            OssQuoteCacheEntry cache,
            Bars bars,
            int closedIndex)
        {
            if (cache.StableQuotes == null)
                cache.StableQuotes = new List<StockQuote>();

            int startIndex =
                Math.Max(
                    0,
                    cache.StableClosedIndex + 1);

            if (closedIndex < cache.StableClosedIndex)
            {
                cache.StableQuotes.Clear();
                cache.StableClosedIndex = -1;
                startIndex = 0;
            }

            for (int i = startIndex;
                 i <= closedIndex;
                 i++)
            {
                cache.StableQuotes.Add(
                    CreateQuote(
                        bars,
                        i));

                cache.StableLastOpenTime = bars.OpenTimes[i];
                cache.StableLastOpen = bars.OpenPrices[i];
                cache.StableLastHigh = bars.HighPrices[i];
                cache.StableLastLow = bars.LowPrices[i];
                cache.StableLastClose = bars.ClosePrices[i];
                cache.StableLastTickVolume = bars.TickVolumes[i];
                cache.StableClosedIndex = i;
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
                Math.Max(
                    0,
                    closedIndex -
                    OssIndicatorParameters.RollingQuoteWindowSize +
                    1);

            if (closedIndex < cache.RollingClosedIndex ||
                expectedFirstIndex < cache.RollingFirstIndex ||
                expectedFirstIndex > cache.RollingClosedIndex + 1)
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
                    1m,
                    (decimal)bars.TickVolumes[index])
            };
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
