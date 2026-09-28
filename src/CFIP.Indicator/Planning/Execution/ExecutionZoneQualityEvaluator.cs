using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int EvaluateExecutionZoneQuality(
            int closedM5,
            int direction,
            int quality)
        {
            int retest =
                RetestQuality(
                    _m5Bars,
                    closedM5,
                    direction);

            if ((direction == 1 &&
                 PremiumDiscountBias(
                     _m5Bars,
                     closedM5) == 1) ||
                (direction == -1 &&
                 PremiumDiscountBias(
                     _m5Bars,
                     closedM5) == -1))
                quality += 5;

            if (_m15Frame != null &&
                _m15Frame.Direction ==
                direction)
                quality += 5;

            if (_m30Frame != null &&
                _m30Frame.Direction ==
                direction)
                quality += 3;

            if (retest >=
                MinimumRetestQuality)
                quality += 5;

            return ClampInt(
                quality,
                0,
                100);
        }
    }
}
