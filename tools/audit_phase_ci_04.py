#!/usr/bin/env python3
"""Static acceptance gate for CI-04 structure/swing/liquidity semantics and unified alert delivery synchronization."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

swing_rule = read("src/CFIP.Indicator/Core/Math/SwingPlateauRule.cs")
struct_rule = read("src/CFIP.Indicator/Core/Math/StructuralEventRule.cs")
struct_evidence = read("src/CFIP.Indicator/Core/Math/StructuralEvidenceRule.cs")
swing_analyzer = read("src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs")
equal_levels = read("src/CFIP.Indicator/Analysis/Structure/EqualLevelAnalyzer.cs")
structure_analyzer = read("src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs")
liquidity_analyzer = read("src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs")
frame_scoring = read("src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs")
struct_confirm = read("src/CFIP.Indicator/Analysis/Market/Decision/StructuralConfirmationAnalyzer.cs")
context_alerts = read("src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs")
alert_engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
alert_event = read("src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs")
alert_queue = read("src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs")
alert_processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
calc_cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

production = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
)

check(
    "canonical swing plateau owner is bounded by the closed index",
    "candidateIndex + strength <= closedIndex" in swing_rule and
    "right + 1 <= closedIndex" in swing_rule and
    "right + strength > closedIndex" in swing_rule
)

check(
    "swing analyzers delegate to the canonical plateau rule",
    "SwingPlateauRule.TryGetHighPlateau(" in swing_analyzer and
    "SwingPlateauRule.TryGetLowPlateau(" in swing_analyzer
)

check(
    "equal-high/low tolerance uses fixed-anchor comparison",
    "Math.Abs" not in equal_levels and
    "SwingPlateauRule.IsWithinAnchor(" in equal_levels and
    "EqualLevelToleranceAtr" in equal_levels
)

check(
    "confirmed structure/MSS breaks cannot reuse an already-broken threshold",
    "IsFreshBreak(" in struct_rule and
    "previouslyBroken" in struct_rule and
    "plateauEnd" in structure_analyzer and
    structure_analyzer.count("StructuralEventRule.IsFreshBreak(") >= 4
)

check(
    "structure/MSS derive their source from canonical confirmed swings",
    "TryFindLatestSwingHigh(" in structure_analyzer and
    "TryFindLatestSwingLow(" in structure_analyzer and
    "confirmationIndex" in structure_analyzer
)

check(
    "liquidity sweeps require an active unbroken structural level",
    "LiquiditySweepRule.IsActiveUnbrokenLevel(" in liquidity_analyzer and
    "TryFindLatestSwingLow(" in liquidity_analyzer and
    "TryFindLatestSwingHigh(" in liquidity_analyzer and
    "confirmationIndex" in liquidity_analyzer
)

check(
    "structural evidence remains one canonical event for one frame",
    "StructuralEvidenceRule.HasCanonicalStructuralEvent(" in frame_scoring and
    "StructuralEvidenceRule.CanonicalEventCount(" in struct_confirm
)

structural_alert_region_start = context_alerts.find(
    "                            bool structuralBull =")
structural_alert_region_end = context_alerts.find(
    "                            if (AlertOnLiquiditySweep &&",
    structural_alert_region_start
)
structural_alert_region = (
    context_alerts[structural_alert_region_start:structural_alert_region_end]
    if structural_alert_region_start >= 0 and structural_alert_region_end >= 0
    else ""
)
check(
    "BOS/MSS/CHOCH are collapsed to one user-facing structural event",
    "StructuralEvidenceRule.HasCanonicalStructuralEvent(" in structural_alert_region and
    structural_alert_region.count("SendUnifiedAlert(") == 1
)

check(
    "audio has one production delivery owner",
    "Notifications.PlaySound(" not in alert_engine and
    production.count("Notifications.PlaySound(") == alert_processor.count("Notifications.PlaySound(") and
    alert_processor.count("Notifications.PlaySound(") == 3
)

check(
    "sound cue is resolved only at the cTrader delivery boundary",
    "SoundTypeName" in alert_event and
    "Enum.TryParse<SoundType>(" in alert_processor and
    "soundType.ToString()" in alert_engine
)

check(
    "sound and panel rail consume the same queued alert event",
    "_alertDeliveryQueue.Enqueue(" in alert_engine and
    "ProcessQueuedAlertDelivery();" in calc_cycle and
    "ProcessQueuedAlertDelivery();" in initialization and
    "RecordPanelAlertDelivery(next)" in alert_processor and
    "Notifications.PlaySound(" in alert_processor
)

check(
    "delivery order updates panel rail before the audible cue",
    "RecordPanelAlertDelivery(next)" in alert_processor and
    alert_processor.index("RecordPanelAlertDelivery(next)") <
    alert_processor.index("Notifications.PlaySound(")
)

check(
    "alert delivery event has a dedicated file owner",
    "internal struct AlertDelivery" in alert_event and
    "internal struct AlertDelivery" not in alert_queue
)

check(
    "alert delivery queue remains bounded and priority-aware",
    "_capacity" in alert_queue and
    "if (delivery.Critical)" in alert_queue and
    "_normal.Dequeue()" in alert_queue
)

check(
    "explicit restriction alerts are no longer accidentally dead",
    'key.StartsWith(\n                                    "RESTRICT|"' in alert_engine and
    'message.StartsWith(\n                                    "CFIP ENTRY BLOCKED"' in alert_engine and
    "restrictionAlert =" in alert_engine and
    "blockedCandidateAlert" in alert_engine and
    "!blockedCandidateAlert" in alert_engine
)

check(
    "runtime state owns the unified queue",
    "AlertDeliveryQueue(16)" in state and
    "_alertDeliveryQueue" in state
)

check(
    "obsolete popup-only delivery owner is removed",
    "PopupAlertQueue" not in production and
    "ProcessQueuedPopups" not in production
)

check(
    "legacy runtime UI audit points to the unified queue",
    "AlertDeliveryQueue.cs" in read("tools/audit_phase_3_4.py") and
    "AlertDeliveryProcessor.cs" in read("tools/audit_phase_3_4.py")
)

check(
    "runtime UI audit points to the unified queue",
    "Core/Runtime/AlertDeliveryQueue.cs" in read("tools/audit_runtime_ui.py") and
    "UI/Panel/AlertDeliveryProcessor.cs" in read("tools/audit_runtime_ui.py")
)

check(
    "deterministic structural regression contracts are wired",
    "StructuralEventRule.IsFreshBreak(" in runtime and
    "bullish re-break" in runtime and
    "bearish re-break" in runtime
)

check(
    "deterministic alert-delivery queue contract is wired",
    "VerifyAlertDeliveryQueueSemantics();" in runtime and
    "AlertDeliveryQueue.cs" in runtime_project and
    "AlertDelivery.cs" in runtime_project
)

check(
    "CI-04 audit is accumulated immediately after CI-03",
    "audit_phase_ci_03.py" in workflow and
    "audit_phase_ci_04.py" in workflow and
    workflow.index("audit_phase_ci_04.py") > workflow.index("audit_phase_ci_03.py")
)

check(
    "CI-04 remains recorded while the CI track advances",
    "CI-04 closeout" in roadmap and
    "CI-04 closeout" in continuation and
    "## 2.0.1 — Current certification state" in roadmap and
    (
        "CI-17A" in roadmap or
        "CI-17" in roadmap
    ) and
    "CI-17" in continuation
)

calculate_start = calc_cycle.rfind("public override void Calculate(")
calculate_body = calc_cycle[calculate_start:] if calculate_start >= 0 else calc_cycle
live_stage = calculate_body.find("ProcessLiveCalculationStages(")
delivery_stage = calculate_body.find("ProcessQueuedAlertDelivery();", live_stage)
completion_stage = calculate_body.rfind("CompleteRuntimeFaultCycle();")

check(
    "Calculate drains alert delivery only after live calculation/presentation",
    live_stage >= 0 and
    delivery_stage >= 0 and
    completion_stage >= 0 and
    live_stage < delivery_stage < completion_stage
)

print("CI-04 STRUCTURE / SWING / LIQUIDITY / ALERT SYNC SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-04 STATIC GATE PASS")
