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

                BeginRuntimeFaultCycle();

                int closedM5;
                bool newClosedBar;
                DateTime reference;
                MtfClosedContext mtf;

                bool prepared =
                    RunCalculationPreparationStage(
                        index,
                        out closedM5,
                        out newClosedBar,
                        out reference,
                        out mtf);

                if (!prepared)
                {
                    if (!_runtimeFaultStateMachine.CycleFaulted)
                        return;

                    // A preparation fault must not starve management. Reuse the
                    // last known closed context while the fault state keeps
                    // automatic entry blocked.
                    closedM5 =
                        Math.Max(
                            1,
                            _lastEvaluatedM5);

                    ProcessLiveCalculationStages(
                        index,
                        closedM5);

                    CompleteRuntimeFaultCycle();
                    return;
                }

                if (newClosedBar)
                {
                    // Closed-bar analysis may fail, or be in retry backoff. In
                    // either case management/protection must still run on the
                    // already-known broker state.
                    RunClosedBarAnalysisStage(
                        index,
                        closedM5,
                        reference,
                        mtf);
                }

                ProcessLiveCalculationStages(
                    index,
                    closedM5);

                CompleteRuntimeFaultCycle();
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
