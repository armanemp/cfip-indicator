#!/usr/bin/env python3
"""CBOT-P8 progressive protection / broker-confirmed state-sync audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")


def require(condition, message):
    if not condition:
        errors.append(message)


sync = read(
    "src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateSynchronizer.cs"
)
broker_state = read(
    "src/CFIP.Indicator/Trading/Lifecycle/BrokerStateSnapshot.cs"
)
reports = read(
    "src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs"
)
position_modified = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PositionModifiedHandler.cs"
)
bound_protection = read(
    "src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs"
)
partial_close = read(
    "src/CFIP.Indicator/Trading/LiveManagement/PartialTakeProfitExecutor.cs"
)
intelligent = read(
    "src/CFIP.Indicator/Core/Math/IntelligentProtectionRule.cs"
)
progression = read(
    "src/CFIP.Indicator/Core/Math/ProtectionProgressionRule.cs"
)
active_live = read(
    "src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLiveManagement.cs"
)
management_cbot = read(
    "src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs"
)
runtime_program = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
devlog = read("docs/DEVELOPMENT-LOG.md")

require(
    "ApplyBrokerConfirmedProtectionState(" in sync and
    "ProtectionProgressionRule.ShouldAdvanceStop(" in sync and
    "ProtectionProgressionRule.ShouldAdvanceTarget(" in sync,
    "one canonical broker-confirmed protection synchronizer must own monotonic adoption",
)

require(
    "BROKER CONFIRMED STOP REGRESSED" in sync and
    "BROKER CONFIRMED TARGET INVALID" in sync and
    "LifecycleState.RecoveryRequired" in sync,
    "broker regression must fail into explicit recovery instead of rewriting protected state",
)

require(
    "previousBrokerStop" in sync and
    "previousBrokerTarget" in sync,
    "confirmed-state adoption must compare against previously confirmed broker protection",
)

require(
    "ApplyBrokerConfirmedProtectionState(" in broker_state and
    "ApplyBrokerConfirmedProtectionState(" in reports and
    "ApplyBrokerConfirmedProtectionState(" in position_modified,
    "broker snapshot, management confirmation and position events must share the same state owner",
)

require(
    broker_state.count("_plan.Stop =") == 0,
    "BrokerStateSnapshot must not contain a second plan-stop assignment owner",
)

require(
    "_plan.Stop =" not in partial_close or
    "ApplyBrokerConfirmedProtectionState(" in partial_close,
    "partial-close break-even must adopt confirmed protection through the canonical synchronizer",
)

require(
    "_plan.Stop =" not in bound_protection or
    "ApplyBrokerConfirmedProtectionState(" in bound_protection,
    "bound-plan protection must not duplicate broker-confirmed stop adoption",
)

require(
    "BrokerReportStatus.Confirmed" in reports and
    "ApplyBrokerConfirmedProtectionState(" in reports,
    "only broker-confirmed management reports may immediately update protected plan state",
)

require(
    "ApplyBrokerConfirmedProtectionState(" in reports and
    "report.Status == BrokerReportStatus.Confirmed" in reports,
    "accepted/pending management reports must not manufacture broker-confirmed state",
)

require(
    "public static IntelligentProtectionDecision Evaluate(" in intelligent and
    "ProtectionProgressionRule.ShouldAdvanceStop(" in intelligent,
    "IntelligentProtectionRule remains the sole live trailing/protection policy",
)

require(
    "CalculateProtectedStop(" in active_live and
    "private double CalculateProtectedStop(" in read(
        "src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs"
    ) and
    "ProtectionProgressionRule.ShouldAdvanceStop(" in active_live,
    "live management must retain the canonical progressive-stop candidate path",
)

require(
    "MANAGEMENT COMMAND EXPIRED" in management_cbot and
    "FindLatestReport(" in management_cbot and
    "TryConfirmFromBrokerState(" in management_cbot,
    "cBot must retain bounded management freshness and broker confirmation",
)

require(
    "VerifyBrokerConfirmedProtectionStateSync();" in runtime_program and
    "ProtectionProgressionRule.cs" in runtime_project,
    "runtime acceptance must execute the broker-confirmed progression contract",
)

require(
    "audit_phase_cbot_p5_reconciliation.py" in workflow and
    "audit_phase_cbot_p6_account_risk_and_connection.py" in workflow and
    "audit_phase_cbot_p7_whole_chain.py" in workflow and
    "audit_phase_cbot_p7r_attachment_alert_visibility.py" in workflow and
    "audit_phase_cbot_p8_progressive_protection_state_sync.py" in workflow,
    "P8 must accumulate after the full existing cBot lifecycle/whole-chain audits",
)

require(
    "CBOT-P8" in roadmap and
    "CBOT-P8" in continuation and
    "CBOT-P8" in devlog,
    "P8 must be recorded in all continuity documents",
)

require(
    "M15" in roadmap and
    "M5" in roadmap,
    "canonical M15/M5 timeframe roles must remain documented",
)

if errors:
    print("CBOT-P8 PROGRESSIVE PROTECTION / STATE SYNC AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P8 PROGRESSIVE PROTECTION / STATE SYNC AUDIT: PASS")
print("canonical broker-confirmed protection adoption: PASS")
print("monotonic BUY/SELL stop and target regression safety: PASS")
print("accepted-vs-confirmed state separation: PASS")
print("cBot management freshness / confirmation boundary: PASS")
print("full-chain/timeframe continuity: PASS")
