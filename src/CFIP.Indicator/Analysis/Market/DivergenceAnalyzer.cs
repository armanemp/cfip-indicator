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
            if (!UseWaveTrendEvidence ||
                bars == null ||
                index < 30 ||
                index >= bars.Count)
                return DivergenceResult.None();

            double atr = Atr(bars, index);
            if (!IsFinitePositive(atr))
                return DivergenceResult.None();

            int olderLow;
            int newerLow;
            double olderLowPrice;
            double newerLowPrice;

            int olderHigh;
            int newerHigh;
            double olderHighPrice;
            double newerHighPrice;

            bool hasLows =
                TryFindDivergenceLows(
                    bars,
                    index,
                    out olderLow,
                    out newerLow,
                    out olderLowPrice,
                    out newerLowPrice);

            bool hasHighs =
                TryFindDivergenceHighs(
                    bars,
                    index,
                    out olderHigh,
                    out newerHigh,
                    out olderHighPrice,
                    out newerHighPrice);

            DivergenceCandidate bull =
                hasLows
                    ? EvaluateBullishDivergence(
                        bars,
                        index,
                        atr,
                        olderLow,
                        newerLow,
                        olderLowPrice,
                        newerLowPrice)
                    : default(DivergenceCandidate);

            DivergenceCandidate bear =
                hasHighs
                    ? EvaluateBearishDivergence(
                        bars,
                        index,
                        atr,
                        olderHigh,
                        newerHigh,
                        olderHighPrice,
                        newerHighPrice)
                    : default(DivergenceCandidate);

            bool bullValid =
                bull.Quality >= 62 &&
                bull.OscillatorConfluence >= 1;

            bool bearValid =
                bear.Quality >= 62 &&
                bear.OscillatorConfluence >= 1;

            if (!bullValid && !bearValid)
                return DivergenceResult.None();

            if (bullValid &&
                bearValid &&
                Math.Abs(
                    bull.Quality -
                    bear.Quality) < 12)
            {
                return new DivergenceResult(
                    0,
                    Math.Max(
                        bull.Quality,
                        bear.Quality),
                    "CONFLICT",
                    bull.Regular,
                    bear.Regular,
                    bull.Hidden,
                    bear.Hidden,
                    Math.Min(
                        bull.AgeBars,
                        bear.AgeBars));
            }

            DivergenceCandidate selected =
                bullValid &&
                (!bearValid ||
                 bull.Quality > bear.Quality)
                    ? bull
                    : bear;

            return new DivergenceResult(
                selected.Direction,
                selected.Quality,
                selected.Type,
                selected.Direction == 1 && selected.Regular,
                selected.Direction == -1 && selected.Regular,
                selected.Direction == 1 && selected.Hidden,
                selected.Direction == -1 && selected.Hidden,
                selected.AgeBars);
        }

        private bool TryFindDivergenceLows(
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

            int latest =
                Math.Min(
                    closedIndex - strength,
                    bars.Count - strength - 1);

            int earliest =
                Math.Max(
                    strength,
                    closedIndex -
                    Math.Max(
                        30,
                        Math.Min(
                            StructureLookback,
                            80)));

            int previousCenter = -1;

            for (int i = latest;
                 i >= earliest;
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

        private bool TryFindDivergenceHighs(
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

            int latest =
                Math.Min(
                    closedIndex - strength,
                    bars.Count - strength - 1);

            int earliest =
                Math.Max(
                    strength,
                    closedIndex -
                    Math.Max(
                        30,
                        Math.Min(
                            StructureLookback,
                            80)));

            int previousCenter = -1;

            for (int i = latest;
                 i >= earliest;
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

        private DivergenceCandidate EvaluateBullishDivergence(
            Bars bars,
            int index,
            double atr,
            int olderIndex,
            int newerIndex,
            double olderPrice,
            double newerPrice)
        {
            double priceDelta =
                olderPrice -
                newerPrice;

            double priceExcursion =
                Math.Abs(priceDelta) /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            double oldRsi = Rsi(bars, olderIndex);
            double newRsi = Rsi(bars, newerIndex);
            double rsiDelta =
                newRsi -
                oldRsi;

            WaveTrendSnapshot oldWave =
                GetWaveTrendSnapshot(
                    bars,
                    olderIndex);
            WaveTrendSnapshot newWave =
                GetWaveTrendSnapshot(
                    bars,
                    newerIndex);

            if (!oldWave.Valid ||
                !newWave.Valid)
                return DivergenceCandidate.None(1);

            double waveDelta =
                newWave.Wave -
                oldWave.Wave;

            bool regular =
                priceDelta >=
                    Math.Max(
                        atr * 0.15,
                        Symbol.PipSize * 3) &&
                rsiDelta >= 3.0 &&
                waveDelta >= 2.50;

            bool hidden =
                -priceDelta >=
                    Math.Max(
                        atr * 0.12,
                        Symbol.PipSize * 3) &&
                rsiDelta <= -3.0 &&
                waveDelta <= -2.50;

            if (!regular && !hidden)
                return DivergenceCandidate.None(1);

            int oscillatorConfluence = 0;

            if (regular)
            {
                if (rsiDelta >= 3.0)
                    oscillatorConfluence++;

                if (waveDelta >= 2.50)
                    oscillatorConfluence++;

                if (newWave.Oversold ||
                    newWave.BullCross ||
                    newWave.Rising)
                    oscillatorConfluence++;
            }
            else
            {
                if (rsiDelta <= -3.0)
                    oscillatorConfluence++;

                if (waveDelta <= -2.50)
                    oscillatorConfluence++;

                if (newWave.Falling)
                    oscillatorConfluence++;
            }

            int quality =
                45 +
                Math.Min(
                    20,
                    (int)Math.Round(
                        priceExcursion * 18.0)) +
                Math.Min(
                    20,
                    (int)Math.Round(
                        Math.Abs(rsiDelta) * 2.2)) +
                Math.Min(
                    12,
                    (int)Math.Round(
                        Math.Abs(waveDelta) * 2.0)) +
                oscillatorConfluence * 4;

            if (newWave.Oversold)
                quality += 4;

            int ageBars =
                Math.Max(
                    0,
                    index -
                    newerIndex);

            if (ageBars <= 4)
                quality += 10;
            else if (ageBars <= 8)
                quality += 6;
            else if (ageBars <= 14)
                quality += 2;
            else
                quality -= Math.Min(
                    15,
                    ageBars - 14);

            return new DivergenceCandidate(
                1,
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality)),
                regular,
                hidden,
                oscillatorConfluence,
                ageBars,
                regular
                    ? "REGULAR_BULL"
                    : "HIDDEN_BULL");
        }

        private DivergenceCandidate EvaluateBearishDivergence(
            Bars bars,
            int index,
            double atr,
            int olderIndex,
            int newerIndex,
            double olderPrice,
            double newerPrice)
        {
            double priceDelta =
                newerPrice -
                olderPrice;

            double priceExcursion =
                Math.Abs(priceDelta) /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            double oldRsi = Rsi(bars, olderIndex);
            double newRsi = Rsi(bars, newerIndex);
            double rsiDelta =
                newRsi -
                oldRsi;

            WaveTrendSnapshot oldWave =
                GetWaveTrendSnapshot(
                    bars,
                    olderIndex);
            WaveTrendSnapshot newWave =
                GetWaveTrendSnapshot(
                    bars,
                    newerIndex);

            if (!oldWave.Valid ||
                !newWave.Valid)
                return DivergenceCandidate.None(-1);

            double waveDelta =
                newWave.Wave -
                oldWave.Wave;

            bool regular =
                priceDelta >=
                    Math.Max(
                        atr * 0.15,
                        Symbol.PipSize * 3) &&
                rsiDelta <= -3.0 &&
                waveDelta <= -2.50;

            bool hidden =
                -priceDelta >=
                    Math.Max(
                        atr * 0.12,
                        Symbol.PipSize * 3) &&
                rsiDelta >= 3.0 &&
                waveDelta >= 2.50;

            if (!regular && !hidden)
                return DivergenceCandidate.None(-1);

            int oscillatorConfluence = 0;

            if (regular)
            {
                if (rsiDelta <= -3.0)
                    oscillatorConfluence++;

                if (waveDelta <= -2.50)
                    oscillatorConfluence++;

                if (newWave.Overbought ||
                    newWave.BearCross ||
                    newWave.Falling)
                    oscillatorConfluence++;
            }
            else
            {
                if (rsiDelta >= 3.0)
                    oscillatorConfluence++;

                if (waveDelta >= 2.50)
                    oscillatorConfluence++;

                if (newWave.Rising)
                    oscillatorConfluence++;
            }

            int quality =
                45 +
                Math.Min(
                    20,
                    (int)Math.Round(
                        priceExcursion * 18.0)) +
                Math.Min(
                    20,
                    (int)Math.Round(
                        Math.Abs(rsiDelta) * 2.2)) +
                Math.Min(
                    12,
                    (int)Math.Round(
                        Math.Abs(waveDelta) * 2.0)) +
                oscillatorConfluence * 4;

            if (newWave.Overbought)
                quality += 4;

            int ageBars =
                Math.Max(
                    0,
                    index -
                    newerIndex);

            if (ageBars <= 4)
                quality += 10;
            else if (ageBars <= 8)
                quality += 6;
            else if (ageBars <= 14)
                quality += 2;
            else
                quality -= Math.Min(
                    15,
                    ageBars - 14);

            return new DivergenceCandidate(
                -1,
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality)),
                regular,
                hidden,
                oscillatorConfluence,
                ageBars,
                regular
                    ? "REGULAR_BEAR"
                    : "HIDDEN_BEAR");
        }

        private readonly struct DivergenceCandidate
        {
            public int Direction { get; }
            public int Quality { get; }
            public bool Regular { get; }
            public bool Hidden { get; }
            public int OscillatorConfluence { get; }
            public int AgeBars { get; }
            public string Type { get; }

            public DivergenceCandidate(
                int direction,
                int quality,
                bool regular,
                bool hidden,
                int oscillatorConfluence,
                int ageBars,
                string type)
            {
                Direction = direction;
                Quality = quality;
                Regular = regular;
                Hidden = hidden;
                OscillatorConfluence =
                    Math.Max(
                        0,
                        oscillatorConfluence);
                AgeBars =
                    Math.Max(
                        0,
                        ageBars);
                Type =
                    type ?? "NONE";
            }

            public static DivergenceCandidate None(
                int direction)
            {
                return new DivergenceCandidate(
                    direction,
                    0,
                    false,
                    false,
                    0,
                    0,
                    "NONE");
            }
        }
    }
}
