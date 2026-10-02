#!/usr/bin/env python3
"""CBOT-0 boundary inventory and execution-authority freeze audit.

This is a read-only inventory gate. It must not change production behavior.
It scans the current Indicator source tree for direct broker mutation APIs,
broker/account state access, execution-related parameters, and parameter usage
across analytical/execution/UI domains.

The gate intentionally freezes the current architecture before any cBot code is
created. It does not create CFIP.Contracts or CFIP.cBot and does not move code.
"""

from __future__ import annotations

import re
import sys
from pathlib import Path
from collections import Counter, defaultdict

ROOT = Path(__file__).resolve().parents[1]
PROD = ROOT / "src" / "CFIP.Indicator"

EXPECTED_PARAMETER_COUNT = 568

BROKER_MUTATION_APIS = (
    "ExecuteMarketOrder",
    "ExecuteMarketRangeOrder",
    "PlaceStopOrder",
    "PlaceLimitOrder",
    "ModifyPosition",
    "ModifyPendingOrder",
    "ClosePosition",
    "CancelPendingOrder",
    "ModifyStopLossPrice",
    "ModifyTakeProfitPrice",
    "ModifyTakeProfitPips",
    "ModifyTakeProfit",
)

KNOWN_BROKER_MUTATION_OWNERS = {
    "Trading/Execution/BrokerAggressiveOrderMutation.cs",
    "Trading/Execution/BrokerPendingOrderPlacement.cs",
    "Trading/Execution/BrokerLimitOrderPlacement.cs",
    "Trading/Execution/BrokerPendingOrderCancellation.cs",
    "Trading/Execution/BrokerPositionCloseMutation.cs",
    "Trading/Execution/BrokerStopLossMutation.cs",
    "Trading/Execution/BrokerTakeProfitMutation.cs",
}

EXECUTION_DOMAIN_PREFIXES = (
    "Trading/Execution/",
    "Trading/Aggressive/",
    "Trading/LiveManagement/",
    "Trading/Lifecycle/",
    "Trading/Pending/",
    "Trading/Risk/",
    "Trading/Identity/",
)

ANALYTICAL_DOMAIN_PREFIXES = (
    "Analysis/",
    "Planning/",
)

UI_PREFIX = "UI/"

EXECUTION_RELATED_PATTERN = re.compile(
    r"(auto|trade|order|position|broker|account|margin|spread|daily|"
    r"session|live|trail|break.?even|protect|execution|pending|partial|"
    r"target|reprice|close|sizing|volume|cooldown|permission|managed)",
    re.I,
)

PARAMETER_RE = re.compile(
    r'\[Parameter\s*\((?P<attribute>[^\]]*)\]\s*'
    r'public\s+(?P<type>[A-Za-z0-9_<>\[\],.?]+)\s+'
    r'(?P<name>[A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}',
    re.S,
)

METHOD_RE = re.compile(
    r"(?m)\b(?:private|public|protected|internal)\s+"
    r"(?:static\s+)?[A-Za-z0-9_<>\[\]?]+\s+"
    r"(?P<name>[A-Za-z0-9_]+)\s*\("
)

def rel(path: Path) -> str:
    return path.relative_to(PROD).as_posix()

def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")

def find_method(source: str, pos: int) -> str:
    method = None
    for match in METHOD_RE.finditer(source, 0, pos):
        method = match.group("name")
    return method or "<unknown>"

def domain_for(path: Path) -> str:
    p = rel(path)
    if p.startswith(EXECUTION_DOMAIN_PREFIXES):
        return "execution"
    if p.startswith(ANALYTICAL_DOMAIN_PREFIXES):
        return "analysis"
    if p.startswith(UI_PREFIX):
        return "ui"
    if p.startswith("Runtime/"):
        return "runtime"
    if p.startswith("Indicator/Parameters/"):
        return "parameter"
    if p.startswith("Core/"):
        return "core"
    return "other"

