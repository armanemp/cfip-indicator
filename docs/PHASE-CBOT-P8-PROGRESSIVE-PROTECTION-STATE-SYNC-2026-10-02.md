# CBOT-P8 — Progressive Protection / Broker-Confirmed State Sync — 2026-10-02

## Status

**Implementation complete on branch `phase/cbot-p8-progressive-protection-state-sync-2026-10-02`; repository verification pending.**

## Purpose

This phase hardens the protection path after broker execution has been separated from the Indicator.

The key contract is now explicit:

**The broker-confirmed stop/target is the observed execution truth. The Indicator may not rewrite that state from an unconfirmed request. The plan stop may only advance protectively. A broker regression is a recovery condition, not a new plan state.**

The future TP1→TP4 analytical ladder remains separate from the currently active broker target.

## Canonical changes

### 1. One broker-confirmed state owner

Added:

`src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateSynchronizer.cs`

This is the single adoption boundary used by:

- broker-state refresh;
- confirmed management reports;
- position-modified lifecycle events;
- bound-plan protection confirmation;
- partial-close break-even confirmation.

No separate consumer is allowed to assign the protected plan stop from a broker-confirmed mutation.

### 2. Stop progression is monotonic in both directions

BUY:

- a confirmed broker stop can stay equal or move upward;
- a lower confirmed stop never rewrites the protected plan backward;
- a backward/invalid state enters explicit recovery.

SELL:

- a confirmed broker stop can stay equal or move downward;
- a higher confirmed stop never rewrites the protected plan backward;
- a backward/invalid state enters explicit recovery.

The existing canonical `ProtectionProgressionRule` remains the mathematical authority.

### 3. Target progression is monotonic

The active broker target is treated as broker-confirmed state.

A confirmed target must remain beyond entry and cannot regress relative to the previous confirmed target. The future TP1→TP4 plan ladder is not destroyed merely because the broker is currently sitting on one active target stage.

The existing `ProtectionProgressionRule.ShouldAdvanceTarget(..., preventBackward: true)` remains the target monotonicity authority.

### 4. Accepted vs confirmed is explicit

A management command being queued, submitted or merely accepted does not mutate the Indicator's broker-confirmed state.

Only `BrokerReportStatus.Confirmed` reports immediately update the broker-confirmed protection snapshot. The cBot remains the only broker mutation owner.

### 5. Trailing remains intelligent, not tick-chasing

The existing `IntelligentProtectionRule` remains the sole live protection policy.

The phase does not create a second trailing algorithm. Existing structural/momentum/pressure logic remains responsible for producing a candidate, while this phase guarantees that the candidate cannot move confirmed protection backward.

The existing structural pulse is bounded, and protection mutation remains behind the existing material-difference/progression gates.

### 6. Chart/panel consequence

Once a broker-confirmed protective stop advances, the canonical plan state can adopt that forward stop, so the existing Entry/SL/TP renderers display the confirmed protection instead of a stale pre-confirmation value.

The active broker target remains separately observable through `BrokerTarget`, preserving future target-stage geometry.

## Full-chain audit

Every work unit re-audits:

`Pre-analysis → M15 decision → M5 trigger/tuning → M1 optional confirmation → entry geometry → signal/alert → Indicator→cBot contract → cBot safety → broker mutation → broker confirmation → lifecycle → protection/trailing → chart/panel`

Canonical timeframe roles remain:

- **M15 = primary trade decision / execution reference**
- **M5 = trigger, entry tuning and entry precision**
- **M1 = optional confirmation**
- **H1+ = context/reward support**
- **host chart timeframe = presentation only**

No phase change is permitted to turn M5 into a competing execution clock.

## Verification contract

Required automated gates:

- Source / Architecture;
- accumulated historical audits;
- P8 progressive-protection/state-sync audit;
- Runtime Acceptance;
- cTrader Compile/Build.

Target-terminal validation remains required for:

- actual cBot attachment and restart/reconnect;
- broker-confirmed SL/TP mutation timing;
- trailing frequency and step quality;
- no-regression behavior after reconnect;
- chart line/label lifecycle while a live position exists;
- empirical realized-R and false-signal measurements.

## Interaction with CBOT-6M

This phase does **not** remove the current single-plan execution capacity.

Concurrent Market/Aggressive + Pending Stop/Limit scenarios require the separately documented **CBOT-6M** contract with per-ScenarioId identity, idempotency, risk/capacity, reconciliation and protection ownership.

The current single-plan gate remains until that phase is complete and verified.

## Performance / cleanliness

The phase keeps broker reads bounded by the existing broker refresh policy, uses the existing dirty-state invalidation path, avoids duplicate trailing engines, and moves confirmed-state adoption into one canonical owner.

## Operator action

Do not run `git pull` until the relevant PR is merged into `main`.

