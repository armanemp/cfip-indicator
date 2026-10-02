from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src" / "CFIP.Indicator"
WORKFLOW = ROOT / ".github" / "workflows" / "source-check.yml"
CS_PROJ = ROOT / "tools" / "CFIP.Planning.Contracts" / "CFIP.Planning.Contracts.csproj"
CONTRACTS = ROOT / "tools" / "CFIP.Planning.Contracts" / "Program.cs"

errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    if not condition:
        errors.append(name)


canonical = read("src/CFIP.Indicator/Core/Math/RiskRewardMathRule.cs")
plan_quality = read("src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs")
execution_geometry = read("src/CFIP.Indicator/Core/Math/ExecutionPlanGeometryRule.cs")
candidate_constraints = read("src/CFIP.Indicator/Core/Math/TargetCandidateConstraintRule.cs")
live_exit = read("src/CFIP.Indicator/Core/Math/LiveExitGeometryRule.cs")
target_envelope = read("src/CFIP.Indicator/Core/Math/TargetRewardEnvelopeRule.cs")
target_preparation = read("src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs")
plan_reward = read("src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs")
actionability = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
auto_pretrade = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs")
auto_submission = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs")
pending_submission = read("src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs")
aggressive = read("src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs")
parallel = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
tactical = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs")
factory = read("src/CFIP.Indicator/Trading/Lifecycle/LivePlanFactory.cs")
pending_snapshot = read("src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs")
live_recalc = read("src/CFIP.Indicator/Trading/LiveManagement/PlanRiskRewardRecalculator.cs")
live_target = read("src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs")
live_further = read("src/CFIP.Indicator/Trading/Lifecycle/LivePlanFurtherTargetSelector.cs")
panel_live = read("src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLiveRowsRenderer.cs")
panel_heartbeat = read("src/CFIP.Indicator/Runtime/Supervision/PanelHeartbeatLiveState.cs")
signal_trace = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs")
structural_stop = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
execution_prep = read("src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs")
fill_reconciler = read("src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs")

check(
    "canonical RR owner exposes risk/reward/effective-RR fields",
    all(token in canonical for token in (
        "RiskRewardMathResult",
        "Evaluate(",
        "EvaluateFromRisk(",
        "NominalRRFromDistance(",
        "DirectionalProgressRR(",
        "TargetFromRR(",
        "NormalizeMaximumRR(",
    )),
)
check(
    "canonical RR owner exposes shared risk-from-levels helper",
    "RiskFromLevels(" in canonical and
    "Math.Max(floor, risk)" in canonical,
)

check(
    "canonical RR owner normalizes risk by the supplied physical floor",
    "Math.Max(" in canonical and "riskFloor" in canonical,
)
check(
    "canonical RR owner accounts for spread only in effective risk",
    "double safeSpread" in canonical and
    "effectiveRisk =" in canonical and
    "risk + safeSpread" in canonical,
)
check(
    "canonical RR owner normalizes maximum RR against minimum RR",
    "NormalizeMaximumRR(" in canonical and
    "maximumRR" in canonical and
    "minimumRR" in canonical,
)

check(
    "PlanRewardRiskQualityRule consumes canonical RR geometry",
    "RiskRewardMathRule.Evaluate(" in plan_quality,
)
check(
    "PlanRewardRiskQualityRule exposes canonical risk/reward values",
    all(token in plan_quality for token in (
        "public double Risk",
        "public double Reward",
        "public double EffectiveRisk",
        "public double NominalRR",
        "public double EffectiveRR",
        "public double RequiredRR",
        "public double MaximumRR",
    )),
)
check(
    "ExecutionPlanGeometryRule is only a canonical adapter",
    "RiskRewardMathRule.Evaluate(" in execution_geometry and
    "double rr =" not in execution_geometry and
    "Math.Abs(entry - stop)" not in execution_geometry,
)
check(
    "LiveExitGeometryRule consumes canonical RR math",
    "RiskRewardMathRule.EvaluateFromRisk(" in live_exit and
    "entryDistance / risk" not in live_exit,
)
check(
    "TargetCandidateConstraintRule consumes canonical RR math",
    "RiskRewardMathRule.EvaluateFromRisk(" in candidate_constraints and
    "distance /" not in candidate_constraints,
)
check(
    "TargetRewardEnvelopeRule consumes canonical distance RR",
    "RiskRewardMathRule.NominalRRFromDistance(" in target_envelope and
    "extension /" not in target_envelope,
)

check(
    "plan target preparation uses canonical RR",
    "RiskRewardMathRule.EvaluateFromRisk(" in target_preparation and
    "Math.Abs(\n                        tp1" not in target_preparation,
)
check(
    "plan reward integrity uses canonical stage RR",
    plan_reward.count("RiskRewardMathRule.Evaluate(") >= 4 and
    "plan.Tp1 -\n                    plan.Entry) /" not in plan_reward and
    "plan.Tp2 -\n                        plan.Entry) /" not in plan_reward and
    "plan.Tp3 -\n                        plan.Entry) /" not in plan_reward and
    "plan.Tp4 -\n                        plan.Entry) /" not in plan_reward,
)

for name, source in (
    ("actionability", actionability),
    ("automatic market submission", auto_submission),
    ("pending submission", pending_submission),
    ("aggressive execution", aggressive),
    ("parallel opportunity", parallel),
    ("signal trace", signal_trace),
):
    check(
        f"{name} reward-risk consumer receives canonical max/floor",
        "PlanRewardRiskQualityRule.Evaluate(" in source and
        "MaximumRewardRR" in source and
        "Symbol.PipSize" in source,
    )