def parse_parameters(parameter_file_paths: list[Path]) -> list[dict[str, str]]:
    parsed: list[dict[str, str]] = []
    for path in parameter_file_paths:
        source = read_text(path)
        for match in PARAMETER_RE.finditer(source):
            parsed.append(
                {
                    "file": path.name,
                    "label": (
                        re.search(
                            r'^\s*"([^"]+)"',
                            match.group("attribute") or "",
                        ).group(1)
                        if re.search(
                            r'^\s*"([^"]+)"',
                            match.group("attribute") or "",
                        )
                        else path.name
                    ),
                    "type": match.group("type"),
                    "name": match.group("name"),
                }
            )
    return parsed

def classify_parameter(
    parameter: dict[str, str],
    usage: dict[str, set[str]],
) -> str:
    name = parameter["name"]
    domains = usage.get(name, set())

    if "execution" in domains and "analysis" in domains:
        return "SPLIT"
    if "execution" in domains:
        return "CBOT"
    if "analysis" in domains or "ui" in domains:
        return "INDICATOR"

    # Explicit safe defaults for parameters that are control/presentation only
    # or whose references are concentrated in runtime/core glue.
    if parameter["file"] in {"10_live_management.cs", "13_auto_trading.cs"}:
        return "CBOT"
    return "INDICATOR"

