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
            int waveTrendQuality,
            int structuralEvidence,
            int independentEvidence,
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
            bool strongHtfConflict =
                htfDirection != 0 &&
                htfAlignment >=
                Math.Max(
                    60,
                    strongHtfThreshold) &&
                htfDirection != m5Direction;

            int requiredQuality =
                (int)Math.Round(
                    m5Quality * 0.55 +
                    waveTrendQuality * 0.20 +
                    Math.Min(
                        100,
                        structuralEvidence * 12) * 0.15 +
                    Math.Min(
                        100,
                        independentEvidence * 12) * 0.10);

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
