#!/usr/bin/env python3
"""Static acceptance gate for CR4.9 / D9 live reversal action and alert semantics."""

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


analyzer = read(
    "src/CFIP.Indicator/Trading/Intelligence/Prediction/LiveReversalAnalyzer.cs"
)
episode = read(
    "src/CFIP.Indicator/Trading/LiveManagement/LiveReversalEpisodeState.cs"
)
decision_rule = read(
    "src/CFIP.Indicator/Core/Math/LiveReversalDecisionRule.cs"
)
thresholds = read(
    "src/CFIP.Indicator/Core/Math/ExecutionThresholdPolicy.cs"
)
closed_handler = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs"
)
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
phase_doc = read("docs/PHASE-CR4-9-LIVE-REVERSAL-SEMANTICS.md")


check(
    "live reversal uses one Core action/direction owner",
    "class LiveReversalDecisionRule" in decision_rule
    and "class LiveReversalEpisodeRule" in read("src/CFIP.Indicator/Core/Math/LiveReversalEpisodeRule.cs")
    and "OppositeDirection(" in decision_rule
    and "ResolveDirectionalConfidence(" in decision_rule
    and "ResolveAction(" in decision_rule
    and "LiveReversalDecisionRule.ResolveAction(" in analyzer,
)

check(
    "reversal quality is direction-specific",
    "ResolveDirectionalConfidence(" in analyzer
    and "_reaction.Direction" in analyzer
    and "_m5Frame.Direction" in analyzer
    and "Math.Max(" not in analyzer.split(
        "int reversalConfidence =", 1
    )[1].split("int minimumConfidence =", 1)[0],
)

check(
    "same-direction reaction confidence cannot qualify an opposite reversal",
    "reactionDirection" in decision_rule
    and "reactionDirection == opposite" in decision_rule
    and "frameDirection == opposite" in decision_rule
    and "same-direction SELL evidence cannot qualify" in contracts,
)

check(
    "reversal alert is episode-bounded and carries chart M5 identity",
    "_reversalEpisodeAlerted" in episode
    and "TryMarkReversalAlertEmitted(" in analyzer
    and '"REVERSAL|"' in analyzer
    and "closedM5" in analyzer
    and "livePosition.Id" in analyzer
    and "opposite" in analyzer
    and "REVERSAL-CLOSE|" not in analyzer.split(
        "TryMarkReversalAlertEmitted(", 1
    )[0],
)

check(
    "confirmed position close clears the reversal episode state",
    "ResetReversalEpisodeOnClosedPosition(args.Position.Id)" in closed_handler,
)

check(
    "retained position is not presented as globally blocked",
    '"WAIT"' in analyzer
    and "REVERSAL DETECTED • POSITION RETAINED" in analyzer
    and '"BLOCKED"' not in analyzer,
)

check(
    "accepted close request is not falsely labelled EXECUTED",
    '"EXIT_REQUESTED"' in analyzer
    and "REVERSAL EXIT REQUESTED" in analyzer
    and '"EXECUTED"' not in analyzer,
)

check(
    "missing managed position is reconciled instead of synthesizing closure/outcome",
    "POSITION STATE RECONCILING" in analyzer
    and "MarkBrokerStateDirty();" in analyzer
    and 'LifecycleState.RecoveryRequired' in analyzer
    and "LifecycleState.Closed" not in analyzer
    and "_plan = null" not in analyzer,
)

check(
    "live reversal bounds have explicit named ownership",
    "NormalizeLiveReversalConfidence(" in thresholds
    and "NormalizeLiveReversalStructuralScore(" in thresholds
    and "NormalizeLiveReversalConfidence(" in analyzer
    and "NormalizeLiveReversalStructuralScore(" in analyzer,
)

check(
    "deterministic D9 contracts cover direction, action, lifecycle and symmetry",
    "VerifyLiveReversalD9();" in contracts
    and "same-direction reaction confidence is ignored" in contracts
    and "accepted exit waits for broker confirmation" in contracts
    and "reversal detection alert is emitted once per episode" in contracts
    and "missing broker position requires reconciliation" in contracts
    and "BUY position accepts SELL evidence" in contracts
    and "SELL reversal uses opposite BUY evidence" in contracts,
)

check(
    "planning contract project includes all D9 Core owners",
    "ExecutionThresholdPolicy.cs" in contracts_project
    and "LiveReversalDecisionRule.cs" in contracts_project
    and "LiveReversalEpisodeRule.cs" in contracts_project,
)

check(
    "D9 static gate is wired after CR4.8",
    "audit_phase_4_8.py" in workflow
    and "audit_phase_4_9.py" in workflow
    and workflow.index("audit_phase_4_9.py") >
    workflow.index("audit_phase_4_8.py"),
)

check(
    "phase documentation records completion and next transition",
    "CR4.9" in roadmap
    and "CR4.9" in continuation
    and "CR4.9" in review
    and "CR4.9" in phase_doc
    and "CR4.10" in roadmap
    and "CR4.10" in continuation,
)

check(
    "phase preserves no-tuning and target-terminal boundaries",
    "no public parameter name/type/defaultvalue changed" in phase_doc.lower()
    and "no default rr" in phase_doc.lower()
    and "target-terminal" in phase_doc.lower()
    and "no second decision or broker-execution authority was introduced" in phase_doc.lower(),
)

print("CR4.9 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR4.9 STATIC GATE PASS")
