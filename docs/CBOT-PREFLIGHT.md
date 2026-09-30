# CBOT-Preflight — Local cTrader Capability Gate

Date: 2026-09-30

Status: IMPLEMENTED — target-terminal verification required

This is a blocking, no-trade capability test. It proves the local cTrader host boundary before creating the platform-neutral Contracts project or moving broker execution into a cBot.

## 1. What is being proven

1. A cBot can reference a custom indicator through cTrader's supported custom-indicator mechanism.
2. Indicators.GetIndicator<T>() can instantiate a custom indicator reference.
3. Reading an indicator Output forces the referenced indicator calculation to run.
4. Public read-only indicator state can be consumed by the cBot when the indicator has exposed it.
5. Symbol/timeframe scope and a monotonic revision can be observed deterministically.
6. The cBot can create CFIPIndicator with automatic execution disabled.
7. Startup failure, unavailable indicator, stale/uninitialised data and invalid instances can be treated as blocked states.
8. No chart-object scraping, reflection, static singleton, file IPC, HTTP or broker mutation is needed.

cTrader officially documents custom-indicator references in cBots through Manage References followed by Indicators.GetIndicator<>(). It also documents an anonymous-object form for indicator parameters. citeturn826740view0turn687827search0

A cTrader staff response documents lazy calculation behavior for referenced custom indicators and explains that reading an Output can cause the calculation to run before trusting public state. citeturn534851search0

## 2. Test-only files

- preflight/CFIPPreflightProbeIndicator.cs
- preflight/CFIPPreflightBot.cs

The probe exposes a harmless numeric Output plus read-only diagnostics. The cBot intentionally contains no broker mutation API.

## 3. Target-terminal procedure

### A. Build the probe indicator

Create a temporary custom Indicator named exactly CFIPPreflightProbeIndicator, paste the repository preflight source and build it successfully.

### B. Prepare CFIP

Build the current CFIPIndicator from the repository. Do not enable Auto Trading, Automatic Orders or Aggressive Auto Entry for this test.

### C. Build the preflight cBot

Create a temporary cBot named CFIPPreflightBot, paste preflight/CFIPPreflightBot.cs, then use Manage References to add CFIPPreflightProbeIndicator and CFIPIndicator. citeturn826740view0

### D. Run with no-trade conditions

Attach the cBot to a demo/test symbol only. The source must remain completely incapable of placing, modifying, cancelling or closing a broker order.

Expected log sequence:

- CFIP PREFLIGHT START | NO TRADE
- PROBE | calculated=True ...
- CFIP INSTANCE | created=True
- CFIP PREFLIGHT RESULT | HOST+INSTANCE PASS | NO BROKER MUTATION EXECUTED

## 4. Startup-order matrix

| Case | Action | Required result |
| --- | --- | --- |
| P1 | Start cBot first, then make Indicator available | stays blocked until valid instance exists |
| P2 | Indicator first, cBot second | instance can be created and read |
| P3 | Restart cBot while Indicator exists | no fabricated signal / no broker action |
| P4 | Restart Indicator before cBot | blocked until valid instance |
| P5 | Indicator becomes unavailable | fail closed |
| P6 | Indicator output stale/uninitialised | fail closed |
| P7 | Different symbol/timeframe | scope remains deterministic |
| P8 | Two Indicator instances | no cross-instance static/global state |

## 5. Important current-project boundary

The current CFIP build does not yet expose the final public structured signal/provider surface. That surface belongs to CBOT-2. Therefore this phase proves the host/reference mechanism and no-trade behavior; it does not pretend that CBOT-2 is already complete.

## 6. Required evidence

- cTrader version/build;
- CFIPIndicator build result;
- preflight cBot build result;
- P1–P8 log excerpts;
- confirmation that no broker mutation occurred;
- exact compiler/runtime errors, if any.

Do not replace a target-terminal failure with a source-only PASS.

## 7. Static gate

Run: python tools/audit_cbot_preflight.py

The static gate rejects broker mutation APIs, reflection, chart scraping, file/HTTP/WebSocket transport and static singleton usage in the preflight kit.

## 8. Blocking rule

CBOT-1 cannot start until the target-terminal capability gate is accepted.

Nothing in this phase introduces Cloud transport or changes production trading behavior.