# CBOT-DEMO-MARKET — Demo-only Market Execution Bridge

Date: 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02 repository gates passed; target-terminal demo execution remains manual.**

## Why this phase exists

The cBot is already a separate cTrader project and can be built, but the previous shadow host did not yet consume the exact visible Indicator signal or own a controlled broker submission path. This phase creates the smallest useful demo-only Market bridge without activating the remaining pending/aggressive/management mutation paths.

## Warning correction

The current cTrader.Automate package marks the string-bearing IndicatorAttribute(string) constructor obsolete even though the stable indicator display name remains part of the attribute metadata. The code therefore suppresses CS0612 only around that attribute. No class rename, parameter change or strategy behavior is introduced.

## Data path

CFIP Smart Indicator
-> canonical SignalEnvelope
-> LocalStorageScope.Device
-> exact InstanceId key
-> CFIP Smart Execution Bot
-> freshness / identity / broker-safety validation
-> demo Market execution.

The cBot does not instantiate a second analysis engine and does not reference the Indicator project.

## Demo execution safeguards

1. Execution switch defaults OFF.
2. A live account is rejected at startup.
3. Session execution cap defaults to 1.
4. Only Market action is accepted.
5. Missing or duplicate Indicator instance blocks execution.
6. Stale/future/mismatched-symbol signals block execution.
7. BUY/SELL stop/target side geometry is checked again in cBot.
8. Volume and broker minimum/maximum are rechecked.
9. Existing managed position/pending capacity blocks a second execution.
10. Idempotency keys are bounded and never submitted twice per cBot session.
11. Pending, Aggressive, Close, SL modification, TP modification and recovery lifecycle are not activated by this phase.

## Operator configuration for demo

Indicator:
- Enable Auto Trading = OFF
- Enable Automatic Orders = OFF
- Enable Aggressive Auto Entry = OFF
- Auto Protect Broker Positions = OFF
- Enable Live Exit Management = OFF

cBot:
- Enable Demo Market Execution = ON only for the test;
- Max Demo Market Executions Per Session = 1;
- Provider Stale After Seconds = 15.

The cBot contains an Account.IsLive guard, so this build is intentionally demo-only.

## Verification

Required repository gates:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile / Build;
- focused CBOT demo market boundary audits.

Required terminal evidence:
- cBot appears as CFIP Smart Execution Bot;
- it binds to exactly one CFIP Smart Indicator on the same M5 chart;
- the log shows the canonical envelope becoming ready;
- exactly one demo Market position is opened for the accepted envelope when armed;
- initial broker SL/TP correspond to the canonical execution geometry;
- a second tick does not resubmit the same idempotency key;
- attaching the cBot to a live account terminates it without trading.

## Scope boundary

This is a validation bridge, not the final execution migration. The remaining Indicator Market mutation owner is removed only in the current-main reissued CBOT-P4A phase after target-terminal parity is demonstrated. Later phases migrate Aggressive, Pending, Close, SL, TP, lifecycle, account risk and recovery.
