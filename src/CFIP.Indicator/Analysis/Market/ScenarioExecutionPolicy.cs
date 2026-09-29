using System;

namespace cAlgo
{
    internal sealed class ScenarioExecutionPolicyResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public ScenarioExecutionPolicyResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? (allowed
                        ? "POLICY ALLOWED"
                        : "POLICY BLOCKED")
                    : reason;
        }
    }

    public partial class CFIPIndicator
    {
        private ScenarioExecutionPolicyResult EvaluateScenarioExecutionPolicy(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return new ScenarioExecutionPolicyResult(
                    false,
                    "NO SCENARIO");

            if (string.IsNullOrWhiteSpace(
                    candidate.SourceTimeframe))
                return new ScenarioExecutionPolicyResult(
                    false,
                    "SOURCE TIMEFRAME UNKNOWN");

            // Independent timeframe scenarios are intentionally observe-only in
            // this phase. A future execution policy must pass a separate audit
            // before it may become an execution authority.
            if (!string.Equals(
                    candidate.SourceTimeframe,
                    "M5",
                    StringComparison.OrdinalIgnoreCase))
                return new ScenarioExecutionPolicyResult(
                    false,
                    "OBSERVE-ONLY TF SCENARIO");

            if (_decision == null ||
                _decision.Direction != candidate.Direction)
                return new ScenarioExecutionPolicyResult(
                    false,
                    "CANONICAL DIRECTION MISMATCH");

            if (!_decision.EntryAllowed ||
                !_decision.TriggerReady ||
                !_decision.ActionableNow)
                return new ScenarioExecutionPolicyResult(
                    false,
                    "CANONICAL ACTIONABILITY NOT READY");

            return new ScenarioExecutionPolicyResult(
                true,
                "M5 SCENARIO ALIGNS WITH CANONICAL PLAN");
        }

        private void EnrichScenarioEvidence(
            TradeOpportunityCandidate candidate,
            Frame frame,
            int direction)
        {
            if (candidate == null ||
                frame == null ||
                (direction != 1 &&
                 direction != -1))
                return;

            int independent =
                0;

            int location =
                0;

            if (direction == 1)
            {
                if (frame.StructureBull)
                    independent++;

                if (frame.MssBull)
                    independent++;

                if (frame.ChochBull)
                    independent++;

                if (frame.DisplacementBull)
                    independent++;

                if (frame.LiquidityBull)
                    independent++;

                if (frame.VolumeBull)
                    independent++;

                if (frame.MacdBull)
                    independent++;

                if (frame.VwapBull)
                    independent++;

                if (frame.WaveTrendBull)
                    independent++;

                location =
                    Math.Max(
                        frame.FvgBullQuality,
                        frame.ObBullQuality);

                if (frame.FvgObBullConfluence)
                    location =
                        Math.Min(
                            100,
                            location + 12);
            }
            else
            {
                if (frame.StructureBear)
                    independent++;

                if (frame.MssBear)
                    independent++;

                if (frame.ChochBear)
                    independent++;

                if (frame.DisplacementBear)
                    independent++;

                if (frame.LiquidityBear)
                    independent++;

                if (frame.VolumeBear)
                    independent++;

                if (frame.MacdBear)
                    independent++;

                if (frame.VwapBear)
                    independent++;

                if (frame.WaveTrendBear)
                    independent++;

                location =
                    Math.Max(
                        frame.FvgBearQuality,
                        frame.ObBearQuality);

                if (frame.FvgObBearConfluence)
                    location =
                        Math.Min(
                            100,
                            location + 12);
            }

            candidate.IndependentEvidenceScore =
                independent;

            candidate.LocationConfluenceScore =
                location;

            candidate.WaveTrendQuality =
                frame.WaveTrendQuality;

            ScenarioExecutionPolicyResult policy =
                EvaluateScenarioExecutionPolicy(
                    candidate);

            candidate.ExecutionPolicyAllowed =
                policy.Allowed;

            candidate.ExecutionPolicyReason =
                policy.Reason;
        }
    }
}
