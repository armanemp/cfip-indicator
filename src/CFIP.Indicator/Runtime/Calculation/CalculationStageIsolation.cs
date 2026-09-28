using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool RunCalculationPreparationStage(
            int index,
            out int closedM5,
            out bool newClosedBar,
            out DateTime reference,
            out MtfClosedContext mtf)
        {
            closedM5 = -1;
            newClosedBar = false;
            reference = DateTime.MinValue;
            mtf = null;

            try
            {
                return TryPrepareCalculationCycle(
                    out closedM5,
                    out newClosedBar,
                    out reference,
                    out mtf);
            }
            catch (OutOfMemoryException)
            {
                Print(
                    "CFIP fatal runtime fault: OutOfMemoryException during preparation at index {0}",
                    index);
                throw;
            }
            catch (StackOverflowException)
            {
                Print(
                    "CFIP fatal runtime fault: StackOverflowException during preparation at index {0}",
                    index);
                throw;
            }
            catch (Exception ex)
            {
                HandleRuntimeFault(
                    ex,
                    index,
                    "CALCULATION PREPARATION");
                return false;
            }
        }

        private bool RunClosedBarAnalysisStage(
            int index,
            int closedM5,
            DateTime reference,
            MtfClosedContext mtf)
        {
            string retryReason;

            if (!_runtimeFaultStateMachine.CanAttemptClosedBarAnalysis(
                    closedM5,
                    TimeInUtc,
                    out retryReason))
            {
                _autoExecutionBlockReason =
                    retryReason;

                SetAutoTradingState(
                    "BLOCKED",
                    retryReason);

                return false;
            }

            bool result =
                RunCalculationStage(
                    () =>
                        ProcessNewClosedBar(
                            index,
                            closedM5,
                            reference,
                            mtf),
                    index,
                    "CLOSED-BAR ANALYSIS",
                    closedM5);

            if (!_runtimeFaultStateMachine.CycleFaulted)
            {
                _runtimeFaultStateMachine.RecordClosedBarAnalysisSuccess(
                    closedM5);
            }

            return result;
        }

        private void ProcessLiveCalculationStages(
            int index,
            int closedM5)
        {
            // Management-first rule:
            // reconcile broker state, recover managed live state, evaluate active
            // protection/exits and synchronize broker protection before optional
            // live intelligence can run.
            RunCalculationStage(
                () =>
                {
                    SynchronizeLiveBrokerState();
                    return true;
                },
                index,
                "BROKER RECONCILIATION • PREFLIGHT");

            RunCalculationStage(
                () =>
                {
                    RecoverManagedLivePlan(
                        closedM5);
                    return true;
                },
                index,
                "BROKER LIFECYCLE RECOVERY");

            RunCalculationStage(
                () =>
                {
                    SynchronizeLiveBrokerState();
                    return true;
                },
                index,
                "BROKER RECONCILIATION • POST-RECOVERY");

            RunCalculationStage(
                () =>
                {
                    EvaluateActivePlan(
                        closedM5);
                    return true;
                },
                index,
                "ACTIVE PLAN MANAGEMENT");

            RunCalculationStage(
                () =>
                {
                    ProtectBrokerPositions(
                        closedM5);
                    return true;
                },
                index,
                "BROKER PROTECTION • PRE-ANALYSIS");

            MarkRuntimeManagementReadyForRecovery();

            RunCalculationStage(
                () =>
                {
                    UpdateLiveReaction();
                    return true;
                },
                index,
                "LIVE ANALYSIS");

            RunCalculationStage(
                () =>
                {
                    SynchronizePreTradePlanWithDecision();
                    return true;
                },
                index,
                "PLAN SYNCHRONIZATION");

            RunCalculationStage(
                () =>
                {
                    if (!AutoTradingEnabled &&
                        AutoTradingReminder)
                    {
                        CheckAutoTradingDisabledReminder(
                            closedM5);
                    }

                    return true;
                },
                index,
                "EXECUTION REMINDER");

            RunCalculationStage(
                () =>
                {
                    SyncQuickExecutionControls();
                    return true;
                },
                index,
                "EXECUTION CONTROL SYNCHRONIZATION");

            RunCalculationStage(
                () =>
                {
                    TryEnsureAutomaticPlan(
                        closedM5);
                    return true;
                },
                index,
                "PLAN CREATION");

            RunCalculationStage(
                () =>
                {
                    UpdateExecutionModel(
                        closedM5);
                    return true;
                },
                index,
                "EXECUTION MODEL");

            RunCalculationStage(
                () =>
                {
                    TryAutoTrade(
                        closedM5);

                    TryAggressiveAutoTrade(
                        closedM5);

                    TrySmartPendingOrders(
                        closedM5);

                    return true;
                },
                index,
                "EXECUTION");

            RunCalculationStage(
                () =>
                {
                    SynchronizeLiveBrokerState();
                    return true;
                },
                index,
                "BROKER RECONCILIATION • POST-EXECUTION");

            RunCalculationStage(
                () =>
                {
                    ProtectBrokerPositions(
                        closedM5);
                    return true;
                },
                index,
                "BROKER PROTECTION • POST-EXECUTION");

            RunCalculationStage(
                () =>
                {
                    MonitorOutcome(
                        closedM5);

                    CheckEndOfDayAlert(
                        TimeInUtc);

                    return true;
                },
                index,
                "TELEMETRY");

            RunCalculationStage(
                () =>
                {
                    CheckReversalProtection();
                    return true;
                },
                index,
                "REVERSAL MANAGEMENT");

            RunCalculationStage(
                () =>
                {
                    SynchronizeLiveBrokerState();
                    return true;
                },
                index,
                "BROKER STATE FINALIZATION");

            RunCalculationStage(
                () =>
                {
                    RenderCalculationState(
                        index,
                        closedM5);
                    return true;
                },
                index,
                "PRESENTATION");
        }

        private bool RunCalculationStage(
            Func<bool> stage,
            int index,
            string stageName,
            int stateKey = -1)
        {
            try
            {
                return stage == null || stage();
            }
            catch (OutOfMemoryException)
            {
                Print(
                    "CFIP fatal runtime fault: OutOfMemoryException in {0} at index {1}",
                    stageName,
                    index);
                throw;
            }
            catch (StackOverflowException)
            {
                Print(
                    "CFIP fatal runtime fault: StackOverflowException in {0} at index {1}",
                    stageName,
                    index);
                throw;
            }
            catch (Exception ex)
            {
                if (stageName ==
                    "CLOSED-BAR ANALYSIS" &&
                    stateKey >= 0)
                {
                    _runtimeFaultStateMachine.RecordClosedBarAnalysisFailure(
                        stateKey,
                        TimeInUtc);
                }

                HandleRuntimeFault(
                    ex,
                    index,
                    stageName);

                return
                    stageName ==
                    "CLOSED-BAR ANALYSIS"
                        ? false
                        : true;
            }
        }
    }
}
