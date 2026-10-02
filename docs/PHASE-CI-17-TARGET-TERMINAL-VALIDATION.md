# CI-17 — Target-terminal cTrader Validation

Date: 2026-10-02

Status: **IMPLEMENTATION PACKAGE COMPLETE — target-terminal manual acceptance remains required.**

## Scope

CI-17 is the first phase in this track that cannot be truthfully certified from repository CI alone. The repository-side work therefore does two things:

1. preserves the deterministic semantics already verified in CI-14..CI-16;
2. provides a no-trade target-terminal probe and an exact acceptance runbook for the real cTrader terminal/broker.

No public strategy parameter or trading threshold is introduced by this phase.

## Target-terminal probe

`preflight/CFIPPreflightBot.cs` remains a strict no-trade capability probe.

It now records:

- startup UTC and first-tick UTC;
- startup → first-tick latency;
- probe calculation revision;
- first/last probe calculation UTC;
- probe calculation age at observation;
- Symbol Bid / Ask and derived spread in pips;
- bar count and latest bar-open UTC;
- cTrader symbol/timeframe scope;
- final no-trade result.

The probe contains no broker order, position, protection, close or cancel mutation API.

Run the same probe on the target cTrader terminal for at least M1 and M5, and repeat after a history reload/restart.

## Required target-terminal evidence

### 1. Initialization / data readiness

Record the terminal version, broker/server, account type, symbol, timeframe, Automate assembly, and the exact production CFIP build.

PASS requires a successful CFIP instance and a progressing probe revision with finite quote data.

### 2. M1/M5 timing and closed-bar boundary

Run M1 and M5 separately.

Record:

- latest closed-bar boundary;
- live bar/open-bar boundary;
- probe calculation timestamps;
- observed Actionability Timing entries from the CFIP runtime log.

PASS requires semantic agreement with the closed-bar rules already proven in CI-10/CI-11/CI-16. A moving bar must not become a replacement for the closed decision reference.

### 3. Bid/Ask and execution-side geometry

Use the target broker's live Bid/Ask behavior.

For the later execution tests, record requested Entry/SL/TP and broker-confirmed values separately.

PASS requires:

- directionally valid SL/TP;
- correct market-side anchor;
- no hidden reconstruction of Entry/SL/TP between validated intent and broker submission.

### 4. Market fill / slippage

On demo first, test BUY and SELL market execution.

Record requested intent, broker result, actual fill and post-fill protection.

PASS requires the existing fill-acceptance envelope to be applied and broker-confirmed state to become authoritative.

### 5. Pending Stop / Limit

On demo, test BUY/SELL Stop and BUY/SELL Limit.

Record pending request, broker-confirmed pending object, fill event, resulting position and protection.

PASS requires no pending order to be treated as a position before broker fill confirmation.

### 6. Pending cancellation / expiration

Exercise explicit cancellation and configured expiration.

PASS requires the pending state to disappear only after broker-confirmed cancellation/expiration and no stale plan adoption.

### 7. Restart / reconnect

Perform a controlled terminal restart and a reconnect/network recovery.

Record the runtime log and broker state before/after.

PASS requires reconciliation before new execution assumptions, with no duplicate order or duplicate lifecycle transition.

### 8. Panel / chart responsiveness

With historical rendering enabled and during normal M5 operation, record:

- startup-to-visible-panel behavior;
- update latency;
- hide/show control response;
- signal/plan line freshness;
- absence of duplicate/stale drawings.

PASS requires no visible regression attributable to the current phase.

### 9. Signal → plan → execution synchronization

For one accepted scenario, trace the same identity through:

`Decision → Trigger → Actionable → Plan → ExecutionIntent → submission → broker confirmation → protection`.

Record the common signal/scenario identity and Entry/SL/TP at each boundary.

PASS requires exact semantic agreement. Any downstream drift is a blocker.

## Manual evidence table

| Test | Evidence | Result |
|---|---|---|
| M1 probe | terminal log | PENDING |
| M5 probe | terminal log | PENDING |
| history reload | terminal log | PENDING |
| restart/reconnect | terminal + broker state | PENDING |
| BUY market | request/fill/protection | PENDING |
| SELL market | request/fill/protection | PENDING |
| BUY STOP | pending confirmation/fill | PENDING |
| SELL STOP | pending confirmation/fill | PENDING |
| BUY LIMIT | pending confirmation/fill | PENDING |
| SELL LIMIT | pending confirmation/fill | PENDING |
| cancellation/expiration | broker state | PENDING |
| panel/chart responsiveness | screenshots + timestamps | PENDING |
| signal/plan synchronization | runtime trace | PENDING |

## Boundary

The repository can certify source/build/contract consistency, but it cannot manufacture evidence for a real terminal, broker server, network reconnect or chart renderer.

Therefore CI-17 is **not marked VERIFIED COMPLETE** until the manual evidence table has actual target-terminal PASS results.

The automated contribution of this phase is complete and remains intentionally no-trade.

## Next step

After the operator records the target-terminal evidence, the remaining Full-Stack track is **CI-FINAL — Full-stack Calculation Integrity Certification**.
