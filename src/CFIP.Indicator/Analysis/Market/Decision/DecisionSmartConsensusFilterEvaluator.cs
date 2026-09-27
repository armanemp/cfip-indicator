using System;

namespace cAlgo
{
    internal sealed class DecisionSmartConsensusFilterEvaluator
    {
        public DecisionFilterResult Evaluate(
            DecisionSmartConsensusFilterInput input)
        {
            if (!input.Enabled)
                return new DecisionFilterResult(true, string.Empty);

            if (input.RequireConsensus &&
                input.StrongestShare <
                Math.Max(
                    input.ConsensusThreshold,
                    input.AdaptiveShareThreshold))
            {
                bool soft =
                    input.AllowSoftGate &&
                    input.SmartQuality >= input.StrongSetupQuality &&
                    input.Edge >= input.StrongSetupEdge &&
                    input.IndependentEvidence >=
                        input.MinimumIndependentEvidence + 1;

                if (!soft)
                    return new DecisionFilterResult(false, "SMART CONSENSUS");
            }

            if (input.TimeframeAgreement <
                input.MinimumTimeframeAgreement)
                return new DecisionFilterResult(false, "SMART MTF");

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}