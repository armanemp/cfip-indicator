# CI-19 — Signal / Target Quality Coherence — 2026-10-02

## Status

VERIFIED COMPLETE — merged to `main` via PR #200 as `623dd198885c684b85be01459b4e72faafa00b8a`.

## Root causes addressed

The audit found structural causes behind missing opportunities and near-fixed 2R target behavior:
1. DecisionStructureGates still globally required TriggerReady, allowing a valid Retest to be rejected before actionability.
2. TREND regime M5 evidence could reject a tactical opportunity that had already passed its own quality/RR path.
3. TargetLevelBuilder capped candidates before MergeLevels, allowing farther structural/HTF/OB/FVG targets to disappear before confluence scoring.
4. TargetCandidateEvaluator rewarded RR near the minimum and also applied a nearest-level bonus, turning a validity floor into an implicit optimization target.
5. Live target progression used a separate quality-only score, so initial and progressive TP selection did not share reward semantics.

## Canonical corrections

- remove the remaining global TriggerReady blocker from DecisionStructureGates;
- preserve the tactical Retest route in TREND when the canonical TacticalOpportunity contract already passes;
- merge all valid target levels before applying SmartTargetMaxCandidates;
- use TargetCandidateRewardScoreRule as the shared reward score owner;
- treat minimum RR as a validity floor and reward additional valid RR only in proportion to source quality;
- reuse the same reward score in live progressive target selection, with the current confirmed target RR as the progression baseline;
- expose BARRIER TRACE and trigger lifecycle details in the panel.

## Signal quality boundary

No public minimum-confidence, SmartQuality, Entry, SL or safety gate is removed. The changes prevent legitimate opportunity lanes from being discarded before their proper actionability evaluation.

No profitability guarantee is made. Empirical signal accuracy still requires replay/OOS validation.

## Verification

- Source / Architecture #3100: **PASS**;
- Runtime Acceptance #2909: **PASS**;
- cTrader Compile #3093: **PASS**;
- CI-19 signal/target quality audit: **PASS**;
- accumulated signal/target/MTF/architecture audits: **PASS**;

Required:
- Source / Architecture accumulated audits;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- Planning deterministic target reward contract;
- CI-19 signal/target quality audit.

Target-terminal validation remains required for empirical signal frequency, target placement, progressive TP behavior, chart/panel synchronization and alert timing.

## Next

After CI-19 acceptance, continue with intelligent protection: monotonic SL tightening, profit-locking and progressive target expansion driven by structure and reward potential without constant TP movement.