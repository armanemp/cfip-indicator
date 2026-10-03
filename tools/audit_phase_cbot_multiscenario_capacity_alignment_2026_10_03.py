#!/usr/bin/env python3
"""cBot bounded multi-scenario capacity alignment regression gate."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
bot = ROOT / "src/CFIP.cBot/CFIPExecutionBot.cs"
capacity = ROOT / "src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs"
workflow = ROOT / ".github/workflows/source-check.yml"
errors = []

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

bot_text = bot.read_text(encoding="utf-8") if bot.exists() else ""
capacity_text = capacity.read_text(encoding="utf-8") if capacity.exists() else ""
workflow_text = workflow.read_text(encoding="utf-8") if workflow.exists() else ""

check(
    "bounded concurrent capacity defaults to five with absolute maximum ten",
    '"Max Concurrent Scenarios"' in bot_text and
    "DefaultValue = 5" in bot_text and
    "MaxValue = 10" in bot_text
)

check(
    "demo session execution default is ten with absolute maximum twenty",
    '"Max Demo Executions Per Session"' in bot_text and
    "DefaultValue = 10" in bot_text and
    "MaxValue = 20" in bot_text
)

check(
    "multi-scenario capacity remains cBot-owned and scenario-aware",
    "CountManagedScenarioObjects(" in capacity_text and
    "MaxConcurrentScenarios" in bot_text and
    "CONCURRENT SCENARIO CAPACITY BLOCKED" in capacity_text
)

check(
    "live-account block remains fail-closed",
    "if (Account.IsLive)" in bot_text and
    "this build is demo-only" in bot_text
)

check(
    "capacity alignment audit is accumulated in Source/Architecture CI",
    "python tools/audit_phase_cbot_multiscenario_capacity_alignment_2026_10_03.py" in workflow_text
)

if errors:
    print("CBOT MULTI-SCENARIO CAPACITY ALIGNMENT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CBOT MULTI-SCENARIO CAPACITY ALIGNMENT: PASS")
