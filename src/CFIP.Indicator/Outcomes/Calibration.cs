using System;
using System.Collections.Generic;

namespace CFIP.Indicator
{
    public sealed class CalibrationSnapshot
    {
        public int TotalSamples { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public double WinRate { get; private set; }
        public DateTime GeneratedUtc { get; private set; }

        public CalibrationSnapshot(int totalSamples, int wins, int losses, DateTime generatedUtc)
        {
            TotalSamples = Math.Max(0, totalSamples);
            Wins = Math.Max(0, Math.Min(TotalSamples, wins));
            Losses = Math.Max(0, Math.Min(TotalSamples - Wins, losses));
            WinRate = TotalSamples <= 0 ? 0.0 : (double)Wins / TotalSamples;
            GeneratedUtc = generatedUtc;
        }
    }

    public sealed class CalibrationEngine : IOutcomeRecorder
    {
        private readonly Dictionary<Direction, int> _samples =
            new Dictionary<Direction, int>();

        private readonly Dictionary<Direction, int> _wins =
            new Dictionary<Direction, int>();

        private int _total;
        private int _totalWins;

        public CalibrationSnapshot Snapshot(DateTime utc)
        {
            return new CalibrationSnapshot(
                _total,
                _totalWins,
                Math.Max(0, _total - _totalWins),
                utc);
        }

        public void Record(OutcomeEvent outcome)
        {
            if (outcome == null)
                return;

            Direction direction = outcome.Direction;

            if (direction == Direction.Wait)
                return;

            Increment(_samples, direction);
            if (outcome.ResultAmount > 0)
                Increment(_wins, direction);

            _total++;
            if (outcome.ResultAmount > 0)
                _totalWins++;
        }

        public int AdjustConfidence(
            int baseConfidence,
            Direction direction,
            ConfigSnapshot configuration)
        {
            int result = Clamp(baseConfidence, 0, 100);

            if (configuration == null ||
                !configuration.Get("UseEmpiricalCalibration", true) ||
                !configuration.Get("EnableConfidenceCalibration", true) ||
                !configuration.Get("EnableOutcomeTelemetry", true) ||
                direction == Direction.Wait)
                return result;

            int minimumSamples = Math.Max(
                1,
                configuration.Get("CalibrationMinimumSamples", 5));

            int directionalMinimum = Math.Max(
                1,
                configuration.Get("CalibrationDirectionalMinimumSamples", 6));

            if (_total < minimumSamples)
                return result;

            int samples = Get(_samples, direction);
            if (samples < directionalMinimum)
                return result;

            int wins = Get(_wins, direction);
            double rate = samples <= 0 ? 0.5 : (double)wins / samples;

            int maximumAdjustment = Math.Max(
                0,
                configuration.Get("CalibrationMaxConfidenceAdjustment", 8));

            int adjustment = Clamp(
                (int)Math.Round(
                    (rate - 0.5) * 2.0 * maximumAdjustment),
                -maximumAdjustment,
                maximumAdjustment);

            return Clamp(result + adjustment, 0, 100);
        }

        public string GetSummary()
        {
            return _total <= 0
                ? "W0/L0"
                : (100.0 * _totalWins / _total).ToString("F0") + "%";
        }

        private static void Increment(
            Dictionary<Direction, int> map,
            Direction key)
        {
            int value;
            map.TryGetValue(key, out value);
            map[key] = value + 1;
        }

        private static int Get(
            Dictionary<Direction, int> map,
            Direction key)
        {
            int value;
            return map.TryGetValue(key, out value) ? value : 0;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
