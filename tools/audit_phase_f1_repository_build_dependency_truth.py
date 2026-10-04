#!/usr/bin/env python3
"""F1 repository/build/dependency truth gate.

This gate verifies project boundaries and dependency direction without executing
trading logic. It intentionally treats cTrader as an outer platform dependency.
"""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src"
TOOLS = ROOT / "tools"
SOLUTION = ROOT / "CFIP.Indicator.sln"

EXPECTED_PROJECTS = {
    "src/CFIP.Indicator/CFIP.Indicator.csproj",
    "src/CFIP.Contracts/CFIP.Contracts.csproj",
    "src/CFIP.cBot/CFIP.cBot.csproj",
}

def fail(message: str) -> None:
    raise SystemExit("F1 FAIL: " + message)

def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")

def project_refs(path: Path) -> list[str]:
    root = ET.fromstring(read(path))
    return [
        (path.parent / node.attrib["Include"].replace("\\", "/")).resolve().relative_to(ROOT).as_posix()
        for node in root.iter()
        if node.tag.endswith("ProjectReference") and "Include" in node.attrib
    ]

# 1. Solution/project boundary.
if not SOLUTION.exists():
    fail("solution is missing")

solution = read(SOLUTION)
for project in sorted(EXPECTED_PROJECTS):
    if project.replace("/", "\\") not in solution:
        fail(f"solution missing canonical project: {project}")

csprojs = sorted(ROOT.rglob("*.csproj"))
if not csprojs:
    fail("no project files found")

# ProjectReference duplication and unresolved local references.
for project in csprojs:
    refs = project_refs(project)
    if len(refs) != len(set(refs)):
        fail(f"duplicate ProjectReference in {project.relative_to(ROOT)}")
    for ref in refs:
        if not (ROOT / ref).exists():
            fail(f"broken ProjectReference {project.relative_to(ROOT)} -> {ref}")

# Active dependency direction.
indicator = SRC / "CFIP.Indicator" / "CFIP.Indicator.csproj"
contracts = SRC / "CFIP.Contracts" / "CFIP.Contracts.csproj"
cbot = SRC / "CFIP.cBot" / "CFIP.cBot.csproj"
if contracts.relative_to(ROOT).as_posix() not in project_refs(indicator):
    fail("Indicator must reference CFIP.Contracts")
if contracts.relative_to(ROOT).as_posix() not in project_refs(cbot):
    fail("cBot must reference CFIP.Contracts")
if indicator.relative_to(ROOT).as_posix() in project_refs(cbot):
    fail("cBot must not reference Indicator project")

# 2. Package/framework consistency.
package_versions: dict[str, set[str]] = {}
frameworks: dict[str, str] = {}
for project in csprojs:
    root = ET.fromstring(read(project))
    frameworks[project.relative_to(ROOT).as_posix()] = next(
        (n.text for n in root.iter() if n.tag.endswith("TargetFramework") and n.text),
        "",
    )
    for node in root.iter():
        if not node.tag.endswith("PackageReference"):
            continue
        include = node.attrib.get("Include", "")
        version = node.attrib.get("Version", "")
        if include:
            package_versions.setdefault(include, set()).add(version)

if package_versions.get("cTrader.Automate") != {"1.0.21"}:
    fail(f"cTrader.Automate versions drift: {package_versions.get('cTrader.Automate')}")
if package_versions.get("Skender.Stock.Indicators") != {"2.7.3"}:
    fail(f"Skender.Stock.Indicators versions drift: {package_versions.get('Skender.Stock.Indicators')}")
if frameworks.get("tools/CFIP.StockIndicators.Benchmark/CFIP.StockIndicators.Benchmark.csproj") != "net8.0":
    fail("benchmark target framework changed unexpectedly")
for path, framework in frameworks.items():
    if path != "tools/CFIP.StockIndicators.Benchmark/CFIP.StockIndicators.Benchmark.csproj" and framework != "net6.0":
        fail(f"unexpected TargetFramework {framework!r}: {path}")

# 3. Platform leakage.
for root in (SRC / "CFIP.Contracts", SRC / "CFIP.Indicator" / "Core"):
    for path in sorted(root.rglob("*.cs")):
        source = read(path)
        if "cAlgo.API" in source or "cTrader.Automate" in source:
            fail(f"platform dependency leaked into neutral boundary: {path.relative_to(ROOT)}")

plan_rule = SRC / "CFIP.Indicator" / "Core" / "Math" / "PlanLinePresentationRule.cs"
if "Color" in read(plan_rule):
    fail("PlanLinePresentationRule must remain platform-neutral; color adaptation belongs to UI")
