#!/usr/bin/env python3
"""Build-warning and panel-height integrity audit."""

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

report = read("src/CFIP.Contracts/BrokerExecutionReport.cs")
command = read("src/CFIP.Contracts/ManagementCommand.cs")
bus = read("src/CFIP.Contracts/ManagementBusKey.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
panel_factory = read("src/CFIP.Indicator/UI/Panel/PanelFactory.cs")
panel_layout = read("src/CFIP.Indicator/UI/Panel/PanelLayoutManager.cs")
indicator = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs")
b4b = read("tools/audit_phase_cbot_p4b.py")

if "public string CommandIdempotencyKey { get; init; } = string.Empty;" not in report:
    errors.append("BrokerExecutionReport correlation key is not explicitly initialized")

if "public string ExecutionLabel { get; init; } = string.Empty;" not in command:
    errors.append("ManagementCommand execution label is not explicitly initialized")

if "commands = null;" not in bus or "reports = null;" not in bus:
    errors.append("Management transport deserializers lost nullable failure-state contract")
if "ManagementCommand[]? parsed" not in bus or "BrokerExecutionReport[]? parsed" not in bus:
    errors.append("Management transport deserializers must flow nullable JSON results explicitly")

if "_lastPendingSignalM5" in state:
    errors.append("dead _lastPendingSignalM5 field remains")
if "_panelGeometryBaselineChartHeight" in state:
    errors.append("Chart-height geometry baseline state remains")

if "Chart.Height" in panel_factory or "Chart.Height" in panel_layout:
    errors.append("panel geometry still depends on Chart.Height")
if "CapturePanelGeometryBaseline(" in panel_factory:
    errors.append("obsolete chart-height baseline capture remains")
if "return Math.Max(" not in panel_layout or "configuredMaxHeight" not in panel_layout:
    errors.append("panel maximum-height resolver is not configuration-owned")

if "AutoRescale = false" not in indicator:
    errors.append("overlay Indicator must disable automatic chart rescaling")
if "ProviderHeartbeat[index]" not in provider:
    errors.append("provider heartbeat output contract unexpectedly disappeared")
if 'LineColor = "Transparent"' not in provider:
    errors.append("provider heartbeat must remain invisible")

if "Chart.Height" in b4b:
    errors.append("P4B audit must not enforce a Chart.Height-derived panel geometry path")

print("BUILD-WARNING / PANEL-HEIGHT INTEGRITY AUDIT")
print("=" * 72)
print("Contract nullable diagnostics: explicit")
print("Dead state cleanup: explicit")
print("Panel geometry source: configuration only")
print("Indicator AutoRescale: disabled")
print("Provider heartbeat remains invisible: yes")
print("Chart-height feedback path: forbidden")

if errors:
    print("AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("AUDIT: PASS")
