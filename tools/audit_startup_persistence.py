#!/usr/bin/env python3
"""Audit Phase 9.15 startup and persistent-history invariants."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
INIT = ROOT / "src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs"
STARTUP_HELPERS = ROOT / "src/CFIP.Indicator/Runtime/Initialization/StartupDataHelpers.cs"
MTF = ROOT / "src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs"
ARCHIVE = ROOT / "src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs"
OUTCOME = ROOT / "src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs"
CAL = ROOT / "src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs"
HOST = ROOT / "src/CFIP.Indicator/Indicator/CFIPIndicator.cs"
SEED = ROOT / "src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs"

errors = []


def require(path: Path, pattern: str, label: str) -> None:
    text = path.read_text(encoding="utf-8")
    if not re.search(pattern, text, re.MULTILINE | re.DOTALL):
        errors.append(f"missing {label}")


require(
    INIT,
    r"RequestOptionalBars\(\s*TimeFrame\.Daily[\s\S]*?RequestOptionalBars\(\s*TimeFrame\.Weekly",
    "optional D1/W1 startup requests",
)
require(
    STARTUP_HELPERS,
    r"private void RequestOptionalBars\(",
    "optional bar request owner",
)
require(
    MTF,
    r"_m5Bars\.Count\s*>=\s*60[\s\S]*?_m15Bars\.Count\s*>=\s*50[\s\S]*?_m30Bars\.Count\s*>=\s*45[\s\S]*?_h1Bars\.Count\s*>=\s*40[\s\S]*?_h4Bars\.Count\s*>=\s*36",
    "startup readiness thresholds",
)
require(
    SEED,
    r"QueueOutcomeArchiveImport\(\)",
    "post-seed archive import scheduling",
)
require(
    ARCHIVE,
    r'OutcomeArchiveDirectory\s*=\s*"History"',
    "portable history directory",
)
require(
    ARCHIVE,
    r'OutcomeArchiveSchema\s*=\s*"CFIP-OUTCOME-ARCHIVE,1"',
    "archive schema marker",
)
require(
    ARCHIVE,
    r"OutcomeArchivePeriodStart\([\s\S]*?periodDays[\s\S]*?90",
    "90-day archive rotation",
)
require(
    ARCHIVE,
    r"File\.AppendAllText\(",
    "append-only archive writes",
)
require(
    ARCHIVE,
    r"Directory\.GetFiles\([\s\S]*?\);[\s\S]*?OutcomeArchivePrefix\(\)",
    "archive file discovery",
)
require(
    ARCHIVE,
    r"new DateTime\([\s\S]*?DateTimeKind\.Utc[\s\S]*?item\.ObservedUtcTicks",
    "UTC archive period compatibility",
)
require(
    CAL,
    r"_archiveLearningOutcomeCount[\s\S]*?_archiveCalibrationSamples[\s\S]*?_archiveCalibrationWins",
    "long-term archive learning fallback",
)
require(
    OUTCOME,
    r"ArchiveOutcomeObservation\(observation\)[\s\S]*?RegisterArchiveLearningObservation\(observation\)",
    "live outcome archive + learning hook",
)

archive_text = ARCHIVE.read_text(encoding="utf-8")
if "File.Delete" in archive_text or "Directory.Delete" in archive_text:
    errors.append("portable outcome archive must not delete historical files")

host_text = HOST.read_text(encoding="utf-8")
if "AccessRights.FullAccess" in host_text:
    errors.append("CFIP indicator host must retain AccessRights.None")

init_text = INIT.read_text(encoding="utf-8")
if "RequestBars(\n                TimeFrame.Daily" in init_text or "RequestBars(\n                    TimeFrame.Weekly" in init_text:
    errors.append("D1/W1 must not block mandatory startup pending-load count")

if errors:
    print("Phase 9.15 startup/persistence audit FAILED")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("Phase 9.15 startup/persistence audit OK")
