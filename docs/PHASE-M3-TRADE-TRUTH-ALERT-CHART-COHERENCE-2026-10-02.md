# M3 — Single Trade Truth / Alert–Chart Coherence — 2026-10-02

## Scope

This phase closes the alert/presentation coherence gap across the full path:

Market data → MTF closed-bar analysis → decision → prediction → plan → signal state → panel/chart presentation → popup/sound → Indicator provider → cBot.

The phase does not add a second signal engine, a second alert engine, or any broker authority. It makes the existing canonical state explicit across the presentation boundary.

## Findings and corrections

### 1. Alert transport

Before this phase, popup and sound were already drained from one bounded queue, but the queue payload did not carry the canonical signal identity. The event could therefore be delivered consistently while remaining difficult to prove as the same Signal/Scenario/Plan consumed by the chart and provider.

Correction:
- introduced immutable CFIP.Contracts.AlertEnvelope;
- AlertDelivery now carries that envelope;
- AlertEngine builds the envelope from the existing provider identity resolvers;
- popup and sound continue to consume the same queued event.

### 2. Blocked signal side effects

Restriction/blocked candidates are diagnostics, not actionable signal events.

Correction:
- blocked candidate alerts can still use the explicitly configured restriction popup;
- blocked candidates do not emit the normal signal sound;
- blocked candidates do not authorize the directional watch marker.

### 3. Chart signal ownership

The legacy alert mirror had no legitimate second rendering authority. Keeping state only to say that an audible alert should be mirrored was redundant because the canonical visual snapshot already owns the chart signal state.

Correction:
- removed the obsolete _lastVisualAlert* state and RememberVisualSignalAlert side-channel;
- retained the legacy cleanup entry point without allowing it to draw a duplicate marker;
- directional watch rendering now requires an entry-allowed snapshot and rejects pending/live states.

### 4. Canonical identity in presentation

The visual snapshot now carries:
- SignalId;
- ScenarioId;
- PlanId;
- SourceTimeframe;
- Revision.

These values are resolved through the same provider identity functions already used at the Indicator→cBot boundary.

### 5. Chart level labels

The main plan label previously exposed a fixed (MTF) tag. The actual canonical source timeframe is now resolved from the canonical scenario snapshot, with the existing M5 fallback only when no scenario timeframe is available.

The existing geometry contract is preserved:
- solid lines;
- bounded thickness policy;
- 40-candle compact span by default;
- labels start to the left of the line endpoint with a deterministic horizontal gap;
- no infinite extension.

## Full-chain ordering audited

The closed-bar path is ordered as:
1. closed MTF frames;
2. canonical market-state snapshot;
3. authoritative Decision;
4. early Prediction;
5. canonical plan synchronization/creation;
6. canonical alert evaluation against that Plan;
7. presentation from the visual snapshot;
8. queued alert delivery;
9. read-only provider/cBot handoff.

The regular calculation cycle also evaluates the decision-owned watch/reaction alerts before presenting the final visual snapshot and draining the delivery queue.

## Safety/ownership

No broker mutation authority moved back into the Indicator.

The cBot remains the broker mutation authority, while the Indicator remains the analysis/signal/provider source.

## Verification boundary

Required repository gates:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build.

Target-terminal verification remains required for:
- popup visibility and lifetime;
- audible sound playback;
- chart marker alignment;
- M1/M5 timing;
- multiple simultaneous scenarios;
- cBot startup/reconnect;
- live broker confirmation.

## Operator action

After the phase PR is merged, run:

git pull --ff-only
