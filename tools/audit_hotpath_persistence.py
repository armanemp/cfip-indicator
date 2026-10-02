#!/usr/bin/env python3

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

BUFFER = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "BufferedArchivePersistence.cs"
COORDINATOR = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "BufferedPersistenceCoordinator.cs"
RUNTIME_LOG = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "RuntimeLogPersistence.cs"
OUTCOME_ARCHIVE = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "OutcomeHistoryArchiveStore.cs"
SIGNAL_ARCHIVE = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "SignalEvaluationTraceArchivePersistence.cs"
DAILY_LOSS = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Risk" / "DailyLossPersistence.cs"
OUTCOME_MEMORY = ROOT / "src" / "CFIP.Indicator" / "Trading" / "Intelligence" / "OutcomeMemoryStore.cs"
HEARTBEAT = ROOT / "src" / "CFIP.Indicator" / "Runtime" / "Supervision" / "RuntimePanelHeartbeat.cs"
ON_DESTROY = ROOT / "src" / "CFIP.Indicator" / "Runtime" / "Initialization" / "RuntimeInitialization.cs"
FVG = ROOT / "src" / "CFIP.Indicator" / "Analysis" / "Structure" / "Zones" / "FvgDetectionAnalyzer.cs"
OB = ROOT / "src" / "CFIP.Indicator" / "Analysis" / "Structure" / "Zones" / "OrderBlockAnalyzer.cs"
ZONE_CACHE = ROOT / "src" / "CFIP.Indicator" / "Analysis" / "Structure" / "Zones" / "ZoneLookupHotCache.cs"

errors = []


def read(path: Path) -> str:
    if not path.exists():
        errors.append(f"missing required file: {path.relative_to(ROOT)}")
        return ""
    return path.read_text(encoding="utf-8")


buffer = read(BUFFER)
coordinator = read(COORDINATOR)
runtime_log = read(RUNTIME_LOG)
outcome_archive = read(OUTCOME_ARCHIVE)
signal_archive = read(SIGNAL_ARCHIVE)
daily_loss = read(DAILY_LOSS)
outcome_memory = read(OUTCOME_MEMORY)
heartbeat = read(HEARTBEAT)
on_destroy = read(ON_DESTROY)
fvg = read(FVG)
ob = read(OB)
zone_cache = read(ZONE_CACHE)

if "_bufferedArchivePersistence.Enqueue(" not in runtime_log:
    errors.append("runtime log must enqueue rows into buffered archive persistence")

for source_name, source in (
    ("OutcomeHistoryArchiveStore.cs", outcome_archive),
    ("SignalEvaluationTraceArchivePersistence.cs", signal_archive),
):
    if "File.AppendAllText(" in source or "File.WriteAllText(" in source:
        errors.append(
            f"{source_name}: direct file write remains in production archive path"
        )

if "File.AppendAllText(" not in buffer or "File.WriteAllText(" not in buffer:
    errors.append("buffered archive writer must own file creation and batched append")

if "FlushBufferedPersistence(" not in heartbeat:
    errors.append("runtime heartbeat must flush buffered persistence")

if "FlushBufferedPersistenceOnShutdown()" not in on_destroy:
    errors.append("indicator shutdown must flush buffered persistence")

# LocalStorage Flush must not remain in the calculation-facing persistence owners.
if "LocalStorage.Flush(" in daily_loss:
    errors.append("DailyLossPersistence.cs contains synchronous LocalStorage.Flush")
if "LocalStorage.Flush(" in outcome_memory:
    # The portable-memory restore is startup-only; the regular persist method must
    # stay buffered. Keep only the explicit startup migration if present.
    regular_start = outcome_memory.find("private void PersistOutcomeHistory")
    if regular_start >= 0 and "LocalStorage.Flush(" in outcome_memory[regular_start:]:
        errors.append("OutcomeMemoryStore PersistOutcomeHistory contains LocalStorage.Flush")

if "MarkDailyLossPersistenceDirty(" not in daily_loss:
    errors.append("daily-loss persistence must mark dirty instead of flushing on hot path")
if "MarkOutcomeMemoryPersistenceDirty()" not in outcome_memory:
    errors.append("outcome-memory persistence must mark dirty instead of flushing on hot path")
