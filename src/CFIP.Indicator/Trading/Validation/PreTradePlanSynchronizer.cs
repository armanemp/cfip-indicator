using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SynchronizePreTradePlanWithDecision()
        {
            if (_plan == null ||
                _plan.IsLivePosition)
                return;

            if (_decision == null ||
                _decision.Direction == 0 ||
                _decision.Direction != _plan.Direction ||
                IsHardDecisionBlockReason(
                    _decision.BlockReason))
            {
                _plan = null;
                _executionModel = null;
                RemovePlanObjects();
            }
        }
    }
}