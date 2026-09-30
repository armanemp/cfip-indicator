using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DivergenceResult AnalyzeDivergence(
            Bars bars,
            int index)
        {
            if (bars == null ||
                index < 30 ||
                index >= bars.Count)
                return DivergenceResult.CreateNoDivergence();

            double atr = Atr(bars, index);
            if (!IsFinitePositive(atr))
                return DivergenceResult.CreateNoDivergence();

            int lowOldIndex;
            int lowNewIndex;
            double lowOld;
            double lowNew;
            int highOldIndex;
            int highNewIndex;
            double highOld;
            double highNew;

            bool lowsFound =
                TryFindLastTwoSwingLows(
                    bars,
                    index,
                    out lowOldIndex,
                    out lowNewIndex,
                    out lowOld,
                    out lowNew);

            bool highsFound =
                TryFindLastTwoSwingHighs(
                    bars,
                    index,
                    out highOldIndex,
                    out highNewIndex,
                    out highOld,
                    out highNew);

            DivergenceCandidate bull =
                lowsFound
                    ? EvaluateLowDivergence(
                        bars,
                        index,
                        atr,
                        lowOldIndex,
                        lowNewIndex,
                        lowOld,
                        lowNew)
                    : default(DivergenceCandidate);

            DivergenceCandidate bear =
                highsFound
                    ? EvaluateHighDivergence(
                        bars,
                        index,
                        atr,
                        highOldIndex,
                        highNewIndex,
                        highOld,
                        highNew)
                    : default(DivergenceCandidate);

            bool bullValid = bull.Quality >= DivergenceThresholdRule.MinimumQuality;
            bool bearValid = bear.Quality >= DivergenceThresholdRule.MinimumQuality;

            if (!bullValid && !bearValid)
                return DivergenceResult.CreateNoDivergence();

            if (bullValid &&
                bearValid &&
                Math.Abs(
                    bull.Quality -
                    bear.Quality) < DivergenceThresholdRule.ConflictQualityMargin)
            {
                return DivergenceResult.CreateConflict(
                    bull.Regular,
                    bear.BearRegular,
                    bull.Hidden,
                    bear.Hidden);
            }

            if (bullValid &&
                (!bearValid ||
                 bull.Quality > bear.Quality))
            {
                return new DivergenceResult(
                    1,
                    bull.Quality,
                    bull.Type,
                    bull.Regular,
                    false,
                    bull.Hidden,
                    false);
            }

            return new DivergenceResult(
                -1,
                bear.Quality,
                bear.Type,
                false,
                bear.BearRegular,
                false,
                bear.Hidden);
        }

        private bool TryFindLastTwoSwingLows(
            Bars bars,
            int closedIndex,
            out int olderIndex,
            out int newerIndex,
            out double olderPrice,
            out double newerPrice)
        {
            olderIndex = -1;
            newerIndex = -1;
            olderPrice = 0;
            newerPrice = 0;

            int strength =
                Math.Max(
                    1,
                    SwingStrength);

            int last =
                Math.Min(
                    closedIndex - strength,
                    bars.Count -
                    strength - 1);

            int first =
                Math.Max(
                    strength,
                    closedIndex -
                    Math.Max(
                        20,
                        Math.Min(
                            StructureLookback,
                            60)));

            int previousCenter = -1;

            for (int i = last;
                 i >= first;
                 i--)
            {
                int start;
                int end;
                double level;

                if (!IsCanonicalSwingLow(
                        bars,
                        i,
                        closedIndex,
                        strength,
                        out start,
                        out end,
                        out level))
                    continue;

                int center =
                    (start + end) / 2;

                if (center == previousCenter)
                    continue;

                if (newerIndex < 0)
                {
                    newerIndex = center;
                    newerPrice = level;
                    previousCenter = center;
                    continue;
                }

                if (center <=
                    newerIndex -
                    Math.Max(
                        2,
                        strength * 2))
                {
                    olderIndex = center;
                    olderPrice = level;
                    return true;
                }

                previousCenter = center;
            }

            return false;
        }

        private bool TryFindLastTwoSwingHighs(
            Bars bars,
            int closedIndex,
            out int olderIndex,
            out int newerIndex,
            out double olderPrice,
            out double newerPrice)
        {
            olderIndex = -1;
            newerIndex = -1;
            olderPrice = 0;
            newerPrice = 0;

            int strength =
                Math.Max(
                    1,
                    SwingStrength);

            int last =
                Math.Min(
                    closedIndex - strength,
                    bars.Count -
                    strength - 1);

            int first =
                Math.Max(
                    strength,
                    closedIndex -
                    Math.Max(
                        20,
                        Math.Min(
                            StructureLookback,
                            60)));

            int previousCenter = -1;

            for (int i = last;
                 i >= first;
                 i--)
            {
                int start;
                int end;
                double level;

                if (!IsCanonicalSwingHigh(
                        bars,
                        i,
                        closedIndex,
                        strength,
                        out start,
                        out end,
                        out level))
                    continue;

                int center =
                    (start + end) / 2;

                if (center == previousCenter)
                    continue;

                if (newerIndex < 0)
                {
                    newerIndex = center;
                    newerPrice = level;
                    previousCenter = center;
                    continue;
                }

                if (center <=
                    newerIndex -
                    Math.Max(
                        2,
                        strength * 2))
                {
                    olderIndex = center;
                    olderPrice = level;
                    return true;
                }

                previousCenter = center;
            }

            return false;
        }

        private DivergenceCandidate EvaluateLowDivergence(
            Bars bars,
            int index,
            double atr,
            int olderIndex,
            int newerIndex,
            double olderPrice,
            double newerPrice)
        {
            double priceDelta =
                olderPrice - newerPrice;

            double priceExcursion =
                Math.Abs(priceDelta) / atr;

            double oldRsi =
                Rsi(
                    bars,
                    olderIndex);

            double newRsi =
                Rsi(
                    bars,
                    newerIndex);

            double rsiDelta =
                newRsi - oldRsi;

            WaveTrendSnapshot oldWave =
                GetWaveTrendSnapshot(
                    bars,
                    olderIndex);

            WaveTrendSnapshot newWave =
                GetWaveTrendSnapshot(
                    bars,
                    newerIndex);

            double waveDelta =
                oldWave.Valid &&
                newWave.Valid
                    ? newWave.Wave -
                      oldWave.Wave
                    : 0;

            bool regular =
                priceDelta >=
                    Math.Max(
                        0.10,
                        atr * DivergenceThresholdRule.RegularPriceAtr) &&
                rsiDelta >= DivergenceThresholdRule.RegularRsiDelta &&
                (!oldWave.Valid ||
                 !newWave.Valid ||
                 waveDelta >= DivergenceThresholdRule.RegularWaveDelta);

            bool hidden =
                -priceDelta >=
                    Math.Max(
                        0.08,
                        atr * DivergenceThresholdRule.HiddenPriceAtr) &&
                rsiDelta <= -DivergenceThresholdRule.HiddenRsiDelta &&
                oldWave.Valid &&
                newWave.Valid &&
                waveDelta <= -DivergenceThresholdRule.HiddenWaveDelta;

            int oscillatorAgreement = 0;
            if (rsiDelta >= DivergenceThresholdRule.RegularRsiDelta)
                oscillatorAgreement++;
            if (waveDelta >= DivergenceThresholdRule.RegularWaveDelta)
                oscillatorAgreement++;

            if (!regular && !hidden)
                return default(DivergenceCandidate);

            bool chosenHidden =
                hidden &&
                !regular;

            double recentBoost =
                newerIndex >= index - DivergenceThresholdRule.RecentBoostBarsStrong
                    ? DivergenceThresholdRule.RecentBoostStrong
                    : newerIndex >= index - DivergenceThresholdRule.RecentBoostBarsModerate
                        ? DivergenceThresholdRule.RecentBoostModerate
                        : 0;

            int quality =
                DivergenceThresholdRule.CalculateQuality(
                    priceExcursion,
                    oscillatorAgreement,
                    chosenHidden,
                    (int)recentBoost);

            return new DivergenceCandidate(
                Math.Min(
                    100,
                    quality),
                chosenHidden
                    ? "HIDDEN_BULL"
                    : "REGULAR_BULL",
                regular &&
                !chosenHidden,
                false,
                hidden &&
                chosenHidden);
        }

        private DivergenceCandidate EvaluateHighDivergence(
            Bars bars,
            int index,
            double atr,
            int olderIndex,
            int newerIndex,
            double olderPrice,
            double newerPrice)
        {
            double priceDelta =
                newerPrice - olderPrice;

            double priceExcursion =
                Math.Abs(priceDelta) / atr;

            double oldRsi =
                Rsi(
                    bars,
                    olderIndex);

            double newRsi =
                Rsi(
                    bars,
                    newerIndex);

            double rsiDelta =
                newRsi - oldRsi;

            WaveTrendSnapshot oldWave =
                GetWaveTrendSnapshot(
                    bars,
                    olderIndex);

            WaveTrendSnapshot newWave =
                GetWaveTrendSnapshot(
                    bars,
                    newerIndex);

            double waveDelta =
                oldWave.Valid &&
                newWave.Valid
                    ? newWave.Wave -
                      oldWave.Wave
                    : 0;

            bool regular =
                priceDelta >=
                    Math.Max(
                        0.10,
                        atr * DivergenceThresholdRule.RegularPriceAtr) &&
                rsiDelta <= -DivergenceThresholdRule.RegularRsiDelta &&
                (!oldWave.Valid ||
                 !newWave.Valid ||
                 waveDelta <= -DivergenceThresholdRule.RegularWaveDelta);

            bool hidden =
                -priceDelta >=
                    Math.Max(
                        0.08,
                        atr * DivergenceThresholdRule.HiddenPriceAtr) &&
                rsiDelta >= DivergenceThresholdRule.HiddenRsiDelta &&
                oldWave.Valid &&
                newWave.Valid &&
                waveDelta >= DivergenceThresholdRule.HiddenWaveDelta;

            int oscillatorAgreement = 0;
            if (rsiDelta <= -DivergenceThresholdRule.RegularRsiDelta)
                oscillatorAgreement++;
            if (waveDelta <= -DivergenceThresholdRule.RegularWaveDelta)
                oscillatorAgreement++;

            if (!regular && !hidden)
                return default(DivergenceCandidate);

            bool chosenHidden =
                hidden &&
                !regular;

            double recentBoost =
                newerIndex >= index - DivergenceThresholdRule.RecentBoostBarsStrong
                    ? 10
                    : newerIndex >= index - DivergenceThresholdRule.RecentBoostBarsModerate
                        ? 5
                        : 0;

            int quality =
                DivergenceThresholdRule.CalculateQuality(
                    priceExcursion,
                    oscillatorAgreement,
                    chosenHidden,
                    (int)recentBoost);

            return new DivergenceCandidate(
                Math.Min(
                    100,
                    quality),
                chosenHidden
                    ? "HIDDEN_BEAR"
                    : "REGULAR_BEAR",
                false,
                regular &&
                !chosenHidden,
                chosenHidden);
        }
    }
}
