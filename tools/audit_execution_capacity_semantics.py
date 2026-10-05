from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")

files = sorted(ROOT.rglob("*.cs"))
if not files:
    raise SystemExit("No production C# files found")

sources = {p: p.read_text(encoding="utf-8") for p in files}
production = "\n".join(sources.values())

parameter_root = ROOT / "Indicator" / "Parameters"
parameter_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in sorted(parameter_root.glob("*.cs"))
)

# Execution capacity is cBot-owned. The Indicator must not expose a broker
# execution-capacity parameter or recreate the cBot setting.
if "MaximumOpenPositions" in parameter_source:
    raise SystemExit("MaximumOpenPositions must be absent from Indicator parameters")

if "BlockNewSignalWhileActive" in production:
    raise SystemExit(
        "Unsupported optional active-plan blocking control remains in production source"
    )

rule = ROOT / "Core" / "Math" / "ExecutionCapacityRule.cs"
guard = ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs"
plan_creation = ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs"
if not rule.exists() or not guard.exists() or not plan_creation.exists():
    raise SystemExit("Single-plan capacity owner files are missing")

rule_text = rule.read_text(encoding="utf-8")
guard_text = guard.read_text(encoding="utf-8")
plan_text = plan_creation.read_text(encoding="utf-8")

for required in (
    "SupportedMaximumOpenPositions",
    "IsSupportedSinglePlanCapacity(",
    "AllowsNewSinglePlan(",
    "AllowsNewSingleExecution(",
):
    if required not in rule_text:
        raise SystemExit(f"Canonical capacity rule is missing: {required}")

for required in (
    "ValidateSinglePlanCapacity(",
    "ValidateSingleExecutionCapacity(",
    "ExecutionCapacityRule.AllowsNewSinglePlan(",
    "ExecutionCapacityRule.AllowsNewSingleExecution(",
    "ManagedPositionCount()",
    "ManagedPendingOrderCount()",
):
    if required not in guard_text:
        raise SystemExit(f"Canonical capacity guard is missing: {required}")

if "ValidateSinglePlanCapacity(" not in plan_text:
    raise SystemExit(
        "Plan creation must consume the canonical single-plan capacity guard"
    )

for removed_path in (
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs",
    ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs",
):
    if removed_path.exists():
        raise SystemExit(f"Removed Indicator execution path was reintroduced: {removed_path}")

pending_path = ROOT / "Trading" / "Pending" / "Placement" / "SmartPendingOrderOrchestrator.cs"
if "ValidateSingleExecutionCapacity(" not in pending_path.read_text(encoding="utf-8"):
    raise SystemExit("Pending intent path does not consume the shared execution-capacity guard")

print("Phase 7.4 execution-capacity semantics audit PASS")
print(f"Production C# files scanned: {len(files)}")
print("MaximumOpenPositions: cBot-owned; absent from Indicator")
print("Unsupported BlockNewSignalWhileActive parameter: absent")
print("Single-plan capacity rule: PASS")
print("New-plan and new-execution semantics: PASS")
print("Shared cTrader capacity guard: PASS")
print("Indicator execution paths removed; pending intent capacity remains guarded: PASS")
