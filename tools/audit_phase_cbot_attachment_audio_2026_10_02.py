from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

contracts_cbot = read("src/CFIP.Contracts/CbotIdentity.cs")
contracts_indicator = read("src/CFIP.Contracts/IndicatorIdentity.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
indicator = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")

checks = {
    "shared cBot display/type identity exists":
        'DisplayName = "CFIP Smart Execution Bot"' in contracts_cbot and
        'TypeName = "CFIPExecutionBot"' in contracts_cbot,
    "stable Indicator type identity exists":
        'TypeName = "CFIPIndicator"' in contracts_indicator and
        "IndicatorIdentity.TypeName" in binding,
    "Indicator keeps stable cTrader display registration":
        '"CFIP Smart Indicator"' in indicator and
        "[Indicator(" in indicator,
    "cBot-to-indicator binding accepts stable indicator type":
        "IndicatorIdentity.TypeName" in binding and
        "candidate.Type.Name" in binding,
    "Indicator cBot reader accepts stable cBot type":
        "CbotIdentity.TypeName" in reader and
        "candidate.Type.Name" in reader,
    "LocalStorage empty payload is heartbeat-pending, not attachment truth":
        '"CBOT HEARTBEAT PENDING"' in reader and
        '"CBOT NOT ATTACHED"' in reader,
    "alert diagnostics record queueing":
        '"CFIP ALERT QUEUED' in engine and
        '"CFIP ALERT QUEUE REJECTED' in engine,
    "sound delivery has semantic fallback":
        "DeliverAlertSound(next)" in processor and
        "falling back to semantic cue" in processor and
        "Notifications.PlaySound(soundType)" in processor,
    "sound delivery is bounded per pump":
        "MaxAlertDeliveriesPerPump = 4" in processor and
        "while (processed < MaxAlertDeliveriesPerPump" in processor,
    "runtime startup logs effective audio configuration":
        '"CFIP ALERT AUDIO | enabled=' in initialization,
    "timer remains an alert delivery boundary":
        "ProcessQueuedAlertDelivery();" in initialization and
        "OnTimer()" in initialization,
}

errors = [name for name, ok in checks.items() if not ok]
if errors:
    for error in errors:
        print("FAIL:", error)
    raise SystemExit(1)

print("CBOT attachment + alert audio hardening audit: PASS")
for name in checks:
    print("PASS:", name)
