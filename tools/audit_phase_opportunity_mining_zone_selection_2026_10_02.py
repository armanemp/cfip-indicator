from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding='utf-8')

rule = read('src/CFIP.Indicator/Core/Math/ExecutionZoneSelectionRule.cs')
selector = read('src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs')
selection_core = read('src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs')
selection_candidates = read('src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs')
selection_consumer = selector + "\n" + selection_core + "\n" + selection_candidates
zone = read('src/CFIP.Indicator/Planning/Execution/ExecutionZoneBuilder.cs')
architecture = read('tools/verify_architecture.py')
workflow = read('.github/workflows/source-check.yml')

checks = {
    'canonical selection rule exists':
        'class ExecutionZoneSelectionRule' in rule and
        'ExecutionZoneSelectionInput' in rule and
        'ExecutionZoneSelectionResult' in rule,
    'selection uses quality, proximity and age':
        'input.Quality' in rule and
        'input.DistanceAtr' in rule and
        'input.Age' in rule,
    'selection rewards OB+FVG and MTF confluence':
        'input.IsObFvgConfluence' in rule and
        'input.HasMtfOverlap' in rule,
    'selector evaluates M5 FVG and OB independently':
        'm5Fvg' in selection_core and 'm5Ob' in selection_core and
        'AddZoneCandidate(' in selection_core,
    'selector evaluates M15 FVG and OB independently':
        'm15Fvg' in selection_core and 'm15Ob' in selection_core,
    'selector can form same-frame OB+FVG candidates':
        'M5 FVG+OB' in selection_core and 'M15 FVG+OB' in selection_core and
        'AddOverlapCandidate(' in selection_candidates,
    'selector can form cross-timeframe confluence':
        'AddMtfOverlapCandidate(' in selection_core and
        'M5 ' in selection_candidates and 'M15 ' in selection_candidates,
    'selector selects highest scored valid candidate':
        'candidate.Score >' in selection_core and
        'candidate.Quality >' in selection_core,
    'execution FVG lookup uses quality-aware selection':
        'preferQualityForSelection' in read('src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs') and
        'SelectBestFvgForExecution(' in read('src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs') and
        'market,' in read('src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookup.cs'),
    'legacy swing fallback remains':
        'M5 SWING' in selection_core and
        'FindSwingLowBelow(' in selection_core and
        'FindSwingHighAbove(' in selection_core,
    'execution builder remains canonical caller':
        'TrySelectExecutionZoneCandidate(' in zone and
        'EvaluateExecutionZoneQuality(' in zone,
    'selection remains inside Indicator planning boundary':
        'ExecutionZoneCandidateSelector.cs' in architecture,
    'phase audit accumulated into CI':
        'audit_phase_opportunity_mining_zone_selection_2026_10_02.py' in workflow,
}

errors = [name for name, ok in checks.items() if not ok]
if errors:
    for error in errors:
        print('FAIL:', error)
    raise SystemExit(1)

print('Opportunity mining / zone selection audit: PASS')
for name in checks:
    print('PASS:', name)
