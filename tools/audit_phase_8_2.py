#!/usr/bin/env python3
"""Static acceptance gate for CR8.2 / H2 top-down absolute-strength semantics."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")

def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

rule = read("src/CFIP.Indicator/Core/Math/TopDownCalibrationRule.cs")
group = read("src/CFIP.Indicator/Core/Math/TopDownCalibrationGroupResult.cs")
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
runtime = read("tools/CFIP.Decision.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
state = read("src/CFIP.Indicator/Indicator/State.cs")
sync = read("src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs")

check(
    "top-down group exposes bounded absolute strength",
    "AbsoluteStrength" in group and "AbsoluteStrength" in rule and
    "Math.Min" in group,
)

check(
    "absolute strength averages dominant-direction quality only",
    "bullWeight" in rule and "bearWeight" in rule and
    "dominantWeight" in rule and "Math.Max(bull, bear)" in rule,
)

check(
    "HTF strong requires both relative alignment and absolute strength",
    "htf.Alignment >= strongThreshold" in rule and
    "htf.AbsoluteStrength >= strongThreshold" in rule,
)

check(
    "mid-frame conflict requires absolute strength as well as alignment",
    "mid.Alignment >= middleConflictThreshold" in rule and
    "mid.AbsoluteStrength >= middleConflictThreshold" in rule,
)

check(
    "entry conflict requires sufficient absolute strength",
    "entryAbsoluteStrength >= middleConflictThreshold" in rule and
    "entryAlignment == 0" in rule,
)

check(
    "Decision and orchestration expose canonical H2 diagnostics",
    "HtfAbsoluteStrength" in decision and
    "MidframeAbsoluteStrength" in decision and
    "EntryFrameAbsoluteStrength" in decision and
    "topDown.HtfAbsoluteStrength" in orchestration and
    "topDown.MidAbsoluteStrength" in orchestration and
    "topDown.EntryAbsoluteStrength" in orchestration,
)

check(
    "panel presents absolute-strength values from canonical Decision state",
    "HtfAbsoluteStrength" in panel and
    "MidframeAbsoluteStrength" in panel and
    "EntryFrameAbsoluteStrength" in panel,
)

check(
    "Decision Contracts cover weak and strong alignment/conflict cases",
    "VerifyTopDownCalibrationAbsoluteStrength();" in runtime and
    "perfect HTF alignment does not imply strong absolute strength" in runtime and
    "strong counter-frame alignment plus strength blocks" in runtime and
    "strong entry conflict blocks a strong anchor" in runtime and
    "top-down absolute-strength evaluation is deterministic" in runtime,
)

check(
    "H2 audit is accumulated in Source/Architecture CI",
    "audit_phase_8_2.py" in workflow,
)

check(
    "execution-toggle synchronization state is read and guarded",
    "_executionToggleSyncing" in state and
    "if (_executionToggleSyncing)" in sync and
    "_executionToggleSyncing = true" in sync and
    "_executionToggleSyncing = false" in sync,
)

print("CR8.2 / H2 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR8.2 / H2 STATIC GATE PASS")
