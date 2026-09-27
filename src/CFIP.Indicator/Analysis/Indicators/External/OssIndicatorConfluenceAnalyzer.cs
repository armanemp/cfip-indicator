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
                    SuperTrend = FacioQuoSuperTrend(bars, index),
                    AroonOscillator = FacioQuoAroonOscillator(bars, index),
                    Cci = FacioQuoCci(bars, index),
                    ObvBias = FacioQuoObvBias(bars, index),
                    ParabolicSar = FacioQuoParabolicSar(bars, index)
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
            bool aroonValid = IsFiniteValue(snapshot.AroonOscillator);
            bool cciValid = IsFiniteValue(snapshot.Cci);
            bool obvValid = IsFiniteValue(snapshot.ObvBias);
            bool parabolicSarValid = IsFiniteValue(snapshot.ParabolicSar);

            AddDirectionalVote(snapshot.Rsi, 50, true, rsiValid, snapshot);
            AddDirectionalVote(snapshot.MacdHistogram, 0, true, macdValid, snapshot);
            AddDirectionalVote(snapshot.BollingerPercentB, 0.5, true, bollingerValid, snapshot);
            AddDirectionalVote(snapshot.Mfi, 50, true, mfiValid, snapshot);
            AddDirectionalVote(snapshot.AroonOscillator, 0, true, aroonValid, snapshot);
            AddDirectionalVote(snapshot.Cci, 0, true, cciValid, snapshot);

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

            return snapshot;
        }

        private void AddDirectionalVote(
            double value,
            double neutralLevel,
            bool symmetric,
            bool valid,
            OssIndicatorSnapshot snapshot)
        {
            if (!valid || snapshot == null)
                return;

            if (symmetric && value > neutralLevel)
                snapshot.BullVotes++;
            else if (symmetric && value < neutralLevel)
                snapshot.BearVotes++;
        }

        private bool IsFiniteValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
