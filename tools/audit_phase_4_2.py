#!/usr/bin/env python3
"""Static acceptance gate for CR4.2 persistence paths and observability."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src/CFIP.Indicator"

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing required CR4.2 file: {relative}")
    return path.read_text(encoding="utf-8")

indicator = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
portable = read("src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs")
archive = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs")
runtime = read("src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs")
trace = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs")
buffered = read("src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs")
workflow = read(".github/workflows/source-check.yml")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
errors = []

def check(name: str, ok: bool) -> None:
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check("indicator keeps AccessRights.None",
      "AccessRights = AccessRights.None" in indicator)
check("History remains a sandbox-relative directory",
      'private const string OutcomeArchiveDirectory = "History";' in archive and
      "OutcomeArchiveDirectory + Path.DirectorySeparatorChar" in portable)
check("portable marker performs a real write/read round trip",
      "TryWriteAllText(" in portable and "TryReadAllText(" in portable and
      "MarkProbeResult(" in portable)
check("portable snapshot uses the centralized persistence owner",
      "TryReadAllLines(" in portable and "TryWriteAllText(" in portable)
check("archive writes use buffered persistence",
      "_bufferedArchivePersistence.Enqueue(" in archive and
      "File.WriteAllText(" not in archive and "File.AppendAllText(" not in archive)
check("runtime log writes use buffered persistence",
      "_bufferedArchivePersistence.Enqueue(" in runtime and
      "File.WriteAllText(" not in runtime and "File.AppendAllText(" not in runtime)
check("signal trace writes use buffered persistence",
      "_bufferedArchivePersistence.Enqueue(" in trace and
      "File.WriteAllText(" not in trace and "File.AppendAllText(" not in trace)
check("signal trace identity is account scoped",
      "MemoryAccountScopeToken()" in trace)
check("buffered persistence exposes health counters",
      "WriteFailureCount" in buffered and "ReadFailureCount" in buffered and
      "PendingLineCount" in buffered and "LastError" in buffered)
check("persistence health is visible in the panel",
      "PERSISTENCE  •" in panel and "PersistenceHealthRule.Resolve(" in panel)
check("persistence health and sandbox path are contract-tested",
      "VerifyPersistenceHealthSemantics();" in contracts and
      "IsRelativeHistoryPath(" in contracts)
check("CR4.2 gate is wired into CI",
      "python tools/audit_phase_4_2.py" in workflow)

# Centralize all production file writes in the bounded persistence owner.
for path in SRC.rglob("*.cs"):
    if path.as_posix().endswith("/Trading/Intelligence/BufferedArchivePersistence.cs"):
        continue
    text = path.read_text(encoding="utf-8")
    if "File.WriteAllText(" in text or "File.AppendAllText(" in text or "File.WriteAllBytes(" in text:
        check(f"no direct production file write outside persistence owner: {path.relative_to(ROOT)}", False)

print("CR4.2 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR4.2 STATIC GATE PASS")
