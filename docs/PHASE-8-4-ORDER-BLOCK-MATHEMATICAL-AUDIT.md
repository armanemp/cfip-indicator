# Phase 8.4 — Order Block Mathematical Audit

Status: implementation verified and ready to merge.

## Scope

Audit and normalize the production Order Block chain without adding public parameters:
- source-candle direction and zone geometry;
- displacement threshold anchored to source/creation ATR;
- structure-break evidence anchored to source/creation ATR;
- liquidity-sweep evidence timing;
- FVG confluence using the canonical FVG owner from Phase 8.3;
- partial/full mitigation and remaining width;
- stable OB identity and deterministic candidate lifecycle.

## Decisions

1. A bullish OB is sourced from an opposite bearish candle; a bearish OB is sourced from an opposite bullish candle.
2. Zone geometry is either the full source candle range or its body according to the existing parameter. No new parameter is added.
3. Displacement and structure-break thresholds refer to ATR at the OB source candle, not current ATR.
4. FVG confluence must use the canonical `FvgRule`, including creation-bar ATR for each FVG event.
5. Mitigation is directional and monotonic: bullish OB upper boundary moves downward on partial fill; bearish OB lower boundary moves upward.
6. A full fill invalidates the OB. Active geometry must remain wider than one tick and above the minimum retained-width policy.
7. Managed OBs retain deterministic source identity.
8. Current ATR remains allowed only for present-market selection/context, not historical source-event qualification.
9. User-supplied WaveTrend source is not currently present in accessible conversation/Library/repository content; therefore no claim of exact WaveTrend reproduction is made. The phase keeps external indicator integration behind explicit confluence inputs rather than inventing a formula.

## Reference-indicator integration notes

The user-provided FVG source was inspected from the Library archive. Its 2-bar and 3-bar zone formulas match the canonical FVG geometry already owned by `FvgRule`; its wick/body zone-break and partial-shrink lifecycle also aligns with the mitigation direction used here. The custom `CUSTOMWAVETREND` source was inspected separately: it combines RSI on typical price, MFI and an RMI-like momentum oscillator, then applies smoothing and a signal line. It is deliberately not turned into a standalone trigger in Phase 8.4; a closed-bar adapter with exact reference semantics is reserved for the dedicated confluence phase.

## Verification

Automated acceptance will cover source direction, geometry, creation-ATR displacement/break thresholds, mitigation symmetry, stable identity, FVG confluence consumption, Runtime, Build and Source/Architecture.

Head `41a578ba72fec2219447ddc1ceff12b96ee353e7`: Runtime Acceptance PASS; Build PASS; Source/Architecture PASS. Target cTrader replay remains required for empirical signal-quality measurement. No empirical false-signal or win-rate improvement is claimed from CI alone.