if "LocalStorage.Flush(" not in coordinator:
    errors.append("buffered persistence coordinator must own LocalStorage.Flush")

if "ResetZoneLookupCacheIfNeeded(" not in fvg:
    errors.append("FVG lookup must use closed-context cache invalidation")
if "TryGetCachedFvgCandidates(" not in fvg:
    errors.append("FVG lookup must read candidate cache")
if "ResetZoneLookupCacheIfNeeded(" not in ob:
    errors.append("Order Block lookup must use closed-context cache invalidation")
if "TryGetCachedObCandidates(" not in ob:
    errors.append("Order Block lookup must read candidate cache")
if "_zoneLookupCacheBars" not in zone_cache or "_zoneLookupCacheIndex" not in zone_cache:
    errors.append("zone cache must remain tied to explicit Bars/index context")

# Runtime log rows are queued in Calculate; the only synchronous disk ownership
# must live in the buffer implementation and its timer/shutdown callers.
for relative in (
    "src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs",
    "src/CFIP.Indicator/Trading/Risk/DailyLossPersistence.cs",
):
    path = ROOT / relative
    source = read(path)
    if "File.AppendAllText(" in source or "File.WriteAllText(" in source:
        errors.append(f"{relative}: synchronous file write remains")


# No production source may call direct file writes outside the buffer owner.
for path in (ROOT / "src").rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    relative = str(path.relative_to(ROOT)).replace("\\", "/")
    if relative not in (
        "src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs",
        "src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs",
    ):
        if "File.WriteAllText(" in source or "File.AppendAllText(" in source:
            errors.append(f"{relative}: direct file write bypasses buffered persistence")

refresh_start = daily_loss.find("private void RefreshSharedDailyLossLock(")
refresh_end = daily_loss.find("private void ReloadSharedDailyLossLockFromStorage(", refresh_start)
if refresh_start >= 0 and refresh_end > refresh_start:
    refresh_method = daily_loss[refresh_start:refresh_end]
    if "LocalStorage.Reload(" in refresh_method:
        errors.append("RefreshSharedDailyLossLock must not reload disk during Calculate")


# Broker-state reads are event-aware and time-bounded rather than repeated blindly.
broker = read(ROOT / "src" / "CFIP.Indicator" / "Trading" / "Lifecycle" / "BrokerStateSnapshot.cs")
broker_rule = read(ROOT / "src" / "CFIP.Indicator" / "Core" / "Math" / "BrokerStateRefreshRule.cs")
if "MarkBrokerStateDirty()" not in broker:
    errors.append("BrokerStateSnapshot must expose dirty invalidation")
if "BrokerStateRefreshRule.IsRefreshDue(" not in broker:
    errors.append("BrokerStateSnapshot must use the canonical broker refresh rule")
if "IsRefreshDue(" not in broker_rule:
    errors.append("BrokerStateRefreshRule must own broker refresh cadence")

for relative in (
    "src/CFIP.Indicator/Trading/Execution/BrokerPendingOrderPlacement.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerPendingOrderCancellation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerPositionCloseMutation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerStopLossMutation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerTakeProfitMutation.cs",
):
    source = read(ROOT / relative)
    if "MarkBrokerStateDirty();" not in source:
        errors.append(f"{relative}: successful/attempted broker mutation must invalidate state cache")

for relative in (
    "src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PositionModifiedHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PendingCreatedHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PendingModifiedHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PendingCancelledHandler.cs",
):
    source = read(ROOT / relative)
    if "MarkBrokerStateDirty();" not in source:
        errors.append(f"{relative}: managed broker event must invalidate state cache")

if "BufferedArchiveFlushIntervalMilliseconds" not in coordinator:
    errors.append("buffered persistence must have an explicit timer cadence")
if "BufferedArchiveFlushBudget" not in coordinator:
    errors.append("buffered persistence must have a bounded flush budget")

if errors:
    print("HOT-PATH PERSISTENCE AUDIT: FAIL")
    for error in errors:
        print(f"  - {error}")
    raise SystemExit(1)

print("HOT-PATH PERSISTENCE AUDIT: PASS")
