using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

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

            if (newClosedBar)
                ProcessNewClosedBar(
                    index,
                    closedM5,
                    reference,
                    mtf);

            ProcessLiveCalculation(
                index,
                closedM5);
        }
    }
}
