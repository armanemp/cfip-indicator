using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic WaveTrend moving-average implementation.
    /// The calculator is independent from Bars/UI/broker state.
    /// </summary>
    internal sealed partial class WaveTrendMovingAverageCalculator
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
    }
}
