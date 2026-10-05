# Phase 9.7 — Regime-Aware No-Trade & Auto-Execution Hardening

Date: 2026-09-29

## Status

VERIFIED COMPLETE on `469f66bd2d461016c9283e0e4243ca429c694aa3`.

Pre-merge gates on the final implementation head:
- Runtime Acceptance: PASS
- cTrader Compile/Build: PASS
- Source/Architecture: PASS

Merge closeout: PR #49 merged into `main` as `954e5021648e43a11d0de08f35b7fc7aaa8d2125`.

Local operator action: pull `main` before the next continuation.

## User-reported issues addressed

- weak signals appearing during RANGE conditions;
- weak directional visual fallbacks surviving when the decision is not actionable;
- level labels whose backgrounds can disappear in the terminal;
- need to improve the complete analysis → decision → plan → automatic execution path.

## Analysis / signal-quality changes

### RANGE / COMPRESSION policy

COMPRESSION is now a hard no-trade state.

RANGE is no longer treated as an unconditional blanket rejection. It is a specialist setup regime:

- mid-range directional signals are rejected;
- reversal candidates require range-edge location, liquidity event, structural reversal, displacement and WaveTrend confirmation;
- range reversals require stronger evidence, confidence and Smart Quality;
- strong closed-bar breakouts can still qualify when price breaks the prior range with structural/displacement/WaveTrend alignment;
- the same range-quality decision is consumed by decision filtering, visual presentation and automatic pending-order eligibility.

This prevents the system from emitting weak range trades while preserving genuinely strong range reversals/breakouts.

### Visual authority

Weak RANGE/COMPRESSION fallbacks no longer produce a directional chart arrow.

Reaction and prediction presentation continue to use the existing canonical visual state and are additionally filtered by the range-quality rule.

## Automatic market execution hardening

### Fresh suitability

Market suitability is forcibly refreshed immediately before automatic market entry rather than relying only on the normal throttled suitability cadence.

### Spread-to-stop-risk control

The existing Maximum Spread / Stop Risk Ratio parameter is now explicitly enforced against the live quote and the current structural stop distance before market submission.

### Bounded Market Range

Automatic market orders now use the cTrader market-range submission path.

The market-range envelope is derived from current spread and ATR but is hard-capped by the existing entry-extension envelope, so it cannot become an unrestricted chase.

The intent remains the same canonical ExecutionIntent and the same broker-confirmation/reconciliation lifecycle.

## Automatic pending-order hardening

Before a new pending order is placed:

- range-quality is checked;
- suitability is force-refreshed;
- existing single-plan/single-managed-identity capacity remains enforced.

Existing pending orders are now cleaned up when the regime invalidates them:

- COMPRESSION invalidates managed pending entries;
- RANGE preserves only qualified reversal LIMITs or qualified continuation/breakout STOPs.

## Label background correction

Compact level labels now use an explicitly opaque background color and bounded geometry so the fill cannot disappear solely because the supplied line color carries transparency.

The canonical label anchor/size work from Phase 9.6 remains in force.

## Safety / architecture boundary

No second decision engine was introduced.

No new broker mutation authority was introduced.

The single managed execution capacity remains unchanged.

No new public parameters were introduced.

## Verification boundary

Required gates:

- Runtime Acceptance / Contracts;
- cTrader Compile/Build;
- Source/Architecture.

Target cTrader replay remains required for empirical validation of:

- RANGE false-signal frequency;
- chart label visibility/placement;
- market-range execution behavior;
- actual slippage;
- realized RR and execution quality.

No profitability or win-rate improvement is inferred from automated static/contract verification.
