# Phase 11 — Economic News Guard, Reference-Indicator Reassessment & Calibration

Date: 2026-09-29

## Findings

The previous CFIP news control was not a real economic-calendar integration. It was a manual `NewsBlackoutUtc` time-window parser. Consequently, unless the operator manually entered a blackout interval, scheduled news did not participate in the decision gate.

The user-provided economic reference indicator provides a useful weekly XML feed and symbol-currency relevance model. The user-provided WaveTrend and FVG indicators were also re-inspected.

## Reference integration decisions

### WaveTrend
The project already has a cleaner internal WaveTrendEngine with closed-bar snapshots and evidence scoring. The duplicate reference implementation is retained only as a parity reference.

### FVG
The project already has canonical two-bar and three-bar FVG geometry, retest selection, partial mitigation and wick/body/full-fill controls. The reference implementation therefore does not replace the canonical FVG engine. Its opening-gap concept is kept separate from structural FVG evidence to avoid double-counting displacement/volatility.

### Economic calendar
The useful portion of the reference implementation is now integrated into CFIP's canonical risk path.

## Phase 11 implementation

- enabled Internet access for the indicator;
- added configurable economic-calendar feed using the supplied weekly XML source as the default;
- added cached refresh on the existing runtime Timer rather than on every tick;
- inferred relevant currencies from the symbol and configured additional currencies;
- added high-impact blackout windows before and after events;
- optional medium-impact blocking;
- added stale-feed fail-closed behavior for auto trading;
- added pending-order cancellation before high-impact events;
- added optional pre-news active-position close (default off);
- retained the manual UTC blackout as an explicit override;
- added news state to the execution panel;
- added NEWS_RISK runtime telemetry;
- added automated source-level News Guard audit.

## Safety boundary

The news guard prevents new automatic risk around scheduled events and can cancel pending orders. It does not guarantee that an already-open position cannot hit its broker stop during a fast release, gap or slippage event. Optional pre-event close is therefore explicit and disabled by default.

## Verification boundary

Required automated checks:
- cTrader compile/build;
- runtime acceptance contracts;
- source/architecture and News Guard audit.

Required terminal evidence:
- actual feed access from the target cTrader environment;
- symbol/currency mapping for the target instrument;
- news-block timing around a scheduled high-impact release;
- pending cancellation behavior;
- optional active-position close behavior when explicitly enabled.

Prediction/accuracy claims still require outcome/replay data.
