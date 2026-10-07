from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    return (ROOT / rel).read_text(encoding="utf-8")

state = read("src/CFIP.Indicator/Indicator/State.cs")
analyzer = read("src/CFIP.Indicator/Analysis/Market/AggressiveFlowAnalyzer.cs")
runtime = read("src/CFIP.Indicator/Runtime/Market/AggressiveFlowRuntime.cs")
init = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
footer = read("src/CFIP.Indicator/UI/Panel/PanelFooterFactory.cs")
constants = read("src/CFIP.Indicator/UI/Panel/PanelConstants.cs")

errors = []

def check(name, ok):
    if not ok:
        errors.append(name)

check("aggressive flow owner exists", "internal sealed class AggressiveFlowAnalyzer" in analyzer)
check("tick proxy explicitly avoids executed volume claim", "never" in analyzer and "executed trade size" in analyzer)
check("bounded rolling sample exists", "MaxSamples = 4096" in analyzer and "TimeSpan.FromSeconds(30)" in analyzer)
check("runtime subscribes to tick stream", "MarketData.GetTicks(Symbol.Name)" in runtime and ".Tick += OnAggressiveFlowTick" in runtime)
check("runtime unsubscribes", ".Tick -= OnAggressiveFlowTick" in runtime)
check("runtime starts after data initialization", "StartAggressiveFlowRuntime();" in init)
check("runtime stops on destroy", "StopAggressiveFlowRuntime();" in init)
check("DOM uses broker depth volume", "entry.VolumeInUnits" in footer and "BidEntries" in footer and "AskEntries" in footer)
check("UI labels DOM as DOM", "DOM BUY" in footer and "DOM SELL" in footer)
check("UI exposes directional tick proxy separately", "FLOW BUY TICKS" in footer and "FLOW SELL TICKS" in footer)
check("footer reserves four rows", "PanelFlowPressureRailHeight = 84" in constants and "PanelFooterMinHeight = 146" in constants)

check("live reaction uses fresh flow only as opposition veto", "StronglyOpposes(" in analyzer and "flowOpposes" in reaction and "FLOW OPPOSITION" in reaction and "TryGetFreshAggressiveFlowSnapshot" in reaction)

if errors:
    for e in errors:
        print("FAIL | " + e)
    raise SystemExit(1)

print("PASS | flow/dom/aggressive-flow architecture audit")
