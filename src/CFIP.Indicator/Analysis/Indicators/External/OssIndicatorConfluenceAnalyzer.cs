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
                new OssIndicatorSnapshot();

            snapshot.Rsi =
                FacioQuoRsi(
                    bars,
                    index);

            snapshot.MacdHistogram =
                FacioQuoMacdHistogram(
                    bars,
                    index);

            snapshot.BollingerPercentB =
                FacioQuoBollingerPercentB(
                    bars,
                    index);

            snapshot.Mfi =
                FacioQuoMfi(
                    bars,
                    index);

            FacioQuoStochBias(
                bars,
                index,
                out double k,
                out double d);

            snapshot.StochK = k;
            snapshot.StochD = d;
            snapshot.SuperTrend =
                FacioQuoSuperTrend(
                    bars,
                    index);

            double close =
                bars.ClosePrices[index];

            if (snapshot.Rsi > 50)
                snapshot.BullVotes++;
            else if (snapshot.Rsi > 0 &&
                     snapshot.Rsi < 50)
                snapshot.BearVotes++;

            if (snapshot.MacdHistogram > 0)
                snapshot.BullVotes++;
            else if (snapshot.MacdHistogram < 0)
                snapshot.BearVotes++;

            if (snapshot.BollingerPercentB > 0.5)
                snapshot.BullVotes++;
            else if (snapshot.BollingerPercentB > 0 &&
                     snapshot.BollingerPercentB < 0.5)
                snapshot.BearVotes++;

            if (snapshot.Mfi > 50)
                snapshot.BullVotes++;
            else if (snapshot.Mfi > 0 &&
                     snapshot.Mfi < 50)
                snapshot.BearVotes++;

            if (k > 0 && d > 0)
            {
                if (k > d)
                    snapshot.BullVotes++;
                else if (k < d)
                    snapshot.BearVotes++;
            }

            if (snapshot.SuperTrend > 0)
            {
                if (close > snapshot.SuperTrend)
                    snapshot.BullVotes++;
                else if (close < snapshot.SuperTrend)
                    snapshot.BearVotes++;
            }

            snapshot.IndicatorCount =
                Math.Min(
                    6,
                    (snapshot.Rsi > 0 ? 1 : 0) +
                    (Math.Abs(snapshot.MacdHistogram) > 0 ? 1 : 0) +
                    (snapshot.BollingerPercentB > 0 ? 1 : 0) +
                    (snapshot.Mfi > 0 ? 1 : 0) +
                    (k > 0 && d > 0 ? 1 : 0) +
                    (snapshot.SuperTrend > 0 ? 1 : 0));

            return snapshot;
        }
    }
}
