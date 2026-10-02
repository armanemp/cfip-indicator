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
            string capacityReason;
            if (!ValidateSinglePlanCapacity(
                    out capacityReason))
                return false;

            if (_decision == null ||
                _decision.Direction == 0)
                return false;

            if (!_decision.EntryAllowed)
                return false;

            // The generic Decision.TriggerReady flag is intentionally not a
            // global plan-creation gate. RetestMarket is a zone-driven entry
            // and may be planned before the generic closed-bar trigger fires.
            // Breakout and predictive pending modes remain trigger-dependent.
            if (EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    _executionModel == null
                        ? ExecutionMode.None
                        : _executionModel.Mode,
                    M5OnlyConfirmedTrigger) &&
                !_decision.TriggerReady)
                return false;

            // Plan is an analytical artifact. Current-quote actionability is evaluated
            // separately at the execution-facing boundary and must not prevent the
            // Indicator from publishing a complete plan to the cBot.
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
