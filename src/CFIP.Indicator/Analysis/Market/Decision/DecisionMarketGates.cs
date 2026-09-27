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

            string reason;
            if (UseNewsEventGuard &&
                NewsBlocked(
                    TimeInUtc,
                    out reason))
                return new DecisionFilterResult(
                    false,
                    string.IsNullOrWhiteSpace(reason)
                        ? "NEWS BLACKOUT"
                        : reason);

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}