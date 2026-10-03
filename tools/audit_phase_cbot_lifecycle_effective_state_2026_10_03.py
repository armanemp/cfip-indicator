#!/usr/bin/env python3
"""cBot effective lifecycle-state regression audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path):
    p = ROOT / path
    if not p.exists():
        errors.append("missing " + path)
        return ""
    return p.read_text(encoding="utf-8")

def require(condition, message):
    if not condition:
        errors.append(message)

rule = read("src/CFIP.cBot/Execution/CbotExecutionLifecycleRule.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
test = read("tools/CFIP.cBot.Shadow.Tests/Program.cs")
csproj = read("tools/CFIP.cBot.Shadow.Tests/CFIP.cBot.Shadow.Tests.csproj")
workflow = read(".github/workflows/source-check.yml")

require("AllowsExecution(" in rule, "canonical lifecycle rule missing")
for state in ("READY", "ACTIVE", "PENDING"):
    require(
        'StartsWithState(state, "' + state + '")' in rule,
        "lifecycle rule missing family: " + state,
    )
require(
    'state.StartsWith(
                    prefix + " /"' in rule,
    "lifecycle variants are not accepted by a prefix-aware rule",
)
require(
    "recoveryRequired ||" in rule,
    "recovery flag must fail closed",
)
require(
    "CbotExecutionLifecycleRule.AllowsExecution(" in publisher,
    "publisher does not consume canonical lifecycle rule",
)
require(
    '"ACTIVE / RECONCILED"' in test and
    '"ACTIVE / MULTI-SCENARIO"' in test and
    '"PENDING / MULTI-SCENARIO"' in test and
    '"RECOVERY REQUIRED"' in test,
    "behavioral lifecycle variants are not covered",
)
require(
    "CbotExecutionLifecycleRule.cs" in csproj,
    "shadow behavioral test project does not compile the canonical lifecycle rule",
)
require(
    "audit_phase_cbot_lifecycle_effective_state_2026_10_03.py" in workflow,
    "dedicated lifecycle-state audit is not wired into CI",
)

if errors:
    print("CBOT LIFECYCLE EFFECTIVE STATE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT LIFECYCLE EFFECTIVE STATE AUDIT: PASS")
