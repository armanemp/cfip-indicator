# M1 — Full Forensic Audit

Date: 2026-10-02

Status: **VERIFIED COMPLETE — all findings have an explicit disposition.**

Branch: `phase/M1-full-forensic-audit`

## 1. Audit baseline

The audit was performed from the post-M0 `main` baseline and covered:

- `src/CFIP.Indicator/**` production code;
- execution/broker boundary and ownership documents;
- accumulated static audits in `tools/audit_*.py`;
- runtime/build workflow coverage;
- current panel/chart/alert/persistence owners;
- public-parameter inventory.

Repository inventory at audit start:

| Metric | Current evidence |
| --- | --- |
| Production Indicator C# files | **633** |
| Public parameters | **568** |
| Direct broker mutation call-sites | **15** |
| Source/Architecture gate | **PASS** on M0 closeout HEAD |
| Runtime Acceptance gate | **PASS** on M0 closeout HEAD |
| cTrader Compile gate | **PASS** on M0 closeout HEAD |

The public-parameter baseline is machine-verified by `audit_parameter_count.py` / `audit_parameter_semantics.py` and documented as 568 in README and architecture verification.

## 2. Findings and dispositions

| # | Area | Status | Finding | Disposition |
| --- | --- | --- | --- | --- |
| F1 | BUY/SELL symmetry | **VERIFIED** | Canonical stop/target geometry rejects wrong-side/non-finite levels symmetrically for BUY and SELL; deterministic exit-geometry contracts cover both directions. | Preserve. Re-run symmetric fixtures in M8 before any execution extraction. |
| F2 | Closed/live boundary | **VERIFIED** | Calculation ordering explicitly separates preparation, same-cycle broker reconciliation, closed-bar analysis, live stages and queued alert delivery. Closed-bar reference/readiness has dedicated owners. | Preserve. No redesign required in M1. |
| F3 | NaN/Infinity/zero/negative | **PARTIAL** | Canonical protection/RR rules contain finite/positive validation, but a repository-wide proof for every arithmetic consumer is broader than one static gate. | M8 performs exhaustive geometry validation; M11 Replay adds deterministic invalid-input fixtures. |
| F4 | Hidden constants / clamps | **PARTIAL / DESIGN-RISK** | The code contains many named constants and `Math.Min/Math.Max` bounds. Most important values have named Core owners, but there is not yet a single machine-derived inventory proving every threshold has one policy owner. | M2 ownership/dead-code audit, then M15 parameter/constant simplification. No tuning in M1. |
| F5 | Bounded collections | **VERIFIED for reviewed critical stores** | Outcome/telemetry/history stores expose explicit bounds; buffered persistence removes flushed file entries and does not perform synchronous disk writes from the hot path. | Preserve. M10 extends measurement to allocation/cache pressure. |
| F6 | Hot-path I/O | **VERIFIED** | Buffered archive persistence owns synchronous file writes; runtime log/outcome paths enqueue; news refresh runs on timer rather than every tick. | Preserve. No blocking I/O added. |
| F7 | Time/session semantics | **PARTIAL / MANUAL-ONLY** | UTC-oriented owners and session rules exist, but restart, DST, broker-timezone and target-terminal behavior require runtime evidence. | M4 + M39 manual terminal matrix. |
| F8 | Persistence/history | **PARTIAL / MANUAL-ONLY** | 90-day rolling archive, history marker, buffered writes and outcome deduplication are implemented, but storage-path and recovery behavior need terminal verification. | M4 certification tests and target-terminal acceptance. |
| F9 | Event idempotency | **VERIFIED** | Submission/lifecycle identity and duplicate suppression are represented by dedicated owners; managed outcome recording checks position identity before recording. | Preserve. M9 deterministic mock-broker scenarios remain the acceptance boundary. |
| F10 | Identity propagation | **VERIFIED** | Managed execution identity is instance-scoped; managed position/pending lookup requires the canonical identity and execution telemetry carries the instance identity. | Preserve. Required unchanged during cBot split. |
| F11 | Decision/execution authority | **VERIFIED boundary / DESIGN-RISK until extraction** | Broker mutation remains intentionally inside the Indicator today, concentrated in 15 explicit owner call-sites. Analytical Planning is not itself a broker-mutation owner. | Frozen by design until M29–M38. No new mutation authority may appear. |
| F12 | Mixed ownership | **DESIGN-RISK** | `BrokerProtectionCoordinator` and parts of `Trading/LiveManagement/**` mix analytical decisions with broker mutation/reconciliation. | Split by dependency closure in cBot extraction; do not copy whole classes. |
| F13 | Visual authority | **PARTIAL / MANUAL-ONLY** | One visual snapshot path and panel content-refresh architecture exist, but live panel/chart responsiveness still requires target-terminal validation. | M5 + M7 + M39. |
| F14 | Alert authority | **VERIFIED static / MANUAL residual** | Popup and sound consume the same queued delivery event and production sound has a single delivery owner. | Preserve. Manual terminal audio/popup race acceptance remains M6/M39. |
| F15 | Outcome accounting | **PARTIAL** | Position-id deduplication, realized net profit/pips/R, lane/regime/confidence attribution and 90-day archival are implemented; full empirical attribution quality is not yet proven without Replay/OOS evidence. | M11–M16. Do not use current outcome memory as proof of profitability. |
| F16 | Large-method ownership | **DESIGN-RISK** | Two production methods are disproportionately large: `SignalVisualSnapshotBuilder.BuildSignalVisualSnapshot` ≈401 lines and `TradeActionabilityEvaluator.EvaluateTradeActionability` ≈533 lines. | M2/M10 decomposition with golden/regression tests before optimization. |
| F17 | Public-parameter baseline | **VERIFIED + TOOLING FIXED** | README/architecture/current semantic audits use **568**, while `audit_cbot_boundary.py` still expected **564**. This was a stale audit contract. | Corrected audit baseline from 564 → 568 on this branch. No production behavior changed. |
| F18 | cBot separation readiness | **PARTIAL** | Boundary inventory is clear enough to continue, but dependency closure and terminal custom-indicator transport remain unproven. | M29+; keep CBOT-Preflight/manual host capability as a blocking boundary. |

