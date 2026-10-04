#!/usr/bin/env python3
"""Static acceptance gate for CR5.7 / E7 WATCH/REACTION alert ownership."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
renderer = read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
presentation = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
rule = read("src/CFIP.Indicator/Core/Math/WatchReactionAlertRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
phase_doc = read("docs/PHASE-CR5-7-WATCH-REACTION-ALERTS.md")
continuation = read("docs/CONTINUATION-STATE.md")


check(
    "E7 has one platform-neutral Core qualification owner",
    "class WatchReactionAlertRule" in rule and
    "IsStrongWatch(" in rule and
    "IsWatchAlertEligible(" in rule and
    "IsReactionAlertEligible(" in rule,
)

check(
    "the established early-WATCH threshold floor and gap are named without value changes",
    "EarlyWatchConfidenceFloor = 60" in rule and
    "EarlyWatchConfidenceGap = 4" in rule and
    "ResolveEarlyWatchMinimumConfidence(" in rule,
)

check(
    "WATCH alert qualification does not depend on chart-rendering switches",
    "ShowSignalArrow" not in alerts and
    "ShowEarlyWatch" not in alerts and
    "ShowReactionArrow" not in alerts,
)

check(
    "WATCH/REACTION alert emission is owned by the runtime decision-alert boundary",
    "ProcessDecisionOwnedWatchReactionAlerts(" in alerts and
    "BuildWatchAlertKey(" in alerts and
    "BuildReactionAlertKey(" in alerts and
    'SendUnifiedAlert(' in alerts,
)

check(
    "chart signal renderer is presentation-only for WATCH/REACTION",
    "SendUnifiedAlert(" not in renderer and
    "AlertOnEarlyWatch" not in renderer and
    "AlertOnReaction" not in renderer and
    "AlertOnLiveReaction" not in renderer and
    "_lastEarlyAlertM5" not in renderer and
    "_lastReactionAlertBar" not in renderer,
)

check(
    "the visual watch-strength helper reuses the Core rule",
    "WatchReactionAlertRule.IsStrongWatch(" in presentation,
)

check(
    "the alert stage runs after live reaction calculation and before presentation",
    stage.index("UpdateLiveReaction();") < stage.index("ProcessDecisionOwnedWatchReactionAlerts(") < stage.index("RenderCalculationState("),
)

check(
    "blocked WATCH/REACTION states fail closed",
    "entryAllowed" in rule and
    "!entryAllowed" in rule and
    "!alertEnabled" in rule,
)

check(
    "existing plan/pending/live state cannot create duplicate WATCH/REACTION alerts",
    "hasPlan" in rule and
    "hasPendingOrder" in rule and
    "hasLivePosition" in rule and
    "hasPlan" in rule and
    "hasPendingOrder" in rule and
    "hasLivePosition" in rule,
)

check(
    "alert identities are deterministic and direction-distinct",
    "BuildWatchAlertKey(" in rule and
    "BuildReactionAlertKey(" in rule and
    '"WATCH|" +' in rule and
    '"REACTION|" +' in rule,
)

check(
    "runtime contracts cover E7 and the new Core owner is compiled",
    "VerifyWatchReactionAlertSemantics();" in contracts and
    "WatchReactionAlertRule.ResolveEarlyWatchMinimumConfidence(" in contracts and
    "WatchReactionAlertRule.IsWatchAlertEligible(" in contracts and
    "WatchReactionAlertRule.IsReactionAlertEligible(" in contracts and
    "WatchReactionAlertRule.cs" in project,
)

check(
    "E7 static gate is wired immediately after E6",
    "audit_phase_5_6.py" in workflow and
    "audit_phase_5_7.py" in workflow and
    workflow.index("audit_phase_5_7.py") >
    workflow.index("audit_phase_5_6.py"),
)

check(
    "E7 continuity documentation is synchronized",
    "CR5.7 / E7" in historical_roadmap and
    "CR5.7 / E7" in phase_doc and
    "CR5.7 / E7" in continuation,
)

print("CR5.7 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.7 STATIC GATE PASS")
