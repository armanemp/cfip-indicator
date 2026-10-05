from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CBOT = ROOT / "src" / "CFIP.cBot"

def read(path):
    return path.read_text(encoding="utf-8")

errors = []

indicator_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in INDICATOR.rglob("*.cs")
)

cbot_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in CBOT.rglob("*.cs")
)

host = read(CBOT / "CFIPExecutionBot.cs")
gate = read(CBOT / "Execution" / "CbotExecutionEnvironmentGate.cs")
market = read(CBOT / "Execution" / "DemoMarketExecutionCoordinator.cs")
pending = read(CBOT / "Execution" / "DemoPendingOrderExecutionCoordinator.cs")

if "QuickTradeButtons" in indicator_source or "QuickTradeButtons" in cbot_source:
    errors.append("CFIP production source must not mutate cTrader Quick Trade visibility")

if "DisplaySettings.QuickTradeButtons" in indicator_source or "DisplaySettings.QuickTradeButtons" in cbot_source:
    errors.append("CFIP must not write Chart.DisplaySettings.QuickTradeButtons")

if "Permissions.TradingPermission.Request()" in cbot_source:
    errors.append("cBot must not request the Indicator trading permission")

if 'src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs' in "":
    errors.append("unreachable guard")

parameter_dir = INDICATOR / "Indicator" / "Parameters"
for path in parameter_dir.glob("*.cs"):
    if "EnableAutoTrading" in read(path) or "EnableAutomaticOrders" in read(path):
        errors.append(f"Indicator execution parameters remain in {path.relative_to(ROOT)}")

if "EnableAutoTrading" not in host:
    errors.append("cBot master automatic-trading parameter is missing")

if 'DefaultValue = true)]' not in host.split('"Enable Automatic Trading"', 1)[1].split('public bool EnableAutoTrading', 1)[0]:
    errors.append("cBot master automatic trading must default ON for the demo-first execution path")

if 'DefaultValue = true)]' not in host.split('"Enable Demo Market Execution"', 1)[1].split('public bool EnableDemoMarketExecution', 1)[0]:
    errors.append("demo market execution capability must default ON")

if "EnableAutoTrading &&" not in host:
    errors.append("cBot execution paths must be gated by the cBot-owned master automatic-trading setting")

if "LIVE EXECUTION NOT ARMED" not in gate:
    errors.append("live execution must remain fail-closed")

if "CBOT AUTO TRADING DISABLED" not in gate:
    errors.append("execution gate must report cBot-owned automatic-trading state")

if "AUTO TRADING DISABLED IN INDICATOR" in gate or "AUTOMATIC ORDERS DISABLED IN INDICATOR" in gate:
    errors.append("execution gate contains stale Indicator-owned execution messages")

for token in (
    "ExecuteMarketOrder(",
    "ExecuteMarketRangeOrder(",
):
    if token not in market:
        errors.append(f"market broker mutation owner missing {token}")

for token in (
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
):
    if token not in pending:
        errors.append(f"pending broker mutation owner missing {token}")

if "ProcessSignalEnvelope(" not in host or "TryReadScenarioBatch(" not in host:
    errors.append("cBot signal-to-execution handoff is incomplete")

if "ProcessSignalEnvelope(" in host and "CbotSignalPreflight.TryValidate(" not in host:
    errors.append("cBot must preflight every incoming signal before broker mutation")

if "if (!executionEnabled)" not in host:
    errors.append("cBot must expose an explicit disarmed execution branch")

if "if (!actionEnabled)" not in host:
    errors.append("cBot must expose an explicit unsupported/disarmed action branch")

if errors:
    for error in errors:
        print("FAIL:", error)
    raise SystemExit(1)

print("CFIP CBOT EXECUTION READINESS AUDIT PASS")
