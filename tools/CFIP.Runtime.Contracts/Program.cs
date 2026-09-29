using System;
using System.IO;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyMtfContextIntegrity();
            VerifyClosedBarReferenceContract();
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
            VerifyClosedBarRetryPolicy();
            VerifyUnifiedSubmissionGate();
            VerifyVisualAndExecutionControls();
            VerifyResponsivePanelRuntime();
            VerifyAggressiveEntryPolicy();

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

        private static void VerifyClosedBarReferenceContract()
        {
            DateTime[] opens =
            {
                Utc(12, 0),
                Utc(12, 5),
                Utc(12, 15),
                Utc(12, 20)
            };

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 4),
                    index => opens[index]) == -1,
                "no closed bar before first boundary");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 5),
                    index => opens[index]) == 0,
                "exact boundary closes prior bar");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 14),
                    index => opens[index]) == 0,
                "between boundaries keeps prior bar closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 15),
                    index => opens[index]) == 1,
                "gap boundary uses next actual open time");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 21),
                    index => opens[index]) == 2,
                "latest available fully closed bar");

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    1,
                    Utc(12, 15),
                    index => opens[index]),
                "resolved bar is fully closed at reference");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    2,
                    Utc(12, 19),
                    index => opens[index]),
                "future bar cannot be considered closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 30),
                    index => opens[index]) == 2,
                "reference after last open is bounded to last closed bar");
        }

        private static DateTime Utc(int hour, int minute)
        {
            return new DateTime(
                2026,
                1,
                1,
                hour,
                minute,
                0,
                DateTimeKind.Utc);
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

        private static void VerifyClosedBarRetryPolicy()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            string reason;

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t,
                    out reason),
                "first closed-bar attempt allowed");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t);

            Assert(
                machine.ClosedBarFailureCount == 1,
                "first closed-bar failure counted");

            Assert(
                machine.ClosedBarLastFailureUtc == t,
                "failure timestamp recorded");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddMilliseconds(500),
                    out reason),
                "first closed-bar retry is backed off");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(1),
                    out reason),
                "first closed-bar retry becomes eligible");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(1));

            Assert(
                machine.ClosedBarFailureCount == 2,
                "second closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(2),
                    out reason),
                "second retry uses longer backoff");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(2),
                    out reason),
                "new closed bar is not blocked by prior bar failure");

            Assert(
                machine.HasStaleClosedBarFailure(102),
                "prior closed-bar failure is detectable as stale");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(3));

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(7));

            Assert(
                machine.ClosedBarFailureCount == 4,
                "fourth closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(8),
                    out reason),
                "repeated closed-bar failure opens retry circuit");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(8),
                    out reason),
                "new closed bar bypasses stale retry circuit");

            machine.RecordClosedBarAnalysisSuccess(101);

            Assert(
                machine.ClosedBarFailureCount == 0,
                "successful retry clears failure state");

            Assert(
                machine.ClosedBarLastFailureUtc == DateTime.MinValue,
                "successful retry clears failure timestamp");
        }

        private static void VerifyUnifiedSubmissionGate()
        {
            SubmissionGate gate =
                new SubmissionGate();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            SubmissionAttemptIdentity firstSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "AutomaticMarket|100|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity secondSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|101|1",
                    "AutomaticMarket|101|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity otherPath =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "PendingStop|100|1",
                    ExecutionSubmissionPath.PendingStop);

            string reason;

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t,
                    out reason),
                "first submission attempt allowed");

            gate.Record(
                firstSignal,
                t,
                false);

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "failed signal enters backoff");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "different signal remains independently eligible");

            Assert(
                gate.TryAcquire(
                    otherPath,
                    t.AddMilliseconds(500),
                    out reason),
                "different execution path remains independently eligible");

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(1),
                    out reason),
                "first retry becomes eligible after backoff");

            for (int i = 0; i < 3; i++)
            {
                gate.Record(
                    firstSignal,
                    t.AddSeconds(2 + i * 2),
                    false);
            }

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(10),
                    out reason),
                "repeated same-attempt failures open circuit");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddSeconds(10),
                    out reason),
                "open circuit is isolated to the failing attempt");

            gate.Record(
                firstSignal,
                t.AddSeconds(70),
                true);

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(70),
                    out reason),
                "successful submission clears retry state");

            Assert(
                firstSignal.CanonicalKey !=
                secondSignal.CanonicalKey &&
                firstSignal.CanonicalKey !=
                otherPath.CanonicalKey,
                "submission identities remain distinct");
        }
        private static void VerifyVisualAndExecutionControls()
        {
            string snapshotPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "SignalVisualSnapshot.cs");
            string previewPath = Path.Combine("src", "CFIP.Indicator", "Core", "Models", "TradeSetupPreview.cs");
            string previewBuilderPath = Path.Combine("src", "CFIP.Indicator", "Planning", "TradePlan", "PlanPreviewBuilder.cs");
            string calculationPath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationLiveCycle.cs");
            string calculationStagePath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationStageIsolation.cs");
            string rendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanRenderCoordinator.cs");
            string pendingRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PendingOrderRenderer.cs");
            string alertRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "AlertSignalRenderer.cs");
            string alertEnginePath = Path.Combine("src", "CFIP.Indicator", "Trading", "Alerts", "AlertEngine.cs");
            string statePath = Path.Combine("src", "CFIP.Indicator", "Indicator", "State.cs");
            string predictiveSelectorPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingLevelSelector.cs");
            string reversalLimitPath = Path.Combine("src", "CFIP.Indicator", "Trading", "Pending", "Placement", "ReversalLimitPreparation.cs");
            string controlFactoryPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionControlsFactory.cs");
            string controlHandlersPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionToggleHandlers.cs");

            Assert(
                File.Exists(snapshotPath) &&
                File.Exists(previewPath) &&
                File.Exists(previewBuilderPath) &&
                File.Exists(calculationPath) &&
                File.Exists(calculationStagePath) &&
                File.Exists(rendererPath) &&
                File.Exists(pendingRendererPath) &&
                File.Exists(alertRendererPath) &&
                File.Exists(alertEnginePath) &&
                File.Exists(statePath) &&
                File.Exists(predictiveSelectorPath) &&
                File.Exists(reversalLimitPath) &&
                File.Exists(controlFactoryPath) &&
                File.Exists(controlHandlersPath),
                "visual/control sources exist");

            string snapshot = File.ReadAllText(snapshotPath);
            string preview = File.ReadAllText(previewPath);
            string previewBuilder = File.ReadAllText(previewBuilderPath);
            string calculation = File.ReadAllText(calculationPath);
            string calculationStage = File.ReadAllText(calculationStagePath);
            string renderer = File.ReadAllText(rendererPath);
            string pendingRenderer = File.ReadAllText(pendingRendererPath);
            string alertRenderer = File.ReadAllText(alertRendererPath);
            string alertEngine = File.ReadAllText(alertEnginePath);
            string state = File.ReadAllText(statePath);
            string predictiveSelector = File.ReadAllText(predictiveSelectorPath);
            string reversalLimit = File.ReadAllText(reversalLimitPath);
            string controlFactory = File.ReadAllText(controlFactoryPath);
            string controlHandlers = File.ReadAllText(controlHandlersPath);

            Assert(
                snapshot.Contains("SetupPreviewActive") &&
                snapshot.Contains("SetupEntry") &&
                snapshot.Contains("SetupStop") &&
                snapshot.Contains("SetupTp1"),
                "canonical snapshot carries setup levels");

            Assert(
                preview.Contains("never consumed by broker submission"),
                "visual preview is non-executable");

            Assert(
                previewBuilder.Contains("BuildStructuralStop(") &&
                previewBuilder.Contains("BuildTargetLevels(") &&
                previewBuilder.Contains("SelectTargets("),
                "visual preview reuses planning authorities");

            Assert(
                calculation.Contains("RenderSetupPreview("),
                "calculation renders setup preview");

            Assert(
                renderer.Contains("RenderLevelLines("),
                "plan renderer owns shared level rendering");

            Assert(
                controlFactory.Contains("_autoTradingQuickToggle.Click +=") &&
                controlFactory.Contains("_automaticOrdersQuickToggle.Click +=") &&
                !controlFactory.Contains("_autoTradingQuickToggle.Checked +=") &&
                !controlFactory.Contains("_automaticOrdersQuickToggle.Checked +=") &&
                !controlFactory.Contains("_autoTradingQuickToggle.Unchecked +=") &&
                !controlFactory.Contains("_automaticOrdersQuickToggle.Unchecked +="),
                "execution toggles use direct click authority");

            Assert(
                controlHandlers.Contains("ApplyAutoTradingQuickToggleClick(") &&
                controlHandlers.Contains("ApplyAutomaticOrdersQuickToggleClick(") &&
                controlHandlers.Contains("SetAutoTradingRuntimeState(") &&
                controlHandlers.Contains("SetAutomaticOrdersRuntimeState("),
                "toggle handlers use runtime state authority");

            int pendingExecution =
                calculationStage.IndexOf(
                    "PREDICTIVE PENDING EXECUTION",
                    StringComparison.Ordinal);
            int aggressiveExecution =
                calculationStage.IndexOf(
                    "AGGRESSIVE AUTO EXECUTION",
                    StringComparison.Ordinal);
            int planCreation =
                calculationStage.IndexOf(
                    "PLAN CREATION",
                    StringComparison.Ordinal);
            int marketExecution =
                calculationStage.IndexOf(
                    "AUTOMATIC MARKET EXECUTION",
                    StringComparison.Ordinal);

            Assert(
                pendingExecution >= 0 &&
                aggressiveExecution > pendingExecution &&
                planCreation > aggressiveExecution &&
                marketExecution > planCreation,
                "execution priority is predictive pending -> aggressive -> plan -> market");

            Assert(
                calculationStage.Contains("TrySmartPendingOrders(") &&
                calculationStage.Contains("TryAggressiveAutoTrade(") &&
                calculationStage.Contains("TryAutoTrade("),
                "all automatic execution paths remain connected");

            Assert(
                calculation.Contains("TryEnsureAutomaticPlan(") &&
                calculation.Contains("RenderPredictionObjects(") &&
                calculation.Contains("RenderLatestAlertSignalMarker("),
                "live cycle retains plan plus prediction/alert presentation orchestration");

            Assert(
                predictiveSelector.Contains("TrySelectPredictivePendingLevel(") &&
                predictiveSelector.Contains("BuildManagedFvgZone(") &&
                predictiveSelector.Contains("BuildOrderBlockCandidate(") &&
                predictiveSelector.Contains("FindEqualLow(") &&
                predictiveSelector.Contains("FindEqualHigh("),
                "predictive pending selector combines structural level sources");

            Assert(
                reversalLimit.Contains("TrySelectPredictivePendingLevel(") &&
                reversalLimit.Contains("candidate.Source") &&
                reversalLimit.Contains("candidate.DistanceAtr"),
                "reversal limit consumes predictive candidate evidence");

            Assert(
                pendingRenderer.Contains("RenderCompactPlanLabel(") &&
                pendingRenderer.Contains("RemovePlanLabel("),
                "pending levels render through compact semantic boxes");

            Assert(
                alertEngine.Contains("RememberVisualSignalAlert(") &&
                state.Contains("_lastVisualAlertM5") &&
                state.Contains("_lastVisualAlertDirection") &&
                alertRenderer.Contains("RenderLatestAlertSignalMarker("),
                "audible signal alert has a non-authoritative visual presentation path");

            string protectionPath = Path.Combine(
                "src", "CFIP.Indicator", "Trading", "LiveManagement", "ProtectionManager.cs");
            string protection = File.ReadAllText(protectionPath);
            Assert(
                !protection.Contains("pressureStop") &&
                !protection.Contains("market - tightRoom") &&
                !protection.Contains("market + tightRoom") &&
                protection.Contains("pressureTighten"),
                "smart trailing remains structural under exit pressure");
        }

        private static void VerifyResponsivePanelRuntime()
        {
            string heartbeatPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Supervision", "RuntimePanelHeartbeat.cs");
            string initPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Initialization", "RuntimeInitialization.cs");
            string panelPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelMainRenderer.cs");
            string rowsPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowsFactory.cs");
            string writerPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowWriter.cs");

            Assert(
                File.Exists(heartbeatPath) &&
                File.Exists(initPath) &&
                File.Exists(panelPath) &&
                File.Exists(rowsPath) &&
                File.Exists(writerPath),
                "responsive panel sources exist");

            string heartbeat = File.ReadAllText(heartbeatPath);
            string init = File.ReadAllText(initPath);
            string panel = File.ReadAllText(panelPath);
            string rows = File.ReadAllText(rowsPath);
            string writer = File.ReadAllText(writerPath);

            Assert(
                heartbeat.Contains("RenderPanel();"),
                "heartbeat refreshes full panel");

            Assert(
                heartbeat.Contains("TotalMilliseconds >= 1000"),
                "safety supervisor remains one-second bounded");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(500)"),
                "ready timer uses responsive panel cadence");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(250)"),
                "initialization polling avoids 100ms timer churn");

            Assert(
                init.Contains("_status =") &&
                init.Contains("RenderPanel();"),
                "initialization status reaches the panel");

            Assert(
                panel.Contains("BuildSignalVisualSnapshot("),
                "panel builds at most one canonical visual snapshot per refresh path");

            Assert(
                rows.Contains("EnsurePanelRow("),
                "panel row allocation is lazy");

            Assert(
                writer.Contains("if (row.Text != nextText)"),
                "panel writer skips duplicate text writes");
        }

        private static void VerifyAggressiveEntryPolicy()
        {
            AggressiveEntryPolicy policy =
                new AggressiveEntryPolicy();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "first intrabar qualification sample does not arm");

            Assert(
                policy.QualifyingSamples == 1 &&
                policy.Direction == 1 &&
                policy.ReactionM5 == 200,
                "first qualifying sample is tracked");

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "duplicate reaction sample does not count twice");

            Assert(
                policy.QualifyingSamples == 1,
                "duplicate sample count remains stable");

            Assert(
                policy.ObserveReactionSample(
                    200,
                    1,
                    true,
                    t.AddMilliseconds(800)),
                "second qualifying intrabar sample arms");

            Assert(
                policy.IsQualified &&
                policy.GetQualificationStateText() == "ARMED",
                "policy reaches armed state");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    true,
                    t.AddMilliseconds(1600)) &&
                !policy.IsQualified &&
                policy.Direction == -1 &&
                policy.QualifyingSamples == 1,
                "direction change invalidates prior qualification");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    false,
                    t.AddMilliseconds(2400)) &&
                !policy.IsQualified &&
                policy.QualifyingSamples == 0,
                "lost reaction qualification invalidates immediately");

            Assert(
                !policy.ObserveReactionSample(
                    201,
                    1,
                    true,
                    t.AddMilliseconds(3200)) &&
                policy.ReactionM5 == 201 &&
                policy.Direction == 1 &&
                policy.QualifyingSamples == 1,
                "new M5 starts a fresh qualification window");
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