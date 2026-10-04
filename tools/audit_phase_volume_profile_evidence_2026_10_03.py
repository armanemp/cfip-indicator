#!/usr/bin/env python3
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append(f"missing: {rel}")
        return ""
    return path.read_text(encoding="utf-8")

snapshot = read("src/CFIP.Indicator/Core/Models/VolumeProfileSnapshot.cs")
rule = read("src/CFIP.Indicator/Core/Math/VolumeProfileEvidenceRule.cs")
analyzer = read("src/CFIP.Indicator/Analysis/Market/VolumeProfileAnalyzer.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
ranking = read("src/CFIP.Indicator/Core/Math/TradeOpportunityQualityRule.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
builder = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
phase = read("docs/PHASE-VOLUME-PROFILE-EVIDENCE-2026-10-03.md")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check(
    "portable Volume Profile snapshot is present",
    "struct VolumeProfileSnapshot" in snapshot and
    "public static VolumeProfileSnapshot Empty" in snapshot and
    "cAlgo.API" not in snapshot
)

check(
    "profile analyzer uses bounded closed-bar M15 source",
    "VolumeProfileAnalyzer" in analyzer and
    "bars.TickVolumes[i]" in analyzer and
    "PriceToBin(" in analyzer and
    "valueAreaPercent" in analyzer
)

check(
    "70 percent value area expands from POC",
    "targetVolume" in analyzer and
    "pocIndex" in analyzer and
    "valueAreaVolume" in analyzer and
    "upperIndex" in analyzer and
    "lowerIndex" in analyzer
)

check(
    "directional VP evidence is contextual, not standalone execution authority",
    "VolumeProfileEvidenceRule" in rule and
    "nearBullValueEdge" in rule and
    "nearBearValueEdge" in rule and
    "acceptedAboveValue" in rule and
    "acceptedBelowValue" in rule and
    "confluence" in rule
)

check(
    "candidate carries auditable VP evidence",
    "VolumeProfileQuality" in candidate and
    "VolumeProfileConfluence" in candidate and
    "VolumeProfileLocation" in candidate and
    "VolumeProfilePoc" in candidate and
    "VolumeProfileValueAreaLow" in candidate and
    "VolumeProfileValueAreaHigh" in candidate
)

check(
    "M15 VP snapshot is cached by closed M15 index",
    "_m15VolumeProfile" in state and
    "_m15VolumeProfileClosedIndex" in state and
    "GetM15VolumeProfileSnapshot()" in builder and
    "_m15VolumeProfileClosedIndex == closedM15" in builder
)

check(
    "VP evidence reaches live scenario quality and ranking",
    "VolumeProfileEvidenceRule.Evaluate(" in builder and
    "candidate.VolumeProfileQuality" in builder and
    "candidate.VolumeProfileConfluence" in builder and
    "candidate.Quality" in builder and
    "Math.Min(" in builder and
    "volumeProfileEvidence.Quality" in builder and
    "candidate.VolumeProfileConfluence" in ranking
)

check(
    "workflow accumulates the dedicated audit",
    "python tools/audit_phase_volume_profile_evidence_2026_10_03.py" in workflow
)

check(
    "phase/roadmap continuity is recorded",
    "Volume Profile" in roadmap and
    "Volume Profile" in phase
)

if errors:
    print("=" * 72)
    print("VOLUME PROFILE EVIDENCE AUDIT: FAIL")
    for e in errors:
        print("- " + e)
    sys.exit(1)

print("=" * 72)
print("VOLUME PROFILE EVIDENCE AUDIT: PASS")
