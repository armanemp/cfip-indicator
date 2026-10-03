#!/usr/bin/env python3
"""Static acceptance gate for CR7.3 / G3 plan-line display parameter truth."""

from pathlib import Path
import re
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

parameter = read("src/CFIP.Indicator/Indicator/Parameters/14_display_core.cs")
rule = read("src/CFIP.Indicator/UI/Chart/PlanLinePresentationRule.cs")
renderer = read("src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
development = read("docs/DEVELOPMENT-LOG.md")
phase = read("docs/PHASE-CR7-3-G3-PLAN-LINE-THICKNESS.md")

check(
    "existing public Level Line Thickness parameter remains named and bounded 1..3",
    re.search(
        r'\[Parameter\("Level Line Thickness"[^\n]*DefaultValue\s*=\s*1[^\n]*MinValue\s*=\s*1[^\n]*MaxValue\s*=\s*3',
        parameter,
    ) is not None
)

check(
    "canonical presentation rule exists with one-pixel thickness owner",
    "class PlanLinePresentationRule" in rule and
    "MinimumThickness = 1" in rule and
    "return MinimumThickness;" in rule and
    "ResolveThickness(" in rule
)

check(
    "configured thickness resolves through the canonical one-pixel rule",
    "PlanLineThicknessRule.ResolveThickness(" in renderer and
    "LevelLineThickness" in renderer and
    "Math.Min(1" not in renderer and
    "Math.Min(1," not in renderer
)

check(
    "resolved thickness reaches both new and existing chart trend lines",
    "Chart.DrawTrendLine(" in renderer and
    "thickness," in renderer and
    "line.Thickness =\n                    thickness;" in renderer
)

check(
    "Solid remains the only level-line style",
    "return LineStyle.Solid;" in renderer and
    "ResolvePlanLineStyle(" in renderer
)

check(
    "deterministic runtime contract covers 1/2/3 and safe bounds",
    "VerifyPlanLineThicknessG3();" in contracts and
    "PlanLineThicknessRule.ResolveThickness(1) == 1" in contracts and
    "PlanLineThicknessRule.ResolveThickness(2) == 1" in contracts and
    "PlanLineThicknessRule.ResolveThickness(3) == 1" in contracts and
    "PlanLineThicknessRule.ResolveThickness(0) == 1" in contracts and
    "PlanLineThicknessRule.ResolveThickness(4) == 1" in contracts
)

check(
    "runtime contract project compiles the canonical G3 rule",
    "Core/Math/PlanLineThicknessRule.cs" in contracts_project
)

check(
    "G3 audit is accumulated after G2",
    "audit_phase_7_2.py" in workflow and
    "audit_phase_7_3.py" in workflow and
    workflow.index("audit_phase_7_3.py") > workflow.index("audit_phase_7_2.py")
)

check(
    "roadmap advances from G3 to G4",
    "CR7.3 / G3 closeout" in roadmap and
    "**Next phase: CR7.4 / G4" in roadmap
)

check(
    "continuation state advances from G3 to G4",
    "CR7.3 / G3 closeout" in continuation and
    "Current phase: CR7.4 / G4" in continuation
)

check(
    "Claude remediation continuity advances from G3 to G4",
    "CR7.3 / G3 closeout" in review and
    "**Current active phase: CR7.4 / G4" in review
)

check(
    "development log records G3 closeout and next phase",
    "CR7.3 / G3" in development and
    "Next phase: **CR7.4 / G4" in development
)

check(
    "phase document records the superseding one-pixel presentation contract",
    "Status: **VERIFIED COMPLETE" in phase and
    "superseded" in phase and
    "one-pixel" in phase and
    "no public parameter name/type/DefaultValue changed" in phase
)

print("CR7.3 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR7.3 STATIC GATE PASS")
