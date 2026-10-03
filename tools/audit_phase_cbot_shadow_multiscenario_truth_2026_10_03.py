#!/usr/bin/env python3
"""CBOT shadow-host multi-scenario revision/idempotency regression gate."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path: str) -> str:
    p = ROOT / path
    if not p.exists():
        errors.append("missing: " + path)
        return ""
    return p.read_text(encoding="utf-8")

def check(name: str, ok: bool):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

coord = read("src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs")
batch = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
workflow = read(".github/workflows/source-check.yml")

check(
    "shadow host stores last accepted revision/idempotency per ScenarioId",
    "_lastAcceptedRevisionByScenario" in coord and
    "_lastAcceptedIdempotencyKeyByScenario" in coord and
    "GetLastRevision(scenarioKey)" in coord and
    "GetLastIdempotencyKey(scenarioKey)" in coord
)
check(
    "shadow validation uses scenario-scoped revision truth",
    "lastScenarioRevision" in coord and
    "lastScenarioIdempotencyKey" in coord and
    "ShadowHostValidator.Validate(" in coord
)
check(
    "batch envelopes may share a provider revision while retaining independent ScenarioIds",
    "candidate.ScenarioId" in batch and
    "candidate.CreatedM5 == closedM5" in batch and
    "new SignalScenarioBatch(" in batch
)
check(
    "cBot processes every scenario in the batch",
    "for (int scenarioIndex = 0;" in bot and
    "ProcessSignalEnvelope(\n                        scenarios[scenarioIndex]," in bot
)
check(
    "shadow global telemetry remains available without being the per-scenario gate",
    "public long LastAcceptedRevision" in coord and
    "public string LastAcceptedIdempotencyKey" in coord and
    "GetLastRevision(scenarioKey)" in coord
)
check(
    "dedicated multi-scenario shadow audit is accumulated into Source/Architecture CI",
    "python tools/audit_phase_cbot_shadow_multiscenario_truth_2026_10_03.py" in workflow
)

if errors:
    print("=" * 72)
    print("FAIL | CBOT SHADOW MULTI-SCENARIO TRUTH")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("CBOT SHADOW MULTI-SCENARIO TRUTH: PASS")
