#!/usr/bin/env python3
"""CFIP 2026-10-06 signal-direction stability single-owner audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    path = ROOT / rel
    return path.read_text(encoding="utf-8") if path.exists() else ""

filters = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilters.cs")
gate = read("src/CFIP.Indicator/Analysis/Market/Decision/DirectionAcceptanceGate.cs")
resolver = read("src/CFIP.Indicator/UI/Chart/SignalVisualDirectionResolver.cs")
activation = read("src/CFIP.Indicator/Trading/LiveManagement/PlanActivation.cs")
snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")

errors = []

def require(ok, message):
    if not ok:
        errors.append(message)

require(
    "_lastConfirmedM5 = closedM5" in filters and
    "_lastConfirmedDirection = decision.Direction" in filters,
    "accepted direction must be committed only after the full decision gate chain passes",
)
require(
    "CanAcceptConfirmedDirection(" in gate and
    "PreventRapidDirectionFlip" in gate and
    "RequireM15ReversalForOpposite" in gate,
    "direction lifecycle gate must remain the anti-flip owner",
)
require(
    "_lastConfirmedM5 =" not in activation and
    "_lastConfirmedDirection =" not in activation,
    "PlanActivation must not overwrite direction lifecycle state",
)
require(
    "if (_decision.EntryAllowed)" in resolver and
    "_lastConfirmedDirection" in resolver,
    "visual direction must consume accepted lifecycle direction",
)
require(
    "ResolveCanonicalVisualDirection(" in snapshot and
    "AuthoritativeDirection" in snapshot,
    "canonical snapshot must remain the visual direction owner",
)
require(
    "return _lastConfirmedDirection;" in resolver,
    "blocked opposite decisions must hold the last accepted direction",
)

if errors:
    print("CFIP SIGNAL DIRECTION STABILITY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CFIP SIGNAL DIRECTION STABILITY AUDIT: PASS")
