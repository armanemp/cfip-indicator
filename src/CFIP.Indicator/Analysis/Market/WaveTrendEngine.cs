using System;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class WaveTrendEngine
    {
        private readonly Bars _bars;
        private readonly int _length;
        private readonly int _momentumLength;
        private readonly MovingAverageType _smoothType;
        private readonly int _smoothLength;
        private readonly MovingAverageType _signalType;
        private readonly int _signalLength;
        private readonly int _os1;
        private readonly int _os2;
        private readonly int _ob1;
        private readonly int _ob2;

        private double[] _typical;
        private double[] _rsi;
        private double[] _rsiAvgGain;
        private double[] _rsiAvgLoss;
        private double[] _mfi;
        private double[] _rmi;
        private double[] _rmiUpRaw;
        private double[] _rmiDownRaw;
        private double[] _rmiUpEma;
        private double[] _rmiDownEma;
        private double[] _together;
        private double[] _all;
        private double[] _signal;
        private int _calculatedTo = -1;

        internal WaveTrendEngine(
            Bars bars,
            int length,
            int momentumLength,
            MovingAverageType smoothType,
            int smoothLength,
            MovingAverageType signalType,
            int signalLength,
            int os1,
            int os2,
            int ob1,
            int ob2)
        {
            _bars = bars;
            _length = Math.Max(2, length);
            _momentumLength = Math.Max(1, momentumLength);
            _smoothType = smoothType;
            _smoothLength = Math.Max(1, smoothLength);
            _signalType = signalType;
            _signalLength = Math.Max(1, signalLength);
            _os1 = Math.Min(os1, os2);
            _os2 = Math.Max(os1, os2);
            _ob1 = Math.Min(ob1, ob2);
            _ob2 = Math.Max(ob1, ob2);
        }

        internal WaveTrendSnapshot GetSnapshot(int index)
        {
            if (_bars == null ||
                index < 1 ||
                index >= _bars.Count)
                return default(WaveTrendSnapshot);

            EnsureCapacity(_bars.Count);

            int start =
                Math.Max(
                    1,
                    _calculatedTo + 1);

            for (int i = start; i <= index; i++)
                CalculateIndex(i);

            _calculatedTo =
                Math.Max(
                    _calculatedTo,
                    index);

            if (index < _signalLength + _smoothLength)
                return default(WaveTrendSnapshot);

            double wave =
                _all[index] - 50.0;
            double signal =
                _signal[index] - 50.0;
            double previousWave =
                _all[index - 1] - 50.0;
            double previousSignal =
                _signal[index - 1] - 50.0;

            if (!IsFinite(wave) ||
                !IsFinite(signal) ||
                !IsFinite(previousWave) ||
                !IsFinite(previousSignal))
                return default(WaveTrendSnapshot);

            double histogram =
                wave - signal;
            double delta =
                wave - previousWave;

            bool bullCross =
                previousWave <= previousSignal &&
                wave > signal;

            bool bearCross =
                previousWave >= previousSignal &&
                wave < signal;

            return new WaveTrendSnapshot(
                true,
                wave,
                signal,
                histogram,
                previousWave,
                previousSignal,
                delta,
                bullCross,
                bearCross,
                wave >= 0,
                wave <= 0,
                wave <= _os1,
                wave >= _ob1,
                delta > 0,
                delta < 0);
        }

        private void CalculateIndex(int i)
        {
            _typical[i] =
                (_bars.HighPrices[i] +
                 _bars.LowPrices[i] +
                 _bars.ClosePrices[i]) /
                3.0;

            _rsi[i] =
                CalculateRsi(i);

            _mfi[i] =
                CalculateMfi(i);

            _rmiUpRaw[i] =
                i >= _momentumLength
                    ? Math.Max(
                        _bars.ClosePrices[i] -
                        _bars.ClosePrices[i - _momentumLength],
                        0)
                    : 0;

            _rmiDownRaw[i] =
                i >= _momentumLength
                    ? Math.Max(
                        _bars.ClosePrices[i - _momentumLength] -
                        _bars.ClosePrices[i],
                        0)
                    : 0;

            _rmiUpEma[i] =
                UpdateEma(
                    _rmiUpRaw,
                    _rmiUpEma,
                    i,
                    _length);

            _rmiDownEma[i] =
                UpdateEma(
                    _rmiDownRaw,
                    _rmiDownEma,
                    i,
                    _length);

            _rmi[i] =
                CalculateRatioOscillator(
                    _rmiUpEma[i],
                    _rmiDownEma[i]);

            _together[i] =
                Average3(
                    _rsi[i],
                    _mfi[i],
                    _rmi[i]);

            _all[i] =
                UpdateMovingAverage(
                    _together,
                    _all,
                    i,
                    _smoothLength,
                    _smoothType);

            _signal[i] =
                UpdateMovingAverage(
                    _all,
                    _signal,
                    i,
                    _signalLength,
                    _signalType);
        }

        private double CalculateRsi(int i)
        {
            if (i < _length)
                return 50.0;

            if (i == _length)
            {
                double totalGain = 0;
                double totalLoss = 0;

                for (int j = 1; j <= _length; j++)
                {
                    double change =
                        _typical[j] -
                        _typical[j - 1];

                    if (change > 0)
                        totalGain += change;
                    else
                        totalLoss -= change;
                }

                _rsiAvgGain[i] =
                    totalGain /
                    _length;
                _rsiAvgLoss[i] =
                    totalLoss /
                    _length;
            }
            else
            {
                double change =
                    _typical[i] -
                    _typical[i - 1];

                double gain =
                    Math.Max(
                        change,
                        0);

                double loss =
                    Math.Max(
                        -change,
                        0);

                _rsiAvgGain[i] =
                    (_rsiAvgGain[i - 1] *
                     (_length - 1) +
                     gain) /
                    _length;

                _rsiAvgLoss[i] =
                    (_rsiAvgLoss[i - 1] *
                     (_length - 1) +
                     loss) /
                    _length;
            }

            return RatioToPercent(
                _rsiAvgGain[i],
                _rsiAvgLoss[i]);
        }

        private double CalculateMfi(int i)
        {
            if (i < _length)
                return 50.0;

            double positive = 0;
            double negative = 0;

            int start =
                Math.Max(
                    1,
                    i - _length + 1);

            for (int j = start; j <= i; j++)
            {
                double money =
                    _typical[j] *
                    Math.Max(
                        1.0,
                        _bars.TickVolumes[j]);

                if (_typical[j] >
                    _typical[j - 1])
                    positive += money;
                else if (_typical[j] <
                         _typical[j - 1])
                    negative += money;
            }

            if (negative <= 0)
                return positive > 0 ? 100.0 : 50.0;

            if (positive <= 0)
                return 0.0;

            return
                100.0 -
                100.0 /
                (1.0 + positive / negative);
        }

        private double UpdateEma(
            double[] raw,
            double[] output,
            int i,
            int length)
        {
            if (i == 0)
                return raw[i];

            if (i < length - 1)
                return raw[i];

            if (i == length - 1)
            {
                double sum = 0;
                for (int j = 0; j < length; j++)
                    sum += raw[j];

                return sum / length;
            }

            double alpha =
                2.0 /
                (length + 1.0);

            return
                output[i - 1] +
                alpha *
                (raw[i] -
                 output[i - 1]);
        }

        private double UpdateMovingAverage(
            double[] source,
            double[] output,
            int i,
            int length,
            MovingAverageType type)
        {
            if (i < length - 1)
                return source[i];

            if (i == length - 1)
            {
                double sum = 0;
                for (int j = 0; j < length; j++)
                    sum += source[j];
                return sum / length;
            }

            if (type == MovingAverageType.Exponential)
            {
                double alpha =
                    2.0 /
                    (length + 1.0);

                return
                    output[i - 1] +
                    alpha *
                    (source[i] -
                     output[i - 1]);
            }

            double simple = 0;
            for (int j = i - length + 1; j <= i; j++)
                simple += source[j];

            return simple / length;
        }

        private double CalculateRatioOscillator(
            double up,
            double down)
        {
            if (down <= 0)
                return up > 0 ? 100.0 : 50.0;

            if (up <= 0)
                return 0.0;

            return
                100.0 -
                100.0 /
                (1.0 + up / down);
        }

        private double RatioToPercent(
            double up,
            double down)
        {
            return CalculateRatioOscillator(up, down);
        }

        private double Average3(
            double a,
            double b,
            double c)
        {
            return
                (a + b + c) /
                3.0;
        }

        private void EnsureCapacity(int count)
        {
            if (_typical != null &&
                _typical.Length >= count)
                return;

            int target =
                Math.Max(
                    count,
                    _typical == null
                        ? 256
                        : _typical.Length * 2);

            Array.Resize(ref _typical, target);
            Array.Resize(ref _rsi, target);
            Array.Resize(ref _rsiAvgGain, target);
            Array.Resize(ref _rsiAvgLoss, target);
            Array.Resize(ref _mfi, target);
            Array.Resize(ref _rmi, target);
            Array.Resize(ref _rmiUpRaw, target);
            Array.Resize(ref _rmiDownRaw, target);
            Array.Resize(ref _rmiUpEma, target);
            Array.Resize(ref _rmiDownEma, target);
            Array.Resize(ref _together, target);
            Array.Resize(ref _all, target);
            Array.Resize(ref _signal, target);
        }

        private bool IsFinite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
