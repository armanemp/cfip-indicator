using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SynchronizeSignalVisualState()
        {
            if (_plan == null)
            {
                RemovePlanObjects();
                return;
            }

            if (_plan.IsLivePosition)
                return;

            if (_decision == null ||
                _decision.Direction == 0 ||
                _decision.Direction == _plan.Direction)
                return;

            PendingOrder pending =
                GetManagedPendingOrder();

            if (pending != null)
                return;

            _plan = null;
            _executionModel = null;
            _authoritativeDirection = 0;
            _authoritativeState = "WAITING";

            RemovePlanObjects();

            _status =
                "PLAN INVALIDATED • DIRECTION CHANGED";
        }
    }
}