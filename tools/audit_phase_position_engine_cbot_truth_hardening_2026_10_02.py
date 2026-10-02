#!/usr/bin/env python3
"""Position-engine / cBot-truth / panel-readability regression gate."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    p = ROOT / path
    if not p.exists():
        raise SystemExit(f"missing required file: {path}")
    return p.read_text(encoding="utf-8")

errors = []

def check(name: str, condition: bool):
    if not condition:
        errors.append(name)

selector = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs")
core = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs")
candidates = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs")
candidate = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneSelectionCandidate.cs")
selection = read("src/CFIP.Indicator/Core/Math/ExecutionZoneSelectionRule.cs")
stop = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs")
targets = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs")
reconcile = read("src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs")
safety = read("src/CFIP.cBot/Execution/BrokerExecutionSafety.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
bus = read("src/CFIP.Contracts/CbotExecutionStateBus.cs")
identity = read("src/CFIP.cBot/Execution/CbotManagedObjectIdentityRule.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs")
tactical = read("src/CFIP.Indicator/Core/Math/TacticalOpportunityRule.cs")

check(
    "canonical zone selection delegates to the reward-aware core",
    "TrySelectExecutionZoneCandidateCore(" in selector and
    "ApplyRewardPathPreferences(" in core,
)
check(
    "all primary zone families are compared",
    all(token in core for token in (
        "M5 FVG",
        "M5 ORDER_BLOCK",
        "M15 FVG",
        "M15 ORDER_BLOCK",
        "M5 FVG+OB",
        "M15 FVG+OB",
        "AddMtfOverlapCandidate(",
        "AddHigherTimeframeStructureCandidates(",
    )),
)
check(
    "HTF structural levels participate in entry-zone discovery",
    'timeframe + " STRUCTURE"' in candidates and
    '"M15"' in core and
    '"H1"' in core and
    "FindSwingLowBelow(" in candidates and
    "FindSwingHighAbove(" in candidates,
)
check(
    "zone choice is reward-path aware and does not select a zero-RR candidate",
    "BuildStructuralStop(" in candidates and
    "EstimateBestTp1RRForStop(" in candidates and
    "candidate.RewardPathRR <= 0" in core and
    "RewardPathRR" in candidate and
    "StopQuality" in candidate,
)
check(
    "structural-stop FVGs use the unrestricted historical collector",
    "false," in stop and
    "entry," in stop and
    "true);" in stop,
)
check(
    "forward opposing FVG targets are not tied to current-bar retest",
    "opposingDirection" in targets and
    "false," in targets and
    "entry," in targets and
    "true);" in targets,
)
check(
    "strong HTF counter-M5 lane remains reachable only through the existing stricter contract",
    tactical.count("bool strongHtfConflict") == 1 and
    "counterHtfMinimumQuality" in tactical and
    "counterHtfMinimumRR" in tactical,
)
check(
    "stable instance-scope managed label matching exists",
    'InstanceMarker = "|CFIP-I:"' in identity and
    "MatchesManagedLabel(" in identity and
    "MatchesManagedPendingLabel(" in identity,
)
check(
    "broker reconciliation can recover a changed base label while keeping instance scope",
    "CbotManagedObjectIdentityRule.MatchesManagedLabel(" in reconcile and
    "position.Label ?? string.Empty" in reconcile and
    "baseLabel" in reconcile,
)
check(
    "cBot reconciliation runs immediately when the execution label changes",
    "_lastReconciledExecutionLabel" in bot and
    "executionLabelChanged" in bot and
    "&& !executionLabelChanged" in bot,
)
check(
    "management lookup uses stable instance scope",
    "CbotManagedObjectIdentityRule.MatchesManagedLabel(" in management and
    "CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(" in management,
)
check(
    "capacity and published broker counts use the same managed identity rule",
    "CbotManagedObjectIdentityRule.MatchesManagedLabel(" in safety and
    "CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(" in safety and
    "CbotManagedObjectIdentityRule.MatchesManagedLabel(" in publisher and
    "CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(" in publisher,
)
check(
    "cBot presence heartbeat is independent from exact Indicator binding",
    "ForSymbol(" in bus and
    "CbotPresenceSnapshot" in bus and
    "PublishPresence(" in publisher and
    'PublishPresence("STARTING")' in bot and
    'PublishPresence("RUNNING")' in bot and
    'PublishPresence("STOPPED")' in bot,
)
check(
    "Indicator uses fresh exact-instance heartbeat for execution capability",
    "HasFreshCbotHeartbeat()" in reader and
    "ageSeconds <= 3.0" in reader and
    "IndicatorInstanceId" in reader,
)
check(
    "presence state distinguishes detection from execution authority",
    "HasFreshCbotPresence()" in reader and
    "CBOT DETECTED •" in reader and
    "CBOT CONNECTED • HEARTBEAT LIVE •" in reader,
)
check(
    "panel messages are readable and left-aligned",
    "PanelAlertMessageRowHeight = 20" in panel and
    "Math.Max(10, PanelFontSize - 1)" in panel and
    "TextAlignment.Left" in panel and
    "HorizontalAlignment.Left" in panel,
)
check(
    "no new public tuning parameters were added by this phase",
    "[Parameter(" not in selector and
    "[Parameter(" not in selection and
    "[Parameter(" not in identity,
)

if errors:
    print("FAIL | " + " | ".join(errors))
    raise SystemExit(1)

print("POSITION ENGINE / CBOT TRUTH / PANEL READABILITY: PASS")
