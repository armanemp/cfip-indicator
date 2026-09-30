using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool _brokerStateReconciledThisCycle;

        private bool RunPreDecisionBrokerReconciliation(
            int index)
        {
            _brokerStateReconciledThisCycle = false;

            bool result =
                RunCalculationStage(
                    () =>
                    {
                        SynchronizeLiveBrokerState();
                        _brokerStateReconciledThisCycle = true;
                        return true;
                    },
                    index,
                    "BROKER RECONCILIATION • PRE-DECISION");

            if (!result)
                _brokerStateReconciledThisCycle = false;

            return _brokerStateReconciledThisCycle;
        }
    }
}
