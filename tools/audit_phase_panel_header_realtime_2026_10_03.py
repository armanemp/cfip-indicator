#!/usr/bin/env python3
"""Panel header realtime-truth regression audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")

header = read("src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs")
refresh = read("src/CFIP.Indicator/UI/Panel/PanelContentRefresh.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
workflow = read(".github/workflows/source-check.yml")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check(
    "dedicated panel header owner exists",
    "private void RefreshPanelHeader()" in header
)
check(
    "header reads current canonical signal state",
    "GetCanonicalSignalPanelStatus()" in header and
    "GetAuthoritativeDirection()" in header
)
check(
    "header reads current cBot runtime truth",
    "IsCbotExecutionStateFresh()" in header and
    "HasFreshCbotPresence()" in header
)
check(
    "header is refreshed by live panel paths",
    "RefreshPanelHeader();" in panel and
    "RefreshPanelHeader();" in refresh
)
check(
    "header uses a change-aware cache",
    "_panelStableHeader" in state and
    "_panelStableHeaderSinceUtc" in state and
    "_panelHeaderTitle.Text = header" in header
)
check(
    "header audit is accumulated",
    "python tools/audit_phase_panel_header_realtime_2026_10_03.py" in workflow
)

if errors:
    print("PANEL HEADER REALTIME TRUTH AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("PANEL HEADER REALTIME TRUTH AUDIT: PASS")
