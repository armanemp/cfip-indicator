using System;

namespace cAlgo
{
    internal sealed partial class WaveTrendMovingAverageCalculator
    {

        private double CalculateWaveTrendTriangular(
            double[] source,
            int index,
            int length)
        {
            int firstLength =
                (length + 1) / 2;

            int secondLength =
                length % 2 == 0
                    ? length / 2 + 1
                    : firstLength;

            double outerSum = 0;

            for (int outer =
                     0;
                 outer < secondLength;
                 outer++)
            {
                double inner =
                    CalculateWaveTrendSimple(
                        source,
                        index -
                        secondLength +
                        1 +
                        outer,
                        firstLength);

                if (!IsFiniteWaveTrendAverage(
                        inner))
                    return double.NaN;

                outerSum += inner;
            }

            return
                outerSum /
                secondLength;
        }

        private double CalculateWaveTrendVidya(
            double[] source,
            int index,
            int length)
        {
            if (index ==
                _firstOutputIndex)
            {
                double seed =
                    CalculateWaveTrendSimple(
                        source,
                        index,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        seed))
                    return double.NaN;

                _aux1[index] = seed;
                return seed;
            }

            double previous =
                _aux1[
                    index -
                    1];

            if (!IsFiniteWaveTrendAverage(
                    previous))
                return double.NaN;

            double up = 0;
            double down = 0;

            for (int i =
                     index -
                     length +
                     1;
                 i <= index;
                 i++)
            {
                double current =
                    source[i];
                double prior =
                    source[i - 1];

                if (!IsFiniteWaveTrendAverage(
                        current) ||
                    !IsFiniteWaveTrendAverage(
                        prior))
                    return double.NaN;

                double change =
                    current -
                    prior;

                if (change > 0)
                    up += change;
                else
                    down -= change;
            }

            double total =
                up + down;

            double efficiency =
                total > 0
                    ? Math.Abs(
                        (up - down) /
                        total)
                    : 0;

            double alpha =
                2.0 /
                (length + 1.0) *
                efficiency;

            double result =
                previous +
                alpha *
                (source[index] -
                 previous);

            _aux1[index] = result;
            return result;
        }

        private double CalculateWaveTrendHull(
            double[] source,
            int index,
            int length)
        {
            int halfLength =
                Math.Max(
                    1,
                    length / 2);

            int finalLength =
                Math.Max(
                    1,
                    (int)Math.Round(
                        Math.Sqrt(
                            length)));

            int firstRawIndex =
                index -
                finalLength +
                1;

            double weightedSum = 0;
            double weightSum = 0;

            for (int i = 0;
                 i < finalLength;
                 i++)
            {
                double raw =
                    CalculateWaveTrendHullRaw(
                        source,
                        firstRawIndex + i,
                        halfLength,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        raw))
                    return double.NaN;

                double weight =
                    i + 1;

                weightedSum +=
                    raw *
                    weight;

                weightSum +=
                    weight;
            }

