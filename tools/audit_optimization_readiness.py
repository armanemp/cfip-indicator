from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")

def read(rel: str) -> str:
    path = ROOT / rel
    if not path.exists():
        raise SystemExit(f"Missing required optimization owner: {rel}")
    return path.read_text(encoding="utf-8")

memory = read("Trading/Intelligence/OutcomeMemoryStore.cs")
outcome = read("Trading/Intelligence/OutcomeTelemetryEngine.cs")
risk = read("Trading/Risk/AdaptiveOutcomeRiskPolicy.cs")
risk_owner = read("Trading/Risk/AutoRiskPolicy.cs")
state = read("Indicator/State.cs")
init = read("Runtime/Initialization/RuntimeInitialization.cs")

if "LocalStorageScope.Type" not in memory:
    raise SystemExit("Persistent memory must use cTrader LocalStorage")
if "OutcomeMemoryMaxAgeDays = 90" not in memory:
    raise SystemExit("Persistent memory age bound is missing")
if "OutcomeMemoryKey()" not in memory or "MemoryConfigurationFingerprint()" not in memory:
    raise SystemExit("Persistent memory must be configuration-scoped")
if "_outcomeHistory.Count - 128" not in memory:
    raise SystemExit("Persistent memory must retain at most 128 outcomes")
if "PersistOutcomeHistory()" not in outcome:
    raise SystemExit("Broker-confirmed outcomes must persist")
if "RestoreOutcomeHistory()" not in init:
    raise SystemExit("Outcome memory must restore during initialization")
if "EnableOutcomeTelemetry" not in risk_owner:
    raise SystemExit("Outcome-aware risk must honor telemetry enablement")
if "AdaptiveOutcomeRiskPolicy.Calculate(" not in risk_owner:
    raise SystemExit("Auto risk must consume the adaptive outcome policy")

for token in ("outcomes.Count < 8", "outcomes.Count - 12", "0.18", "0.10", "NumericGuards.Clamp"):
    if token not in risk:
        raise SystemExit(f"Adaptive outcome risk safeguard missing: {token}")

if "public bool" in memory or re.search(r"\[Parameter\s*\(", memory):
    raise SystemExit("Optimization memory owner must not introduce public parameters")

if "RiskPercentEquity" not in risk_owner or "SuitabilityRiskMultiplier()" not in risk_owner:
    raise SystemExit("Adaptive outcome risk must remain downstream of canonical suitability risk")

parameter_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in sorted((ROOT / "Indicator" / "Parameters").glob("*.cs"))
)
parameter_count = len(re.findall(r"\[Parameter\s*\(", parameter_source))
if parameter_count != 521:
    raise SystemExit(f"Public parameter count changed: {parameter_count}")

if "AccessRights.FullAccess" in read("Indicator/CFIPIndicator.cs"):
    raise SystemExit("Indicator must not request FullAccess")

print("Optimization readiness audit PASS")
print("Persistent outcome memory: LocalStorage / 90-day bound / 128-record bound / configuration scoped")
print("Adaptive risk: minimum 8 observations / latest 12 outcomes / hard penalty cap")
print("Canonical risk authority: preserved")
print(f"Public parameters scanned: {parameter_count}")
