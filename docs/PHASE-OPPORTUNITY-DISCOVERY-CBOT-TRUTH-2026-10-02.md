# Opportunity Discovery + cBot Truth + SL/TP Coverage — 2026-10-02

## Focus

analysis → opportunity discovery → entry-zone selection → structural SL → reward path / TP → actionability → signal/alert → cBot handoff

## Position discovery changes

- Execution-zone selection compares M5/M15 FVG, Order Block, same-frame OB+FVG, cross-timeframe overlap and M15/H1 structural levels.
- Zone selection is reward-path aware: the downstream structural-stop candidate and available TP1 reward path affect selection.
- Strong-HTF counter-M5 tactical discovery is reachable when the selected direction follows the strong HTF anchor; the dedicated counter lane keeps stricter quality/RR requirements.
- Target FVG discovery now includes forward opposing FVGs without requiring current-bar retest and uses quality/distance/age-aware selection.
- M5 and HTF structural-stop FVG discovery likewise includes valid unretested support/resistance zones and remains bounded by structural geometry, risk and reward gates.

## cBot recognition

- cBot and Indicator now have stable display/type identities.
- The cBot publishes a symbol-scoped presence heartbeat even before Indicator binding succeeds.
- The Indicator distinguishes CBOT DETECTED from CBOT NOT ATTACHED.
- Exact instance-scoped fresh heartbeat remains mandatory for execution capability. Presence alone never authorizes broker action.
- Empty/stale/invalid heartbeat remains fail-closed.

## Alerts / panel

- Alert sound remains single-owned by the bounded delivery processor.
- Custom sound failure falls back to the semantic/default cTrader cue.
- Queue/sound delivery is explicitly logged.
- Panel alert messages are left aligned and use a larger readable minimum font.

## Preserved trading contracts

- M15 remains the canonical trade-decision/execution reference.
- M5 remains trigger / entry-tuning / precision; it is not a competing execution clock.
- M1 remains optional confirmation.
- H1+ remains context and reward support.
- Existing ActionableNow, structural risk, RR, regime, news, spread and broker safety gates remain authoritative.
- Broker execution remains cBot-owned and demo/staged until the dedicated multi-scenario execution capacity phase.

## Verification

Source/architecture, runtime-contract and cTrader build checks must pass before merging this phase. Target-terminal validation is still required for actual attachment/heartbeat, sound playback, panel rendering and empirical opportunity frequency/quality.

This implementation is not a profitability claim. Empirical replay/OOS/walk-forward evidence is still required.

## Next focus

The next position-focused unit should mine persisted SignalEvaluationTrace and outcome history by candidate family and rejection reason, then add only measured coverage improvements. Multiple analytical scenarios can be displayed; broker multi-position execution remains a separate execution-capacity concern.
