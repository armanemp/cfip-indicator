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

            double rawNow =
                CalculateWaveTrendHullRaw(
                    source,
                    index,
                    halfLength,
                    length);

            if (!IsFiniteWaveTrendAverage(
                    rawNow))
                return double.NaN;

            double sum = rawNow;
            double weighted = rawNow;

            for (int i = 1;
                 i < finalLength;
                 i++)
            {
                double raw =
                    CalculateWaveTrendHullRaw(
                        source,
                        index -
                        finalLength +
                        1 +
                        i,
                        halfLength,
                        length);

                if (!IsFiniteWaveTrendAverage(
                        raw))
                    return double.NaN;

                sum += raw;
                weighted += raw * (i + 1);
            }

            double firstWeight =
                finalLength *
                (finalLength + 1) /
                2.0;

            if (firstWeight <= 0)
                return double.NaN;

            return
                weighted /
                firstWeight;
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
            double ema1 =
                CalculateWaveTrendEmaSeries(
                    source,
                    index,
                    length,
                    _validFrom,
                    _aux1);

            if (!IsFiniteWaveTrendAverage(
                    ema1))
                return double.NaN;

            double ema2 =
                CalculateWaveTrendEmaSeries(
                    _aux1,
                    index,
                    length,
                    _validFrom +
                    length -
                    1,
                    _aux2);

            if (!IsFiniteWaveTrendAverage(
                    ema2))
                return double.NaN;

            return
                2.0 *
                ema1 -
                ema2;
        }

        private double CalculateWaveTrendTripleExponential(
            double[] source,
            int index,
            int length)
        {
            double ema1 =
                CalculateWaveTrendEmaSeries(
                    source,
                    index,
                    length,
                    _validFrom,
                    _aux1);

            if (!IsFiniteWaveTrendAverage(
                    ema1))
                return double.NaN;

            double ema2 =
                CalculateWaveTrendEmaSeries(
                    _aux1,
                    index,
                    length,
                    _validFrom +
                    length -
                    1,
                    _aux2);

            if (!IsFiniteWaveTrendAverage(
                    ema2))
                return double.NaN;

            double ema3 =
                CalculateWaveTrendEmaSeries(
                    _aux2,
                    index,
                    length,
                    _validFrom +
                    2 *
                    (length - 1),
                    _aux3);

            if (!IsFiniteWaveTrendAverage(
                    ema3))
                return double.NaN;

            return
                3.0 *
                ema1 -
                3.0 *
                ema2 +
                ema3;
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
