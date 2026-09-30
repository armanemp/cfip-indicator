# CR3.3 — Partial TP, server ladder, BE and trailing

Date: 2026-09-30

Status: **MERGED TO MAIN — PR #99, merge commit `1e8681f57cb48bc51367b3df2ca129803d93d0c4`.**

Covers: Claude review findings C5 and C6.

## Partial TP retry and broker confirmation

Manual partial-close execution now carries the canonical closed M5 identity. A logical TP1/TP2 partial mutation is attempted at most once per stage per closed M5 bar, so a rejected request cannot spin duplicate broker close calls on every tick.

Post-partial break-even uses the same `SmartBreakEvenRule` as server-side ladder construction. The plan stop is updated only after broker mutation confirmation; a rejected break-even mutation leaves the last known protected plan stop unchanged and records an explicit diagnostic.

## Server-side TP ladder

Server-side TP1/TP2 stage recognition no longer relies on position volume alone. It reads broker `Position.Deals` and requires a matching closing deal for the managed position, correct opposing trade direction, expected execution price and expected partial volume.

The ladder is explicitly broker-owned for the lifetime of the plan. TP3 is not synthesized from a market-price crossing while that ownership is active, including after the ladder collapses to its final broker TP.

Ladder mutations use stage-specific closed-bar retry identity. Post-TP1 and post-TP2 target updates reject backward movement against the current broker target.

## Restart / recovery

Peak-RR context is reconstructed from broker `Position.EntryTime` mapped to the corresponding closed M5 bar, plus historical closed-bar highs/lows and the current executable market. This avoids treating the current quote as the historical peak after restart when sufficient history is available.

The existing broker stop/target remain authoritative during recovery; history is used only to reconstruct the analytical peak context and lifecycle start bar.

## Verification

Deterministic runtime contracts cover:

- bounded partial-close retry;
- broker closing-deal evidence and BUY/SELL symmetry;
- peak-price reconstruction across restart;
- spread-aware break-even applicability and explicit diagnostics;
- monotonic BUY/SELL target progression.

The phase also adds `tools/audit_phase_3_3.py` and wires it into the source/architecture workflow.

No profitability or signal-accuracy claim is made from source/CI evidence. Target-terminal broker timing and restart/reconnect behavior remain manual acceptance boundaries.
