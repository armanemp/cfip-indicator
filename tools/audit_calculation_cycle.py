#!/usr/bin/env python3

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REQUIRED = {
    "src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs": [
        "RunPreDecisionBrokerReconciliation(",
        "ProcessWaitingForDataStages(",
        "GetManagementClosedM5Fallback(",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationBrokerBoundary.cs": [
        "_brokerStateReconciledThisCycle",
        "SynchronizeLiveBrokerState();",
        "BROKER RECONCILIATION • PRE-DECISION",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs": [
        "ShouldProbeCalculationReadiness(",
        "CalculationReadinessRule.ResolveState(",
        "RenderCalculationReadinessIfNeeded(",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationReadinessStateStore.cs": [
        "CalculationReadinessState",
        "ShouldProbeCalculationReadiness(",
        "RenderCalculationReadinessIfNeeded(",
        "GetManagementClosedM5Fallback(",
    ],
    "src/CFIP.Indicator/Core/Math/CalculationReadinessRule.cs": [
        "ResolveState(",
        "IsProbeDue(",
        "WaitingForMtfData",
        "WaitingForClosedM5",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs": [
        "ProcessWaitingForDataStages(",
        "RECOVER",
        "NEWS RISK PROTECTION • WAITING",
        "BROKER PROTECTION • WAITING",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs": [
        "ProcessDecisionAlerts(",
        "ProcessParallelOpportunityAlerts(",
    ],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs": [
        "RunPreDecisionBrokerReconciliation(",
    ],
}

errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing required file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


for relative, needles in REQUIRED.items():
    source = read(relative)
    for needle in needles:
        if needle not in source:
            errors.append(f"{relative}: missing {needle}")

cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
prep = read("src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs")
readiness_store = read(
    "src/CFIP.Indicator/Runtime/Calculation/CalculationReadinessStateStore.cs"
)
readiness_rule = read(
    "src/CFIP.Indicator/Core/Math/CalculationReadinessRule.cs"
)
startup = read(
    "src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs"
)

if cycle.index("RunCalculationPreparationStage(") > cycle.index(
    "if (newClosedBar)"
):
    errors.append(
        "Calculate: preparation must occur before the new closed-bar decision branch"
    )

preparation_index = cycle.index(
    "RunCalculationPreparationStage("
)
pre_decision_index = cycle.index(
    "RunPreDecisionBrokerReconciliation("
)
closed_analysis_index = cycle.index(
    "RunClosedBarAnalysisStage("
)

if not (
    preparation_index <
    pre_decision_index <
    closed_analysis_index
):
    errors.append(
        "Calculate: new closed-bar analysis must occur after preparation and same-cycle broker reconciliation"
    )

if cycle.index("RunCalculationPreparationStage(") > cycle.index(
    "ProcessWaitingForDataStages("
):
    errors.append(
        "Calculate: preparation must precede the waiting-state management path"
    )

if "RunClosedBarAnalysisStage(" not in cycle:
    errors.append("Calculate: closed-bar analysis call missing")

if "ProcessLiveCalculationStages(" not in cycle:
    errors.append("Calculate: live stage call missing")

if "ProcessDecisionAlerts(" in alerts:
    if "ProcessParallelOpportunityAlerts(" not in alerts:
        errors.append(
            "decision alert stage must include canonical parallel-opportunity alerting"
        )
    if "!_brokerStateReconciledThisCycle" in alerts:
        errors.append(
            "analysis alerts must not be blocked by broker reconciliation"
        )

if "RunPreDecisionBrokerReconciliation(" not in startup:
    errors.append(
        "startup calculation seed must reconcile broker state before decision analysis"
    )

if "RenderPanel();" in prep:
    errors.append(
        "CalculationPreparation.cs must not render the waiting panel directly per tick"
    )

if "RunCalculationStage" not in stage or "ProcessWaitingForDataStages" not in stage:
    errors.append("waiting-state management must use isolated stage execution")

for forbidden in (
    "TrySmartPendingOrders(",
    "TryAggressiveAutoTrade(",
    "TryAutoTrade(",
    "TryEnsureAutomaticPlan(",
    "EnsureSignalPlan(",
):
    waiting_start = stage.find("private void ProcessWaitingForDataStages(")
    waiting_end = stage.find(
        "private void ProcessLiveCalculationStages(",
        waiting_start,
    )
    if waiting_start >= 0 and waiting_end >= 0:
        waiting = stage[waiting_start:waiting_end]
        if forbidden in waiting:
            errors.append(
                f"waiting-state management must not enter execution/planning stage: {forbidden}"
            )

if "CalculationReadinessRule.ResolveState(" not in prep:
    errors.append("preparation must consume the canonical readiness rule")

if "CalculationReadinessRule.IsProbeDue(" not in readiness_store:
    errors.append("readiness probing must be centrally throttled")

if "WaitingForMtfData" not in readiness_rule or "WaitingForClosedM5" not in readiness_rule:
    errors.append("explicit MTF/closed-M5 waiting states are required")

if errors:
    print("CALCULATION CYCLE AUDIT: FAIL")
    for error in errors:
        print(f"  - {error}")
    raise SystemExit(1)

print("CALCULATION CYCLE AUDIT: PASS")
for relative in REQUIRED:
    print(f"  - {relative}")
