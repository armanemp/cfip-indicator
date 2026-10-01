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

        private readonly int _componentReadyIndex;
        private readonly int _smoothReadyIndex;
        private readonly int _signalReadyIndex;

        private readonly WaveTrendMovingAverageCalculator _rmiUpAverage;
        private readonly WaveTrendMovingAverageCalculator _rmiDownAverage;
        private readonly WaveTrendMovingAverageCalculator _smoothAverage;
        private readonly WaveTrendMovingAverageCalculator _signalAverage;

        private double[] _typical;
        private double[] _rsi;
        private double[] _rsiAvgGain;
        private double[] _rsiAvgLoss;
        private double[] _mfi;
        private double[] _rmi;
        private double[] _rmiUpRaw;
        private double[] _rmiDownRaw;
        private double[] _together;
        private double[] _all;
        private double[] _signal;

        private int _calculatedTo = -1;
        private int _knownBarCount = -1;
        private DateTime _knownFirstBarOpenTime = DateTime.MinValue;
        private bool _historyContextInitialized;
        private bool _historyInvalidated;

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

            int smoothMaType =
                MapWaveTrendMovingAverageType(
                    _smoothType);
            int signalMaType =
                MapWaveTrendMovingAverageType(
                    _signalType);

            _componentReadyIndex =
                WaveTrendReadinessRule.ResolveWaveTrendComponentReadyIndex(
                    _length,
                    _momentumLength);

            _smoothReadyIndex =
                WaveTrendReadinessRule.ResolveWaveTrendSmoothReadyIndex(
                    _componentReadyIndex,
                    smoothMaType,
                    _smoothLength);

            _signalReadyIndex =
                WaveTrendReadinessRule.ResolveWaveTrendSignalReadyIndex(
                    _smoothReadyIndex,
                    signalMaType,
                    _signalLength);

            _rmiUpAverage =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    _length,
                    _momentumLength);

            _rmiDownAverage =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    _length,
                    _momentumLength);

            _smoothAverage =
                new WaveTrendMovingAverageCalculator(
                    smoothMaType,
                    _smoothLength,
                    _componentReadyIndex);

            _signalAverage =
                new WaveTrendMovingAverageCalculator(
                    signalMaType,
                    _signalLength,
                    _smoothReadyIndex);

            if (_bars != null)
            {
                _bars.HistoryLoaded +=
                    OnWaveTrendHistoryLoaded;
                _bars.Reloaded +=
                    OnWaveTrendBarsReloaded;
            }
        }

        internal WaveTrendSnapshot GetSnapshot(int index)
        {
            if (_bars == null ||
                index < 1 ||
                index >= _bars.Count)
                return default(WaveTrendSnapshot);

            EnsureWaveTrendHistoryContext();
            EnsureCapacity(_bars.Count);

            if (_calculatedTo < 0)
            {
                _typical[0] =
                    CalculateTypicalPrice(0);
            }

            int start =
                Math.Max(
                    1,
                    _calculatedTo + 1);

            for (int i = start;
                 i <= index;
                 i++)
                CalculateWaveTrendIndex(i);

            _calculatedTo =
                Math.Max(
                    _calculatedTo,
                    index);

            if (!WaveTrendReadinessRule.IsWaveTrendSnapshotReady(
                    index,
                    _signalReadyIndex))
                return default(WaveTrendSnapshot);

            double wave =
                _all[index] - 50.0;
            double signal =
                _signal[index] - 50.0;
            double previousWave =
                _all[index - 1] - 50.0;
            double previousSignal =
                _signal[index - 1] - 50.0;

            if (!IsFiniteWaveTrendValue(wave) ||
                !IsFiniteWaveTrendValue(signal) ||
                !IsFiniteWaveTrendValue(previousWave) ||
                !IsFiniteWaveTrendValue(previousSignal))
                return default(WaveTrendSnapshot);

            double histogram =
                wave -
                signal;
            double delta =
                wave -
                previousWave;

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

        private void CalculateWaveTrendIndex(int i)
        {
            _typical[i] =
                CalculateTypicalPrice(i);

            _rsi[i] =
                CalculateWaveTrendRsi(i);

            _mfi[i] =
                CalculateWaveTrendMfi(i);

            if (i >= _momentumLength)
            {
                _rmiUpRaw[i] =
                    Math.Max(
                        _bars.ClosePrices[i] -
                        _bars.ClosePrices[
                            i -
                            _momentumLength],
                        0);

                _rmiDownRaw[i] =
                    Math.Max(
                        _bars.ClosePrices[
                            i -
                            _momentumLength] -
                        _bars.ClosePrices[i],
                        0);
            }
            else
            {
                _rmiUpRaw[i] = double.NaN;
                _rmiDownRaw[i] = double.NaN;
            }

            double rmiUp;
            double rmiDown;

            bool rmiUpReady =
                _rmiUpAverage.TryCalculateWaveTrendAverage(
                    _rmiUpRaw,
                    i,
                    out rmiUp);

            bool rmiDownReady =
                _rmiDownAverage.TryCalculateWaveTrendAverage(
                    _rmiDownRaw,
                    i,
                    out rmiDown);

            _rmi[i] =
                rmiUpReady &&
                rmiDownReady
                    ? CalculateWaveTrendRatioOscillator(
                        rmiUp,
                        rmiDown)
                    : double.NaN;

            _together[i] =
                CalculateWaveTrendAverage3(
                    _rsi[i],
                    _mfi[i],
                    _rmi[i]);

            double allValue;

            if (_smoothAverage.TryCalculateWaveTrendAverage(
                    _together,
                    i,
                    out allValue))
                _all[i] = allValue;
            else
                _all[i] = double.NaN;

            double signalValue;

            if (_signalAverage.TryCalculateWaveTrendAverage(
                    _all,
                    i,
                    out signalValue))
                _signal[i] = signalValue;
            else
                _signal[i] = double.NaN;
        }

        private double CalculateWaveTrendRsi(int i)
        {
            if (i <
                _length)
                return double.NaN;

            if (i ==
                _length)
            {
                double totalGain = 0;
                double totalLoss = 0;

                for (int j = 1;
                     j <= _length;
                     j++)
                {
                    double change =
                        _typical[j] -
                        _typical[j - 1];

                    if (!IsFiniteWaveTrendValue(
                            change))
                        return double.NaN;

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

                if (!IsFiniteWaveTrendValue(
                        change) ||
                    !IsFiniteWaveTrendValue(
                        _rsiAvgGain[i - 1]) ||
                    !IsFiniteWaveTrendValue(
                        _rsiAvgLoss[i - 1]))
                    return double.NaN;

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

            return CalculateWaveTrendRatioOscillator(
                _rsiAvgGain[i],
                _rsiAvgLoss[i]);
        }

        private double CalculateWaveTrendMfi(int i)
        {
            if (i <
                _length)
                return double.NaN;

            double positive = 0;
            double negative = 0;

            int start =
                Math.Max(
                    1,
                    i -
                    _length +
                    1);

            for (int j = start;
                 j <= i;
                 j++)
            {
                double typical =
                    _typical[j];

                double previousTypical =
                    _typical[j - 1];

                double positiveFlow;
                double negativeFlow;

                if (!WaveTrendMoneyFlowRule.TryCalculateContribution(
                        typical,
                        previousTypical,
                        _bars.TickVolumes[j],
                        out positiveFlow,
                        out negativeFlow))
                    return double.NaN;

                positive += positiveFlow;
                negative += negativeFlow;
            }

            if (negative <= 0)
                return positive > 0
                    ? 100.0
                    : 50.0;

            if (positive <= 0)
                return 0.0;

            return CalculateWaveTrendRatioOscillator(
                positive,
                negative);
        }

        private double CalculateWaveTrendRatioOscillator(
            double up,
            double down)
        {
            if (!IsFiniteWaveTrendValue(up) ||
                !IsFiniteWaveTrendValue(down) ||
                up < 0 ||
                down < 0)
                return double.NaN;

            if (down <= 0)
                return up > 0
                    ? 100.0
                    : 50.0;

            if (up <= 0)
                return 0.0;

            return
                100.0 -
                100.0 /
                (1.0 +
                 up /
                 down);
        }

        private double CalculateWaveTrendAverage3(
            double a,
            double b,
            double c)
        {
            if (!IsFiniteWaveTrendValue(a) ||
                !IsFiniteWaveTrendValue(b) ||
                !IsFiniteWaveTrendValue(c))
                return double.NaN;

            return
                (a + b + c) /
                3.0;
        }

        private double CalculateTypicalPrice(int index)
        {
            double high =
                _bars.HighPrices[index];
            double low =
                _bars.LowPrices[index];
            double close =
                _bars.ClosePrices[index];

            if (!IsFiniteWaveTrendValue(high) ||
                !IsFiniteWaveTrendValue(low) ||
                !IsFiniteWaveTrendValue(close))
                return double.NaN;

            return
                (high +
                 low +
                 close) /
                3.0;
        }

        private int MapWaveTrendMovingAverageType(
            MovingAverageType type)
        {
            switch (type)
            {
                case MovingAverageType.Simple:
                    return WaveTrendMovingAverageCalculator.Simple;

                case MovingAverageType.Exponential:
                    return WaveTrendMovingAverageCalculator.Exponential;

                case MovingAverageType.TimeSeries:
                    return WaveTrendMovingAverageCalculator.TimeSeries;

                case MovingAverageType.Triangular:
                    return WaveTrendMovingAverageCalculator.Triangular;

                case MovingAverageType.VIDYA:
                    return WaveTrendMovingAverageCalculator.Vidya;

                case MovingAverageType.Weighted:
                    return WaveTrendMovingAverageCalculator.Weighted;

                case MovingAverageType.WilderSmoothing:
                    return WaveTrendMovingAverageCalculator.WilderSmoothing;

                case MovingAverageType.Hull:
                    return WaveTrendMovingAverageCalculator.Hull;

                case MovingAverageType.DoubleExponential:
                    return WaveTrendMovingAverageCalculator.DoubleExponential;

                case MovingAverageType.TripleExponential:
                    return WaveTrendMovingAverageCalculator.TripleExponential;

                case MovingAverageType.KaufmanAdaptive:
                    return WaveTrendMovingAverageCalculator.KaufmanAdaptive;

                default:
                    return WaveTrendMovingAverageCalculator.Simple;
            }
        }

        private void EnsureWaveTrendHistoryContext()
        {
            if (_bars == null ||
                _bars.Count <= 0)
                return;

            DateTime firstOpenTime =
                _bars.OpenTimes[0];

            if (!_historyContextInitialized)
            {
                _knownFirstBarOpenTime =
                    firstOpenTime;
                _knownBarCount =
                    _bars.Count;
                _historyContextInitialized = true;
                return;
            }

            if (_historyInvalidated ||
                firstOpenTime !=
                _knownFirstBarOpenTime ||
                _bars.Count <
                _knownBarCount)
            {
                ResetWaveTrendCalculationState();
                _knownFirstBarOpenTime =
                    firstOpenTime;
            }

            _knownBarCount =
                _bars.Count;
            _historyInvalidated = false;
        }

        private void ResetWaveTrendCalculationState()
        {
            _calculatedTo = -1;

            _typical = null;
            _rsi = null;
            _rsiAvgGain = null;
            _rsiAvgLoss = null;
            _mfi = null;
            _rmi = null;
            _rmiUpRaw = null;
            _rmiDownRaw = null;
            _together = null;
            _all = null;
            _signal = null;

            _rmiUpAverage.ResetWaveTrendAverage();
            _rmiDownAverage.ResetWaveTrendAverage();
            _smoothAverage.ResetWaveTrendAverage();
            _signalAverage.ResetWaveTrendAverage();
        }

        private void OnWaveTrendHistoryLoaded(
            BarsHistoryLoadedEventArgs args)
        {
            _historyInvalidated = true;
        }

        private void OnWaveTrendBarsReloaded(
            BarsHistoryLoadedEventArgs args)
        {
            _historyInvalidated = true;
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

            Array.Resize(
                ref _typical,
                target);
            Array.Resize(
                ref _rsi,
                target);
            Array.Resize(
                ref _rsiAvgGain,
                target);
            Array.Resize(
                ref _rsiAvgLoss,
                target);
            Array.Resize(
                ref _mfi,
                target);
            Array.Resize(
                ref _rmi,
                target);
            Array.Resize(
                ref _rmiUpRaw,
                target);
            Array.Resize(
                ref _rmiDownRaw,
                target);
            Array.Resize(
                ref _together,
                target);
            Array.Resize(
                ref _all,
                target);
            Array.Resize(
                ref _signal,
                target);
        }

        private bool IsFiniteWaveTrendValue(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
