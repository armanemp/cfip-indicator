#!/usr/bin/env python3
"""CBOT-P4C acceptance gate: Pending Stop authority + host-timeframe independence."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"

errors = []

def read(path):
    if not path.exists():
        errors.append("missing " + str(path.relative_to(ROOT)))
        return ""
    return path.read_text(encoding="utf-8")

placement = read(IND / "Trading/Pending/Placement/ContinuationStopPlacement.cs")
orchestrator = read(IND / "Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs")
prep = read(IND / "Trading/Pending/Placement/ContinuationStopPreparation.cs")
provider = read(IND / "Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
provider_plan = read(IND / "Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
price_rule = read(IND / "Core/Math/PendingEntryPriceRule.cs")
timeframe = read(IND / "Core/Math/ExecutionTimeframePolicy.cs")
bot = read(BOT / "CFIPExecutionBot.cs")
pending = read(BOT / "Execution/DemoPendingOrderExecutionCoordinator.cs")
market = read(BOT / "Execution/DemoMarketExecutionCoordinator.cs")
shared = read(BOT / "Execution/BrokerExecutionSafety.cs")

def check(condition, message):
    if not condition:
        errors.append(message)

check("PrepareContinuationStopForCbot(" in placement,
      "Indicator Pending Stop owner must expose analysis-only intent preparation")
check("PlaceStopOrder(" not in placement,
      "Indicator Pending Stop placement must contain zero broker mutation")
check("PrepareContinuationStopForCbot(" in orchestrator,
      "Pending orchestrator must route continuation stop to intent preparation")
check("TryAcquireSubmission(" not in placement,
      "Indicator Pending Stop must not own broker submission gate")
check("PlaceStopOrder(" in pending and
      "ExecutionAction.PendingStop" in pending and
      "BrokerAction.SubmitPendingStop" in pending,
      "cBot must own Pending Stop mutation")
check("private static bool TryConstrainVolumeForMargin(" not in pending and
      "BrokerExecutionSafety.TryConstrainVolumeForMargin(" in pending,
      "Pending Stop must use the shared cBot margin-safety owner")
check("private static int CountManagedPositions(" not in pending and
      "BrokerExecutionSafety.CountManagedPositions(" in pending,
      "Pending Stop must use the shared cBot capacity owner")
check("PendingEntryPriceRule.ForExecutableStop(" in prep,
      "Pending Stop trigger must use canonical spread-aware executable-price rule")
check("double spread =" in prep and "Symbol.Ask - Symbol.Bid" in prep,
      "Pending Stop preparation must read canonical live spread")
check("PrimaryExecution = \"M15\"" in timeframe and
      "LowerDefensiveM5 = \"M5\"" in timeframe and
      "LowerDefensiveM1 = \"M1\"" in timeframe,
      "execution timeframe roles must remain canonical")
check("Bars.TimeFrame != TimeFrame.Minute15" not in bot,
      "cBot must not bind execution to host Chart TF")
check('DefaultTimeFrame = "M15"' in bot,
      "cBot launch default should remain M15")
check("ExecutionAction.PendingStop" in bot and
      "EnableDemoPendingStopExecution" in bot,
      "cBot Pending Stop routing/arm missing")
check("ExecutionTimeframePolicy.PrimaryExecution" in provider,
      "provider identity must use internal M15 execution clock")
check("Bars.TimeFrame" not in provider,
      "provider identity must not read host Chart TF")
check("ManagedExecutionLabel()" in provider_plan and
      "executionLabel," in provider_plan,
      "provider must transport the canonical instance-scoped broker label")
check("envelope.Intent.ExecutionLabel" in pending,
      "cBot Pending Stop must consume the transported instance-scoped broker label")
check(not (IND / "Trading/Execution/BrokerPendingOrderPlacement.cs").exists(),
      "migrated Indicator Pending Stop broker owner must be deleted")
check("ExecuteMarketOrder(" in market,
      "existing Market cBot owner must remain intact")
check("TryConstrainVolumeForMargin(" in shared,
      "shared cBot margin-safety owner missing")
check("ForExecutableStop(" in price_rule,
      "canonical pending executable-price owner missing")

if errors:
    print("CBOT-P4C AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P4C AUDIT: PASS")
print("Pending Stop broker authority: cBot only")
print("Indicator Pending Stop: intent-only")
print("Execution clock: internal M15")
print("Chart timeframe dependency: none")
print("M5/M1: defensive tuning")
print("H1+: context/reward support")
print("Pending trigger: spread-aware")
print("Margin/capacity helpers: shared cBot owner")
