# CFIP Phase 5.6 — Responsive panel runtime

Status: complete on the phase branch; merge verification passed.

Root causes addressed:
- Full panel rendering was tied primarily to the Calculate presentation stage while the timer heartbeat refreshed only the clock row. The panel could therefore remain stale when analysis was delayed or absent.
- Panel rendering rebuilt the canonical visual snapshot multiple times per refresh because fallback panel helpers could each construct a snapshot independently.
- Panel startup allocated all 64 TextBlock rows eagerly before the first useful data render.
- The panel writer rewrote the same UI properties on every refresh.
- Safety supervision and UI refresh had one timer cadence, limiting UI responsiveness without increasing safety work.
- Initialization status changed during async data loading but did not trigger panel refreshes.

Implementation:
- Ready-state timer cadence is 500 ms for responsive panel refresh.
- Safety supervision remains independently bounded to a 1-second cadence.
- Initialization polling is 250 ms instead of 100 ms to reduce startup timer churn.
- The panel heartbeat requests the full panel render without running full analysis.
- Each standalone panel refresh creates one canonical SignalVisualSnapshot and releases it after rendering; calculation presentation reuses its existing snapshot.
- Panel rows are allocated lazily up to the existing 64-row capacity.
- Panel row properties are only written when their effective values change.
- The panel skips full content rendering while hidden and refreshes immediately when restored.
- The panel exposes calculation freshness as CALC …s AGO / CALC NO CYCLE for direct diagnosis of calculation-vs-render staleness.
- The first render after initialization reaches READY is forced to avoid the panel throttle delaying the initial ready state.

Non-goals:
- no signal threshold change;
- no decision-rule change;
- no RR policy change;
- no risk sizing change;
- no trailing change;
- no broker execution semantic change.

Verification on final phase head:
- Source / Architecture: PASS.
- Runtime Acceptance Contracts: PASS.
- cTrader Compile: PASS.

Next continuation point:
- Phase 6.1 — Decision closed-bar contract.