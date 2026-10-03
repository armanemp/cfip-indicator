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
            double riskAtr,
            string regime,
            int confidence)
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
                    m5Quality * 0.55 +
                    waveTrendQuality * 0.20 +
                    Math.Min(100, structuralEvidence * 12) * 0.15 +
                    Math.Min(100, independentEvidence * 12) * 0.10);

            bool strongHtfConflict =
                htfDirection != 0 &&
                htfAlignment >=
                Math.Max(60, strongHtfThreshold) &&
                htfDirection != m5Direction;

            OpportunityLane lane =
                strongHtfConflict
                    ? OpportunityLane.CounterHtfTactical
                    : OpportunityLane.Tactical;

            AdaptiveRewardRiskProfile profile =
                AdaptiveRewardRiskProfileRule.Resolve(
                    regime,
                    lane,
                    riskAtr,
                    confidence,
                    quality,
                    60,
                    Math.Min(100, structuralEvidence * 12),
                    4.0);

            double requiredRR =
                profile.RequiredTp1RR;

            return new TacticalOpportunityResult(
                quality >=
                    (strongHtfConflict ? 82 : 70) &&
                tp1RR >= requiredRR,
                lane,
                quality,
                tp1RR);
        }
    }
}
