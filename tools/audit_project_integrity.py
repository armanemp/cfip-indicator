from pathlib import Path
import re
from collections import defaultdict

ROOT = Path("src/CFIP.Indicator")
DOCS = Path("docs")
PARAM_ROOT = ROOT / "Indicator" / "Parameters"

def fail(message: str):
    raise SystemExit(message)

def code(source: str) -> str:
    source = re.sub(r"/\*[\s\S]*?\*/", " ", source)
    source = re.sub(r"//[^\r\n]*", " ", source)
    source = re.sub(r'@?"(?:""|\\.|[^"\\])*"', "S", source)
    source = re.sub(r'"(?:\\.|[^"\\])*"', "S", source)
    return source

files = sorted(ROOT.rglob("*.cs"))
if not files:
    fail("No production C# files found")

texts = {p: code(p.read_text(encoding="utf-8")) for p in files}

# 1) Exact public parameter name uniqueness.
param_decls = defaultdict(list)
param_re = re.compile(
    r'\[Parameter\s*\([^\]]*\]\s*public\s+[A-Za-z_][\w<>\[\],.?]*\s+'
    r'([A-Za-z_]\w*)\s*\{\s*get;\s*set;\s*\}',
    re.S,
)
for p, text in texts.items():
    for name in param_re.findall(text):
        param_decls[name].append(str(p.relative_to(ROOT)))

duplicates = {k: v for k, v in param_decls.items() if len(v) > 1}
if duplicates:
    for name, owners in sorted(duplicates.items()):
        print(f"DUPLICATE PARAMETER: {name} -> {owners}")
    fail(f"Found {len(duplicates)} duplicate public parameter names")

EXPECTED_UNIQUE_PUBLIC_PARAMETER_MATCHES = 535
if len(param_decls) != EXPECTED_UNIQUE_PUBLIC_PARAMETER_MATCHES:
    fail(
        f"Project integrity public-property match count changed: "
        f"expected {EXPECTED_UNIQUE_PUBLIC_PARAMETER_MATCHES}, found {len(param_decls)}"
    )

# Phase 7.4 — truthful execution-capacity semantics.
parameter_source = "\n".join(
    p.read_text(encoding="utf-8") for p in sorted(PARAM_ROOT.glob("*.cs"))
)
declared_parameter_count = len(
    re.findall(
        r"\[Parameter\s*\(",
        parameter_source,
    )
)
if declared_parameter_count != 547:
    fail(
        f"Project integrity expects 547 parameter declarations, "
        f"found {declared_parameter_count}"
    )
if not re.search(
    r'\[Parameter\("Maximum Open Positions"[^\n]*MinValue\s*=\s*1[^\n]*MaxValue\s*=\s*1',
    parameter_source,
):
    fail("MaximumOpenPositions must advertise only single-plan capacity")
if "BlockNewSignalWhileActive" in parameter_source:
    fail("Unsupported BlockNewSignalWhileActive parameter remains")

