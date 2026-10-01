using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double LiveBias(
            int closedM5,
            DateTime reference,
            int direction)
        {
            if (_m5Bars == null ||
                _m5Bars.Count < 10 ||
                closedM5 < 1 ||
                closedM5 >= _m5Bars.Count - 1 ||
                !ClosedBarReferenceRule.IsFullyClosed(
                    _m5Bars.Count,
                    closedM5,
                    reference,
                    index => _m5Bars.OpenTimes[index]))
                return 0;

            double fast =
                Ema(
                    _m5Bars,
                    closedM5,
                    true);

            double slow =
                Ema(
                    _m5Bars,
                    closedM5,
                    false);

            return LiveM5BiasRule.Evaluate(
                _m5Bars.ClosePrices[closedM5],
                fast,
                slow,
                direction);
        }
    }
}
