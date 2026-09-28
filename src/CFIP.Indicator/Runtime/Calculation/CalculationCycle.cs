using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        public override void Calculate(int index)
        {
            if (_calculationBusy)
                return;

            _calculationBusy = true;

            try
            {
                if (!_initializationReady)
                    return;

                int closedM5;
                bool newClosedBar;
                DateTime reference;
                MtfClosedContext mtf;

                if (!RunCalculationPreparationStage(
                    index,
                    out closedM5,
                    out newClosedBar,
                    out reference,
                    out mtf))
                    return;

                if (newClosedBar &&
                    !RunClosedBarAnalysisStage(
                        index,
                        closedM5,
                        reference,
                        mtf))
                    return;

                ProcessLiveCalculationStages(
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
                    "CALCULATE ORCHESTRATION");
            }
            finally
            {
                _calculationBusy = false;
            }
        }
    }
}
