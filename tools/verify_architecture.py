from pathlib import Path
import re
import sys

ROOT = Path('src/CFIP.Indicator')
PARAMETERS = ROOT / 'Indicator' / 'Parameters.cs'

LEGACY_FILES = {
    'Analysis/Indicators.cs', 'Analysis/Market.cs', 'Analysis/Reaction.cs', 'Analysis/Structure.cs',
    'Planning/Entry.cs', 'Planning/Filters.cs', 'Planning/TradePlan.cs',
    'Runtime/Lifecycle.cs', 'Trading/ActiveManagement.cs', 'Trading/Alerts.cs',
    'Trading/Execution.cs', 'Trading/Validation.cs', 'UI/Chart.cs', 'UI/Historical.cs',
    'UI/Panel.cs', 'UI/Popup.cs',
    'Analysis/Market/DecisionEngine.cs', 'Analysis/Structure/ZoneAnalyzer.cs',
    'Planning/Execution/EntryExecutionPolicy.cs', 'Planning/TradePlan/TargetAggregationEngine.cs',
    'Runtime/RuntimeOrchestrator.cs', 'Trading/Execution/AggressiveExecution.cs',
    'Trading/Execution/AutomaticMarketExecution.cs', 'Trading/Execution/ExecutionState.cs',
    'Trading/Intelligence/PredictionEngine.cs', 'Trading/Lifecycle/BrokerLifecycleEvents.cs',
    'Trading/LiveManagement/LivePositionManager.cs', 'Trading/LiveManagement/LiveExitGuards.cs',
    'Trading/Pending/PendingOrderEngine.cs', 'Trading/Risk/MarketSuitabilityEngine.cs',
    'Trading/Validation/TradeValidation.cs', 'UI/Chart/PlanRenderer.cs',
    'UI/Panel/PanelLayout.cs', 'UI/Panel/PanelRenderer.cs', 'UI/Panel/PanelState.cs',
}

files = list(ROOT.rglob('*.cs'))
rel = {str(p.relative_to(ROOT)).replace('\\', '/') for p in files}
bad_legacy = sorted(LEGACY_FILES & rel)
if bad_legacy:
    raise SystemExit('Legacy monolithic files remain: ' + ', '.join(bad_legacy))

def strip_comments_and_strings(text):
    text = re.sub(r'/\\*[\\s\\S]*?\\*/', ' ', text)
    text = re.sub(r'//[^\\r\\n]*', ' ', text)
    text = re.sub(r'@?"(?:""|\\.|[^"\\])*"', 'S', text)
    text = re.sub(r"'(?:\\\\.|[^'\\\\])*'", 'C', text)
    return text

source = '\\n'.join(p.read_text(encoding='utf-8') for p in files)
code = strip_comments_and_strings(source)

parameters = len(re.findall(r'\\[Parameter\\s*\\(', code))
if parameters != 513:
    raise SystemExit(f'Expected 513 parameters, found {parameters}')

method_pattern = re.compile(
    r'\\b(?:public|private|protected|internal)\\s+'
    r'(?:static\\s+|sealed\\s+|virtual\\s+|override\\s+|async\\s+|readonly\\s+|unsafe\\s+|partial\\s+)*'
    r'[\\w<>\\[\\],.?]+\\s+([A-Za-z_]\\w*)\\s*\\('
)
methods = method_pattern.findall(code)
if len(methods) != 311:
    raise SystemExit(f'Expected 311 reference methods, found {len(methods)}')

if re.search(r'CFIPClean\\d+|Clean\\d+|CFIP_MTF_LiveEntryEngine_Clean', code, re.I):
    raise SystemExit('Legacy/versioned strategy identifier detected')

if re.search(r'\\b(?:Buy|Sell)\\b.{0,100}\\b(?:Button|ToggleButton)\\b', code, re.I):
    raise SystemExit('Manual trade-entry controls detected')

oversized = [str(p.relative_to(ROOT)) for p in files if p != PARAMETERS and p.stat().st_size > 65536]
if oversized:
    raise SystemExit('Oversized production modules: ' + ', '.join(sorted(oversized)))

indicator_files = {
    'ExponentialMovingAverage.cs': 'Ema',
    'AverageTrueRange.cs': 'Atr',
    'RelativeStrengthIndex.cs': 'Rsi',
    'AverageDirectionalIndex.cs': 'Adx',
    'DirectionalMovementIndex.cs': 'DmiBias',
}
for filename, method in indicator_files.items():
    candidates = [p for p in files if p.name == filename]
    if len(candidates) != 1 or method not in candidates[0].read_text(encoding='utf-8'):
        raise SystemExit(f'Indicator isolation check failed: {filename}')

label = re.search(r'\\[Parameter\\(\"Auto Trade Label\"[^\\n]*DefaultValue\\s*=\\s*\"([^\"]+)\"', PARAMETERS.read_text(encoding='utf-8'))
if not label or label.group(1) != 'CFIP-SMART-CLEAN66':
    raise SystemExit('v73 broker identity label parity check failed')

hosts = len(re.findall(r'\\bpartial\\s+class\\s+CFIPIndicator\\s*:', code))
if hosts < 2:
    raise SystemExit('Expected multiple focused CFIPIndicator partial modules')

print(f'Architecture OK: {len(files)} C# files, {parameters} parameters, {len(methods)} methods, {hosts} partial modules.')