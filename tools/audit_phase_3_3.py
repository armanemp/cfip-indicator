#!/usr/bin/env python3
"""Static acceptance gate for CR3.3 partial TP, server ladder, BE and trailing semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

partial = read("src/CFIP.Indicator/Trading/LiveManagement/PartialTakeProfitExecutor.cs")
level_exit = read("src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLevelExitHandler.cs")
server = read("src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs")
progression = read("src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadderProgression.cs")
live = read("src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLiveManagement.cs")
bound = read("src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs")
recovery = read("src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
activation = read("src/CFIP.Indicator/Trading/LiveManagement/PlanActivation.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")

checks = {
    "manual partial TP carries canonical closed-bar identity": (
        "ExecutePartialClose(" in partial and
        "int closedM5" in partial and
        "ExecutePartialClose(" in level_exit and
        "closedM5" in level_exit
    ),
    "manual partial TP retries are stage-scoped and bounded": (
        "PartialTakeProfitRetryRule.ShouldAttemptStage(" in partial and
        "RecordPartialTakeProfitAttempt(" in partial and
        "_lastPartialTp1AttemptM5" in state and
        "_lastPartialTp2AttemptM5" in state
    ),
    "partial TP mutation rejection does not manufacture plan stop state": (
        "if (!closeAccepted)" in partial and
        "return false;" in partial and
        "_plan.Stop =" not in partial and
        "ApplyBrokerConfirmedProtectionState(" in partial and
        "Break-even mutation failed" not in partial
    ),
    "post-partial BE uses the canonical spread-aware SmartBreakEvenRule": (
        "SmartBreakEvenRule.Evaluate(" in partial and
        "UseSpreadAwareBreakEven" in partial and
        "BreakEvenBufferPips" in partial and
        "RiskFreeLockPips" in partial
    ),
    "BE rejection preserves broker-authoritative plan protection": (
        '"PARTIAL CLOSE • BREAK-EVEN REJECTED"' in partial and
        "_lastBreakEvenDiagnostic" in partial and
        "NormalizePrice(" in partial and
        "breakEvenPrice" in partial
    ),
    "server partial TP observes broker closing deals, not volume coincidence": (
        "position.Deals" in server and
        "DealPositionImpact.Closing" in server and
        "ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(" in server and
        "position.VolumeInUnits <=" not in server
    ),
    "server partial observation is cached by deal-count change": (
        "_lastServerPartialObservationDealCount" in state and
        "int dealCount" in server and
        "_lastServerPartialObservationDealCount" in server
    ),
    "server ladder ownership survives collapse so TP3 is never synthesized": (
        "_serverSideTakeProfitLadderOwned" in state and
        "_serverSideTakeProfitLadderOwned = true" in server and
        "_serverSideTakeProfitLadderOwned" in level_exit
    ),
    "server ladder mutations have bounded retry identity": (
        "PartialTakeProfitRetryRule.ShouldAttemptStage(" in progression and
        "_lastServerTpLadderMutationM5" in state and
        "_lastServerTpLadderMutationKind" in state
    ),
    "post-TP1 ladder refuses a final target behind broker target": (
        "TryAdvanceServerSideTakeProfitLadderAfterTp1" in progression and
        "_activeBrokerTarget > 0" in progression and
        "ShouldAdvanceLiveTarget(" in progression
    ),
    "post-TP2 collapse refuses backward movement before mutation": (
        "TryCollapseServerSideTakeProfitLadderToFinal(" in progression and
        "SERVER-LADDER-AFTER-TP2" in progression and
        "_activeBrokerTarget > 0" in progression
    ),
    "live target progression keeps existing monotonic target guards": (
        "TargetProgressionRule.IsValid(" in progression and
        "ShouldAdvanceLiveTarget(" in progression
    ),
    "protected stop becomes plan state only after mutation confirmation": (
        "_pendingProtectedStopCandidate" in live and
        "_pendingProtectedStopCandidate" in bound and
        "stopMutationSucceeded" in bound and
        "RequestModifyStopLoss(" in bound and
        "ApplyBrokerConfirmedProtectionState(" in bound and
        "NormalizePrice(desiredStop)" in bound
    ),
    "broker stop rejection does not overwrite plan stop": (
        "if (!stopConfirmed" in bound and
        "_plan.Stop =" not in bound and
        "ApplyBrokerConfirmedProtectionState(" in bound
    ),
    "restart recovery derives the original phase from broker EntryTime": (
        "position.EntryTime" in recovery and
        "recoveryCreatedM5" in recovery
    ),
    "restart peak RR is rebuilt from historical closed-bar extremes": (
        "PeakPriceReconstructionRule.TryResolve(" in recovery and
        "_m5Bars.HighPrices[i]" in recovery and
        "_m5Bars.LowPrices[i]" in recovery
    ),
    "per-plan CR3.3 state is reset on activation": (
        "_lastPartialTp1AttemptM5 = -1" in activation and
        "_lastPartialTp2AttemptM5 = -1" in activation and
        "_lastServerTpLadderMutationM5 = -1" in activation and
        "_serverSideTakeProfitLadderOwned = false" in activation
    ),
    "runtime project links all CR3.3 Core contracts": (
        "PartialTakeProfitRetryRule.cs" in runtime_project and
        "ServerPartialTakeProfitEvidenceRule.cs" in runtime_project and
        "PeakPriceReconstructionRule.cs" in runtime_project
    ),
    "runtime contracts cover CR3.3 behavior": (
        "VerifyPartialTakeProfitRetrySemantics();" in contracts and
        "VerifyServerPartialTakeProfitEvidenceSemantics();" in contracts and
        "VerifyPeakPriceReconstructionSemantics();" in contracts and
        "VerifySmartBreakEvenSemantics();" in contracts and
        "VerifyTargetProgressionMonotonicity();" in contracts
    ),
    "workflow runs CR3.3 static audit": (
        "python tools/audit_phase_3_3.py" in workflow
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# Explicitly reject the old executable proxy-EV owner if it ever returns.
if (ROOT / "src/CFIP.Indicator/Trading/Intelligence/ProxyExpectedValueCalculator.cs").exists():
    errors.append("obsolete ProxyExpectedValueCalculator owner still exists")
    print("FAIL | obsolete ProxyExpectedValueCalculator owner still exists")

print("CR3.3 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.3 STATIC GATE PASS")
