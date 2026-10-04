using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TrySelectPredictivePendingLevel(
            int closedM5,
            int direction,
            double atr,
            double market,
            out PredictivePendingCandidate selected)
        {
            selected = null;

            if (_m5Bars == null ||
                _m5Bars.Count < 20 ||
                atr <= 0 ||
                (direction != 1 && direction != -1) ||
                !IsFinitePositive(market))
                return false;

            double minimumFutureDistance =
                Math.Max(
                    Symbol.PipSize * 4,
                    atr * 0.18);

            double maximumFutureDistance =
                atr * 2.50;

            List<PredictivePendingCandidate> candidates =
                new List<PredictivePendingCandidate>();

            CollectPredictiveZoneCandidates(
                _m5Bars,
                closedM5,
                direction,
                atr,
                market,
                minimumFutureDistance,
                maximumFutureDistance,
                1.00,
                "M5",
                candidates);

            if (_m15Bars != null)
            {
                int m15Index =
                    ClosedIndex(
                        _m15Bars,
                        ClosedBarBoundaryReference(
                            _m5Bars,
                            closedM5,
                            Server.TimeInUtc));

                double m15Atr =
                    m15Index >= 20
                        ? Atr(_m15Bars, m15Index)
                        : 0;

                if (m15Index >= 20 &&
                    m15Atr > 0)
                {
                    CollectPredictiveZoneCandidates(
                        _m15Bars,
                        m15Index,
                        direction,
                        m15Atr,
                        market,
                        minimumFutureDistance,
                        maximumFutureDistance,
                        1.20,
                        "M15",
                        candidates);
                }
            }

            double structuralLevel =
                direction == 1
                    ? FindSwingLowBelow(
                        _m5Bars,
                        closedM5,
                        market)
                    : FindSwingHighAbove(
                        _m5Bars,
                        closedM5,
                        market);

            AddPredictivePointCandidate(
                candidates,
                structuralLevel,
                70,
                direction == 1
                    ? "M5 SWING LOW"
                    : "M5 SWING HIGH",
                direction,
                market,
                atr,
                minimumFutureDistance,
                maximumFutureDistance);

            double equalLevel =
                direction == 1
                    ? FindEqualLow(
                        _m5Bars,
                        closedM5,
                        market,
                        atr)
                    : FindEqualHigh(
                        _m5Bars,
                        closedM5,
                        market,
                        atr);

            AddPredictivePointCandidate(
                candidates,
                equalLevel,
                82,
                direction == 1
                    ? "M5 EQUAL LOW / LIQUIDITY"
                    : "M5 EQUAL HIGH / LIQUIDITY",
                direction,
                market,
                atr,
                minimumFutureDistance,
                maximumFutureDistance);

            if (candidates.Count == 0)
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • NO FUTURE STRUCTURAL LEVEL";
                return false;
            }

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                PredictivePendingCandidate candidate =
                    candidates[i];

                HashSet<string> confluenceKeys =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                confluenceKeys.Add(
                    PredictivePendingSourceKey(
                        candidate.Source));

                int confluence = 1;

                List<string> sources =
                    new List<string>
                    {
                        candidate.Source
                    };

                for (int j = 0;
                     j < candidates.Count;
                     j++)
                {
                    if (i == j)
                        continue;

                    PredictivePendingCandidate other =
                        candidates[j];

                    if (Math.Abs(
                            other.Price -
                            candidate.Price) >
                        atr * 0.12)
                        continue;

                    string otherKey =
                        PredictivePendingSourceKey(
                            other.Source);

                    if (!confluenceKeys.Add(
                            otherKey))
                        continue;

                    confluence++;

                    if (sources.Count < 3 &&
                        !sources.Contains(
                            other.Source))
                    {
                        sources.Add(
                            other.Source);
                    }
                }

                candidate.ConfluenceCount =
                    confluence;

                candidate.Score =
                    Math.Min(
                        100,
                        candidate.Score +
                        Math.Min(
                            16,
                            Math.Max(
                                0,
                                confluence - 1) * 7));

                candidate.Source =
                    string.Join(
                        "+",
                        sources.ToArray());

                candidate.Quality =
                    ClampInt(
                        (int)Math.Round(
                            candidate.Score),
                        0,
                        100);
            }

            candidates =
                candidates
                    .OrderByDescending(c => c.Score)
                    .ThenBy(c => c.DistanceAtr)
                    .ToList();

            selected =
                candidates.FirstOrDefault(
                    c =>
                        c.Score >=
                        Math.Max(
                            65,
                            PendingMinimumSmartQuality));

            return selected != null;
        }

    }
}
