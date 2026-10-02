using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RecalculatePlanRR()
        {
            if (_plan == null ||
                _plan.Risk <= 0)
                return;

            _plan.Tp1RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    _plan.Entry,
                    _plan.Tp1,
                    _plan.Risk);

            _plan.Tp2RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    _plan.Entry,
                    _plan.Tp2,
                    _plan.Risk);

            _plan.Tp3RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    _plan.Entry,
                    _plan.Tp3,
                    _plan.Risk);

            _plan.Tp4RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    _plan.Entry,
                    _plan.Tp4,
                    _plan.Risk);
        }
    }
}