## 3. Cross-cutting root causes identified

### A. Historical audit contracts drifted from the current baseline

The concrete M1 example is the cBot boundary audit expecting 564 while the current authoritative parameter inventory is 568. This was documentation/tooling drift rather than a production trading calculation defect.

### B. Architecture is already partially modular, but several large mixed owners remain

The codebase has dedicated Core math owners, explicit broker-mutation owners, persistence buffering and unified alert delivery. The remaining architectural pressure is concentrated in large orchestration/renderer classes and mixed Indicator/cBot execution boundaries.

### C. Several important claims are statically proven but not yet terminal-proven

This applies particularly to live panel refresh, chart rendering, audio timing, restart/reconnect recovery, broker history aggregation and broker-specific protection behavior.

## 4. M1 safety conclusions

- No new trading feature, indicator, threshold tuning or execution policy was introduced.
- No broker-mutation authority was expanded.
- No existing analytical authority was duplicated.
- M1 changes are limited to audit tooling/documentation: the cBot boundary audit baseline/parser was aligned with the authoritative 568-parameter inventory. No production trading behavior changed.
- Open/manual items are explicitly assigned to later roadmap phases; none is silently dropped.

## 5. Required regression boundary for later phases

The following invariants are frozen from M1 onward:

1. Broker-confirmed state remains authoritative.
2. Submitted, accepted and filled remain distinct states.
3. BUY/SELL protection geometry remains symmetric.
4. Protective SL may only tighten.
5. TP progression may never move backward.
6. Blocked signals produce no user-facing sound/mark.
7. One canonical visual snapshot feeds panel/chart consumers for the same refresh.
8. One canonical alert-delivery event feeds popup and sound.
9. One managed execution identity scopes broker ownership.
10. No direct broker mutation may appear outside the frozen broker-owner set before cBot extraction.

## 6. Verification references

The M0 closeout that preceded this audit had all three repository gates green:

- Source / Architecture #2756: PASS
- Runtime Acceptance Contracts #2565: PASS
- cTrader Compile #2749: PASS

M1 itself introduces no new runtime behavior. The new branch must run the same accumulated three-gate suite before M1 closeout.

## 7. Next phase

**M2 — Repository Hygiene / Dead Code / Ownership**

Focus: eliminate dead/unreachable code and duplicate ownership, classify working/evidence/archive documentation, and decompose obvious ownership collisions before M3 canonical trade truth work.
