using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly OssIndicatorSnapshotCache _ossIndicatorSnapshotCache =
            new OssIndicatorSnapshotCache();

        private OssIndicatorSnapshot BuildOssIndicatorSnapshot(
            Bars bars,
            int index)
        {
            if (!UseOssExtendedIndicatorConfluence ||
                bars == null ||
                index < 0 ||
                index >= bars.Count)
                return null;

            if (_ossIndicatorSnapshotCache.TryGet(
                    bars,
                    index,
                    out OssIndicatorSnapshot cachedSnapshot))
            {
                return cachedSnapshot;
            }

            OssIndicatorSnapshot snapshot =
                new OssIndicatorSnapshot
                {
                    Rsi = SkenderRsi(bars, index),
                    MacdHistogram = SkenderMacdHistogram(bars, index),
                    BollingerPercentB = SkenderBollingerPercentB(bars, index),
                    Mfi = SkenderMfi(bars, index),
                    SuperTrend = SkenderSuperTrend(bars, index),
                    AroonOscillator = SkenderAroonOscillator(bars, index),
                    Cci = SkenderCci(bars, index),
                    ObvBias = SkenderObvBias(bars, index),
                    ParabolicSar = SkenderParabolicSar(bars, index)
                };

            SkenderStochBias(
                bars,
                index,
                out double k,
                out double d);

            snapshot.StochK = k;
            snapshot.StochD = d;

            double close = bars.ClosePrices[index];

            bool rsiValid = IsFiniteValue(snapshot.Rsi);
            bool macdValid = IsFiniteValue(snapshot.MacdHistogram);
            bool bollingerValid = IsFiniteValue(snapshot.BollingerPercentB);
            bool mfiValid = IsFiniteValue(snapshot.Mfi);
            bool stochValid =
                IsFiniteValue(snapshot.StochK) &&
                IsFiniteValue(snapshot.StochD);
            bool superTrendValid = IsFiniteValue(snapshot.SuperTrend);
            bool aroonValid = IsFiniteValue(snapshot.AroonOscillator);
            bool cciValid = IsFiniteValue(snapshot.Cci);
            bool obvValid = IsFiniteValue(snapshot.ObvBias);
            bool parabolicSarValid = IsFiniteValue(snapshot.ParabolicSar);

            AddDirectionalVote(snapshot.Rsi, 50, rsiValid, snapshot);
            AddDirectionalVote(snapshot.MacdHistogram, 0, macdValid, snapshot);
            AddDirectionalVote(snapshot.BollingerPercentB, 0.5, bollingerValid, snapshot);
            AddDirectionalVote(snapshot.Mfi, 50, mfiValid, snapshot);
            AddDirectionalVote(snapshot.AroonOscillator, 0, aroonValid, snapshot);
            AddDirectionalVote(snapshot.Cci, 0, cciValid, snapshot);

            if (stochValid)
            {
                if (snapshot.StochK > snapshot.StochD)
                    snapshot.BullVotes++;
                else if (snapshot.StochK < snapshot.StochD)
                    snapshot.BearVotes++;
            }

            if (superTrendValid &&
                IsFiniteValue(close))
            {
                if (close > snapshot.SuperTrend)
                    snapshot.BullVotes++;
                else if (close < snapshot.SuperTrend)
                    snapshot.BearVotes++;
            }

            if (obvValid)
            {
                if (snapshot.ObvBias > 0)
                    snapshot.BullVotes++;
                else if (snapshot.ObvBias < 0)
                    snapshot.BearVotes++;
            }

            if (parabolicSarValid &&
                IsFiniteValue(close))
            {
                if (close > snapshot.ParabolicSar)
                    snapshot.BullVotes++;
                else if (close < snapshot.ParabolicSar)
                    snapshot.BearVotes++;
            }

            snapshot.IndicatorCount =
                (rsiValid ? 1 : 0) +
                (macdValid ? 1 : 0) +
                (bollingerValid ? 1 : 0) +
                (mfiValid ? 1 : 0) +
                (stochValid ? 1 : 0) +
                (superTrendValid ? 1 : 0) +
                (aroonValid ? 1 : 0) +
                (cciValid ? 1 : 0) +
                (obvValid ? 1 : 0) +
                (parabolicSarValid ? 1 : 0);

            _ossIndicatorSnapshotCache.Set(
                bars,
                index,
                snapshot);

            return snapshot;
        }

        private void AddDirectionalVote(
            double value,
            double neutralLevel,
            bool valid,
            OssIndicatorSnapshot snapshot)
        {
            if (!valid || snapshot == null)
                return;

            if (value > neutralLevel)
                snapshot.BullVotes++;
            else if (value < neutralLevel)
                snapshot.BearVotes++;
        }

    }
}
