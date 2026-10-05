# CR7.2 / G2 — Retest Adverse-Momentum Semantics and Rejection Telemetry

Date: 2026-10-01

Status: **VERIFIED COMPLETE — PR #142 merged to `main` as `f983d2fd7eb0baced4b5ff40988e6294b3f5bd28`.**

## Phase acknowledgement

تأیید می‌کنم — the current Retest/trap-risk path was audited before implementation, including EntryTrapRiskRule, TradeActionabilityEvaluator, Decision.ActionabilityReason, DecisionReasonBuilder and the decision panel.

## Scope and finding

The existing trap logic already uses the established adverse-momentum boundaries. The remediation requirement is therefore implemented as semantic hardening, not threshold tuning:

- canonical Core ownership for 0.30 / 0.45 / 0.40 ATR trap boundaries;
- deterministic rejection reason codes: TRAP_ADVERSE_M5, TRAP_ADVERSE_M1, TRAP_EXTREME, TRAP_DIVERGENCE;
- recent adverse movement is classified as pre-zone only when the bounded adverse window has no contact with the Retest execution zone;
- zone-contact cases remain explicitly classified as post-zone/reaction context;
- the existing Retest trap block remains intact; no exemption, threshold change or new entry path is introduced;
- Decision.ActionabilityReason remains the existing single reason channel consumed by both composed decision reason and the panel.

## Compatibility

The existing six-argument EntryTrapRiskRule.Evaluate overload remains available for accumulated F6 contracts. Its legacy human-readable reason text is preserved; the canonical G2 path emits structured reason codes.

## Safety and ownership

- No public parameter name/type/DefaultValue changed.
- No RR/confidence/SL/TP or execution threshold was retuned.
- No second decision or execution authority was introduced.
- Diagnostic context does not alter the Block decision.

## Verification boundary

Repository verification on final G2 head `94e8154e3ec5107bb984be228aed874ba2e1e27c`:
- Source/Architecture: **PASS** — #2283;
- Runtime Acceptance Contracts: **PASS** — #2092;
- cTrader Compile: **PASS** — #2276. Target-terminal intrabar timing, actual zone interaction and empirical Retest signal-quality effects remain manual acceptance items.

## Next phase

After G2 closeout: **CR7.3 / G3 — Display parameter truth for plan-line thickness/style.**
