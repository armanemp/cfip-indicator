using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        public override void Calculate(int index)
        {
            if (!TryPrepareCalculationCycle(
                out int closedM5,
                out bool newClosedBar,
                out DateTime reference,
                out MtfClosedContext mtf))
                return;

            if (newClosedBar &&
                !ProcessNewClosedBar(
                    index,
                    closedM5,
                    reference,
                    mtf))
                return;

            ProcessLiveCalculation(
                index,
                closedM5);
        }
    }
}
