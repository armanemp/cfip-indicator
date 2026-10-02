#!/usr/bin/env python3
"""OPPORTUNITY-DISCOVERY / cBot-truth / panel-readability regression gate."""
# Branch verification deliberately re-runs on each source change.
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]


def read(rel: str) -> str:
    p = ROOT / rel
    if not p.exists():
        raise SystemExit(f"missing required file: {rel}")
    return p.read_text(encoding="utf-8")


errors = []


def check(name: str, condition: bool):
    if not condition:
        errors.append(name)


selector = (
    read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs") +
    read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs") +
    read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs")
)
selection_rule = read("src/CFIP.Indicator/Core/Math/ExecutionZoneSelectionRule.cs")
candidate = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneSelectionCandidate.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs")
primary = read("src/CFIP.Indicator/Core/Math/PrimaryTimeframeSignalRule.cs")
tactical_rule = read("src/CFIP.Indicator/Core/Math/TacticalOpportunityRule.cs")
runtime_contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
parallel = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
timeframes = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
structural = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
targets = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs")

check(
    "zone selector remains canonical and compares multiple source families",
    all(token in selector for token in (
        "M5 FVG",
        "M5 ORDER_BLOCK",
        "M15 FVG",
        "M15 ORDER_BLOCK",
        "M5 FVG+OB",
        "M15 FVG+OB",
        "AddMtfOverlapCandidate(",
    )),
)
check(
    "higher-timeframe structural levels are mined",
    "AddHigherTimeframeStructureCandidates(" in selector and
    "M15 STRUCTURE" in selector and
    "H1 STRUCTURE" in selector,
)
check(
    "zone selection is reward-path aware",
    "ApplyRewardPathPreferences(" in selector and
    "BuildStructuralStop(" in selector and
    "EstimateBestTp1RRForStop(" in selector and
    "candidate.RewardPathRR" in selector,
)
check(
    "zone candidates without a reward path cannot become the selected execution zone",
    "candidate.RewardPathRR <= 0" in selector,
)
check(
    "reward-path scoring stays in the canonical selection owner",
    "ApplyRewardPathPreference(" in selection_rule and
    "qualityFactor" in selection_rule and
    "stopBonus" in selection_rule,
)
check(
    "reward-path diagnostics are part of candidate state",
    "RewardPathRR" in candidate and
    "StopQuality" in candidate,
)
check(
    "M15 remains the canonical execution timeframe",
    '"M15 EXECUTION DIRECTION CONFLICT"' in read(
        "src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs"
    ),
)
check(
    "M5 remains tuning/trigger rather than primary-source veto",
    "PRIMARY SOURCE VALID • WAITING FOR M5/M1 TUNING" in primary and
    "candidate.M5TuningAligned = primary.M5Aligned;" in timeframes,
)
check(
    "cBot publishes independent symbol presence before indicator binding completes",
    "PublishPresence(" in read("src/CFIP.cBot/CFIPExecutionBot.cs") and
    "PublishPresence(" in read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs") and
    "ForSymbol(" in read("src/CFIP.Contracts/CbotExecutionStateBus.cs") and
    "TryDeserializePresence(" in read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs"),
)

check(
    "forward target FVG discovery is not limited to current-bar retest",
    "opposingDirection" in read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs") and
    "false," in read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs") and
    "entry," in read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs"),
)

check(
    "structural stop FVG discovery considers unretested quality-aware zones",
    "FindNearestFvg(" in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs") and
    "false," in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs") and
    "frameAtr" in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs"),
)

check(
    "strong-HTF counter-M5 opportunity path is actually reachable",
    "strongHtfConflict" in tactical_rule and
    "selectedDirection != m5Direction" in tactical_rule and
    "selectedDirection != htfDirection" in tactical_rule and
    "counterStrong" in runtime_contracts and
    "very strong HTF-aligned counter-M5 opportunity" in runtime_contracts,
)
check(
    "parallel presentation still keeps multiple scenarios",
    "MaximumVisibleOpportunities" in parallel and
    "SelectScenariosForDisplay(" in timeframes,
)
check(
    "structural stops remain reward-aware",
    "EstimateBestTp1RRForStop(" in structural and
    "PlanRewardRiskQualityRule.Evaluate(" in structural,
)
check(
    "target pipeline still combines broad level families",
    all(token in targets for token in (
        "AddSupplyDemandAndLiquidityLevels(",
        "AddHtfTargets(",
        "AddPreviousPeriodLevels(",
        "AddSmartExtraTargetLevels(",
    )),
)
check(
    "fresh cBot heartbeat can prove liveness without ChartRobots gating",
    "private bool HasFreshCbotHeartbeat()" in reader and
    "return HasFreshCbotHeartbeat();" in reader and
    "if (HasFreshCbotHeartbeat())" in reader,
)
check(
    "stale heartbeat remains fail-closed",
    "ageSeconds <= 3.0" in reader and
    "CBOT RECONNECTING • HEARTBEAT STALE" in reader,
)
check(
    "panel alert rail is left-aligned and readable",
    "HorizontalAlignment.Left" in panel and
    "TextAlignment.Left" in panel and
    "Math.Max(10, PanelFontSize - 1)" in panel and
    "PanelAlertMessageRowHeight = 20" in panel,
)
check(
    "no new public tuning parameters were introduced",
    "DefaultValue" not in selector and
    "DefaultValue" not in selection_rule and
    "DefaultValue" not in panel,
)

if errors:
    print("FAIL | " + " | ".join(errors))
    raise SystemExit(1)

print("OPPORTUNITY DISCOVERY / CBOT TRUTH / PANEL READABILITY: PASS")
