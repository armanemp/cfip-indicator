# Phase — Smart Trend Arrows Recovery / Single Owner — 2026-10-04

## Root cause
- The 9-level smart-arrow contract was left on a separate branch instead of being present on main.
- The old evaluator used a preferred-direction override, creating a second direction source.
- Scores below 55 became level 0, suppressing many weak trend states.
- Arrow rendering was coupled to ActionableNow / DecisionEntryAllowed / confidence, so a valid MTF trend could disappear.
- Arrow lifecycle was spread across multiple renderers.

## Final contract
- One strength owner: MtfTrendStrengthRule.
- M1/M5/M15/M30/H1/H4/D1/W1 contribute to MTF analysis; M1 remains precision/trigger and has no trend weight.
- Direction comes only from MtfTrendStrengthRule.
- Levels 1–3 are WEAK, 4–6 MEDIUM, 7–9 STRONG.
- Score 35+ enters the ladder; every 5 points advances one level; 75+ clamps to level 9.
- Trend arrows are independent of trade actionability.
- CalculationLiveCycle is the single production lifecycle that calls RenderCanonicalMtfTrendArrows once.
- SignalStackedArrowRenderer consumes the canonical snapshot and renders the 1/2/3 arrows with real vertical spacing.
- M1 trigger remains Circle, never a second directional arrow.

## Files corrected
MtfTrendStrengthRule.cs, MtfTrendStrengthSnapshotBuilder.cs, SignalVisualSnapshot.cs, SignalStackedArrowRenderer.cs, SignalRenderer.cs, SignalPresentationRenderer.cs, PlanRenderCoordinator.cs, CalculationLiveCycle.cs, plus the affected architecture audits.

## Verification status
Cross-file source-path audit completed. GitHub Actions has not yet reported a workflow result for the latest main commit, so CI/build success is not claimed. cTrader visual acceptance still requires a successful local Release build.

## Local action
Run:
git pull --ff-only

Then:
dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release

Do not judge the cTrader visual result until the build succeeds.
