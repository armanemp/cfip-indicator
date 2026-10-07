using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionMarketGates(
            int closedM5)
        {
            if (!SessionAllowed(TimeInUtc))
                return new DecisionFilterResult(false, "SESSION");

            if (!FridayAllowed(TimeInUtc))
                return new DecisionFilterResult(false, "FRIDAY");

            if (!SpreadAllowed(
                _m5Bars,
                closedM5))
                return new DecisionFilterResult(false, "SPREAD");

            if ((UseVolatilityGuard ||
                 UseVolatilityEventGuard) &&
                VolatilityBlocked(
                    _m5Bars,
                    closedM5))
                return new DecisionFilterResult(false, "VOLATILITY GUARD");

            // Fresh aggressive flow is a live execution-quality veto only.
            // It must never create or flip the canonical M15 direction, but a
            // strong current flow directly opposing an otherwise qualified
            // entry is enough to keep the signal out of the executable state.
            AggressiveFlowSnapshot flowSnapshot;
            if (TryGetFreshAggressiveFlowSnapshot(
                    out flowSnapshot) &&
                flowSnapshot.StronglyOpposes(
                    decision.Direction,
                    0.35,
                    0.75))
                return new DecisionFilterResult(
                    false,
                    "FRESH FLOW OPPOSITION");

            string reason;
            if (UseNewsEventGuard &&
                NewsBlocked(
                    TimeInUtc,
                    out reason))
                return new DecisionFilterResult(false, reason);

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}