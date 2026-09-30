using System;

namespace cAlgo
{
    internal sealed partial class WaveTrendMovingAverageCalculator
    {

        private double CalculateWaveTrendSimple(
            double[] source,
            int index,
            int length)
        {
            double sum = 0;

            for (int i =
                     index -
                     length +
                     1;
                 i <= index;
                 i++)
            {
                if (!IsFiniteWaveTrendAverage(
                        source[i]))
                    return double.NaN;

                sum += source[i];
            }

            return sum / length;
        }

        private double CalculateWaveTrendWeighted(
            double[] source,
            int index,
            int length)
        {
            double weightedSum = 0;
            double weightSum = 0;

            for (int i = 0;
                 i < length;
                 i++)
            {
                double sample =
                    source[
                        index -
                        length +
                        1 +
                        i];

                if (!IsFiniteWaveTrendAverage(
                        sample))
                    return double.NaN;

                double weight = i + 1;
                weightedSum += sample * weight;
                weightSum += weight;
            }

            return
                weightSum > 0
                    ? weightedSum /
                      weightSum
                    : double.NaN;
        }

        private double CalculateWaveTrendExponential(
            double[] source,
            int index,
            int length,
            double alpha)
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

            if (index <
                _firstOutputIndex)
                return double.NaN;

            double previous =
                _aux1[
                    index -
                    1];

            if (!IsFiniteWaveTrendAverage(
                    previous) ||
                !IsFiniteWaveTrendAverage(
                    source[index]))
                return double.NaN;

            double result =
                previous +
                alpha *
                (source[index] -
                 previous);

            _aux1[index] = result;
            return result;
        }

        private double CalculateWaveTrendTimeSeries(
            double[] source,
            int index,
            int length)
        {
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumX2 = 0;

            for (int i = 0;
                 i < length;
                 i++)
            {
                double y =
                    source[
                        index -
                        length +
                        1 +
                        i];

                if (!IsFiniteWaveTrendAverage(
                        y))
                    return double.NaN;

                double x = i;
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double denominator =
                length * sumX2 -
                sumX * sumX;

            if (Math.Abs(denominator) <
                1e-12)
                return double.NaN;

            double slope =
                (length * sumXY -
                 sumX * sumY) /
                denominator;

            double intercept =
                (sumY -
                 slope * sumX) /
                length;

            return
                intercept +
                slope *
                (length - 1);
        }

    }
}
