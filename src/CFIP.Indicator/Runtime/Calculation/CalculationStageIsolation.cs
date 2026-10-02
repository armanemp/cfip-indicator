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

        private void ProcessWaitingForDataStages(
            int index,
            int closedM5)
        {
            // There is no decision/alert to consume while readiness is waiting.
            // Reconcile broker truth once, then keep lifecycle/protection alive.
            RunPreDecisionBrokerReconciliation(
                index);

            // Keep lifecycle/protection alive, but do not run
            // decision, signal, plan-creation or execution stages while data
            // readiness is incomplete.
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
                "BROKER RECONCILIATION • WAITING");

            RunCalculationStage(
                () =>
                {
                    ApplyEconomicNewsRiskProtection(
                        closedM5);
                    return true;
                },
                index,
                "NEWS RISK PROTECTION • WAITING");

            RunCalculationStage(
                () =>
                {
                    EvaluateActivePlan(
                        closedM5);
                    return true;
                },
                index,
                "ACTIVE PLAN MANAGEMENT • WAITING");

            RunCalculationStage(
                () =>
                {
                    ProtectBrokerPositions(
                        closedM5);
                    return true;
                },
                index,
                "BROKER PROTECTION • WAITING");

            MarkRuntimeManagementReadyForRecovery();

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
                "OUTCOME/EOD SUPERVISION • WAITING");

            RunCalculationStage(
                () =>
                {
                    CheckReversalProtection();
                    return true;
                },
                index,
                "REVERSAL MANAGEMENT • WAITING");

            RunCalculationStage(
                () =>
                {
                    SynchronizeLiveBrokerState();
                    return true;
                },
                index,
                "BROKER STATE FINALIZATION • WAITING");

            RenderCalculationReadinessIfNeeded(
                TimeInUtc);
        }

        private void ProcessLiveCalculationStages(
            int index,
            int closedM5,
            bool newClosedBar)
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
                    ApplyEconomicNewsRiskProtection(
                        closedM5);
                    return true;
                },
                index,
                "NEWS RISK PROTECTION");

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
                    UpdateM1TriggerRuntime(
                        closedM5,
                        TimeInUtc);
                    return true;
                },
                index,
                "M1 TRIGGER RUNTIME");

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



            // Predictive pending orders get first execution priority. They must be
            // evaluated before a market plan is materialized, otherwise a newly-created
            // market plan can suppress the independent pending-order path.
            RunCalculationStage(
                () =>
                {
                    UpdateExecutionModel(
                        closedM5);
                    return true;
                },
                index,
                "EXECUTION MODEL • PENDING PREFLIGHT");

            RunCalculationStage(
                () =>
                {
                    TrySmartPendingOrders(
                        closedM5);
                    return true;
                },
                index,
                "PREDICTIVE PENDING EXECUTION");

            // Aggressive market entry is the direct AUTO TRADE path. Give it first
            // opportunity after predictive orders; if the intrabar reaction is not
            // sufficiently qualified, the normal plan/market path can still proceed.
            RunCalculationStage(
                () =>
                {
                    TryAggressiveAutoTrade(
                        closedM5);
                    return true;
                },
                index,
                "AGGRESSIVE AUTO EXECUTION");

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
                    return true;
                },
                index,
                "AUTOMATIC MARKET EXECUTION");

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

            if (newClosedBar)
            {
                RunCalculationStage(
                    () =>
                    {
                        RecordSignalEvaluationTrace(
                            _decision,
                            ResolveSignalTraceLane(
                                _decision,
                                _decision == null
                                    ? OpportunityLane.Tactical
                                    : _decision.TacticalOpportunityLane),
                            closedM5);
                        return true;
                    },
                    index,
                    "SIGNAL TRACE • CLOSED-M5");
            }

            RunCalculationStage(
                () =>
                {
                    ProcessDecisionOwnedWatchReactionAlerts(
                        closedM5);
                    return true;
                },
                index,
                "WATCH/REACTION ALERTS");

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

            RunCalculationStage(
                () =>
                {
                    RefreshReadOnlyProvider(
                        closedM5);
                    return true;
                },
                index,
                "CBOT READ-ONLY PROVIDER");
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
                    "CLOSED-BAR ANALYSIS" ||
                    stageName ==
                    "BROKER RECONCILIATION • PRE-DECISION"
                        ? false
                        : true;
            }
        }
    }
}
