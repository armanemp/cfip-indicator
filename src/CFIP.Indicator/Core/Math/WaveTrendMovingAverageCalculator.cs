using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic WaveTrend moving-average implementation.
    /// The calculator is independent from Bars/UI/broker state.
    /// </summary>
    internal sealed class WaveTrendMovingAverageCalculator
    {
        internal const int Simple = 0;
        internal const int Exponential = 1;
        internal const int TimeSeries = 2;
        internal const int Triangular = 3;
        internal const int Vidya = 4;
        internal const int Weighted = 5;
        internal const int WilderSmoothing = 6;
        internal const int Hull = 7;
        internal const int DoubleExponential = 8;
        internal const int TripleExponential = 9;
        internal const int KaufmanAdaptive = 10;

        private readonly int _type;
        private readonly int _length;
        private readonly int _validFrom;
        private readonly int _firstOutputIndex;

        private double[] _output;
        private double[] _aux1;
        private double[] _aux2;
        private double[] _aux3;
        private int _capacity;

        internal WaveTrendMovingAverageCalculator(
            int type,
            int length,
            int validFrom)
        {
            _type = type;
            _length = Math.Max(1, length);
            _validFrom = Math.Max(0, validFrom);
            _firstOutputIndex =
                _validFrom +
                RequiredSourceBars(
                    _type,
                    _length) -
                1;
        }

        internal int FirstOutputIndex
        {
            get { return _firstOutputIndex; }
        }

        internal static int RequiredSourceBars(
            int type,
            int length)
        {
            int safeLength =
                Math.Max(
                    1,
                    length);

            switch (type)
            {
                case DoubleExponential:
                    return
                        2 *
                        safeLength -
                        1;

                case TripleExponential:
                    return
                        3 *
                        safeLength -
                        2;

                case Hull:
                    int squareRootLength =
                        Math.Max(
                            1,
                            (int)Math.Round(
                                Math.Sqrt(
                                    safeLength)));

                    return
                        safeLength +
                        squareRootLength -
                        1;

                case Simple:
                case Exponential:
                case TimeSeries:
                case Triangular:
                case Vidya:
                case Weighted:
                case WilderSmoothing:
                case KaufmanAdaptive:
                default:
                    return safeLength;
            }
        }

        internal bool TryCalculateWaveTrendAverage(
            double[] source,
            int index,
            out double value)
        {
            value = double.NaN;

            if (source == null ||
                index < _validFrom ||
                index >= source.Length ||
                index < _firstOutputIndex)
                return false;

            EnsureWaveTrendAverageCapacity(
                source.Length);

            switch (_type)
            {
                case Simple:
                    value =
                        CalculateWaveTrendSimple(
                            source,
                            index,
                            _length);
                    break;

                case Exponential:
                    value =
                        CalculateWaveTrendExponential(
                            source,
                            index,
                            _length,
                            2.0 /
                            (_length + 1.0));
                    break;

                case TimeSeries:
                    value =
                        CalculateWaveTrendTimeSeries(
                            source,
                            index,
                            _length);
                    break;

                case Triangular:
                    value =
                        CalculateWaveTrendTriangular(
                            source,
                            index,
                            _length);
                    break;

                case Vidya:
                    value =
                        CalculateWaveTrendVidya(
                            source,
                            index,
                            _length);
                    break;

                case Weighted:
                    value =
                        CalculateWaveTrendWeighted(
                            source,
                            index,
                            _length);
                    break;

                case WilderSmoothing:
                    value =
                        CalculateWaveTrendExponential(
                            source,
                            index,
                            _length,
                            1.0 /
                            _length);
                    break;

                case Hull:
                    value =
                        CalculateWaveTrendHull(
                            source,
                            index,
                            _length);
                    break;

                case DoubleExponential:
                    value =
                        CalculateWaveTrendDoubleExponential(
                            source,
                            index,
                            _length);
                    break;

                case TripleExponential:
                    value =
                        CalculateWaveTrendTripleExponential(
                            source,
                            index,
                            _length);
                    break;

                case KaufmanAdaptive:
                    value =
                        CalculateWaveTrendKaufman(
                            source,
                            index,
                            _length);
                    break;

                default:
                    return false;
            }

            if (!IsFiniteWaveTrendAverage(value))
                return false;

            _output[index] = value;
            return true;
        }

        internal double GetWaveTrendAverageValue(
            int index)
        {
            if (_output == null ||
                index < 0 ||
                index >= _output.Length)
                return double.NaN;

            return _output[index];
        }

        internal void ResetWaveTrendAverage()
        {
            _output = null;
            _aux1 = null;
            _aux2 = null;
            _aux3 = null;
            _capacity = 0;
        }

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

        private void EnsureWaveTrendAverageCapacity(
            int count)
        {
            if (_capacity >= count)
                return;

            int target =
                Math.Max(
                    count,
                    _capacity == 0
                        ? 128
                        : _capacity * 2);

            Array.Resize(
                ref _output,
                target);
            Array.Resize(
                ref _aux1,
                target);
            Array.Resize(
                ref _aux2,
                target);
            Array.Resize(
                ref _aux3,
                target);

            _capacity = target;
        }

        private bool IsFiniteWaveTrendAverage(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }}
