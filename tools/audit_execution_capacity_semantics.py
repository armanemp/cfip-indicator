from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")

files = sorted(ROOT.rglob("*.cs"))
if not files:
    raise SystemExit("No production C# files found")

sources = {p: p.read_text(encoding="utf-8") for p in files}
production = "\n".join(sources.values())

# CFIP is single-plan until a real multi-plan lifecycle exists. A public numeric
# capacity setting must not advertise unsupported behaviour.
if "MaximumOpenPositions" in production:
    raise SystemExit("Unsupported multi-position setting remains in production source")

rule = ROOT / "Core" / "Math" / "ExecutionCapacityRule.cs"
guard = ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs"
if not rule.exists() or not guard.exists():
    raise SystemExit("Single-plan capacity owner files are missing")

rule_text = rule.read_text(encoding="utf-8")
guard_text = guard.read_text(encoding="utf-8")
if "AllowsNewSinglePlan(" not in rule_text:
    raise SystemExit("Single-plan capacity rule is missing")
if "return !hasManagedOpenPosition;" not in rule_text:
    raise SystemExit("Single-plan capacity rule must fail closed on an active managed position")
if "ValidateSinglePlanCapacity(" not in guard_text:
    raise SystemExit("Shared single-plan capacity guard is missing")
if "HasManagedOpenPosition()" not in guard_text:
    raise SystemExit("Capacity guard must use the managed-position authority")
if "ExecutionCapacityRule.AllowsNewSinglePlan(" not in guard_text:
    raise SystemExit("Capacity guard must consume the canonical capacity rule")

execution_paths = {
    "automatic_market": ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs",
    "aggressive": ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs",
    "predictive_pending": ROOT / "Trading" / "Pending" / "Placement" / "SmartPendingOrderOrchestrator.cs",
}
for name, path in execution_paths.items():
    text = path.read_text(encoding="utf-8")
    if "ValidateSinglePlanCapacity(" not in text:
        raise SystemExit(f"{name} execution path does not consume the shared single-plan capacity guard")

for pattern in (
    r"Math\.Max\(\s*1\s*,\s*MaximumOpenPositions",
    r"ManagedPositionCount\(\).*MaximumOpenPositions",
):
    if re.search(pattern, production, re.S):
        raise SystemExit("Legacy numeric maximum-position gate remains in production source")

plan_creation = ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs"
plan_text = plan_creation.read_text(encoding="utf-8")
for required in ("BlockNewSignalWhileActive", "_plan != null", "_decision.TriggerReady"):
    if required not in plan_text:
        raise SystemExit(f"Plan lifecycle guard lost required invariant: {required}")

print("Phase 7.4 execution-capacity semantics audit PASS")
print(f"Production C# files scanned: {len(files)}")
print("Public configurable maximum-position parameter: absent")
print("Single-plan capacity rule: PASS")
print("Shared execution capacity guard: PASS")
print("Automatic market / aggressive / predictive-pending consumers: PASS")
print("Plan lifecycle gate remains independent: PASS")
