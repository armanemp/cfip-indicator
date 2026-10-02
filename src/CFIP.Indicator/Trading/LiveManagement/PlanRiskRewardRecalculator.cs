using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RecalculatePlanRR()
        {
            if (_plan == null ||
                !IsFinitePositive(_plan.Entry) ||
                !IsFinitePositive(_plan.Stop))
                return;

            _plan.Risk =
                RiskRewardMathRule.Evaluate(
                    _plan.Direction,
                    _plan.Entry,
                    _plan.Stop,
                    _plan.Tp1,
                    0,
                    0,
                    MaximumRewardRR,
                    Symbol.PipSize).Risk;

            _plan.Tp1RR =
                CalculatePlanStageRR(_plan.Tp1);

            _plan.Tp2RR =
                CalculatePlanStageRR(_plan.Tp2);

            _plan.Tp3RR =
                CalculatePlanStageRR(_plan.Tp3);

            _plan.Tp4RR =
                CalculatePlanStageRR(_plan.Tp4);
        }

        private double CalculatePlanStageRR(
            double target)
        {
            if (_plan == null ||
                !IsFinitePositive(target))
                return 0;

            RiskRewardMathResult geometry =
                RiskRewardMathRule.Evaluate(
                    _plan.Direction,
                    _plan.Entry,
                    _plan.Stop,
                    target,
                    0,
                    0,
                    MaximumRewardRR,
                    Symbol.PipSize);

            return geometry.Valid
                ? geometry.NominalRR
                : 0;
        }
    }
}
