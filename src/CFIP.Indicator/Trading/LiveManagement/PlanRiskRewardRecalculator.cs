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
                _plan.Tp1 > 0
                    ? Math.Abs(
                        _plan.Tp1 -
                        _plan.Entry) /
                      _plan.Risk
                    : 0;

            _plan.Tp2RR =
                _plan.Tp2 > 0
                    ? Math.Abs(
                        _plan.Tp2 -
                        _plan.Entry) /
                      _plan.Risk
                    : 0;

            _plan.Tp3RR =
                _plan.Tp3 > 0
                    ? Math.Abs(
                        _plan.Tp3 -
                        _plan.Entry) /
                      _plan.Risk
                    : 0;

            _plan.Tp4RR =
                _plan.Tp4 > 0
                    ? Math.Abs(
                        _plan.Tp4 -
                        _plan.Entry) /
                      _plan.Risk
                    : 0;
        }
    }
}
