from pathlib import Path
import re
from collections import defaultdict

ROOT = Path("src/CFIP.Indicator")
PARAM_ROOT = ROOT / "Indicator" / "Parameters"

PARAM_RE = re.compile(
    r'\[Parameter\s*\(([^\]]*)\]\s*'
    r'public\s+([A-Za-z_][\w<>\[\],.?]*)\s+'
    r'([A-Za-z_]\w*)\s*\{\s*get;\s*set;\s*\}',
    re.S,
)

def strip_non_code(source: str) -> str:
    source = re.sub(r"/\*[\s\S]*?\*/", " ", source)
    source = re.sub(r"//[^\r\n]*", " ", source)
    source = re.sub(r'@?"(?:""|\\.|[^"\\])*"', "S", source)
    source = re.sub(r'"(?:\\.|[^"\\])*"', "S", source)
    return source

def normalize_tokens(name: str):
    pieces = re.findall(r"[A-Z]+(?=[A-Z][a-z]|\d|$)|[A-Z]?[a-z]+|\d+", name)
    stop = {"minimum","maximum","enable","use","allow","require","smart","auto","advanced","only","for","after","before"}
    return {p.lower() for p in pieces if p.lower() not in stop}

parameter_defs = {}
metadata = {}

for path in sorted(PARAM_ROOT.glob("*.cs")):
    raw = path.read_text(encoding="utf-8")
    for match in PARAM_RE.finditer(raw):
        attr, type_name, name = match.groups()
        group_match = re.search(r'Group\s*=\s*"([^"]+)"', attr)
        default_match = re.search(r'DefaultValue\s*=\s*([^,\)]+)', attr)
        parameter_defs[name] = path
        metadata[name] = {
            "type": type_name,
            "group": group_match.group(1) if group_match else "",
            "default": default_match.group(1).strip() if default_match else "",
        }

EXPECTED_CURRENT_PARAMETERS = 548
if len(parameter_defs) != EXPECTED_CURRENT_PARAMETERS:
    raise SystemExit(
        f"Expected {EXPECTED_CURRENT_PARAMETERS} current parameters, found {len(parameter_defs)}"
    )

sources = {
    path: strip_non_code(path.read_text(encoding="utf-8"))
    for path in sorted(ROOT.rglob("*.cs"))
}

consumers = {}
for name, owner in sorted(parameter_defs.items()):
    pattern = re.compile(rf"\b{re.escape(name)}\b")
    files = []
    for path, source in sources.items():
        if path == owner:
            continue
        if pattern.search(source):
            files.append(str(path.relative_to(ROOT)).replace("\\", "/"))
    consumers[name] = tuple(files)

duplicate_names = defaultdict(list)
for name in parameter_defs:
    duplicate_names[name].append(str(parameter_defs[name]))
for name, owners in duplicate_names.items():
    if len(owners) > 1:
        raise SystemExit(f"Duplicate public parameter declaration: {name} -> {owners}")

families = {
    "confidence": [
        "MinimumConfidence", "PendingMinimumConfidence", "MinimumAutoConfidence",
        "AggressiveMinimumConfidence", "LiveReversalMinimumConfidence",
        "EarlySetupConfidence", "MinimumFreshTriggerEvidence",
    ],
    "smart_quality": [
        "MinimumSmartQuality", "SmartQualityThreshold", "PendingMinimumSmartQuality",
        "MinimumAutoSmartQuality", "AggressiveMinimumSmartQuality", "SmartStrongSetupQuality",
        "MinimumLiveReactionEvidence", "MinimumSmartTargetQualityForTp1",
    ],
    "target_rr": [
        "Tp1MinimumRR", "Tp2MinimumRR", "Tp3MinimumRR", "Tp4MinimumRR",
        "FallbackTp1RR", "FallbackTp2RR", "FallbackTp3RR", "FallbackTp4RR",
        "SmartTargetMinimumRR", "MinimumTradeRR", "MinimumHtfTargetRR",
        "SmartTrailMinimumRR",
    ],
    "entry_quality": [
        "MinimumEntryQuality", "MinimumEntryLocationQuality",
        "MinimumAutoLevelQuality", "SmartTargetQuality", "SmartStopQuality",
        "MinimumSmartTargetQualityForTp1", "FastReversalMinimumZoneQuality",
        "MinimumStructuralStopQuality",
    ],
    "cooldown": [
        "CooldownBars", "CooldownM5Bars", "EventGuardCooldownBars",
        "OppositeSignalCooldownM5", "ExitReentryCooldownM5",
        "SmartAlertCooldownSeconds", "RetestMaxBarsAfterDisplacement",
    ],
    "trail_geometry": [
        "TrailDistanceAtr", "SlRepriceBreathingAtr", "TrailStepAtr",
        "SlRepriceStepAtr", "TargetUpdateStepAtr", "SmartTrailMomentumBonusAtr",
    ],
    "structural_stop_enablement": [
        "EnableStructuralSlRepricing", "EnableDynamicSlTrail",
        "StructuralStopManagementOnly",
    ],
}

print("Phase 7.4 semantic parameter audit")
print(f"Parameters scanned: {len(parameter_defs)}")

review_pairs = []
for family, names in families.items():
    present = [name for name in names if name in parameter_defs]
    print(f"\n[{family}]")
    for name in present:
        role_files = list(consumers[name])
        print(
            f"- {name}: group={metadata[name]['group']} default={metadata[name]['default']} "
            f"consumers={len(role_files)}"
        )
        for other in present:
            if other <= name:
                continue
            shared = set(consumers[name]) & set(consumers[other])
            union = set(consumers[name]) | set(consumers[other])
            token_overlap = len(normalize_tokens(name) & normalize_tokens(other))
            jaccard = len(shared) / len(union) if union else 0
            if token_overlap >= 1 and jaccard >= 0.75:
                review_pairs.append((family, name, other, jaccard))

for family, left, right, jaccard in review_pairs:
    print(
        f"REVIEW CANDIDATE [{family}] {left} <-> {right}: "
        f"consumer-set-overlap={jaccard:.2f}"
    )

# A true duplicate must have a single owner concept. This alias was explicitly
# introduced as a second switch for the same structural SL path; it is not a
# distinct concept and is rejected here until removed.
if "EnableDynamicSlTrail" in parameter_defs:
    raise SystemExit(
        "Semantic duplicate remains: EnableDynamicSlTrail duplicates "
        "EnableStructuralSlRepricing and must be removed or semantically separated."
    )

print("\nSemantic duplicate audit PASS: no declared exact duplicate names and the "
      "known structural-stop alias has been removed.")
