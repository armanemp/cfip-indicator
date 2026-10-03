# Adaptive Reward / Risk / Protection Hardening — 2026-10-04

## Scope

This phase replaces fixed-RR strategy behavior with adaptive reward/risk geometry and closes the Indicator-to-cBot protection gap.

## Canonical owners

- Trigger/entry geometry: existing EntryGeometryRule + canonical actionability/plan pipeline.
- Reward profile: AdaptiveRewardRiskProfileRule.
- Target ladder: TargetSelectionRequiredRrRule using live risk/ATR and setup context.
- Stop/trailing: IntelligentProtectionRule + AdaptiveProtectionProfileRule.
- Break-even: SmartBreakEvenRule, now with an adaptive trigger floor rather than a fixed 0.50 RR floor.
- Broker execution/protection: CFIP cBot.
- No independent cBot signal/SL/TP/trailing strategy was introduced.

## Adaptive inputs

The reward profile considers:
- actual stop risk expressed in ATR;
- market regime;
- opportunity lane;
- confidence;
- smart quality;
- target quality;
- structural quality;
- maximum structural target extension.

RR therefore changes with the setup. TP1..TP4 remain progressive, but their required reward is derived from current geometry instead of a fixed RR ladder.

## Protection behavior

1. Initial stop remains structural and risk-aware.
2. Break-even can activate at the first safe opportunity while respecting spread/buffer and TP1 collision constraints.
3. Structural trailing never widens the stop.
4. Trailing only advances when structural evidence, profit state and live-price breathing room allow it.
5. Momentum and exit pressure can tighten the trailing profile.
6. Targets can progress only forward; they never move backward.

## cBot hardening

The cBot now has a canonical adaptive server-protection execution path for market and pending orders when the Indicator provides a valid server TP ladder:
- relative SL;
- TP1/TP2/final TP ladder;
- server-side break-even;
- cTrader server-side enforcement.

Legacy simple SL/TP submission remains only as a compatibility fallback when the adaptive ladder is unavailable. The robot does not recalculate trading strategy.

cTrader's July 2026 Algo 5.9 release introduced this server-side Advanced Protection API, making it preferable to a polling-only protection implementation for the initial protection contract.

## Verification

Source review completed for the modified architecture. A local Release build was not available in this environment, so compile/runtime acceptance remains pending.

Required next validation:
- Release build;
- planning contract tests;
- cBot compile;
- cTrader runtime;
- historical replay across M15 decision + M5 trigger/entry;
- measure MFE/MAE, entry excursion, stop distance, time-to-BE, realized R, target obstruction and trailing giveback.

Do not tune additional thresholds before replay identifies a concrete failure cluster.