def main() -> int:
    errors: list[str] = []
    cs_files = sorted(PROD.rglob("*.cs"))
    if not cs_files:
        print("ERROR: production C# tree is missing")
        return 1

    texts = {rel(path): read_text(path) for path in cs_files}

    # 1) Exact direct broker mutation inventory.
    mutation_hits: list[tuple[str, str, str]] = []
    for path, source in texts.items():
        for api in BROKER_MUTATION_APIS:
            pattern = re.compile(r"\b" + re.escape(api) + r"\s*\(")
            for match in pattern.finditer(source):
                mutation_hits.append(
                    (path, find_method(source, match.start()), api)
                )

    print("CBOT-0 DIRECT BROKER MUTATION INVENTORY")
    print("=" * 72)
    # P4A: market mutation has physically moved to cBot.
    market_apis_remaining = [
        api for path, _, api in mutation_hits
        if api in ("ExecuteMarketOrder", "ExecuteMarketRangeOrder")
    ]
    if market_apis_remaining:
        errors.append(
            "market broker mutation remains inside Indicator after CBOT-P4A extraction: "
            + ", ".join(market_apis_remaining)
        )

    if mutation_hits:
        for path, method, api in mutation_hits:
            owner_ok = path in KNOWN_BROKER_MUTATION_OWNERS
            print(
                f"{'OK ' if owner_ok else 'ERR'} "
                f"{api:26s} {path} :: {method}"
            )
            if not owner_ok:
                errors.append(
                    f"direct broker mutation {api} is outside the frozen owner set: "
                    f"{path}::{method}"
                )
    else:
        errors.append("no direct broker mutation calls were found; baseline inventory is incomplete")

    # 2) Broker/account state access inventory.
    state_tokens = (
        "Positions",
        "PendingOrders",
        "Account",
        "Symbol",
        "Server",
        "Timer",
    )
    print("\nCBOT-0 BROKER/ACCOUNT STATE ACCESS BY EXECUTION DOMAIN")
    print("=" * 72)
    access_rows = 0
    for path, source in texts.items():
        if not path.startswith(EXECUTION_DOMAIN_PREFIXES):
            continue
        hits = []
        for token in state_tokens:
            if re.search(r"\b" + re.escape(token) + r"\b", source):
                hits.append(token)
        if hits:
            access_rows += 1
            print(f"{path:78s} {'/'.join(hits)}")
    if access_rows == 0:
        errors.append("no execution-domain broker/account state access was found")

    # 3) Lifecycle/event ownership markers.
    lifecycle_tokens = (
        "OnPositionOpened",
        "OnPositionModified",
        "OnPositionClosed",
        "OnPendingOrderCreated",
        "OnPendingOrderModified",
        "OnPendingOrderFilled",
        "OnPendingOrderCancelled",
        "OnTimer",
    )
    print("\nCBOT-0 LIFECYCLE/TIMER OWNERSHIP MARKERS")
    print("=" * 72)
    lifecycle_rows = 0
    for path, source in texts.items():
        hits = [t for t in lifecycle_tokens if t in source]
        if hits:
            lifecycle_rows += 1
            print(f"{path:78s} {','.join(hits)}")
    if lifecycle_rows == 0:
        errors.append("no lifecycle/timer ownership markers were found")

    # 4) Parameter surface and usage domain matrix.
    parameter_paths = sorted((PROD / "Indicator" / "Parameters").glob("*.cs"))
    parameters = parse_parameters(parameter_paths)
    if len(parameters) != EXPECTED_PARAMETER_COUNT:
        errors.append(
            f"public parameter count changed: expected {EXPECTED_PARAMETER_COUNT}, "
            f"found {len(parameters)}"
        )

    usage: dict[str, set[str]] = defaultdict(set)
    usage_files: dict[str, set[str]] = defaultdict(set)

    # One combined identifier scan per source file avoids recompiling and
    # executing hundreds of separate regular expressions for every file.
    parameter_names = sorted({p["name"] for p in parameters}, key=len, reverse=True)
    usage_pattern = (
        re.compile(r"\\b(?:" + "|".join(re.escape(n) for n in parameter_names) + r")\\b")
        if parameter_names
        else None
    )
    name_set = set(parameter_names)

    if usage_pattern is not None:
        for path, source in texts.items():
            domain = domain_for(Path(PROD / path))
            for match in usage_pattern.finditer(source):
                name = match.group(0)
                if name in name_set:
                    usage[name].add(domain)
                    usage_files[name].add(path)

    execution_related = [
        p for p in parameters
        if EXECUTION_RELATED_PATTERN.search(p["name"] + " " + p["label"])
    ]

    print("\nCBOT-0 EXECUTION-RELATED PARAMETER OWNERSHIP MATRIX")
    print("=" * 72)
    print("CLASS | NAME | FILE | USAGE DOMAINS")
    print("-" * 72)
    class_counts = Counter()
    for parameter in execution_related:
        cls = classify_parameter(parameter, usage)
        class_counts[cls] += 1
        domains = ",".join(sorted(usage.get(parameter["name"], set()))) or "unreferenced"
        print(
            f"{cls:8s} | {parameter['name']:44s} | "
            f"{parameter['file']:28s} | {domains}"
        )

    # 5) Parameter duplicate-owner guard: one current name must not appear in
    # multiple parameter declaration files.
    locations: dict[str, list[str]] = defaultdict(list)
    for p in parameters:
        locations[p["name"]].append(p["file"])
    duplicates = {k: v for k, v in locations.items() if len(v) > 1}
    if duplicates:
        for name, files in sorted(duplicates.items()):
            errors.append(
                f"duplicate public parameter declaration '{name}' in {', '.join(files)}"
            )

    print("\nCBOT-0 SUMMARY")
    print("=" * 72)
    print(f"Production C# files: {len(cs_files)}")
    print(f"Public parameters:    {len(parameters)}")
    print(f"Execution-related:    {len(execution_related)}")
    print(
        "Parameter classes: "
        + ", ".join(f"{k}={v}" for k, v in sorted(class_counts.items()))
    )
    print(f"Direct broker calls:  {len(mutation_hits)}")
    print(f"Failures:             {len(errors)}")

    if errors:
        print("\nCBOT-0 FAILURES")
        for error in errors:
            print(f"- {error}")
        return 1

    print("\nCBOT-0 PASS — boundary inventory is frozen; no production behavior changed.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
