from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8")

def check(condition, message):
    if not condition:
        raise SystemExit("CI-17 audit failed: " + message)

bot = read("preflight/CFIPPreflightBot.cs")
preflight_project = read("preflight/CFIP.Preflight.csproj")
preflight_probe_project = read("preflight/CFIP.Preflight.Probe.csproj")
probe = read("preflight/CFIPPreflightProbeIndicator.cs")
preflight_audit = read("tools/audit_cbot_preflight.py")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
phase = read("docs/PHASE-CI-FULL-STACK-CALCULATION-ANALYTICAL-INTEGRITY.md")

forbidden = (
    "ExecuteMarketOrder(",
    "ExecuteMarketRangeOrder(",
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
    "ModifyPosition(",
    "ModifyPendingOrder(",
    "ClosePosition(",
    "CancelPendingOrder(",
)

check(
    all(token not in bot for token in forbidden) and
    all(token not in probe for token in forbidden),
    "target-terminal probe remains strictly no-trade",
)

check(
    "Server.TimeInUtc" in bot and
    "Symbol.Bid" in bot and
    "Symbol.Ask" in bot and
    "Symbol.PipSize" in bot and
    "Bars.Count" in bot and
    "Bars.OpenTimes" in bot,
    "probe records real terminal clock, quote and bar observations",
)

check(
    "FirstCalculatedUtc" in probe and
    "LastCalculatedUtc" in probe and
    "Revision" in probe and
    "Scope" in probe,
    "probe exposes calculation liveness and scope identity",
)

check(
    "startupToFirstTickMs" in bot and
    "probeCalcAgeMs" in bot and
    "spreadPips" in bot and
    "latestBarOpenUtc" in bot,
    "runtime timing fields are emitted",
)

check(
    'PackageReference Include="cTrader.Automate" Version="1.0.21"' in preflight_project and
    'ProjectReference Include="../src/CFIP.Indicator/CFIP.Indicator.csproj"' in preflight_project and
    'ProjectReference Include="CFIP.Preflight.Probe.csproj"' in preflight_project and
    'Compile Remove="CFIPPreflightProbeIndicator.cs"' in preflight_project and
    'Compile Include="CFIPPreflightProbeIndicator.cs"' in preflight_probe_project and
    'Compile Remove="CFIPPreflightBot.cs"' in preflight_probe_project,
    "target-terminal bot and probe are compiled as separate single-algo assemblies",
)

check(
    "CFIP PREFLIGHT RESULT" in bot and
    "NO BROKER MUTATION EXECUTED" in bot,
    "preflight result remains an explicit no-mutation boundary",
)

check(
    "tools/audit_phase_ci_17.py" in workflow and
    "audit_phase_ci_17.py" in workflow,
    "CI-17 audit is accumulated in Source/Architecture",
)

check(
    "### CI-17 — Target-terminal cTrader validation" in phase and
    "indicator initialization" in phase and
    "Ask/Bid behavior" in phase and
    "broker distance rules" in phase and
    "pending fill" in phase and
    "reconnect/reload" in phase and
    "chart/panel timing" in phase and
    "signal and plan synchronization" in phase,
    "master CI track still lists every mandatory terminal acceptance boundary",
)

check(
    "### CI-17 target-terminal acceptance package — 2026-10-02" in roadmap,
    "ROADMAP contains the CI-17 acceptance package record",
)

print("CI-17 target-terminal readiness/static audit PASS")
