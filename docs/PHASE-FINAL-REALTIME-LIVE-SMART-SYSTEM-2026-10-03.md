# CFIP — Final Realtime / Live / Smart System Integration — 2026-10-03

Status: IMPLEMENTED — automated verification pending; target-terminal acceptance pending.

## Objective

Complete the current realtime/live architecture as one coherent analysis-to-execution system while preserving the canonical role boundaries:

history/context -> all MTF evidence -> M15 canonical decision -> M5 entry precision/tuning -> optional M1 confirmation -> current-quote actionability -> current market execution OR future Stop/Limit pending order -> cBot preflight -> broker mutation -> broker confirmation -> protection/management -> outcome/history.

## Integrated work

### Realtime intelligence and scenarios

- All required analytical frames participate in the intelligence path: M1, M5, M15, M30, H1, H4, D1 and W1.
- M15 remains the canonical decision/execution reference.
- M5 remains the entry precision/tuning layer and does not become a competing execution clock.
- M1 remains optional precision/closed-trigger confirmation.
- Current ActionableNow scenarios remain the only immediate market-entry candidates.
- Future FutureOrderReady scenarios remain pre-planned broker Pending Stop/Limit candidates.
- Multiple distinct ScenarioIds remain independently reconciled with bounded concurrency and idempotency.
- Historical evidence and early prediction remain evidence/ranking layers rather than a second execution authority.

### Signal quality / stagnant market

- RewardDistanceAtr and MinimumRequiredRewardDistanceAtr are restored on the candidate contract.
- ScenarioExecutionPolicyRule enforces the regime-adaptive reward-distance floor instead of allowing very small excursions through the execution boundary.
- Existing RANGE structural-evidence and RR protections remain mandatory.
- Existing geometry, risk, spread, margin, session and daily-loss protections remain mandatory.

### Smart HTF arrows

- H1/H4/D1/W1 trend evidence maps into nine presentation levels.
- Levels 1–3 are weak, 4–6 medium and 7–9 strong.
- Each level displays one/two/three vertically stacked arrows according to strength.
- Direction remains owned by the canonical signal visual snapshot; HTF strength changes presentation intensity only.
- Directionless/stale markers are removed.

### Alert/audio

- Indicator signal alerts are split into panel presentation and realtime sound delivery.
- Sound-bearing alerts are queued independently and delivered from the Indicator realtime last-bar Calculate path.
- cBot lifecycle/execution/block/recovery audio remains one modular owner: CbotLifecycleAudioService.
- cBot Start, Stop, live-disarmed safety, blocked state, execution confirmation and rejection all have dedicated semantic cues.

### Live execution

- Live Market, Pending Stop, Pending Limit, Aggressive and Management controls are account-scoped.
- Live mutation remains explicitly armed and OFF by default.
- Demo/live use the same execution owners and safety boundaries.
- Indicator remains broker-mutation-free.

### Panel

- Header content has one realtime live-state owner and is refreshed independently of full panel rendering.
- Header cache is invalidated on panel rebuild.
- A fixed single-row MTF trend lamp rail covers M1/M5/M15/M30/H1/H4/D1/W1.
- The lamp rail is outside the ScrollViewer and remains visible while panel content scrolls.
- Lamp state refresh is lightweight/heartbeat-driven.

## Verification boundary

Automated verification:
- cTrader compile/build
- Runtime Acceptance Contracts
- accumulated Source/Architecture audits
- final integration audit

Target-terminal acceptance remains required for:
- actual cTrader audible signal/cBot sounds;
- exact Indicator/cBot attachment visibility and instance binding;
- same-tick handoff and entry drift behavior;
- simultaneous scenario execution;
- future Stop/Limit placement, fill, invalidation and restart/reconnect;
- live-account mutation with explicit arms;
- fixed MTF lamp visibility during scroll and realtime header freshness.

## Operator action after verified merge

On the local main checkout:

    git pull --ff-only
