from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def read(path):
    return (ROOT / path).read_text(encoding="utf-8")


def require(condition, message):
    if not condition:
        raise SystemExit(message)


time_rule = read("src/CFIP.Indicator/Core/Math/CanonicalTimeRule.cs")
session = read("src/CFIP.Indicator/Core/Math/SessionWindowRule.cs")
daily_loss = read("src/CFIP.Indicator/Trading/Risk/DailyLossGuard.cs")
daily_accounting = read("src/CFIP.Indicator/Trading/Risk/DailyLossAccounting.cs")
daily_persistence = read("src/CFIP.Indicator/Trading/Risk/DailyLossPersistence.cs")
archive = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs")
runtime_log = read("src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs")
persistence = read("src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs")
portable = read("src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs")
eod = read("src/CFIP.Indicator/Trading/Alerts/EndOfDayAlert.cs")
indicator_host = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
cbot_host = read("src/CFIP.cBot/CFIPExecutionBot.cs")
calc = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
stages = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
live = read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

require(
    "public static DateTime EnsureUtc(" not in time_rule and
    "internal static DateTime EnsureUtc(" in time_rule and
    "UtcDayStart(" in time_rule and
    "UtcNextDayStart(" in time_rule and
    "IsSameUtcDay(" in time_rule and
    "RollingPeriodStart(" in time_rule and
    "OutcomeArchivePeriodDays = 90" in time_rule,
    "M4: canonical time owner is incomplete",
)

require(
    "CanonicalTimeRule.UtcDayStart(" in session and
    "CanonicalTimeRule.EnsureUtc(value)" in session and
    "startMinute == endMinute" in session and
    "startMinute < endMinute" in session,
    "M4: session semantics are not owned by the canonical UTC rule",
)

require(
    "CanonicalTimeRule.IsSameUtcDay(" in daily_loss and
    "CanonicalTimeRule.UtcDayStart(" in daily_loss,
    "M4: DailyLoss guard still uses a duplicate day boundary",
)

require(
    "CanonicalTimeRule.UtcDayStart(" in daily_accounting and
    "CanonicalTimeRule.UtcNextDayStart(" in daily_accounting,
    "M4: DailyLoss account facts do not use canonical day boundaries",
)

require(
    "MemoryAccountScopeToken()" in daily_persistence and
    "LegacyDailyLossStorageKey()" in daily_persistence and
    "ReadDailyLossState(" in daily_persistence and
    "CanonicalTimeRule.IsSameUtcDay(" in daily_persistence,
    "M4: daily-loss persistence must be account-scoped and migration-safe",
)

require(
    "CanonicalTimeRule.RollingPeriodStart(" in archive and
    "OutcomeArchivePeriodDays" in archive and
    "BufferedArchivePersistence.Enqueue(" in archive,
    "M4: outcome archive must use one 90-day period owner and buffered writes",
)

require(
    "CanonicalTimeRule.EnsureUtc(" in runtime_log and
    "OutcomeArchivePeriodDays" in runtime_log,
    "M4: runtime log must share canonical UTC and archive periods",
)

require(
    "DateTime? observedUtc" in persistence and
    "CanonicalTimeRule.EnsureUtc(" in persistence and
    "DateTime.UtcNow" in persistence,
    "M4: persistence health timestamps must accept canonical server UTC with local fallback",
)

require(
    "MarkProbeResult(" in portable and
    "Server.TimeInUtc" in portable and
    "HistoryLocationMarkerFileName" in portable,
    "M4: history marker probe is incomplete",
)

require(
    "CanonicalTimeRule.IsSameUtcDay(" in eod and
    "CanonicalTimeRule.EnsureUtc(" in eod and
    "entryUtc < boundaryUtc" in eod,
    "M4: EOD must use canonical day/time and preserve late-created positions",
)

require(
    'TimeZone = TimeZones.UTC' in indicator_host and
    'TimeZone = TimeZones.UTC' in cbot_host and
    'DefaultTimeFrame = "M5"' in cbot_host,
    "M4: algorithm time zone or cBot default host contract regressed",
)

require(
    "RunClosedBarAnalysisStage(" in calc and
    "ProcessLiveCalculationStages(" in calc and
    "ProcessQueuedAlertDelivery();" in calc,
    "M4: closed-bar/live calculation pipeline is missing",
)

require(
    "UpdateLiveReaction();" in stages and
    "UpdateM1TriggerRuntime(" in stages and
    "SynchronizePreTradePlanWithDecision();" in stages and
    "EnsureCanonicalPlan(" in stages and
    "ProcessDecisionOwnedWatchReactionAlerts(" in stages and
    "RenderCalculationState(" in stages and
    "RefreshReadOnlyProvider(" in stages,
    "M4: analysis -> signal -> plan -> presentation -> provider chain regressed",
)

require(
    live.index("RefreshLiveDecisionActionability(") <
    live.index("BuildSignalVisualSnapshot(") <
    live.index("RenderLatestAlertSignalMarker("),
    "M4: final visual state is not built from the refreshed decision/actionability state",
)

require(
    "ContractIdentity identity" in provider and
    "LatestSignalEnvelope" in provider and
    "CFIP.Contracts" in cbot_host,
    "M4: Indicator/cBot provider identity boundary regressed",
)

require(
    "M4TimeHistoryContracts.Run();" in contracts and
    "CanonicalTimeRule.cs" in runtime_project and
    "audit_phase_m4_time_history.py" in workflow,
    "M4: deterministic runtime contracts or CI wiring is missing",
)

# Guard against reintroducing machine-local timezone conversion in the M4-owned
# time boundaries. The algorithm is explicitly configured for UTC.
for name, text in {
    "SessionWindowRule": session,
    "DailyLossAccounting": daily_accounting,
    "DailyLossPersistence": daily_persistence,
    "EndOfDayAlert": eod,
    "RuntimeLogPersistence": runtime_log,
}.items():
    require(
        ".ToUniversalTime()" not in text,
        f"M4: {name} reintroduced machine-local timezone conversion",
    )

print("=" * 72)
print("M4 Time / Session / History / Persistence integrity audit: PASS")
print("=" * 72)
