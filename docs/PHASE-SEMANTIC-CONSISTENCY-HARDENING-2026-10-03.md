# Phase — Cross-Layer Semantic & Visual Consistency Hardening (2026-10-03)

## Scope

This phase closes presentation and semantic mismatches where the same underlying state was being re-encoded differently across panel rows, the live header, MTF alignment and evidence diagnostics.

The goal is not to remove legitimate semantic distinctions. Trade direction, timeframe bias, neutral state and evidence conflict remain distinct concepts. The correction is that each concept is now rendered from its canonical owner instead of being guessed again by each consumer.

## Findings

1. PanelTimeframePresentationState was already the canonical MTF presentation owner, but several downstream consumers translated its resolved direction back into BUY/SELL. This could make one surface show BULL BIAS/BEAR BIAS while another showed BUY/SELL for the same timeframe.
2. MARKET BIAS, primary M15/H1 alignment and the realtime panel header contained this re-encoding seam.
3. The WaveTrend row displayed its own BULL/BEAR evidence text while its color depended on agreement with the separate trade direction. A directional evidence statement could therefore appear in a neutral color without an explicit conflict state.
4. The Top-Down row exposed raw numeric direction values, creating a second visual vocabulary for direction.
5. The Decision row could show READY when EntryAllowed was true while the Entry Gate row simultaneously showed BLOCKED whenever ActionableNow was false. Those fields represent different stages, but the wording made them appear contradictory.
6. PredictionReadinessText could show ENTRY CONFIRMED for any EntryAllowed decision even when the current entry was still waiting on trigger or actionability. The wording mixed setup qualification with current entry execution readiness.

## Corrections

- Market Bias now consumes the canonical DirectionLabel for M15, H1 and M5.
- Primary M15/H1 alignment uses the same canonical timeframe labels while retaining numeric direction only for the actual alignment calculation.
- The realtime header uses the same canonical timeframe labels.
- Top-Down HTF and MID directions are rendered through DirectionText; raw numeric directions are no longer displayed. ENTRY retains only its alignment/strength fields because Decision has no separate EntryFrameDirection owner.
- Decision and Entry Gate statuses now distinguish ACTIONABLE, BLOCKED, WATCH, WAITING TRIGGER and WAITING ENTRY instead of using READY/BLOCKED in overlapping ways.
- WaveTrend text and color now share WaveTrend's own direction. When it opposes the trade direction, CONFLICT is made explicit and the row uses the warning semantic.
- No strategy threshold, decision authority, M15/M5/M1 role, risk rule, execution rule, broker ownership or calculation cadence changed.

## Architecture rule

UI consumers must not translate a canonical state into a competing vocabulary. A consumer may add context only when that context is derived from a distinct semantic state and is made explicit in the rendered text.

## Verification

Added tools/audit_phase_panel_semantic_consistency_2026_10_03.py and registered it in .github/workflows/source-check.yml.

The audit protects canonical timeframe labels, primary alignment, live-header parity, Top-Down text direction, and WaveTrend text/color/conflict consistency.

## Full-chain routine audit

Analysis -> MTF -> Decision -> Signal -> Alert -> cBot execution -> Broker confirmation -> Protection/Lifecycle -> Outcome/History was reviewed at the ownership boundary. This phase intentionally changes presentation semantics only; no business-rule threshold or execution gate was modified.

Performance remains presentation-only because the new consumers use already-calculated frame state and do not create another market-data or execution loop.

## Verification boundary

Required:
- Source/Architecture CI on the exact branch head.
- Runtime Acceptance.
- cTrader Compile/Build.
- Target-terminal inspection of MTF lamps, MTF text, Market Bias, header, Top-Down, WaveTrend conflict presentation and alert readability.

Operator action after merge: git pull --ff-only.
