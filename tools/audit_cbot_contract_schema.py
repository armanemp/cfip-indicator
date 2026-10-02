#!/usr/bin/env python3
"""CBOT-P1 platform-neutral contract schema audit."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
CONTRACTS = ROOT / "src" / "CFIP.Contracts"

required = {
    "ContractEnums.cs",
    "IdentityContracts.cs",
    "PlanSnapshot.cs",
    "SignalEnvelope.cs",
    "ExecutionIntent.cs",
    "ManagementCommand.cs",
    "BrokerExecutionReport.cs",
    "LifecycleEvent.cs",
    "MarketExecutionProfile.cs",
    "ContractVersion.cs",
}

# Pure, platform-neutral transport utilities are allowed to expose behavior:
# serialization and deterministic bus-key derivation are part of the shared
# transport contract, not broker execution ownership.
TRANSPORT_UTILITY_FILES = {
    "SignalEnvelopeCodec.cs",
    "SignalBusKey.cs",
    "ManagementBusKey.cs",
    "CbotExecutionStateBus.cs",
    "ContractBusKeyHash.cs",
}

errors = []

if not CONTRACTS.exists():
    errors.append("src/CFIP.Contracts is missing")
else:
    names = {p.name for p in CONTRACTS.glob("*.cs")}
    missing = sorted(required - names)
    if missing:
        errors.extend("missing contract source: " + x for x in missing)

    for path in CONTRACTS.glob("*.cs"):
        source = path.read_text(encoding="utf-8")
        rel = path.relative_to(ROOT).as_posix()

        for forbidden in (
            r"\bcAlgo\b",
            r"\bcTrader\.Automate\b",
            r"\bcAlgo\.API\b",
            r"\bRobot\b",
            r"\bIndicator\b",
        ):
            if re.search(forbidden, source):
                errors.append(
                    f"platform dependency in {rel}: {forbidden}"
                )

        if re.search(
            r'\b(public|internal|private|protected)\s+'
            r'[A-Za-z0-9_<>,.?\[\]]+\s+'
            r'[A-Za-z_][A-Za-z0-9_]*\s*'
            r'\{\s*get\s*;\s*set\s*;',
            source,
        ):
            errors.append(f"mutable auto-property in contract source: {rel}")

        if re.search(r'\bset\s*;', source):
            errors.append(f"setter found in contract source: {rel}")

        if (
            path.name not in TRANSPORT_UTILITY_FILES
            and (
                "static void " in source
                or " static bool " in source
                or " static int " in source
            )
        ):
            errors.append(f"behavioral method found in data contract source: {rel}")

expected_records = (
    "public sealed record ContractIdentity(",
    "public sealed record PlanSnapshot(",
    "public sealed record SignalEnvelope(",
    "public sealed record ExecutionIntent(",
    "public sealed record ManagementCommand(",
    "public sealed record BrokerExecutionReport(",
    "public sealed record LifecycleEvent(",
    "public sealed record MarketExecutionProfile(",
)

for token in expected_records:
    path = CONTRACTS / {
        "ContractIdentity": "IdentityContracts.cs",
        "PlanSnapshot": "PlanSnapshot.cs",
        "SignalEnvelope": "SignalEnvelope.cs",
        "ExecutionIntent": "ExecutionIntent.cs",
        "ManagementCommand": "ManagementCommand.cs",
        "BrokerExecutionReport": "BrokerExecutionReport.cs",
        "LifecycleEvent": "LifecycleEvent.cs",
        "MarketExecutionProfile": "MarketExecutionProfile.cs",
    }[token.split()[-1].split("(")[0]]
    source = path.read_text(encoding="utf-8") if path.exists() else ""
    if token not in source:
        errors.append(f"required immutable record missing: {token}")

if errors:
    print("CBOT-P1 CONTRACT SCHEMA AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P1 CONTRACT SCHEMA AUDIT: PASS")
print(f"Contract source files: {len(list(CONTRACTS.glob('*.cs')))}")
print("Platform dependency: 0")
print("Mutable setters: 0")
print("Required immutable records: PASS")
