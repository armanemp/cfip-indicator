from pathlib import Path

ROOT = Path("src/CFIP.Indicator")


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing G6A source: {relative}")
    return path.read_text(encoding="utf-8")


panel = read("UI/Panel/PanelRenderOptimization.cs")
auto_state = read("Trading/Execution/State/AutoTradingStateStore.cs")
telemetry = read("Trading/Intelligence/OutcomeTelemetryEngine.cs")
identity = read("Core/Math/ExecutionPanelPresentationIdentityRule.cs")
program = Path("tools/CFIP.Runtime.Contracts/Program.cs").read_text(encoding="utf-8")
runtime_csproj = Path("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj").read_text(encoding="utf-8")
workflow = Path(".github/workflows/source-check.yml").read_text(encoding="utf-8")

if "ExecutionPanelPresentationIdentityRule.Compose(" not in panel:
    raise SystemExit("G6A canonical execution presentation identity is not consumed by the panel key")

compose = panel.split(
    "ExecutionPanelPresentationIdentityRule.Compose(",
    1,
)[1].split(
    "),",
    1,
)[0]

for token in (
    "_autoTradingState",
    "_autoTradingReason",
    "_lastExecutionTelemetryPath",
    "_lastExecutionTelemetryState",
    "_activeExecutionScenarioId",
    "_marketSuitabilityScore",
    "_marketSuitabilityState",
    "_marketSuitabilityReason",
    "_lastBreakEvenDiagnostic",
):
    if token not in compose:
        raise SystemExit(f"G6A presentation identity is missing visible execution input: {token}")

state_method = auto_state.split(
    "private void SetAutoTradingState(",
    1,
)[1].split(
    "private string AutoTradingPanelLine(",
    1,
)[0]

if "InvalidatePanelExecutionProtectionStateCache();" not in state_method:
    raise SystemExit("G6A auto-trading state changes must invalidate the canonical panel snapshot")

if "RecordExecutionTelemetryHistory(" in telemetry:
    method = telemetry.split(
        "private void RecordExecutionTelemetryHistory(",
        1,
    )[1].split(
        "private void RecordLifecycleTelemetry(",
        1,
    )[0]
else:
    raise SystemExit("G6A execution telemetry owner not found")

if "InvalidatePanelExecutionProtectionStateCache();" in method:
    raise SystemExit(
        "G6A telemetry-only presentation changes must not force broker-state snapshot invalidation"
    )

for token in (
    "public static string Compose(",
    "NormalizePanelIdentityField(autoTradingState)",
    "NormalizePanelIdentityField(autoTradingReason)",
    "NormalizePanelIdentityField(executionTelemetryPath)",
    "NormalizePanelIdentityField(executionTelemetryState)",
    "NormalizePanelIdentityField(activeExecutionScenarioId)",
    "marketSuitabilityScore",
    "NormalizePanelIdentityField(marketSuitabilityState)",
    "NormalizePanelIdentityField(marketSuitabilityReason)",
    "NormalizePanelIdentityField(breakEvenDiagnostic)",
):
    if token not in identity:
        raise SystemExit(f"G6A canonical identity owner missing: {token}")

if "VerifyExecutionPanelPresentationIdentityG6A();" not in program:
    raise SystemExit("G6A runtime contract is not invoked")

if "ExecutionPanelPresentationIdentityRule.cs" not in runtime_csproj:
    raise SystemExit("G6A identity rule is not compiled by the runtime contract project")

if "python tools/audit_phase_7_6a.py" not in workflow:
    raise SystemExit("G6A static audit is not in Source/Architecture CI")

print("CR7.6a / G6A execution panel presentation freshness audit PASS")
print("visible execution-state identity is canonical and complete")
print("auto-trading state/reason mutations invalidate the G4 snapshot")
print("telemetry-only presentation changes do not force broker-state reads")
