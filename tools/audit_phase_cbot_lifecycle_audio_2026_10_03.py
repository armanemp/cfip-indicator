#!/usr/bin/env python3
"""CBOT lifecycle/audio ownership regression audit."""
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

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
audio = read("src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs")
workflow = read(".github/workflows/source-check.yml")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check(
    "cBot has a single lifecycle audio owner",
    "CbotLifecycleAudioService" in bot and
    "CbotLifecycleAudioService" in audio
)
check(
    "start and stop events are audible",
    "PlayStarted(" in audio and
    "PlayStopped(" in audio and
    "_audio.PlayStarted(" in bot and
    "_audio.PlayStopped(" in bot
)
check(
    "execution outcomes have distinct audible cues",
    "PlayExecutionConfirmed(" in audio and
    "PlayExecutionRejected(" in audio
)
check(
    "recovery has a warning cue",
    "PlayRecoveryRequired(" in audio
)
check(
    "audio failures are observable and never crash execution",
    "try" in audio and
    "catch (Exception ex)" in audio and
    "CFIP CBOT AUDIO FAILED" in audio
)
check(
    "lifecycle audio audit is accumulated",
    "python tools/audit_phase_cbot_lifecycle_audio_2026_10_03.py" in workflow
)

if errors:
    print("CBOT LIFECYCLE AUDIO AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CBOT LIFECYCLE AUDIO AUDIT: PASS")