            return
                weightSum > 0
                    ? weightedSum /
                      weightSum
                    : double.NaN;
        }

        private double CalculateWaveTrendHullRaw(
            double[] source,
            int index,
            int halfLength,
            int fullLength)
        {
            double shortWma =
                CalculateWaveTrendWeighted(
                    source,
                    index,
                    halfLength);

            double longWma =
                CalculateWaveTrendWeighted(
                    source,
                    index,
                    fullLength);

            if (!IsFiniteWaveTrendAverage(
                    shortWma) ||
                !IsFiniteWaveTrendAverage(
                    longWma))
                return double.NaN;

            return
                2.0 *
                shortWma -
                longWma;
        }

        private double CalculateWaveTrendDoubleExponential(
            double[] source,
            int index,
            int length)
        {
            int ema1FirstIndex =
                _validFrom +
                length -
                1;

            int ema2FirstIndex =
                ema1FirstIndex +
                length -
                1;

            if (index <
                ema2FirstIndex)
                return double.NaN;

            if (index ==
                _firstOutputIndex)
            {
                for (int i =
                         ema1FirstIndex;
                     i <= index;
                     i++)
                {
                    double seeded;

                    if (!TryCalculateWaveTrendEmaPoint(
                            source,
                            i,
                            length,
                            ema1FirstIndex,
                            _aux1,
                            out seeded))
                        return double.NaN;
                }

                for (int i =
                         ema2FirstIndex;
                     i <= index;
                     i++)
                {
                    double seeded;

                    if (!TryCalculateWaveTrendEmaPoint(
                            _aux1,
                            i,
                            length,
                            ema2FirstIndex,
                            _aux2,
                            out seeded))
                        return double.NaN;
                }
            }
            else
            {
                double ema1;
                double ema2;

                if (!TryCalculateWaveTrendEmaPoint(
                        source,
                        index,
                        length,
                        ema1FirstIndex,
                        _aux1,
                        out ema1) ||
                    !TryCalculateWaveTrendEmaPoint(
                        _aux1,
                        index,
                        length,
                        ema2FirstIndex,
                        _aux2,
                        out ema2))
                    return double.NaN;
            }

            double currentEma1 =
                _aux1[index];
            double currentEma2 =
                _aux2[index];

            if (!IsFiniteWaveTrendAverage(
                    currentEma1) ||
                !IsFiniteWaveTrendAverage(
                    currentEma2))
                return double.NaN;

            return
                2.0 *
                currentEma1 -
                currentEma2;
        }

        private double CalculateWaveTrendTripleExponential(
            double[] source,
            int index,
            int length)
        {
            int ema1FirstIndex =
                _validFrom +
                length -
                1;

            int ema2FirstIndex =
                ema1FirstIndex +
                length -
                1;

            int ema3FirstIndex =
                ema2FirstIndex +
                length -
                1;

            if (index <
                ema3FirstIndex)
                return double.NaN;

            if (index ==
                _firstOutputIndex)
            {
                for (int i =
                         ema1FirstIndex;
                     i <= index;
                     i++)
                {
                    double seeded;

                    if (!TryCalculateWaveTrendEmaPoint(
                            source,
                            i,
                            length,
                            ema1FirstIndex,
                            _aux1,
                            out seeded))
                        return double.NaN;
                }

                for (int i =
                         ema2FirstIndex;
                     i <= index;
                     i++)
                {
                    double seeded;

                    if (!TryCalculateWaveTrendEmaPoint(
                            _aux1,
                            i,
                            length,
                            ema2FirstIndex,
                            _aux2,
                            out seeded))
                        return double.NaN;
                }

                for (int i =
                         ema3FirstIndex;
                     i <= index;
                     i++)
                {
                    double seeded;

                    if (!TryCalculateWaveTrendEmaPoint(
                            _aux2,
                            i,
                            length,
                            ema3FirstIndex,
                            _aux3,
                            out seeded))
                        return double.NaN;
                }
            }
            else
            {
                double ema1;
                double ema2;
                double ema3;

                if (!TryCalculateWaveTrendEmaPoint(
                        source,
                        index,
                        length,
                        ema1FirstIndex,
                        _aux1,
                        out ema1) ||
                    !TryCalculateWaveTrendEmaPoint(
                        _aux1,
                        index,
                        length,
                        ema2FirstIndex,
                        _aux2,
                        out ema2) ||
                    !TryCalculateWaveTrendEmaPoint(
                        _aux2,
                        index,
                        length,
                        ema3FirstIndex,
                        _aux3,
                        out ema3))
                    return double.NaN;
            }

            double currentEma1 =
                _aux1[index];
            double currentEma2 =
                _aux2[index];
            double currentEma3 =
                _aux3[index];

            if (!IsFiniteWaveTrendAverage(
                    currentEma1) ||
                !IsFiniteWaveTrendAverage(
                    currentEma2) ||
                !IsFiniteWaveTrendAverage(
                    currentEma3))
                return double.NaN;

            return
                3.0 *
                currentEma1 -
                3.0 *
                currentEma2 +
                currentEma3;
        }

        private bool TryCalculateWaveTrendEmaPoint(
            double[] source,
            int index,
            int length,
            int firstIndex,
            double[] output,
            out double value)
        {
            value = double.NaN;

            if (source == null ||
                index < firstIndex ||
                index >= source.Length)
                return false;

            if (index ==
                firstIndex)
            {
                value =
                    CalculateWaveTrendSimple(
                        source,
                        index,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        value))
                    return false;

                output[index] = value;
                return true;
            }

            double previous =
                output[
                    index -
                    1];

            if (!IsFiniteWaveTrendAverage(
                    previous) ||
                !IsFiniteWaveTrendAverage(
                    source[index]))
                return false;

            double alpha =
                2.0 /
                (length + 1.0);

            value =
                previous +
                alpha *
                (source[index] -
                 previous);

            if (!IsFiniteWaveTrendAverage(
                    value))
                return false;

            output[index] = value;
            return true;
        }

        private double CalculateWaveTrendKaufman(
            double[] source,
            int index,
            int length)
        {
            if (index ==
                _firstOutputIndex)
            {
                double seed =
                    CalculateWaveTrendSimple(
                        source,
                        index,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        seed))
                    return double.NaN;

                _aux1[index] = seed;
                return seed;
            }

            double previous =
                _aux1[
                    index -
                    1];

            if (!IsFiniteWaveTrendAverage(
                    previous))
                return double.NaN;

            double direction =
                Math.Abs(
                    source[index] -
                    source[
                        index -
                        length]);

            double volatility = 0;

            for (int i =
                     index -
                     length +
                     1;
                 i <= index;
                 i++)
            {
                double current =
                    source[i];
                double prior =
                    source[i - 1];

                if (!IsFiniteWaveTrendAverage(
                        current) ||
                    !IsFiniteWaveTrendAverage(
                        prior))
                    return double.NaN;

                volatility +=
                    Math.Abs(
                        current -
                        prior);
            }

            double efficiency =
                volatility > 0
                    ? direction /
                      volatility
                    : 0;

            double fast =
                2.0 /
                (2.0 + 1.0);

            double slow =
                2.0 /
                (30.0 + 1.0);

            double smoothing =
                Math.Pow(
                    efficiency *
                    (fast - slow) +
                    slow,
                    2);

            double result =
                previous +
                smoothing *
                (source[index] -
                 previous);

            _aux1[index] = result;
            return result;
        }

        private double CalculateWaveTrendEmaSeries(
            double[] source,
            int index,
            int length,
            int validFrom,
            double[] output)
        {
            int firstIndex =
                validFrom +
                length -
                1;

            if (index <
                firstIndex)
                return double.NaN;

            if (index ==
                firstIndex)
            {
                double seed =
                    CalculateWaveTrendSimple(
                        source,
                        index,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        seed))
                    return double.NaN;

                output[index] = seed;
                return seed;
            }

            double previous =
                output[
                    index -
                    1];

            if (!IsFiniteWaveTrendAverage(
                    previous) ||
                !IsFiniteWaveTrendAverage(
                    source[index]))
                return double.NaN;

            double alpha =
                2.0 /
                (length + 1.0);

            double result =
                previous +
                alpha *
                (source[index] -
                 previous);

            output[index] = result;
            return result;
        }

    }
}
