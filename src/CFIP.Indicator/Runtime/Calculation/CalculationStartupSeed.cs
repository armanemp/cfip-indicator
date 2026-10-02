using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool RunStartupCalculationSeed()
        {
            if (_startupCalculationSeedDone ||
                !_initializationReady ||
                _calculationBusy)
                return false;

            if (Bars == null ||
                Bars.Count < 2 ||
                !HasEnoughData())
                return false;

            _calculationBusy = true;

            try
            {
                BeginRuntimeFaultCycle();

                DateTime reference =
                    Server.TimeInUtc;

                RunPreDecisionBrokerReconciliation(
                    Bars.Count - 1);

                MtfClosedContext mtf =
                    BuildMtfClosedContext(
                        reference);

                _lastMtfClosedContext =
                    mtf;

                RefreshClosedM1Frame(
                    mtf.M1);

                int closedM5 =
                    mtf.M5;

                if (!mtf.HasPrimaryDecisionHistory)
                {
                    RecordCalculationReadiness(
                        CalculationReadinessState.WaitingForMtfData,
                        reference);

                    RenderCalculationReadinessIfNeeded(
                        reference);
                    CompleteRuntimeFaultCycle();
                    return false;
                }

                RecordCalculationReadiness(
                    CalculationReadinessState.Ready,
                    reference);

                RunClosedBarAnalysisStage(
                    Bars.Count - 1,
                    closedM5,
                    reference,
                    mtf);

                UpdateLiveReaction();

                UpdateExecutionModel(
                    closedM5);

                RenderCalculationState(
                    Bars.Count - 1,
                    closedM5);

                RefreshReadOnlyProvider(
                    closedM5);

                CompleteRuntimeFaultCycle();

                _startupCalculationSeedDone = true;
                _lastCalculationCompletedUtc = TimeInUtc;

                // Import the portable history archive only after the first
                // usable decision cycle has been seeded, keeping disk I/O out
                // of the critical startup path.
                QueueOutcomeArchiveImport();

                return true;
            }
            catch (OutOfMemoryException)
            {
                Print(
                    "CFIP fatal runtime fault: OutOfMemoryException during startup calculation seed");
                throw;
            }
            catch (StackOverflowException)
            {
                Print(
                    "CFIP fatal runtime fault: StackOverflowException during startup calculation seed");
                throw;
            }
            catch (Exception ex)
            {
                HandleRuntimeFault(
                    ex,
                    Bars.Count - 1,
                    "STARTUP CALCULATION SEED");
                return false;
            }
            finally
            {
                _calculationBusy = false;
            }
        }
    }
}
