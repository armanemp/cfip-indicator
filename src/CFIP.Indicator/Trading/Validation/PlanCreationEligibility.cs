// CFIP Indicator — PlanCreationEligibility.cs
// Determine whether the current authoritative decision may create a plan.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldCreatePlan(
            int closedM5,
            DecisionPolicyMode policy)
        {
            if (BlockNewSignalWhileActive &&
                _plan != null)
                return false;

            if (_decision == null ||
                _decision.Direction == 0)
                return false;

            if (!_decision.EntryAllowed)
                return false;

            if (!_decision.TriggerReady)
                return false;

            if (BlockSameBarReentryAfterExit &&
                _lastExitM5 == closedM5)
                return false;

            if (_lastSignalM5 >= 0 &&
                closedM5 - _lastSignalM5 <
                Math.Max(
                    CooldownBars,
                    Math.Max(
                        CooldownM5Bars,
                        ExitReentryCooldownM5)))
                return false;

            return _lastSignalM5 != closedM5;
        }
    }
}
