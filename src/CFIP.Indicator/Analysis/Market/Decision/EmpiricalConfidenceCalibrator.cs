using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal readonly struct ConfidenceCalibrationKey : IEquatable<ConfidenceCalibrationKey>
    {
        public int Direction { get; }
        public OpportunityLane Lane { get; }
        public string Regime { get; }
        public int ConfidenceBucket { get; }

        public ConfidenceCalibrationKey(
            int direction,
            OpportunityLane lane,
            string regime,
            int confidenceBucket)
        {
            Direction = direction;
            Lane = lane;
            Regime =
                string.IsNullOrWhiteSpace(regime)
                    ? "UNKNOWN"
                    : regime.Trim().ToUpperInvariant();
            ConfidenceBucket = confidenceBucket;
        }

        public bool Equals(ConfidenceCalibrationKey other)
        {
            return Direction == other.Direction &&
                   Lane == other.Lane &&
                   ConfidenceBucket == other.ConfidenceBucket &&
                   string.Equals(
                       Regime,
                       other.Regime,
                       StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Direction;
                hash = hash * 31 + (int)Lane;
                hash = hash * 31 + ConfidenceBucket;
                hash = hash * 31 +
                    StringComparer.Ordinal.GetHashCode(Regime);
                return hash;
            }
        }
    }

    internal readonly struct EmpiricalCalibrationSnapshot
    {
        public bool Available { get; }
        public int Adjustment { get; }
        public int Samples { get; }
        public int Wins { get; }
        public int ConfidenceBucket { get; }
        public double ObservedWinRate { get; }
        public double AverageRealizedR { get; }
        public string Source { get; }

        public EmpiricalCalibrationSnapshot(
            bool available,
            int adjustment,
            int samples,
            int wins,
            int confidenceBucket,
            double observedWinRate,
            string source)
        {
            Available = available;
            Adjustment = NumericGuards.ClampInt(
                adjustment,
                -100,
                100);
            Samples = Math.Max(0, samples);
            Wins = Math.Max(0, wins);
            ConfidenceBucket = confidenceBucket;
            ObservedWinRate =
                NumericGuards.Clamp(
                    observedWinRate,
                    0,
                    1);
            AverageRealizedR = 0;
            Source =
                string.IsNullOrWhiteSpace(source)
                    ? "NONE"
                    : source;
        }

        public EmpiricalCalibrationSnapshot(bool available, int adjustment, int samples, int wins, int confidenceBucket, double observedWinRate, double averageRealizedR, string source)
            : this(available, adjustment, samples, wins, confidenceBucket, observedWinRate, source)
        {
            AverageRealizedR = double.IsNaN(averageRealizedR) || double.IsInfinity(averageRealizedR) ? 0 : averageRealizedR;
        }

        public static EmpiricalCalibrationSnapshot None(
            int confidenceBucket)
        {
            return new EmpiricalCalibrationSnapshot(
                false,
                0,
                0,
                0,
                confidenceBucket,
                0.5,
                "NONE");
        }
    }

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

        public EmpiricalCalibrationSnapshot CalculateRecentContextual(
            bool calibrationEnabled,
            bool telemetryEnabled,
            int direction,
            OpportunityLane lane,
            string regime,
            int confidence,
            IList<OutcomeObservation> outcomes,
            int recentMaximum,
            int minimumSamples,
            int minimumDirectionalSamples,
            int maximumAdjustment)
        {
            int bucket = ConfidenceBucket(confidence);

            if (!calibrationEnabled ||
                !telemetryEnabled ||
                (direction != 1 && direction != -1) ||
                outcomes == null ||
                outcomes.Count == 0 ||
                recentMaximum <= 0)
                return EmpiricalCalibrationSnapshot.None(bucket);

            Dictionary<ConfidenceCalibrationKey, int> samples =
                new Dictionary<ConfidenceCalibrationKey, int>();
            Dictionary<ConfidenceCalibrationKey, int> wins =
                new Dictionary<ConfidenceCalibrationKey, int>();

            int start =
                Math.Max(
                    0,
                    outcomes.Count -
                    Math.Max(1, recentMaximum));

            for (int i = start; i < outcomes.Count; i++)
            {
                OutcomeObservation observation = outcomes[i];

                if (observation == null ||
                    !observation.CalibrationEligible ||
                    (observation.Direction != 1 &&
                     observation.Direction != -1))
                    continue;

                ConfidenceCalibrationKey key =
                    new ConfidenceCalibrationKey(
                        observation.Direction,
                        observation.Lane,
                        observation.Regime,
                        ConfidenceBucket(
                            observation.Confidence));

                int sampleCount =
                    Get(samples, key);

                int winCount =
                    Get(wins, key);

                samples[key] = sampleCount + 1;

                if (observation.Profitable)
                    wins[key] = winCount + 1;
                else if (!wins.ContainsKey(key))
                    wins[key] = 0;
            }

            EmpiricalCalibrationSnapshot snapshot =
                CalculateContextual(
                    calibrationEnabled,
                    telemetryEnabled,
                    direction,
                    lane,
                    regime,
                    confidence,
                    samples,
                    wins,
                    minimumSamples,
                    minimumDirectionalSamples,
                    maximumAdjustment);

            if (!snapshot.Available)
                return snapshot;

            return new EmpiricalCalibrationSnapshot(
                true,
                snapshot.Adjustment,
                snapshot.Samples,
                snapshot.Wins,
                snapshot.ConfidenceBucket,
                snapshot.ObservedWinRate,
                CalculateAverageRealizedR(outcomes, direction, lane, NormalizeRegime(regime), snapshot.ConfidenceBucket, snapshot.Source, Math.Max(1, recentMaximum)),
                snapshot.Source + "-RECENT");
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

        private static double CalculateAverageRealizedR(
            IList<OutcomeObservation> outcomes,
            int direction,
            OpportunityLane lane,
            string regime,
            int confidenceBucket,
            string source,
            int recentMaximum)
        {
            double total = 0;
            int count = 0;
            if (outcomes == null)
                return 0;
            bool exact = source.StartsWith("EXACT", StringComparison.Ordinal);
            bool contextual = source.StartsWith("LANE+REGIME", StringComparison.Ordinal);
            int start = Math.Max(0, outcomes.Count - Math.Max(1, recentMaximum));

            for (int i = start; i < outcomes.Count; i++)
            {
                OutcomeObservation observation = outcomes[i];
                if (observation == null || !observation.CalibrationEligible || observation.Direction != direction)
                    continue;
                if (exact && (observation.Lane != lane || !string.Equals(observation.Regime, regime, StringComparison.Ordinal) || ConfidenceBucket(observation.Confidence) != confidenceBucket))
                    continue;
                if (contextual && (observation.Lane != lane || !string.Equals(observation.Regime, regime, StringComparison.Ordinal)))
                    continue;
                if (double.IsNaN(observation.RealizedR) || double.IsInfinity(observation.RealizedR))
                    continue;
                total += observation.RealizedR;
                count++;
            }
            return count > 0 ? total / count : 0;
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