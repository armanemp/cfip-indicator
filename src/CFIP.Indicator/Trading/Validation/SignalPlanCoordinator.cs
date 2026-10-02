// CFIP Indicator — SignalPlanCoordinator.cs
// Coordinate signal-plan creation without owning plan construction.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void EnsureSignalPlan(
            int closedM5,
            DecisionPolicyMode policy)
        {
            if (_plan != null ||
                _decision == null ||
                _decision.Direction == 0)
                return;

            if (!ShouldCreatePlan(
                    closedM5,
                    policy))
                return;

            Plan plan =
                BuildPlan(
                    closedM5,
                    _decision.Direction);

            if (plan == null)
            {
                return;
            }

            _lastAutoPlanAttemptM5 =
                closedM5;

            ActivatePlan(plan);
        }
    }
}
