using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private OssIndicatorSnapshot BuildOssIndicatorSnapshot(
            Bars bars,
            int index)
        {
            if (!UseOssExtendedIndicatorConfluence ||
                bars == null ||
                index < 0 ||
                index >= bars.Count)
                return null;

            OssIndicatorSnapshot snapshot =
                new OssIndicatorSnapshot
                {
                    Rsi = FacioQuoRsi(bars, index),
                    MacdHistogram = FacioQuoMacdHistogram(bars, index),
                    BollingerPercentB = FacioQuoBollingerPercentB(bars, index),
                    Mfi = FacioQuoMfi(bars, index),
                    SuperTrend = FacioQuoSuperTrend(bars, index)
                };

            FacioQuoStochBias(
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

            if (rsiValid)
            {
                if (snapshot.Rsi > 50)
                    snapshot.BullVotes++;
                else if (snapshot.Rsi < 50)
                    snapshot.BearVotes++;
            }

            if (macdValid)
            {
                if (snapshot.MacdHistogram > 0)
                    snapshot.BullVotes++;
                else if (snapshot.MacdHistogram < 0)
                    snapshot.BearVotes++;
            }

            if (bollingerValid)
            {
                if (snapshot.BollingerPercentB > 0.5)
                    snapshot.BullVotes++;
                else if (snapshot.BollingerPercentB < 0.5)
                    snapshot.BearVotes++;
            }

            if (mfiValid)
            {
                if (snapshot.Mfi > 50)
                    snapshot.BullVotes++;
                else if (snapshot.Mfi < 50)
                    snapshot.BearVotes++;
            }

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

            snapshot.IndicatorCount =
                (rsiValid ? 1 : 0) +
                (macdValid ? 1 : 0) +
                (bollingerValid ? 1 : 0) +
                (mfiValid ? 1 : 0) +
                (stochValid ? 1 : 0) +
                (superTrendValid ? 1 : 0);

            return snapshot;
        }

        private bool IsFiniteValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
