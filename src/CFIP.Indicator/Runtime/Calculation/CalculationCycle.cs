using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        public override void Calculate(int index)
        {
            if (!_initializationReady)
                return;

            try
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
            catch (OutOfMemoryException)
            {
                Print(
                    "CFIP fatal runtime fault: OutOfMemoryException at index {0}",
                    index);
                throw;
            }
            catch (StackOverflowException)
            {
                Print(
                    "CFIP fatal runtime fault: StackOverflowException at index {0}",
                    index);
                throw;
            }
            catch (Exception ex)
            {
                HandleRuntimeFault(
                    ex,
                    index,
                    "CALCULATE");
            }
        }
    }
}
