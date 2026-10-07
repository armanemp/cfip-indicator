using System;

namespace cAlgo
{
    internal readonly struct TacticalOpportunityResult
    {
        public bool Allowed { get; }
        public OpportunityLane Lane { get; }
        public int Quality { get; }
        public double RiskReward { get; }

        public TacticalOpportunityResult(
            bool allowed,
            OpportunityLane lane,
            int quality,
            double riskReward)
        {
            Allowed = allowed;
            Lane = lane;
            Quality = Math.Max(0, Math.Min(100, quality));
            RiskReward = Math.Max(0, riskReward);
        }
    }

    internal static class TacticalOpportunityRule
    {
        internal static TacticalOpportunityResult Evaluate(
            int m5Direction,
            int m5Quality,
            int zoneQuality,
            int independentEvidenceGroupCount,
            int selectedDirection,
            int htfDirection,
            int htfAlignment,
            int strongHtfThreshold,
            double tp1RR,
            int tacticalMinimumQuality,
            double tacticalMinimumRR,
            int counterHtfMinimumQuality,
            double counterHtfMinimumRR)
        {
            if (m5Direction == 0 ||
                selectedDirection != m5Direction ||
                tp1RR <= 0)
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    tp1RR);

            int quality =
                (int)Math.Round(
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            m5Quality)) * 0.60 +
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            zoneQuality)) * 0.25 +
                    Math.Max(
                        0,
                        Math.Min(
                            4,
                            independentEvidenceGroupCount)) /
                        4.0 *
                        100.0 *
                        0.15);

            bool strongHtfConflict =
                htfDirection != 0 &&
                htfAlignment >=
                Math.Max(
                    60,
                    strongHtfThreshold) &&
                htfDirection != m5Direction;

            int requiredQuality =
                strongHtfConflict
                    ? counterHtfMinimumQuality
                    : tacticalMinimumQuality;

            double requiredRR =
                strongHtfConflict
                    ? counterHtfMinimumRR
                    : tacticalMinimumRR;

            return new TacticalOpportunityResult(
                quality >= requiredQuality &&
                tp1RR >= requiredRR,
                strongHtfConflict
                    ? OpportunityLane.CounterHtfTactical
                    : OpportunityLane.Tactical,
                quality,
                tp1RR);
        }
    }
}
