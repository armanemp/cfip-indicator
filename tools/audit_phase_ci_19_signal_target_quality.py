#!/usr/bin/env python3
"""CI-19 signal / target quality and reward-coherence audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    p = ROOT / rel
    if not p.exists():
        errors.append("missing " + rel)
        return ""
    return p.read_text(encoding="utf-8")

structure = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionStructureGates.cs")
smart = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs")
target_builder = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs")
target_eval = read("src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs")
target_live = read("src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs")
reward_rule = read("src/CFIP.Indicator/Core/Math/TargetCandidateRewardScoreRule.cs")
planning_project = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
planning_program = read("tools/CFIP.Planning.Contracts/Program.cs")
pipeline = read("src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs")

if "if (!decision.TriggerReady)" in structure:
    errors.append("DecisionStructureGates still globally blocks EntryAllowed on TriggerReady")
if not all(token in smart for token in ("tacticalRetestPath", "decision.TacticalOpportunityAllowed", "decision.TacticalOpportunityQuality", "decision.TacticalOpportunityRR")):
    errors.append("TREND gate lacks canonical tactical Retest preservation path")

merge_pos = target_builder.find("MergeLevels(")
take_pos = target_builder.find(".Take(", merge_pos)
if merge_pos < 0 or take_pos < 0 or take_pos < merge_pos:
    errors.append("TargetLevelBuilder must merge structural candidates before candidate cap")
if "TargetCandidateRewardScoreRule.Calculate(" not in target_eval:
    errors.append("initial target scoring does not use canonical reward score rule")
if "TargetCandidateRewardScoreRule.Calculate(" not in target_live:
    errors.append("live target progression does not use canonical reward score rule")
if "rewardExpansionRr" not in reward_rule or "qualityMultiplier" not in reward_rule:
    errors.append("reward rule must combine RR expansion with source quality")
if "TargetCandidateRewardScoreRule.cs" not in planning_project:
    errors.append("Planning contract project must compile target reward score rule")
if "VerifyTargetCandidateRewardScoring();" not in planning_program:
    errors.append("Planning contract suite must execute target reward scoring contract")
if "higher valid RR must outrank same-quality minimum RR" not in planning_program:
    errors.append("Planning contract must prove reward expansion preference")
if "BARRIER TRACE" not in pipeline:
    errors.append("panel must expose canonical blocker/actionability reason")
if "TRIGGER  " not in pipeline or "_triggerRuntime.Score" not in pipeline:
    errors.append("panel must expose trigger lifecycle truth")

if errors:
    print("CI-19 SIGNAL / TARGET QUALITY COHERENCE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-19 SIGNAL / TARGET QUALITY COHERENCE AUDIT: PASS")
print("Global trigger gate removed: PASS")
print("Tactical Retest preservation: PASS")
print("Target merge-before-cap: PASS")
print("Initial/live shared reward score owner: PASS")
print("Reward expansion quality-weighted: PASS")
print("Planning deterministic target scoring contract: PASS")
print("Panel barrier/trigger diagnostics: PASS")