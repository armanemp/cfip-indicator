from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POLICY = ROOT / "src/CFIP.Indicator/Core/Runtime/AlertSoundPolicy.cs"
ENGINE = ROOT / "src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs"
DELIVERY = ROOT / "src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs"
PROCESSOR = ROOT / "src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs"

policy = POLICY.read_text(encoding="utf-8")
engine = ENGINE.read_text(encoding="utf-8")
delivery = DELIVERY.read_text(encoding="utf-8")
processor = PROCESSOR.read_text(encoding="utf-8")

checks = [
    ("single alert sound policy", policy.count("class AlertSoundPolicy") == 1),
    ("engine delegates sound semantics", "AlertSoundPolicy.Resolve(" in engine),
    ("engine has no duplicate sound classifier", "ResolveAlertSoundType(" not in engine),
    ("processor has no sound classifier", "ResolveSignalSoundPriority(" not in processor and "IsSignalSoundAlertKey(" not in processor),
    ("delivery carries canonical sound group", "SoundGroupKey" in delivery),
    ("engine persists canonical sound group", "soundDecision.GroupKey" in engine),
    ("processor consumes canonical group", "delivery.SoundGroupKey" in processor),
    ("blocked candidates stay silent", "!blockedCandidateAlert" in engine and "playSound =" in engine),
    ("semantic cues are explicit", all(token in policy for token in (
        "PositiveNotification",
        "NegativeNotification",
        "Announcement",
        "Doorbell",
        "Confirmation",
    ))),
    ("semantic mode owns event cue", "!UseSemanticAlertSounds" in processor),
    ("cBot audio remains separate", "CbotLifecycleAudioService" in (ROOT / "src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs").read_text(encoding="utf-8")),
]

failed = [name for name, ok in checks if not ok]
if failed:
    raise SystemExit("ALERT AUDIO AUDIT FAILED: " + "; ".join(failed))

print("ALERT AUDIO AUDIT PASS")
