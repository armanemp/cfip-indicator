using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class ScenarioExecutionPolicyResult
    {
        public bool CandidateEligible { get; }
        public bool ExecutionAuthorized { get; }
        public string CandidateEligibilityReason { get; }
        public string ExecutionReason { get; }

        public ScenarioExecutionPolicyResult(
            bool candidateEligible,
            bool executionAuthorized,
            string candidateEligibilityReason,
            string executionReason)
        {
            CandidateEligible = candidateEligible;
            ExecutionAuthorized = executionAuthorized;
            CandidateEligibilityReason =
                string.IsNullOrWhiteSpace(candidateEligibilityReason)
                    ? (candidateEligible ? "CANDIDATE ELIGIBLE" : "CANDIDATE BLOCKED")
                    : candidateEligibilityReason;
            ExecutionReason =
                string.IsNullOrWhiteSpace(executionReason)
                    ? (executionAuthorized ? "EXECUTION AUTHORIZED" : "EXECUTION BLOCKED")
                    : executionReason;
        }
    }

    internal static class ScenarioExecutionPolicyRule
    {
        internal static ScenarioExecutionPolicyResult Evaluate(
            TradeOpportunityCandidate candidate,
            Decision decision,
            OpportunityLane expectedLane,
            bool m5OnlyConfirmedTrigger)
        {
            if (candidate == null)
                return BlockScenarioExecutionPolicy("NO SCENARIO");

            if (candidate.PresentationOnly)
                return BlockScenarioExecutionPolicy(
                    "PRIMARY PRESENTATION ONLY");

            if (decision == null ||
                decision.Direction == 0)
                return BlockScenarioExecutionPolicy("NO CANONICAL DECISION");

            bool futurePending =
                IsFuturePendingCandidate(candidate);

            // Current market scenarios remain direction-locked to the canonical
            // M15 decision. Future pending scenarios are allowed to represent a
            // separate continuation or reversal path, because their job is to
            // wait at a pre-planned future level rather than enter now.
            if (!futurePending &&
                candidate.Direction != decision.Direction)
                return BlockScenarioExecutionPolicy("SCENARIO / DECISION DIRECTION MISMATCH");

            if (futurePending &&
                candidate.ExecutionMode == ExecutionMode.ContinuationStop &&
                candidate.Direction != decision.Direction)
                return BlockScenarioExecutionPolicy("FUTURE STOP / DECISION DIRECTION MISMATCH");

            // Materialized candidates carry a measured reward distance.
            // Lightweight contract/diagnostic candidates may omit it; those
            // remain governed by the existing policy gates.
            if (NumericGuards.IsFinitePositive(
                    candidate.RewardDistanceAtr))
            {
                double adaptiveRewardFloor =
                    RegimeAdaptiveRewardFloorRule.ResolveAdaptiveRewardFloor(
                        decision.Regime,
                        0,
                        0);

                double requiredRewardFloor =
                    Math.Max(
                        candidate.MinimumRequiredRewardDistanceAtr,
                        adaptiveRewardFloor);

                if (NumericGuards.IsFinitePositive(requiredRewardFloor) &&
                    candidate.RewardDistanceAtr <
                    requiredRewardFloor)
                {
                    return BlockScenarioExecutionPolicy(
                        "REWARD DISTANCE BELOW OPPORTUNITY FLOOR");
                }
            }

            if (candidate.Lane != expectedLane)
                return BlockScenarioExecutionPolicy("SCENARIO / PLAN LANE MISMATCH");

            if (!decision.EntryAllowed)
                return BlockScenarioExecutionPolicy("CANONICAL DECISION BLOCKED");

            // Future pending orders are deliberately armed before the trigger is
            // touched. Their prerequisite is the dedicated pending-quality path,
            // not the current market-entry ActionableNow flag.
            if (!futurePending &&
                EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    candidate.ExecutionMode,
                    m5OnlyConfirmedTrigger) &&
                !decision.TriggerReady)
                return BlockScenarioExecutionPolicy("CANONICAL TRIGGER NOT READY");

            if (!futurePending &&
                !decision.ActionableNow)
                return BlockScenarioExecutionPolicy("CANONICAL DECISION NOT ACTIONABLE");

            if (futurePending)
            {
                if (!candidate.FutureOrderReady)
                    return BlockScenarioExecutionPolicy("FUTURE ORDER NOT ARMED");

                return new ScenarioExecutionPolicyResult(
                    true,
                    true,
                    "FUTURE PENDING SCENARIO ELIGIBLE",
                    "FUTURE PENDING SCENARIO EXECUTION AUTHORIZED");
            }

            if (!candidate.ActionableNow)
                return BlockScenarioExecutionPolicy(
                    string.IsNullOrWhiteSpace(candidate.ActionabilityReason)
                        ? "SCENARIO NOT ACTIONABLE"
                        : candidate.ActionabilityReason);

            bool independentTimeframe =
                !string.IsNullOrWhiteSpace(candidate.SourceTimeframe) &&
                !string.Equals(
                    candidate.SourceTimeframe.Trim(),
                    "M5",
                    StringComparison.OrdinalIgnoreCase);

            bool primaryExecutionTimeframe =
                string.Equals(
                    candidate.SourceTimeframe == null
                        ? ""
                        : candidate.SourceTimeframe.Trim(),
                    "M15",
                    StringComparison.OrdinalIgnoreCase);

            if (independentTimeframe &&
                !primaryExecutionTimeframe)
            {
                return new ScenarioExecutionPolicyResult(
                    true,
                    false,
                    "INDEPENDENT HTF STRUCTURALLY ELIGIBLE",
                    "OBSERVE-ONLY HTF SCENARIO");
            }

            if (primaryExecutionTimeframe)
            {
                return new ScenarioExecutionPolicyResult(
                    true,
                    true,
                    "PRIMARY M15 SCENARIO ELIGIBLE",
                    "M15 SCENARIO EXECUTION AUTHORIZED");
            }

            return new ScenarioExecutionPolicyResult(
                true,
                true,
                "CANONICAL CANDIDATE ELIGIBLE",
                "CANONICAL SCENARIO EXECUTION AUTHORIZED");
        }

        private static bool IsFuturePendingCandidate(
            TradeOpportunityCandidate candidate)
        {
            return candidate != null &&
                candidate.FutureOrderReady &&
                (candidate.ExecutionMode == ExecutionMode.ContinuationStop ||
                 candidate.ExecutionMode == ExecutionMode.ReversalLimit);
        }

        internal static bool TryResolvePlanScenario(
            IReadOnlyList<TradeOpportunityCandidate> candidates,
            Plan plan,
            Decision decision,
            double priceTolerance,
            bool m5OnlyConfirmedTrigger,
            out TradeOpportunityCandidate selected,
            out string reason)
        {
            selected = null;
            reason = string.Empty;

            if (plan == null ||
                decision == null)
            {
                reason = "CANONICAL PLAN / DECISION UNAVAILABLE";
                return false;
            }

            if (candidates == null)
            {
                reason = "NO SCENARIOS";
                return false;
            }

            double tolerance = Math.Max(0, priceTolerance);

            for (int i = 0; i < candidates.Count; i++)
            {
                TradeOpportunityCandidate candidate = candidates[i];
                ScenarioExecutionPolicyResult policy =
                    Evaluate(
                        candidate,
                        decision,
                        plan.Lane,
                        m5OnlyConfirmedTrigger);

                if (!policy.CandidateEligible ||
                    !policy.ExecutionAuthorized)
                    continue;

                if (candidate.CreatedM5 != plan.CreatedM5 ||
                    candidate.Direction != plan.Direction)
                    continue;

                if (!IsScenarioPriceClose(candidate.Entry, plan.Entry, tolerance) ||
                    !IsScenarioPriceClose(candidate.Stop, plan.Stop, tolerance) ||
                    !IsScenarioPriceClose(candidate.Tp1, plan.Tp1, tolerance))
                    continue;

                if (selected == null ||
                    CompareScenarioPriority(candidate, selected) < 0)
                    selected = candidate;
            }

            if (selected != null)
            {
                reason = "SCENARIO MATCHED • " + selected.ScenarioId;
                return true;
            }

            reason = "NO EXACT CANONICAL SCENARIO MATCH";
            return false;
        }

        internal static bool TryResolveDirectionScenario(
            IReadOnlyList<TradeOpportunityCandidate> candidates,
            Decision decision,
            int direction,
            bool m5OnlyConfirmedTrigger,
            out TradeOpportunityCandidate selected)
        {
            selected = null;

            if (decision == null ||
                !decision.EntryAllowed ||
                decision.Direction != direction ||
                candidates == null)
                return false;

            for (int i = 0; i < candidates.Count; i++)
            {
                TradeOpportunityCandidate candidate = candidates[i];
                ScenarioExecutionPolicyResult policy =
                    Evaluate(
                        candidate,
                        decision,
                        candidate == null
                            ? OpportunityLane.Tactical
                            : candidate.Lane,
                        m5OnlyConfirmedTrigger);

                if (!policy.CandidateEligible ||
                    !policy.ExecutionAuthorized)
                    continue;

                if (selected == null ||
                    CompareScenarioPriority(candidate, selected) < 0)
                    selected = candidate;
            }

            return selected != null;
        }

        internal static string CanonicalScenarioId(Plan plan)
        {
            if (plan == null)
                return "CANONICAL-NONE";

            string lane;
            switch (plan.Lane)
            {
                case OpportunityLane.Strategic:
                    lane = "HTF";
                    break;
                case OpportunityLane.CounterHtfTactical:
                    lane = "TACTICAL-COUNTER";
                    break;
                case OpportunityLane.MicroReaction:
                    lane = "MICRO";
                    break;
                default:
                    lane = "TACTICAL";
                    break;
            }

            return
                "CANONICAL-" +
                lane +
                "-" +
                (plan.Direction == 1
                    ? "BUY"
                    : "SELL");
        }

        private static ScenarioExecutionPolicyResult BlockScenarioExecutionPolicy(string reason)
        {
            return new ScenarioExecutionPolicyResult(
                false,
                false,
                reason,
                reason);
        }

        private static bool IsScenarioPriceClose(
            double left,
            double right,
            double tolerance)
        {
            return
                !double.IsNaN(left) &&
                !double.IsInfinity(left) &&
                !double.IsNaN(right) &&
                !double.IsInfinity(right) &&
                Math.Abs(left - right) <= tolerance;
        }

        private static int CompareScenarioPriority(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            int actionable =
                right.ActionableNow.CompareTo(left.ActionableNow);

            if (actionable != 0)
                return actionable;

            int quality =
                right.Quality.CompareTo(left.Quality);

            if (quality != 0)
                return quality;

            int rr =
                right.Tp1RR.CompareTo(left.Tp1RR);

            if (rr != 0)
                return rr;

            int created =
                right.CreatedM5.CompareTo(left.CreatedM5);

            if (created != 0)
                return created;

            return string.CompareOrdinal(
                left.ScenarioId ?? string.Empty,
                right.ScenarioId ?? string.Empty);
        }
    }
}
