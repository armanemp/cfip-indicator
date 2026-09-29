# CFIP Indicator — Phase 7.1 Hidden-Clamp Audit

Status: complete.

Date: 2026-09-29

## Objective

Audit user-facing parameter semantics for hidden hard-coded floors/ceilings,
silent overrides and numeric clamps that could make part of a cTrader setting
ineffective.

## Findings

### 1. Live Trigger Score — corrected

Parameter contract:

- `Live Trigger Score`: MinValue 1, MaxValue 6.
- `Precision Trigger Score`: MinValue 2, MaxValue 6.

The closed-bar trigger path previously applied:

`trigger >= Math.Max(4, requiredTrigger)`

That silently ignored user-selected Live Trigger Score values 1–3 and could
also ignore a Precision Trigger Score below 4.

Correction:

- the effective threshold is now exactly `requiredTrigger`;
- `requiredTrigger` remains the single combination of the user-facing Live
  Trigger Score and Precision Trigger Score according to the existing precision
  execution policy;
- no new parameter was added.

### 2. Target Update Step ATR — corrected

Parameter contract:

- `Target Update Step ATR`: MinValue 0.02, MaxValue 2.0.

The live target candidate path previously applied:

`Math.Max(0.05, TargetUpdateStepAtr)`

which silently made values 0.02–0.049999 ineffective.

Correction:

- the live target update step now uses `TargetUpdateStepAtr` directly;
- the existing cTrader parameter boundary remains the source of the user-facing
  range;
- no new parameter was added.

## Reviewed intentional bounds

The following bounds were inspected and were not treated as hidden user-setting
overrides:

| Expression / bound | Reason |
|---|---|
| `Math.Max(0, index - 6)` | historical index safety |
| `Math.Max(0, EntryBufferAtr)` | matches the parameter minimum of 0 |
| `Math.Max(0.10, StructuralTpRrStep)` | matches the parameter minimum of 0.10 |
| `Math.Max(0.05, MinimumTpSpacingAtr)` | matches the parameter minimum of 0.05 |
| `Math.Max(0, MaximumRewardRR)` | below the parameter minimum of 2.0, therefore no user-facing value is overridden |
| `ClampInt(DirectDisplacementOverrideScore, 1, 6)` | enclosing bounds are within the parameter contract of 3–6 |
| `Math.Min(20, Math.Max(1, level.Hits) * 2)` | bounded internal hit contribution, not a user-facing parameter |

No new parameter was introduced. Production parameter count remains 535.

## Verification

The source/architecture verifier now includes regression checks for both corrected
hidden clamps and verifies the declared cTrader parameter ranges.

Required gates:

- Source / Architecture
- Runtime Acceptance Contracts
- cTrader Compile

Hands-on cTrader validation remains separate and is required for actual signal,
target-update behavior and terminal responsiveness.

## Scope boundary

This phase only restores parameter effectiveness. It does not recalibrate decision
thresholds, redesign Entry/SL/TP logic, change RR policy, or implement new
trailing behavior. Those improvements remain owned by their corresponding
analytical/planning/live-management phases.
