using System;
using System.IO;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyMtfContextIntegrity();
            VerifyMarketExecutionAcceptance();
            VerifyPendingOrderAcceptance();
            VerifyRejectedMutationHandling();
            VerifyFillEnvelopeSymmetry();
            VerifyInitialProtectionDirectionality();
            VerifyManagedBreakEvenDirectionality();
            VerifyProtectionProgression();
            VerifyTargetProgression();
            VerifyExecutionCapacity();
            VerifyStaleLivePlanRecovery();
            VerifyLifecycleFlows();
            VerifyLifecycleIdempotency();
            VerifyRuntimeStageIsolation();
            VerifyRuntimeFaultStateMachine();

            Console.WriteLine("Runtime acceptance contracts OK");
        }

        private static void VerifyMtfContextIntegrity()
        {
            DateTime reference =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    5,
                    0,
                    DateTimeKind.Utc);

            MtfClosedContext context =
                new MtfClosedContext(
                    reference,
                    101,
                    605,
                    41,
                    31,
                    31,
                    30,
                    3,
                    1);

            Assert(
                context.Reference == reference,
                "MTF reference");

            Assert(
                context.M5 == 101 &&
                context.M1 == 605 &&
                context.M15 == 41 &&
                context.M30 == 31 &&
                context.H1 == 31 &&
                context.H4 == 30 &&
                context.D1 == 3 &&
                context.W1 == 1,
                "MTF indices preserved");

            Assert(
                context.HasPrimaryDecisionHistory,
                "primary MTF history");

            MtfClosedContext incomplete =
                new MtfClosedContext(
                    reference,
                    29,
                    605,
                    41,
                    21,
                    11,
                    8,
                    3,
                    1);

            Assert(
                !incomplete.HasPrimaryDecisionHistory,
                "insufficient closed history blocked");
        }

        private static void VerifyMarketExecutionAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    true),
                "confirmed market position");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    false,
                    true),
                "rejected market position");
        }

        private static void VerifyPendingOrderAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    true),
                "confirmed pending order");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    false,
                    true),
                "rejected pending order");
        }

        private static void VerifyRejectedMutationHandling()
        {
            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    false,
                    true),
                "missing mutation result");

            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    true,
                    false),
                "unsuccessful mutation");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    false),
                "missing confirmed position entity");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    false),
                "missing confirmed pending entity");
        }

        private static void VerifyFillEnvelopeSymmetry()
        {
            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    102,
                    10,
                    0.25),
                "BUY-side fill inside envelope");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    98,
                    10,
                    0.25),
                "SELL-side mirrored fill inside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    103,
                    10,
                    0.25),
                "BUY-side fill outside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    97,
                    10,
                    0.25),
                "SELL-side mirrored fill outside envelope");
        }

        private static void VerifyInitialProtectionDirectionality()
        {
            Assert(
                PriceProtectionRule.ValidateStop(
                    1,
                    100,
                    98,
                    1),
                "BUY initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    102,
                    1),
                "BUY initial target");

            Assert(
                PriceProtectionRule.ValidateStop(
                    -1,
                    100,
                    102,
                    1),
                "SELL initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    98,
                    1),
                "SELL initial target");
        }

        private static void VerifyManagedBreakEvenDirectionality()
        {
            Assert(
                ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    102,
                    1),
                "BUY profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    106,
                    1),
                "BUY stop beyond market");

            Assert(
                ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    98,
                    1),
                "SELL profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    94,
                    1),
                "SELL stop beyond market");
        }

        private static void VerifyProtectionProgression()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101),
                "BUY SL advances upward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100),
                "BUY SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99),
                "SELL SL advances downward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "SELL SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    105,
                    106,
                    true),
                "BUY TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    true),
                "BUY TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    95,
                    94,
                    true),
                "SELL TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    94,
                    95,
                    true),
                "SELL TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    false),
                "TP policy can explicitly allow backward move");
        }

        private static void VerifyTargetProgression()
        {
            Assert(
                TargetProgressionRule.IsValid(
                    1,
                    102,
                    105),
                "BUY target progression");

            Assert(
                TargetProgressionRule.IsValid(
                    -1,
                    98,
                    95),
                "SELL target progression");

            Assert(
                !TargetProgressionRule.IsValid(
                    1,
                    105,
                    102),
                "BUY backward target blocked");

            Assert(
                !TargetProgressionRule.IsValid(
                    -1,
                    95,
                    98),
                "SELL backward target blocked");
        }

        private static void VerifyExecutionCapacity()
        {
            Assert(
                ExecutionCapacityRule.IsSupportedSinglePlanCapacity(1),
                "single-plan capacity accepted");

            Assert(
                !ExecutionCapacityRule.IsSupportedSinglePlanCapacity(0),
                "zero capacity rejected");

            Assert(
                !ExecutionCapacityRule.IsSupportedSinglePlanCapacity(2),
                "multi-position capacity rejected");
        }

        private static void VerifyStaleLivePlanRecovery()
        {
            Assert(
                LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    false),
                "stale live plan clears when broker position disappears");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    true),
                "live plan remains when broker position exists");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    false,
                    false),
                "non-live plan is not cleared by broker absence");
        }

        private static void VerifyLifecycleFlows()
        {
            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Flat,
                    LifecycleState.Signal),
                "signal entry");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Signal,
                    LifecycleState.PlanReady),
                "plan readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PlanReady,
                    LifecycleState.ExecutionReady),
                "execution readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.LivePosition),
                "market fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.PendingOrder),
                "pending placement flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PendingOrder,
                    LifecycleState.LivePosition),
                "pending fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.RecoveryRequired),
                "live recovery flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.RecoveryRequired,
                    LifecycleState.LivePosition),
                "recovery completion flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.ExitRequested),
                "reversal/invalidation exit");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExitRequested,
                    LifecycleState.Closed),
                "end-of-day/exit close");

            Assert(
                !LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Closed,
                    LifecycleState.PendingOrder),
                "closed state cannot create pending order");
        }

        private static void VerifyRuntimeStageIsolation()
        {
            string cyclePath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationCycle.cs");

            string stagesPath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationStageIsolation.cs");

            Assert(
                File.Exists(cyclePath),
                "calculation cycle source exists");

            Assert(
                File.Exists(stagesPath),
                "calculation stage isolation source exists");

            string cycle =
                File.ReadAllText(
                    cyclePath);

            string stages =
                File.ReadAllText(
                    stagesPath);

            foreach (string requiredCall in new[]
            {
                "RunCalculationPreparationStage(",
                "RunClosedBarAnalysisStage(",
                "ProcessLiveCalculationStages("
            })
            {
                Assert(
                    cycle.Contains(requiredCall),
                    "Calculate uses " + requiredCall.Trim('(', ' '));
            }

            Assert(
                !cycle.Contains("ProcessNewClosedBar("),
                "Calculate does not directly own closed-bar analysis");

            Assert(
                !cycle.Contains("ProcessLiveCalculation("),
                "Calculate does not directly own live-cycle orchestration");

            foreach (string requiredStage in new[]
            {
                "BROKER RECONCILIATION • PREFLIGHT",
                "BROKER LIFECYCLE RECOVERY",
                "BROKER RECONCILIATION • POST-RECOVERY",
                "ACTIVE PLAN MANAGEMENT",
                "BROKER PROTECTION • PRE-ANALYSIS",
                "LIVE ANALYSIS",
                "PLAN SYNCHRONIZATION",
                "PLAN CREATION",
                "EXECUTION",
                "BROKER RECONCILIATION • POST-EXECUTION",
                "BROKER PROTECTION • POST-EXECUTION",
                "TELEMETRY",
                "REVERSAL MANAGEMENT",
                "BROKER STATE FINALIZATION",
                "PRESENTATION"
            })
            {
                Assert(
                    stages.Contains(requiredStage),
                    "isolated stage " + requiredStage);
            }

            Assert(
                stages.Contains("HandleRuntimeFault("),
                "stage fault containment");

            Assert(
                stages.Contains("return true;"),
                "stage fault continues to next stage");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • PREFLIGHT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker reconciliation precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER LIFECYCLE RECOVERY",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker recovery precedes live analysis");

            Assert(
                stages.IndexOf(
                    "ACTIVE PLAN MANAGEMENT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "active plan management precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • PRE-ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "pre-analysis protection precedes live analysis");

            Assert(
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal),
                "analysis stage precedes downstream planning");

            Assert(
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "EXECUTION",
                    StringComparison.Ordinal),
                "planning synchronization precedes execution");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal),
                "post-execution reconciliation precedes protection");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "TELEMETRY",
                    StringComparison.Ordinal),
                "post-execution protection precedes telemetry");

            Assert(
                stages.IndexOf(
                    "BROKER STATE FINALIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PRESENTATION",
                    StringComparison.Ordinal),
                "final broker state synchronization precedes presentation");
        }

        private static void VerifyLifecycleIdempotency()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 501),
                "first open event");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 501),
                "duplicate open event");

            Assert(
                guard.TryBegin("PENDING_CREATED", 501),
                "different event type");

            Assert(
                guard.TryBegin("POSITION_OPENED", 502),
                "different entity");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 0),
                "invalid entity");
        }

        private static void VerifyRuntimeFaultStateMachine()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            machine.BeginCycle();
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "initial runtime state healthy");

            Assert(
                machine.CanAutomaticEntryProceed,
                "initial automatic entry armed");

            machine.RecordRecoverableFault();

            Assert(
                machine.State == RuntimeFaultState.Degraded,
                "recoverable fault enters degraded");

            machine.BlockAutomaticEntry();

            Assert(
                machine.State == RuntimeFaultState.EntryBlocked,
                "fault blocks automatic entry");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "entry remains blocked after fault");

            machine.BeginCycle();
            machine.MarkManagementReadyForRecovery();

            Assert(
                machine.State == RuntimeFaultState.Recovering,
                "healthy management enters recovery");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "recovery does not re-arm entry");

            machine.CompleteCycle();

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "clean recovery returns healthy");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "healthy recovery remains disarmed");

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.CanAutomaticEntryProceed,
                "explicit enable transition re-arms entry");

            machine.BeginCycle();
            machine.RecordRecoverableFault();
            machine.BlockAutomaticEntry();

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                !machine.CanAutomaticEntryProceed,
                "enable transition cannot bypass blocked state");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Runtime acceptance contract failed: " +
                    name);
        }
    }
}