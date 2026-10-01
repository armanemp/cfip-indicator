#!/usr/bin/env python3
"""Static acceptance gate for CI-03 indicator fusion, correlation and evidence independence."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append('missing file: ' + relative)
        return ''
    return path.read_text(encoding='utf-8')

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

fusion = read('src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs')
independence = read('src/CFIP.Indicator/Core/Math/IndicatorEvidenceIndependenceRule.cs')
analyzer = read('src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceAnalyzer.cs')
scenario = read('src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs')
scoring = read('src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs')
frame = read('src/CFIP.Indicator/Analysis/Market/Models/Frame.cs')
decision = read('src/CFIP.Indicator/Core/Models/Decision.cs')
candidate = read('src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs')
builder = read('src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs')
orchestration = read('src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs')
runtime = read('tools/CFIP.Runtime.Contracts/Program.cs')
runtime_project = read('tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj')
workflow = read('.github/workflows/source-check.yml')
roadmap = read('docs/ROADMAP.md')
continuation = read('docs/CONTINUATION-STATE.md')

check('single numerical indicator-fusion owner', 'class IndicatorEvidenceFusionRule' in fusion and 'Evaluate(' in fusion)
check('indicator independence is diagnostic and phenomenon-grouped',
      'class IndicatorEvidenceIndependenceRule' in independence and
      'CountIndicatorGroups(' in independence and 'bool trend =' in independence and
      'bool momentum =' in independence and 'bool context =' in independence)
check('trend measurements share one correlated group',
      'input.TrendBull ||' in independence and 'input.TrendBear ||' in independence and
      'IsDirectionalDmi(input)' in independence)
check('momentum measurements share one correlated group',
      'input.MomentumBull ||' in independence and 'input.MomentumBear ||' in independence and
      'input.UseMacd' in independence and 'input.Rsi >= 55' in independence and
      'input.WaveTrendQuality >= 58' in independence)
check('context measurements obey optional switches',
      'input.UseVwap' in independence and 'input.UseVolume' in independence and
      'input.UseHealthyVolatility' in independence)
check('divergence and aggregate OSS are not independent groups',
      'Divergence remains a modifier' in independence and
      'aggregate OSS consensus' in independence)
check('market-frame scoring reuses one fusion input for attribution',
      'IndicatorEvidenceFusionInput indicatorFusionInput' in scoring and
      'IndicatorEvidenceFusionRule.Evaluate(' in scoring and
      'IndicatorEvidenceIndependenceRule.CountGroups(' in scoring)
check('per-frame independent evidence delegates to the canonical owner',
      'CalculateIndependentEvidenceForFrame(Frame frame' in analyzer and
      'CountIndependentEvidenceGroupsForFrame(Frame frame' in analyzer and
      'IndependentEvidenceFusionRule.CalculateScore(' in analyzer and
      'IndependentEvidenceFusionRule.CountGroups(' in analyzer)
check('parallel scenario enrichment removes the duplicate raw counter',
      'IndependentEvidence(frame, direction)' in scenario and
      'IndependentEvidenceGroupCount(frame, direction)' in scenario and
      'LocationEvidenceRule.Evaluate(' in scenario and
      'independent++' not in scenario and 'frame.MssBull' not in scenario and
      'frame.ChochBull' not in scenario and 'frame.MssBear' not in scenario and
      'frame.ChochBear' not in scenario)
check('indicator-group provenance reaches the decision chain',
      'IndicatorIndependentEvidenceGroupCount' in frame and
      'IndicatorIndependentEvidenceGroupCount' in decision and
      'IndicatorIndependentEvidenceGroupCount' in candidate and
      'decision.IndicatorIndependentEvidenceGroupCount' in orchestration and
      'IndicatorIndependentEvidenceGroupCount =' in builder)
check('runtime CI-03 contracts are wired',
      'VerifyCi03IndicatorEvidenceIndependence();' in runtime and
      'IndicatorEvidenceIndependenceRule.cs' in runtime_project and
      'CI-03 correlated trend/momentum/context' in runtime and
      'CI-03 disabling MACD removes only MACD directional contribution' in runtime)
check('CI-03 audit is accumulated immediately after CI-02',
      'audit_phase_ci_02.py' in workflow and 'audit_phase_ci_03.py' in workflow and
      workflow.index('audit_phase_ci_03.py') > workflow.index('audit_phase_ci_02.py'))
check('CI-03 is the active documented phase',
      'CI-03 — Indicator fusion / correlation / evidence independence' in roadmap and
      'CI-03 — Indicator fusion / correlation / evidence independence' in continuation)

print('CI-03 INDICATOR FUSION / EVIDENCE INDEPENDENCE SUMMARY')
print('=' * 72)
print(f'Errors: {len(errors)}')
if errors:
    for error in errors:
        print('- ' + error)
    sys.exit(1)
print('CI-03 STATIC GATE PASS')