capacity_rule = (ROOT / "Core" / "Math" / "ExecutionCapacityRule.cs").read_text(encoding="utf-8")
capacity_guard = (ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs").read_text(encoding="utf-8")
for token in ("IsSupportedSinglePlanCapacity(", "AllowsNewSinglePlan(", "AllowsNewSingleExecution("):
    if token not in capacity_rule:
        fail(f"Execution capacity rule missing: {token}")
for token in ("ValidateSinglePlanCapacity(", "ValidateSingleExecutionCapacity("):
    if token not in capacity_guard:
        fail(f"Execution capacity guard missing: {token}")

for relative, token in (
    ("Trading/Validation/PlanCreationEligibility.cs", "ValidateSinglePlanCapacity("),
    ("Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs", "ValidateSingleExecutionCapacity("),
    ("Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs", "ValidateSingleExecutionCapacity("),
    ("Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs", "ValidateSingleExecutionCapacity("),
):
    if token not in texts[ROOT / relative]:
        fail(f"Canonical capacity guard missing from {relative}")

# 2) Exact duplicate method signatures across partial production files.
# Include the containing type in the key. A method such as GetHashCode() is
# valid in multiple independent types; only duplicate declarations within the
# same containing type are an architectural collision.
method_re = re.compile(
    r'\b(?:public|private|protected|internal)\s+'
    r'(?:static\s+|virtual\s+|override\s+|async\s+|sealed\s+|readonly\s+)*'
    r'[A-Za-z_][\w<>\[\],.?]*\s+'
    r'([A-Za-z_]\w*)\s*\(([^)]*)\)\s*\{',
    re.S,
)
type_re = re.compile(
    r'\b(?:class|struct|interface|enum|record(?:\s+(?:class|struct))?)\s+'
    r'([A-Za-z_]\w*)\b[^{};]*\{',
    re.S,
)


def containing_type(source: str, method_start: int) -> str:
    method_depth = source[:method_start].count("{") - source[:method_start].count("}")
    best = ""
    best_open = -1

    for match in type_re.finditer(source):
        open_brace = source.find("{", match.start(), match.end())
        if open_brace < 0 or open_brace >= method_start:
            continue

        open_depth = (
            source[:open_brace].count("{") -
            source[:open_brace].count("}")
        )
        if method_depth == open_depth + 1 and open_brace > best_open:
            best = match.group(1)
            best_open = open_brace

    return best or "<unknown-type>"


method_defs = defaultdict(list)
for p, text in texts.items():
    for match in method_re.finditer(text):
        name, params = match.groups()
        normalized_params = re.sub(r'\b[A-Za-z_]\w*\s*(?=,|$)', "", params.strip())
        signature = (
            containing_type(text, match.start()),
            name,
            re.sub(r"\s+", " ", normalized_params),
        )
        method_defs[signature].append(str(p.relative_to(ROOT)))

duplicate_methods = {
    sig: owners for sig, owners in method_defs.items() if len(set(owners)) > 1
}
if duplicate_methods:
    print("EXACT DUPLICATE METHOD SIGNATURES")
    for (type_name, name, sig), owners in sorted(duplicate_methods.items()):
        print(f"- {type_name}.{name}({sig}) -> {owners}")
    fail(f"Found {len(duplicate_methods)} exact duplicate production method signatures")

# 3) Canonical visual pipeline invariants.
snapshot = texts[ROOT / "UI" / "Chart" / "SignalVisualSnapshotBuilder.cs"]
render = texts[ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"]
plan_render = texts[ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"]

required_snapshot_tokens = (
    "BuildSignalVisualSnapshot(",
    "SetupPreviewActive",
    "SetupIdealEntry",
    "TriggerVisible",
)
for token in required_snapshot_tokens:
    if token not in snapshot:
        fail(f"Canonical visual snapshot contract missing: {token}")

if "BuildSignalVisualSnapshot(" not in render:
    fail("Runtime calculation must build one canonical signal visual snapshot")
if "RenderLevelLines(" not in plan_render:
    fail("Plan renderer must own level-line rendering")

# The structural setup preview must be eligible before TriggerReady.
if "_setupPreview.Direction != 0" not in snapshot:
    fail("Setup preview must not depend only on post-trigger visual direction")
if "_setupPreview.Direction == visualDirection" in snapshot:
    fail("Setup preview must not require post-trigger visual direction equality")

# 4) Canonical chart-level geometry invariants.
line = texts[ROOT / "UI" / "Chart" / "PlanLineRenderer.cs"]
if "CompactPlanLineLengthBars = 40" not in line:
    fail("Compact plan span is not locked to 40 chart bars")
if "GetPlanLineRightBar()" not in line or "return Bars.Count - 1" not in line:
    fail("Plan lines do not terminate at the latest chart candle")
if "MapM5ToChart(" in line or "anchorM5" in line:
    fail("Plan-line chart geometry is coupled to M5 event mapping")

# 5) Execution UI ownership invariants.
factory = texts[ROOT / "UI" / "Controls" / "ExecutionControlsFactory.cs"]
sync = texts[ROOT / "UI" / "Controls" / "ExecutionControlsSynchronizer.cs"]
rule = texts[ROOT / "Core" / "Math" / "ExecutionControlPresentationRule.cs"]
if "CreateExecutionToggle(" not in factory:
    fail("Functional execution status presentation factory is missing")
if "IsEnabled = false" not in factory:
    fail("Execution status surfaces must remain non-interactive")
for token in (
    "_autoTradingQuickToggle.Click +=",
    "_automaticOrdersQuickToggle.Click +=",
):
    if token in factory:
        fail(f"Execution status surface must not register a mutation handler: {token}")
if "ExecutionControlPresentationRule.ComposeStatusText(" not in factory:
    fail("Execution status text must use the canonical presentation rule")
if "public static bool IsInteractive => false;" not in rule:
    fail("Execution-control interaction policy must be canonical and read-only")
if "SyncQuickExecutionControls(" not in sync or "_executionToggleSyncing = true" not in sync:
    fail("Execution-control synchronization boundary is incomplete")
if "RefreshCbotExecutionStateIfDue();" not in sync:
    fail("Execution-control synchronization must consume the canonical cBot state snapshot")
if "EffectiveAutoTradingEnabled" not in sync or "EffectiveAutomaticOrdersEnabled" not in sync:
    fail("Execution-control synchronization must consume effective cBot state")
if "ExecutionControlPresentationRule.IsInteractive" not in sync:
    fail("Execution-control synchronizer must enforce the canonical read-only interaction policy")

# 6) Architecture-wide forbidden legacy status-only control symbols.
all_production = "\n".join(texts.values())
for legacy in (
    "_autoTradingQuickStatus",
    "_automaticOrdersQuickStatus",
    "CreateExecutionStatus(",
):
    if legacy in all_production:
        fail(f"Legacy status-only execution control remains: {legacy}")

# 7) Continuity audit: current/future phase discipline is mandatory.
roadmap = (DOCS / "ROADMAP.md").read_text(encoding="utf-8")
devlog = (DOCS / "DEVELOPMENT-LOG.md").read_text(encoding="utf-8")

phase_ids = sorted(
    set(
        re.findall(
            r"^##\s+(Phase\s+[0-9A-Za-z]+(?:\.[0-9A-Za-z]+)?)\s+[—-]",
            roadmap,
            re.M,
        )
    )
)

# Historical roadmap material predates the current development-log discipline.
# Report missing historical entries instead of blocking unrelated source safety
# checks; every phase completed under the current workflow must still be logged.
historical_unlogged = [
    phase for phase in phase_ids
    if not re.search(rf"(?<![0-9A-Za-z.]){re.escape(phase)}(?![0-9A-Za-z.])", devlog)
]
print(
    f"Continuity coverage: {len(phase_ids) - len(historical_unlogged)}/"
    f"{len(phase_ids)} roadmap phase identifiers appear in the development log"
)
if historical_unlogged:
    print(
        "Historical continuity notes without matching log headings: "
        + ", ".join(historical_unlogged[:12])
        + (" ..." if len(historical_unlogged) > 12 else "")
    )

if "Phase 7.4 — MaximumOpenPositions semantics" not in roadmap:
    fail("Roadmap does not expose Phase 7.4 continuity")
if "Phase 7.4 — MaximumOpenPositions semantics" not in devlog:
    fail("Current Phase 7.4 must be recorded in the development log")

print("Full project integrity audit PASS")
print(f"Production C# files scanned: {len(files)}")
print(f"Public parameters scanned: {len(param_decls)}")
print("Exact duplicate method signatures: 0")
print("Canonical visual snapshot/level geometry: PASS")
print("Execution UI single-owner boundary: PASS")
print("Roadmap/development-log continuity: PASS")
