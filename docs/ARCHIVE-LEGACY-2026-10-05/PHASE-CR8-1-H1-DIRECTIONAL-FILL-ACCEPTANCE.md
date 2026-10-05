# CR8.1 / H1 — Directional execution-fill acceptance and one canonical fill envelope

Date: 2026-10-01

Status: **VERIFIED COMPLETE — PR #149 merged to `main`.**

## Required phase confirmation

تأیید می‌کنم — the verified fill call chain is:
- `Planning/Execution/ExecutionIntentValidation.cs` →
  `ValidateActualMarketFill`;
- `Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs`;
- `Planning/Execution/MarketEntryValidation.cs` →
  `IsExecutableFillPrice`;
- `Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs`;
- canonical Core owner `Core/Math/ExecutionFillAcceptanceRule.cs`.

## Root cause verified

Two incompatible fill-envelope semantics existed:
1. the Core rule used symmetric absolute distance;
2. the Aggressive accepted-fill handler repeated a second absolute
   `Math.Abs(actual - requested)` envelope;
3. Automatic Market validated the fill against a separate trigger tolerance
   instead of the canonical directional envelope.

The Automatic Market reconciliation path also marked the position
`LivePosition` before the fill envelope had been validated, while
`ReconcileLivePlanToActualFill` subsequently rewrites `Plan.Entry` to the
actual fill. Therefore fill-envelope validation must happen before that
reconciliation mutates the requested-entry reference.

## Implementation

- `ExecutionFillAcceptanceRule.IsAcceptable(direction, requestedEntry,
  actualFill, atr, maxAdverseExtensionAtr, allowFavorable)` is now the single
  direction-aware fill-envelope owner.
- Favorable BUY movement (`actual < requested`) and favorable SELL movement
  (`actual > requested`) are accepted when `allowFavorable=true`.
- Only adverse movement is bounded by the canonical ATR envelope.
- Invalid direction, ATR, envelope and price inputs fail closed.
- `ValidateActualMarketFill` passes the explicit trade direction and
  `allowFavorable=true`.
- `IsExecutableFillPrice` now consumes the same canonical envelope using the
  plan's requested Entry and setup M5 ATR before mode-specific checks.
- The Aggressive duplicate absolute-distance envelope was removed.
- Automatic Market now validates the fill before reconciliation and publishes
  `LivePosition` only after canonical fill acceptance and successful actual-fill
  exit reconciliation.

## Behavior change

This phase intentionally changes fill acceptance:
- a favorable BUY fill below the requested entry is no longer rejected by the
  fill envelope;
- a favorable SELL fill above the requested entry is no longer rejected by the
  fill envelope;
- adverse extension beyond the canonical envelope remains rejected.

This is the explicit H1 safety/semantics correction. No public parameter
name/type/`DefaultValue` or numerical default was changed.

## Deterministic verification

Runtime contracts cover:
- BUY favorable fill accepted;
- BUY adverse fill beyond envelope rejected;
- SELL favorable fill accepted;
- SELL adverse fill beyond envelope rejected;
- exact adverse boundary;
- invalid direction/ATR/envelope;
- favorable acceptance requires the explicit policy flag.

`tools/audit_phase_8_1.py` additionally verifies:
- one Core owner and signature;
- both caller paths use the same owner;
- no duplicate Aggressive symmetric envelope;
- Automatic validation happens before reconciliation/live-state publication;
- runtime contract and CI wiring are present.

## Performance / cleanliness audit

The phase removes one repeated Aggressive distance calculation and avoids
recomputing a separate fill envelope from quote spread in the Automatic path.
No new unbounded cache, broker enumeration, decision authority or execution
authority was introduced.

## نیاز به تست دستی در cTrader

- actual cTrader favorable/adverse fill timing and gap behavior;
- broker rejection semantics for fills beyond the envelope;
- post-rejection close behavior;
- automatic-market and aggressive live lifecycle ordering;
- startup/reload/reconnect and panel/chart presentation around execution state.

## Verification

Final implementation HEAD: `f51c842c4778d99428ab583a2bae7cb7839e17e2`.
- Source/Architecture: **PASS** — run #2348.
- Runtime Acceptance Contracts: **PASS** — run #2157.
- cTrader Compile: **PASS** — run #2341.

The accumulated Source/Architecture chain also passed the reconciled historical F2 audit and the new `audit_phase_8_1.py` H1 gate.

## Sequence continuity

Prompt 7 G6B is closed on `main`. The repository contains a roadmap
reference to CR7.6c/G6C but no authoritative G6C scope or implementation
branch exists on the 2026-10-01 `main` HEAD. Prompt 8 provides the next fully
specified remediation sequence; H1 is implemented here without inventing a
G6C behavioral contract.

Merged on 2026-10-01 via PR #149, merge commit recorded in the continuation state/PR history.

Next specified phase: **CR8.2 / H2 — Top-Down alignment must include absolute strength.**
