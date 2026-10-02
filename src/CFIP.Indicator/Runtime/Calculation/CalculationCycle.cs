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

                _brokerStateReconciledThisCycle = false;

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
                    // New decision analysis is intentionally suspended while
                    // history/MTF readiness is incomplete or its probe is throttled.
                    // Broker reconciliation, recovery and protection must continue.
                    closedM5 =
                        GetManagementClosedM5Fallback();

                    ProcessWaitingForDataStages(
                        index,
                        closedM5);

                    CompleteRuntimeFaultCycle();
                    return;
                }

                if (newClosedBar)
                {
                    // Reconcile broker truth immediately before a new decision
                    // is consumed. Non-new-bar ticks do not repeat this boundary.
                    RunPreDecisionBrokerReconciliation(
                        index);

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
                    closedM5,
                    newClosedBar);

                ProcessQueuedAlertDelivery();

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
                if (_initializationReady)
                    _lastCalculationCompletedUtc = TimeInUtc;

                PublishProviderHeartbeatValue(index);

                _calculationBusy = false;
            }
        }
    }
}
