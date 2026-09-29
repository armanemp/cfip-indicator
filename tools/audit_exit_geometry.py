#!/usr/bin/env python3
"""Audit Phase 9.17 live exit geometry, TP progression and protection safety."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

ERRORS = []


def read(path: Path) -> str:
    if not path.exists():
        ERRORS.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")


def require(path: Path, *tokens: str) -> None:
    source = read(path)
    for token in tokens:
        if token not in source:
            ERRORS.append(f"{path.name} missing: {token}")


LIVE_GEOMETRY = ROOT / "src/CFIP.Indicator/Core/Math/LiveExitGeometryRule.cs"
FILL_RECONCILER = ROOT / "src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs"
FILL_RECONCILIATION = ROOT / "src/CFIP.Indicator/Trading/Lifecycle/LiveFillReconciliation.cs"
TARGET_PROGRESS = ROOT / "src/CFIP.Indicator/Trading/LiveManagement/TargetProgression.cs"
TARGET_EVALUATOR = ROOT / "src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs"
FURTHER_TARGET = ROOT / "src/CFIP.Indicator/Trading/Lifecycle/LivePlanFurtherTargetSelector.cs"
TARGET_ENRICH = ROOT / "src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs"
SERVER_LADDER = ROOT / "src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs"
LEVEL_HITS = ROOT / "src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLevelExitHandler.cs"
BROKER_PROTECTION = ROOT / "src/CFIP.Indicator/Trading/Execution/BrokerProtectionCoordinator.cs"
BOUND_PROTECTION = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs"
PROTECTION = ROOT / "src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs"
DECISION_CONTRACTS = ROOT / "tools/CFIP.Decision.Contracts/Program.cs"
DECISION_PROJECT = ROOT / "tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj"

require(
    LIVE_GEOMETRY,
    "ValidateLiveTarget(",
    "TARGET BEHIND MARKET",
    "TARGET WRONG SIDE",
    "ShouldAdvanceLiveTarget(",
    "IsProgressiveTargetLadder(",
    "IsProtectiveStop(",
)

require(
    FILL_RECONCILER,
    "ReconcileLiveFillExitGeometry(",
    "SelectLiveFillTarget(",
    "LiveExitGeometryRule.IsProgressiveTargetLadder(",
)

fill_text = read(FILL_RECONCILIATION)
require(
    FILL_RECONCILIATION,
    "ReconcileLiveFillExitGeometry(",
    "LIVE FILL • EXIT GEOMETRY RECONCILIATION FAILED",
)
if "RebuildSmartExecutionLevels(" in fill_text:
    ERRORS.append(
        "LiveFillReconciliation must not fall back to the non-live-aware smart-level rebuild"
    )

require(
    TARGET_PROGRESS,
    "UpdateUnhitTargetsLive(",
    "TryAdvanceServerSideTakeProfitLadder(",
)
if re.search(
    r"UpdateUnhitTargetsLives*([sS]*?_serverSideTakeProfitLadderActive[sS]*?return;",
    read(TARGET_PROGRESS),
):
    ERRORS.append(
        "target progression must not stop merely because the server-side TP ladder is active"
    )

require(
    TARGET_EVALUATOR,
    "LiveExitGeometryRule.ShouldAdvanceLiveTarget(",
    "market",
)

require(
    FURTHER_TARGET,
    "LiveExitGeometryRule.ShouldAdvanceLiveTarget(",
    "current",
    "market",
)

require(
    TARGET_ENRICH,
    "existingTp2",
    "existingTp3",
    "existingTp4",
    "LiveExitGeometryRule.ShouldAdvanceLiveTarget(",
    "market",
)

require(
    SERVER_LADDER,
    "TryAdvanceServerSideTakeProfitLadderAfterTp1(",
    "TryAdvanceServerSideTakeProfitLadder(",
    "TryCollapseServerSideTakeProfitLadderToFinal(",
    "RelativeTakeProfitProtections(",
    "TryModifyTakeProfitLadder(",
)
server_text = read(SERVER_LADDER)
if "_tp1Hit != 0" in server_text and "TryAdvanceServerSideTakeProfitLadder(" in server_text:
    pass
if "position.VolumeInUnits" not in server_text:
    ERRORS.append(
        "server TP ladder progression must account for remaining broker position volume"
    )

require(
    LEVEL_HITS,
    "ObserveServerSidePartialTakeProfits(",
    "closedM5",
    "market",
)

require(
    BROKER_PROTECTION,
    "ResolveLiveProtectionTarget(",
    "LiveExitGeometryRule.ShouldAdvanceLiveTarget(",
    "NormalizePrice(effectiveTarget)",
)
require(
    BOUND_PROTECTION,
    "ResolveLiveProtectionTarget(",
    "LiveExitGeometryRule.ShouldAdvanceLiveTarget(",
    "liveTarget",
)
require(
    PROTECTION,
    "LiveExitGeometryRule.IsProtectiveStop(",
    "IsValidManagedStop(",
)

require(
    DECISION_CONTRACTS,
    "VerifyLiveExitGeometry();",
    "BUY target behind market rejected",
    "SELL target behind market rejected",
    "BUY/SELL protective stop symmetry",
)
require(
    DECISION_PROJECT,
    "LiveExitGeometryRule.cs",
)

# Live TP broker mutation must remain behind the coordinator/BoundPlan guards.
production_files = list((ROOT).rglob("*.cs"))
direct_tp_mutations = []
for path in production_files:
    source = read(path)
    if "TryModifyTakeProfit(" in source and path.name != "BrokerTakeProfitMutation.cs":
        direct_tp_mutations.append(str(path.relative_to(ROOT)))
allowed = {
    "Trading/Execution/BrokerProtectionCoordinator.cs",
    "Trading/Execution/Aggressive/BoundPlanProtection.cs",
    "Trading/Execution/ServerSideTakeProfitLadder.cs",
}
unexpected = sorted(set(direct_tp_mutations) - allowed)
if unexpected:
    ERRORS.append(
        "Unexpected direct TP mutation call sites: " +
        ", ".join(unexpected)
    )

# A standard broker TP mutation must not be able to regress in live paths.
for path in (BROKER_PROTECTION, BOUND_PROTECTION):
    source = read(path)
    if "ProtectionProgressionRule.ShouldAdvanceTarget(" in source:
        ERRORS.append(
            f"{path.name} still uses the legacy configurable backward-TP rule"
        )

# Server-side ladder must not be treated as immutable after TP1.
if (
    "if (_tp1Hit == 0" in server_text and
    "TryAdvanceServerSideTakeProfitLadderAfterTp1(" not in server_text
):
    ERRORS.append("server ladder has no TP1 follow-up progression")

# No destructive historical cleanup may be introduced by the phase.
for path in production_files:
    source = read(path)
    if "File.Delete(" in source or "Directory.Delete(" in source:
        if "History" in source or "Archive" in source:
            ERRORS.append(
                f"destructive archive operation detected in {path.relative_to(ROOT)}"
            )

if ERRORS:
    print("Phase 9.17 exit geometry audit FAILED")
    for error in sorted(set(ERRORS)):
        print(" - " + error)
    sys.exit(1)

print("Phase 9.17 exit geometry audit OK")
