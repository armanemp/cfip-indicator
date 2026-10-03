// ============================================================================
// CFIP Indicator — HistoricalRenderer.cs
// Historical signal markers are presentation-only; they are not live-signal,
// replay or backtest outcome claims.
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly Dictionary<DateTime, HistoricalSignalPresentation>
            _historicalSignalCache =
                new Dictionary<DateTime, HistoricalSignalPresentation>();

        private DateTime _historicalKnownFirstOpenTime =
            DateTime.MinValue;

        private int _historicalKnownBarCount = -1;

        private bool _historicalRenderingHistoryInvalidated;

        private void HookHistoricalBarsEvents()
        {
            if (Bars == null)
                return;

            Bars.HistoryLoaded -= OnHistoricalBarsHistoryLoaded;
            Bars.Reloaded -= OnHistoricalBarsReloaded;
            Bars.HistoryLoaded += OnHistoricalBarsHistoryLoaded;
            Bars.Reloaded += OnHistoricalBarsReloaded;
        }

        private void UnhookHistoricalBarsEvents()
        {
            if (Bars == null)
                return;

            Bars.HistoryLoaded -= OnHistoricalBarsHistoryLoaded;
            Bars.Reloaded -= OnHistoricalBarsReloaded;
        }

        private void OnHistoricalBarsHistoryLoaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateHistoricalRenderingCache();
        }

        private void OnHistoricalBarsReloaded(
            BarsHistoryLoadedEventArgs args)
        {
            InvalidateHistoricalRenderingCache();
        }

        private void ResetHistoricalRenderingState()
        {
            _historicalSignalCache.Clear();
            _historicalKnownFirstOpenTime =
                DateTime.MinValue;
            _historicalKnownBarCount = -1;
            _historicalRenderingHistoryInvalidated = false;
            _lastHistoricalHostBar = -1;
        }

        private void InvalidateHistoricalRenderingCache()
        {
            _historicalSignalCache.Clear();
            _historicalRenderingHistoryInvalidated = true;
            _lastHistoricalHostBar = -1;
        }

        private bool PrepareHistoricalRenderingContext()
        {
            if (Bars == null ||
                Bars.Count == 0)
                return false;

            DateTime firstOpenTime =
                Bars.OpenTimes[0];

            int barCount =
                Bars.Count;

            bool historyChanged =
                _historicalRenderingHistoryInvalidated ||
                _historicalKnownBarCount < 0 ||
                firstOpenTime !=
                    _historicalKnownFirstOpenTime ||
                barCount <
                    _historicalKnownBarCount;

            _historicalKnownFirstOpenTime =
                firstOpenTime;
            _historicalKnownBarCount =
                barCount;
            _historicalRenderingHistoryInvalidated =
                false;

            if (historyChanged)
            {
                RemoveHistoricalObjects();
                _historicalSignalCache.Clear();
                _lastHistoricalHostBar = -1;
            }

            return true;
        }

        private void RenderHistoricalSignals()
        {
            if (!PrepareHistoricalRenderingContext())
                return;

            if (Bars.Count <
                HistoricalRenderingRule.MinimumBarsRequired)
            {
                RemoveHistoricalObjects();
                return;
            }

            int lastClosed =
                HistoricalRenderingRule.ResolveLastClosedIndex(
                    Bars.Count);

            int oldest =
                HistoricalRenderingRule.ResolveOldestScannedIndex(
                    Bars.Count);

            if (!HistoricalRenderingRule.IsClosedAnalyzableIndex(
                    lastClosed,
                    Bars.Count))
            {
                RemoveHistoricalObjects();
                return;
            }

            HashSet<string> activeNames =
                new HashSet<string>();

            int drawn = 0;

            for (int i = lastClosed;
                 i >= oldest &&
                 drawn < HistoricalSignalLimit;
                 i--)
            {
                if (!HistoricalRenderingRule.IsClosedAnalyzableIndex(
                        i,
                        Bars.Count))
                    continue;

                DateTime openTime =
                    Bars.OpenTimes[i];

                HistoricalSignalPresentation presentation;

                if (!_historicalSignalCache.TryGetValue(
                        openTime,
                        out presentation))
                {
                    presentation =
                        EvaluateHistoricalBar(
                            i,
                            openTime);

                    _historicalSignalCache[openTime] =
                        presentation;
                }

                if (presentation == null ||
                    !presentation.IsSignal)
                    continue;

                string name =
                    H +
                    HistoricalRenderingRule.ObjectIdentity(
                        presentation.OpenTime);

                if (ShowHistoricalArrows)
                {
                    activeNames.Add(name);

                    if (!_historicalDrawn.Contains(name))
                    {
                        Chart.DrawIcon(
                            name,
                            presentation.Direction == 1
                                ? ChartIconType.UpTriangle : ChartIconType.DownTriangle,
                            i,
                            presentation.Price,
                            presentation.Direction == 1
                                ? BuyArrowColor
                                : SellArrowColor);

                        _historicalDrawn.Add(name);
                    }
                }

                drawn++;
            }

            RemoveStaleHistoricalObjects(
                activeNames);

            TrimHistoricalSignalCache(
                oldest,
                lastClosed);
        }

        private HistoricalSignalPresentation EvaluateHistoricalBar(
            int index,
            DateTime openTime)
        {
            HistoricalSignalPresentation result =
                new HistoricalSignalPresentation
                {
                    OpenTime = openTime,
                    Direction = 0,
                    Price = 0,
                    IsSignal = false
                };

            if (!HistoricalRenderingRule.IsClosedAnalyzableIndex(
                    index,
                    Bars.Count))
                return result;

            Frame frame =
                AnalyzeFrame(
                    Bars,
                    index);

            if (frame == null ||
                frame.Direction == 0 ||
                frame.Quality <
                    MinimumSmartQuality)
                return result;

            int trigger =
                frame.Direction == 1
                    ? BullTriggerScore(
                        Bars,
                        index)
                    : BearTriggerScore(
                        Bars,
                        index);

            if (trigger < 4)
                return result;

            double atr =
                Atr(
                    Bars,
                    index);

            double offset =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr * 0.18);

            result.Direction =
                frame.Direction;

            result.Price =
                frame.Direction == 1
                    ? Bars.LowPrices[index] - offset
                    : Bars.HighPrices[index] + offset;

            result.IsSignal =
                IsFinitePositive(result.Price);

            return result;
        }

        private void RemoveStaleHistoricalObjects(
            HashSet<string> activeNames)
        {
            List<string> stale =
                new List<string>();

            foreach (string name in _historicalDrawn)
            {
                if (!activeNames.Contains(name))
                    stale.Add(name);
            }

            for (int i = 0;
                 i < stale.Count;
                 i++)
            {
                Chart.RemoveObject(
                    stale[i]);

                _historicalDrawn.Remove(
                    stale[i]);
            }
        }

        private void TrimHistoricalSignalCache(
            int oldest,
            int lastClosed)
        {
            if (Bars == null ||
                Bars.Count == 0 ||
                oldest < 0 ||
                lastClosed < oldest)
                return;

            DateTime oldestOpenTime =
                Bars.OpenTimes[oldest];

            DateTime latestOpenTime =
                Bars.OpenTimes[lastClosed];

            List<DateTime> staleKeys =
                new List<DateTime>();

            foreach (DateTime openTime in
                     _historicalSignalCache.Keys)
            {
                if (openTime < oldestOpenTime ||
                    openTime > latestOpenTime)
                    staleKeys.Add(openTime);
            }

            for (int i = 0;
                 i < staleKeys.Count;
                 i++)
            {
                _historicalSignalCache.Remove(
                    staleKeys[i]);
            }
        }

        private void RemoveHistoricalObjects()
        {
            foreach (string name in _historicalDrawn)
                Chart.RemoveObject(name);

            _historicalDrawn.Clear();
        }
    }
}
