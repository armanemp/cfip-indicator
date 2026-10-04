#!/usr/bin/env python3
"""MTF-P2 static acceptance gate: preserve source M15/H1 OB/FVG evidence."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]

def read(p):
    path = ROOT / p
    if not path.exists():
        raise SystemExit("MTF-P2 audit failed: missing " + p)
    return path.read_text(encoding="utf-8")

candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
enrichment = read("src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs")
location = read("src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs")
selection = read("src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelWaveTrendAndOpportunityRowsRenderer.cs")
builder = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
workflow = read(".github/workflows/source-check.yml")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
phase = read("docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md")

def require(cond, msg):
    if not cond:
        raise SystemExit("MTF-P2 audit failed: " + msg)

require(
    "SourceFvgQuality" in candidate and
    "SourceOrderBlockQuality" in candidate and
    "SourceFvgObConfluence" in candidate and
    "PrimaryLocationQuality" in candidate,
    "candidate must retain source-timeframe location evidence",
)
require(
    "frame.FvgBullQuality" in enrichment and
    "frame.ObBullQuality" in enrichment and
    "frame.FvgBearQuality" in enrichment and
    "frame.ObBearQuality" in enrichment and
    "PrimaryLocationQuality" in enrichment and
    re.search(r"PrimaryLocationQuality\s*=\s*location\.Score", enrichment),
    "enrichment must copy evidence from the actual source Frame",
)
require(
    "location.Confluence" in enrichment and
    "Math.Min(" in location,
    "the canonical location owner must remain authoritative",
)
require(
    "PrimaryLocationQuality" in selection and
    "PrimaryLocationConfluence" in selection,
    "primary source location evidence must affect display priority",
)
require(
    "OB+FVG " in panel and
    '" • OB "' in panel and
    '" • FVG "' in panel,
    "panel must expose source OB/FVG evidence",
)
require(
    '_m15Frame' in builder and
    '_h1Frame' in builder and
    'PrimaryTimeframeSignalRule.Evaluate(' in builder,
    "primary M15/H1 source layer must remain in force",
)
require(
    "VerifyMtfPrimaryLocationEvidence();" in runtime,
    "runtime contracts must cover MTF-P2",
)
require(
    "tools/audit_phase_mtf_primary_location_obfvg.py" in workflow,
    "MTF-P2 audit must be accumulated in Source/Architecture CI",
)
require(
    "No public parameter" in phase and
    "threshold" in phase.lower(),
    "phase record must document threshold/public-parameter non-change",
)

parameter_count = sum(
    len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8")))
    for p in (ROOT / "src/CFIP.Indicator/Indicator/Parameters").glob("*.cs")
)
require(parameter_count == 545, f"public parameter contract changed: found {parameter_count}")

print("MTF-P2 primary location OB/FVG audit PASS")