renderer = SRC / "CFIP.Indicator" / "UI" / "Chart" / "PlanLineRenderer.cs"
renderer_code = read(renderer)
if "PlanLinePresentationRule.SignalLineAlpha" not in renderer_code:
    fail("PlanLineRenderer must consume canonical line alpha at the platform boundary")
if "Color.FromArgb(" not in renderer_code:
    fail("PlanLineRenderer must own cTrader color materialization")

# 4. Runtime contract harness: linked files must be platform-neutral.
runtime_csproj = TOOLS / "CFIP.Runtime.Contracts" / "CFIP.Runtime.Contracts.csproj"
runtime_text = read(runtime_csproj)
for match in re.findall(r'<Compile\s+Include="([^"]+)"', runtime_text):
    source_path = (runtime_csproj.parent / match).resolve()
    if not source_path.exists():
        fail(f"Runtime.Contracts linked source missing: {match}")
    source = read(source_path)
    if "cAlgo.API" in source or "cTrader.Automate" in source:
        fail(f"Runtime.Contracts links platform source: {source_path.relative_to(ROOT)}")

# 5. Build matrix truth: CI must build the three active boundaries and the
# source-linked Indicator compile harness, plus runtime/contract harnesses.
ci = read(ROOT / ".github" / "workflows" / "ci-build.yml")
runtime_ci = read(ROOT / ".github" / "workflows" / "runtime-acceptance.yml")
required_ci = (
    "src/CFIP.Contracts/CFIP.Contracts.csproj",
    "src/CFIP.cBot/CFIP.cBot.csproj",
    "tools/CFIP.Indicator.CI/CFIP.Indicator.CI.csproj",
    "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj",
)
for item in required_ci:
    matrix = runtime_ci if item == "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj" else ci
    if item not in matrix:
        fail(f"CI build matrix missing {item}")

# 6. Root/build metadata must have one owner.
root_props = ROOT / "Directory.Build.props"
if not root_props.exists():
    fail("Directory.Build.props is missing")
root_props_text = read(root_props)
for property, expected in (
    ("Deterministic", "true"),
    ("ImplicitUsings", "disable"),
):
    if f"<{property}>{expected}</{property}>" not in root_props_text:
        fail(f"Root build default missing: {property}={expected}")

for project in csprojs:
    project_text = read(project)
    relative = project.relative_to(ROOT).as_posix()
    if "<Deterministic>true</Deterministic>" in project_text:
        fail(f"Duplicate Deterministic project override; root owner is Directory.Build.props: {relative}")
    if "<ImplicitUsings>disable</ImplicitUsings>" in project_text:
        fail(f"Duplicate ImplicitUsings project override; root owner is Directory.Build.props: {relative}")

ignore_path = ROOT / ".gitignore"
if not ignore_path.exists():
    fail(".gitignore is missing")
ignore_lines = {
    line.strip()
    for line in read(ignore_path).splitlines()
    if line.strip() and not line.lstrip().startswith("#")
}
for required_ignore in ("bin/", "obj/", "BenchmarkDotNet.Artifacts/", "benchmark-report.md", "*.log", "*.tmp"):
    if required_ignore not in ignore_lines:
        fail(f"Repository hygiene ignore missing: {required_ignore}")
if ".vscode/*" not in ignore_lines or "!.vscode/extensions.json" not in ignore_lines:
    fail("VS Code ignore policy must keep only the canonical extensions recommendation tracked")

# 6. Tracked generated output must not become production source.
for path in ROOT.rglob("*.cs"):
    rel = path.relative_to(ROOT).as_posix()
    if "/bin/" in rel or "/obj/" in rel:
        fail(f"generated build output is present in source tree: {rel}")

print("F1 Repository / Build / Dependency Truth: PASS")
print("Root build metadata owner: Directory.Build.props")
print("Repository artifact ignore policy: PASS")
print(f"Projects scanned: {len(csprojs)}")
print(f"cTrader.Automate: {sorted(package_versions.get('cTrader.Automate', set()))}")
print(f"Skender.Stock.Indicators: {sorted(package_versions.get('Skender.Stock.Indicators', set()))}")
print(f"Neutral Core/Contracts files scanned: {sum(1 for r in (SRC / 'CFIP.Contracts', SRC / 'CFIP.Indicator' / 'Core') for _ in r.rglob('*.cs'))}")
print("Runtime.Contracts linked platform files: 0")
print("cBot -> Indicator project reference: 0")
