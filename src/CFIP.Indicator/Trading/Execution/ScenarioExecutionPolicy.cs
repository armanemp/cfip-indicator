using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal static class ScenarioExecutionPolicy
    {
        public static bool IsCanonicalCandidateEligible(
            TradeOpportunityCandidate candidate,
            Decision decision,
            OpportunityLane lane,
            out string reason)
        {
            reason = string.Empty;

            if (candidate == null)
            {
                reason = "NO SCENARIO";
                return false;
            }

            if (decision == null ||
                decision.Direction == 0)
            {
                reason = "NO CANONICAL DECISION";
                return false;
            }

            if (candidate.Direction != decision.Direction)
            {
                reason = "SCENARIO / DECISION DIRECTION MISMATCH";
                return false;
            }

            if (candidate.Lane != lane)
            {
                reason = "SCENARIO / PLAN LANE MISMATCH";
                return false;
            }

            if (!decision.EntryAllowed)
            {
                reason = "CANONICAL DECISION BLOCKED";
                return false;
            }

            if (!decision.TriggerReady)
            {
                reason = "CANONICAL TRIGGER NOT READY";
                return false;
            }

            if (!decision.ActionableNow)
            {
                reason =
                    string.IsNullOrWhiteSpace(
                        decision.ActionabilityReason)
                        ? "CANONICAL DECISION NOT ACTIONABLE"
                        : decision.ActionabilityReason;
                return false;
            }

            if (!candidate.ActionableNow)
            {
                reason =
                    string.IsNullOrWhiteSpace(
                        candidate.ActionabilityReason)
                        ? "SCENARIO NOT ACTIONABLE"
                        : candidate.ActionabilityReason;
                return false;
            }

            return true;
        }

        public static bool IsExecutionAuthorizedCandidate(
            TradeOpportunityCandidate candidate,
            Decision decision,
            OpportunityLane lane,
            out string reason)
        {
            if (!IsCanonicalCandidateEligible(
                    candidate,
                    decision,
                    lane,
                    out reason))
                return false;

            if (!candidate.ExecutionPolicyAllowed)
            {
                reason =
                    string.IsNullOrWhiteSpace(
                        candidate.ExecutionPolicyReason)
                        ? "SCENARIO EXECUTION POLICY BLOCKED"
                        : candidate.ExecutionPolicyReason;
                return false;
            }

            return true;
        }

        public static bool TryResolvePlanScenario(
            IReadOnlyList<TradeOpportunityCandidate> candidates,
            Plan plan,
            Decision decision,
            double priceTolerance,
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

            double tolerance =
                Math.Max(
                    0,
                    priceTolerance);

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    candidates[i];

                if (!IsExecutionAuthorizedCandidate(
                        candidate,
                        decision,
                        plan.Lane,
                        out _))
                    continue;

                if (candidate.CreatedM5 != plan.CreatedM5 ||
                    candidate.Direction != plan.Direction)
                    continue;

                if (!IsClose(
                        candidate.Entry,
                        plan.Entry,
                        tolerance) ||
                    !IsClose(
                        candidate.Stop,
                        plan.Stop,
                        tolerance) ||
                    !IsClose(
                        candidate.Tp1,
                        plan.Tp1,
                        tolerance))
                    continue;

                if (selected == null ||
                    Compare(
                        candidate,
                        selected) < 0)
                {
                    selected = candidate;
                }
            }

            if (selected != null)
            {
                reason =
                    "SCENARIO MATCHED • " +
                    selected.ScenarioId;
                return true;
            }

            reason = "NO EXACT CANONICAL SCENARIO MATCH";
            return false;
        }

        public static bool TryResolveDirectionScenario(
            IReadOnlyList<TradeOpportunityCandidate> candidates,
            Decision decision,
            int direction,
            out TradeOpportunityCandidate selected)
        {
            selected = null;

            if (decision == null ||
                !decision.EntryAllowed ||
                !decision.TriggerReady ||
                decision.Direction != direction)
                return false;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    candidates[i];

                string reason;

                if (!IsExecutionAuthorizedCandidate(
                        candidate,
                        decision,
                        candidate == null
                            ? OpportunityLane.Tactical
                            : candidate.Lane,
                        out reason))
                    continue;

                if (selected == null ||
                    Compare(
                        candidate,
                        selected) < 0)
                    selected = candidate;
            }

            return selected != null;
        }

        public static string CanonicalScenarioId(
            Plan plan)
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

        private static bool IsClose(
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

        private static int Compare(
            TradeOpportunityCandidate left,
            TradeOpportunityCandidate right)
        {
            int actionable =
                right.ActionableNow.CompareTo(
                    left.ActionableNow);

            if (actionable != 0)
                return actionable;

            int quality =
                right.Quality.CompareTo(
                    left.Quality);

            if (quality != 0)
                return quality;

            int rr =
                right.Tp1RR.CompareTo(
                    left.Tp1RR);

            if (rr != 0)
                return rr;

            int created =
                right.CreatedM5.CompareTo(
                    left.CreatedM5);

            if (created != 0)
                return created;

            return string.CompareOrdinal(
                left.ScenarioId ?? string.Empty,
                right.ScenarioId ?? string.Empty);
        }
    }
}
