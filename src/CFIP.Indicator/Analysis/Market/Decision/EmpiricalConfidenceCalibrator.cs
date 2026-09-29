using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class EmpiricalConfidenceCalibrator
    {
        private const int DefaultPriorStrength = 8;

        public EmpiricalCalibrationSnapshot CalculateContextual(
            bool calibrationEnabled,
            bool telemetryEnabled,
            int direction,
            OpportunityLane lane,
            string regime,
            int confidence,
            IDictionary<ConfidenceCalibrationKey, int> samples,
            IDictionary<ConfidenceCalibrationKey, int> wins,
            int minimumSamples,
            int minimumDirectionalSamples,
            int maximumAdjustment)
        {
            int bucket =
                ConfidenceBucket(confidence);

            if (!calibrationEnabled ||
                !telemetryEnabled ||
                (direction != 1 &&
                 direction != -1) ||
                samples == null ||
                wins == null)
                return EmpiricalCalibrationSnapshot.None(bucket);

            int totalSamples =
                Sum(samples, directionFilter: 0);

            int directionalSamples =
                Aggregate(
                    samples,
                    wins,
                    direction,
                    null,
                    null,
                    out int directionalWins);

            if (totalSamples <
                    Math.Max(1, minimumSamples) ||
                directionalSamples <
                    Math.Max(2, minimumDirectionalSamples))
                return EmpiricalCalibrationSnapshot.None(bucket);

            ConfidenceCalibrationKey exactKey =
                new ConfidenceCalibrationKey(
                    direction,
                    lane,
                    regime,
                    bucket);

            int exactSamples =
                Get(samples, exactKey);

            int exactWins =
                Get(wins, exactKey);

            int minimumExactSamples =
                Math.Max(
                    4,
                    minimumDirectionalSamples);

            int minimumContextSamples =
                Math.Max(
                    minimumExactSamples + 2,
                    Math.Max(
                        10,
                        minimumDirectionalSamples * 2));

            int selectedSamples = 0;
            int selectedWins = 0;
            string source = "NONE";

            if (exactSamples >= minimumExactSamples)
            {
                selectedSamples = exactSamples;
                selectedWins = exactWins;
                source = "EXACT";
            }
            else
            {
                int contextSamples =
                    Aggregate(
                        samples,
                        wins,
                        direction,
                        lane,
                        NormalizeRegime(regime),
                        out int contextWins);

                if (contextSamples >= minimumContextSamples)
                {
                    selectedSamples = contextSamples;
                    selectedWins = contextWins;
                    source = "LANE+REGIME";
                }
                else if (directionalSamples >=
                         minimumDirectionalSamples)
                {
                    selectedSamples = directionalSamples;
                    selectedWins = directionalWins;
                    source = "DIRECTION";
                }
            }

            if (selectedSamples <= 0 ||
                selectedSamples <
                minimumDirectionalSamples)
                return EmpiricalCalibrationSnapshot.None(bucket);

            selectedWins =
                Math.Max(
                    0,
                    Math.Min(
                        selectedSamples,
                        selectedWins));

            double observedWinRate =
                (double)selectedWins /
                selectedSamples;

            double smoothedWinRate =
                (selectedWins +
                 DefaultPriorStrength * 0.5) /
                (selectedSamples +
                 DefaultPriorStrength);

            int adjustment =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        (smoothedWinRate - 0.5) *
                        2.0 *
                        Math.Max(
                            0,
                            maximumAdjustment)),
                    -Math.Max(
                        0,
                        maximumAdjustment),
                    Math.Max(
                        0,
                        maximumAdjustment));

            return new EmpiricalCalibrationSnapshot(
                true,
                adjustment,
                selectedSamples,
                selectedWins,
                bucket,
                observedWinRate,
                source);
        }

        public int CalculateAdjustment(
            bool calibrationEnabled,
            bool telemetryEnabled,
            int totalSamples,
            int directionalSamples,
            int directionalWins,
            int minimumSamples,
            int minimumDirectionalSamples,
            int maximumAdjustment)
        {
            if (!calibrationEnabled ||
                !telemetryEnabled ||
                totalSamples < Math.Max(1, minimumSamples) ||
                directionalSamples < minimumDirectionalSamples)
                return 0;

            int boundedWins =
                Math.Max(
                    0,
                    Math.Min(
                        directionalSamples,
                        directionalWins));

            double smoothedWinRate =
                (boundedWins +
                 DefaultPriorStrength * 0.5) /
                (directionalSamples +
                 DefaultPriorStrength);

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    (smoothedWinRate - 0.5) *
                    2.0 *
                    Math.Max(
                        0,
                        maximumAdjustment)),
                -Math.Max(
                    0,
                    maximumAdjustment),
                Math.Max(
                    0,
                    maximumAdjustment));
        }

        internal static int ConfidenceBucket(
            int confidence)
        {
            int bounded =
                NumericGuards.ClampInt(
                    confidence,
                    0,
                    100);

            if (bounded < 60)
                return 0;
            if (bounded < 70)
                return 1;
            if (bounded < 80)
                return 2;
            if (bounded < 90)
                return 3;

            return 4;
        }

        private static int Aggregate(
            IDictionary<ConfidenceCalibrationKey, int> samples,
            IDictionary<ConfidenceCalibrationKey, int> wins,
            int direction,
            OpportunityLane? lane,
            string regime,
            out int winCount)
        {
            int sampleCount = 0;
            winCount = 0;

            foreach (KeyValuePair<ConfidenceCalibrationKey, int> item
                in samples)
            {
                ConfidenceCalibrationKey key = item.Key;

                if (key.Direction != direction ||
                    (lane.HasValue &&
                     key.Lane != lane.Value) ||
                    (regime != null &&
                     !string.Equals(
                         key.Regime,
                         regime,
                         StringComparison.Ordinal)))
                    continue;

                int itemSamples =
                    Math.Max(
                        0,
                        item.Value);

                sampleCount += itemSamples;
                winCount +=
                    Math.Max(
                        0,
                        Math.Min(
                            itemSamples,
                            Get(
                                wins,
                                key)));
            }

            return sampleCount;
        }

        private static int Get(
            IDictionary<ConfidenceCalibrationKey, int> values,
            ConfidenceCalibrationKey key)
        {
            return values != null &&
                   values.TryGetValue(
                       key,
                       out int value)
                ? Math.Max(0, value)
                : 0;
        }

        private static int Sum(
            IDictionary<ConfidenceCalibrationKey, int> values,
            int directionFilter)
        {
            int sum = 0;

            foreach (KeyValuePair<ConfidenceCalibrationKey, int> item
                in values)
            {
                if (directionFilter != 0 &&
                    item.Key.Direction != directionFilter)
                    continue;

                sum += Math.Max(
                    0,
                    item.Value);
            }

            return sum;
        }

        private static string NormalizeRegime(
            string regime)
        {
            return
                string.IsNullOrWhiteSpace(regime)
                    ? "UNKNOWN"
                    : regime.Trim().ToUpperInvariant();
        }
    }
}