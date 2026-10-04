from pathlib import Path

ROOT = Path("src/CFIP.Indicator")

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing G5 source: {relative}")
    return path.read_text(encoding="utf-8")

panel = read("UI/Panel/PanelRenderOptimization.cs")
panel_state = read("UI/Panel/PanelExecutionState.cs")
broker = read("Trading/Lifecycle/BrokerStateSnapshot.cs")
auto = read("Trading/Execution/State/AutoTradingStateStore.cs")
life = read("Trading/Execution/State/LifecycleStateStore.cs")
state = read("Indicator/State.cs")
program = Path("tools/CFIP.Runtime.Contracts/Program.cs").read_text(encoding="utf-8")
workflow = Path(".github/workflows/source-check.yml").read_text(encoding="utf-8")
roadmap = Path("docs/CFIP-ROADMAP.md").read_text(encoding="utf-8")
historical_roadmap = Path("docs/archive/ROADMAP-LEGACY-2026-10-04.md").read_text(encoding="utf-8")
continuation = Path("docs/CONTINUATION-STATE.md").read_text(encoding="utf-8")
review = Path("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md").read_text(encoding="utf-8")
development = Path("docs/DEVELOPMENT-LOG.md").read_text(encoding="utf-8")
phase = Path("docs/PHASE-CR7-5-G5-PANEL-STATE-FRESHNESS.md").read_text(encoding="utf-8")

build = panel.split("private string BuildPanelPresentationKey(", 1)[1].split("private string PriceKey(", 1)[0]
if "InvalidatePanelExecutionProtectionStateCache();" in build:
    raise SystemExit("G5 presentation-key builder must not force cache invalidation")

mark = broker.split("private void MarkBrokerStateDirty()", 1)[1].split("private void SynchronizeLiveBrokerState()", 1)[0]
if "InvalidatePanelExecutionProtectionStateCache();" not in mark:
    raise SystemExit("G5 broker dirty event must invalidate the canonical panel snapshot")

sync = broker.split("private void SynchronizeLiveBrokerState()", 1)[1]
if "InvalidatePanelExecutionProtectionStateCache();" not in sync:
    raise SystemExit("G5 due broker refresh must invalidate the canonical panel snapshot")

ensure = panel_state.split("private void EnsurePanelExecutionProtectionStateCache()", 1)[1].split("private string GetAutoTradingPanelState()", 1)[0]
if "GetManagedPosition()" not in ensure or "GetManagedPendingOrder()" not in ensure:
    raise SystemExit("G5 broker enumeration must remain inside the canonical G4 snapshot owner")

lifecycle_method = life.split(
    "private void SetLifecycleState(",
    1,
)[1]

if "if (previous != state)" not in lifecycle_method or    "InvalidatePanelExecutionProtectionStateCache();" not in lifecycle_method:
    raise SystemExit(
        "G5 lifecycle state changes must invalidate the canonical panel snapshot"
    )

if "InvalidatePanelExecutionProtectionStateCache();" not in auto:
    raise SystemExit("G5 runtime state owner is missing panel invalidation")

for token in (
    "private string _autoExecutionBlockReasonValue",
    "private string _autoOrdersBlockReasonValue",
    "private bool _serverSideTakeProfitLadderActiveValue",
    "private bool _brokerProtectionRecoveryRequiredValue",
    "private string _autoExecutionBlockReason",
    "private string _autoOrdersBlockReason",
    "private bool _serverSideTakeProfitLadderActive",
    "private bool _brokerProtectionRecoveryRequired",
):
    if token not in state:
        raise SystemExit(f"G5 guarded state input missing: {token}")

if "VerifyExecutionProtectionPanelStateFreshnessG5();" not in program:
    raise SystemExit("G5 runtime contract is not invoked")

if "python tools/audit_phase_7_5.py" not in workflow:
    raise SystemExit("G5 static audit is not in Source/Architecture CI")

for document, token, name in (
    (historical_roadmap, "CR7.5 / G5", "ROADMAP G5 scope"),
    (continuation, "CR7.5 / G5", "CONTINUATION G5 scope"),
    (review, "CR7.5 / G5", "remediation G5 scope"),
    (development, "CR7.5 / G5", "development log G5 entry"),
):
    if token not in document:
        raise SystemExit(f"G5 continuity record missing: {name}")

if (
    "Status: **IMPLEMENTED" not in phase and
    "Status: **VERIFIED COMPLETE" not in phase
) or "CR7.6a" not in phase:
    raise SystemExit("G5 phase document does not record implementation and G6A continuation")

for document, name in (
    (roadmap, "ROADMAP G6A continuation"),
    (continuation, "CONTINUATION G6A continuation"),
    (review, "remediation G6A continuation"),
):
    if "CR7.6a" not in document:
        raise SystemExit(f"G5 continuity record does not retain G6A transition: {name}")

print("CR7.5 / G5 panel-state freshness audit PASS")
print("presentation key does not invalidate the canonical state snapshot")
print("broker/runtime/lifecycle mutations invalidate the snapshot")
print("broker enumeration remains inside the canonical G4 snapshot owner")
print("bounded broker refresh remains the stale-state safety backstop")
