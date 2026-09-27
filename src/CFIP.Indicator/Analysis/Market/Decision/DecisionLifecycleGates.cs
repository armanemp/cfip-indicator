using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionLifecycleGates(
            int closedM5,
            Decision decision)
        {
            if (!CanAcceptConfirmedDirection(
                decision.Direction,
                closedM5))
                return new DecisionFilterResult(false, "DIRECTION FLIP");

            if (CooldownBlocked(closedM5))
                return new DecisionFilterResult(false, "COOLDOWN");

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}