check(
    "actionability no longer rebuilds TP1 RR locally",
    "rewardRisk.NominalRR" in actionability and
    "Math.Abs(\n                        preview.Tp1" not in actionability and
    "Math.Abs(\n                    actualEntry" not in actionability,
)
check(
    "automatic market pre-trade passes spread, min and max into geometry adapter",
    "ExecutionPlanGeometryRule.Evaluate(" in auto_pretrade and
    "priceSnapshot.Spread" in auto_pretrade and
    "MaximumRewardRR" in auto_pretrade and
    "Symbol.PipSize" in auto_pretrade,
)

check(
    "live plan factory derives stored risk and TP1 RR canonically",
    "RiskRewardMathRule.Evaluate(" in factory and
    "Risk = geometry.Risk" in factory and
    "Tp1RR = geometry.NominalRR" in factory,
)
check(
    "pending snapshot derives risk and RR canonically",
    "RiskRewardMathRule.Evaluate(" in pending_snapshot and
    "Math.Abs(tp1 - entry) / risk" not in pending_snapshot and
    "Math.Abs(tp2 - entry) / risk" not in pending_snapshot,
)
check(
    "live RR recalculation is canonical",
    "RiskRewardMathRule.Evaluate(" in live_recalc and
    "Math.Abs(" not in live_recalc,
)
check(
    "live target candidate RR is canonical",
    (
        "RiskRewardMathRule.EvaluateFromRisk(" in live_target or
        "TargetCandidateRewardScoreRule.Calculate(" in live_target
    ) and
    "Math.Abs(" not in live_target,
)
check(
    "further live target selection RR is canonical",
    "RiskRewardMathRule.EvaluateFromRisk(" in live_further and
    "Math.Abs(" not in live_further,
)
check(
    "tactical opportunity RR is canonical",
    "RiskRewardMathRule.EvaluateFromRisk(" in tactical and
    "Math.Abs(\n                    bestTarget" not in tactical,
)
check(
    "parallel opportunity RR fields are canonical",
    "CalculatePreviewStageRR(" in parallel and
    "Math.Abs(\n                    preview.Tp1" not in parallel,
)
check(
    "panel LIVE RR uses canonical directional progress",
    "RiskRewardMathRule.DirectionalProgressRR(" in panel_live and
    "_plan.Direction == 1" not in panel_live,
)
check(
    "panel heartbeat LIVE RR uses canonical directional progress",
    "RiskRewardMathRule.DirectionalProgressRR(" in panel_heartbeat and
    "_plan.Direction == 1" not in panel_heartbeat,
)
check(
    "synthetic execution targets use canonical TargetFromRR",
    "RiskRewardMathRule.TargetFromRR(" in execution_prep,
)
check(
    "fill reconciliation uses canonical risk floor",
    "RiskRewardMathRule.Evaluate(" in fill_reconciler,
)
check(
    "structural stop RR evaluation receives canonical max/floor",
    "PlanRewardRiskQualityRule.Evaluate(" in structural_stop and
    "MaximumRewardRR" in structural_stop and
    "Symbol.PipSize" in structural_stop,
)

workflow = WORKFLOW.read_text(encoding="utf-8")
check(
    "CI-14 audit is wired into accumulated Source/Architecture gate",
    "python tools/audit_phase_ci_14.py" in workflow,
)
check(
    "Planning contracts compile the canonical RR owner",
    "RiskRewardMathRule.cs" in CS_PROJ.read_text(encoding="utf-8"),
)
check(
    "Planning contracts execute CI-14 canonical fixtures",
    "VerifyCanonicalRiskRewardMath()" in CONTRACTS.read_text(encoding="utf-8") and
    "CI-14 BUY/SELL RR mirror" in CONTRACTS.read_text(encoding="utf-8"),
)

# Guard the most important semantic duplication patterns in the production
# consumers audited by CI-14. The canonical owner itself is intentionally
# excluded from these checks.
for relative, source, forbidden in (
    (
        "TargetCandidateConstraintRule",
        candidate_constraints,
        [
            "Math.Abs(target - entry)",
            "distance /",
        ],
    ),
    (
        "PlanRewardIntegrityValidator",
        plan_reward,
        [
            "plan.Tp1 -\n                    plan.Entry) /",
            "plan.Tp2 -\n                        plan.Entry) /",
            "plan.Tp3 -\n                        plan.Entry) /",
            "plan.Tp4 -\n                        plan.Entry) /",
        ],
    ),
    (
        "TradeActionabilityEvaluator",
        actionability,
        [
            "Math.Abs(\n                    actualEntry -\n                    preview.Stop)",
            "Math.Abs(\n                        preview.Tp1 -",
        ],
    ),
    (
        "LivePlanFurtherTargetSelector",
        live_further,
        [
            "Math.Abs(\n                                        level.Price -",
        ],
    ),
):
    for pattern in forbidden:
        check(
            f"{relative} still contains duplicated RR formula: {pattern}",
            pattern not in source,
        )

if errors:
    for error in errors:
        print(f"[FAIL] {error}")
    raise SystemExit(1)

print(
    "CI-14 canonical risk/reward audit PASS: "
    "math owner, candidate/plan/action/execution consumers, live lifecycle, "
    "panel display and accumulated gate wiring are consistent"
)