using System;

namespace cAlgo
{
    internal readonly struct ConfidenceCalibrationKey :
        IEquatable<ConfidenceCalibrationKey>
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

        public bool Equals(
            ConfidenceCalibrationKey other)
        {
            return
                Direction == other.Direction &&
                Lane == other.Lane &&
                ConfidenceBucket == other.ConfidenceBucket &&
                string.Equals(
                    Regime,
                    other.Regime,
                    StringComparison.Ordinal);
        }

        public override bool Equals(
            object obj)
        {
            return
                obj is ConfidenceCalibrationKey other &&
                Equals(other);
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
}