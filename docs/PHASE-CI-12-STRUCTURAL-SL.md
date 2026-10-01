# CI-12 — Structural SL Audit

Date: 2026-10-02

## Status

**IMPLEMENTATION IN PROGRESS on phase/ci-12-structural-sl-audit.**

This phase is the blocking next step after CI-11. Prompt 8 / CR8.4 remains
paused until CI-FINAL, as required by the full-stack calculation-integrity
sequence.

## Audit boundary

The structural-stop chain is audited from source level through the plan
boundary:

- M5 swing;
- M5 FVG;
- M5 OB;
- HTF structure;
- HTF FVG/OB;
- timeframe-specific stop buffer;
- minimum risk;
- maximum risk;
- spread relation;
- broker distance;
- fallback;
- final normalization;
- BUY/SELL symmetry.

The required priority is:

Structural validity -> protection validity -> risk feasibility -> reward optimization

Reward feasibility must not silently redefine the structural meaning of the
stop.

## Findings

1. The structural-stop candidate evaluator and finalizer independently
   reconstructed the same source-price plus/minus ATR-times-buffer geometry.
   That created two mathematical owners and allowed future drift between the
   evaluated stop and the returned stop.

2. Stop-buffer semantics were duplicated between candidate evaluation and final
   materialization. The M5/HTF distinction and the minimum buffer floor therefore
   existed in more than one place.

3. ATR fallback stop arithmetic was repeated across plan preview, parallel
   scenario preparation and lifecycle recovery/protection paths.

4. A prior audit hypothesis that spread could raise maxRiskAtr was re-checked
   against the actual source order and was disproven for the current code:
   the effective maximum is calculated from the configured minimum before the
   spread-derived minimum is applied. No behavior change was made for that
   hypothesis.

## Implementation

- Added Core StructuralStopGeometrySnapshot as the immutable value returned by
  canonical structural-stop geometry evaluation.
- Added Core StructuralStopGeometryRule as the single owner for:
  - M5 versus HTF buffer resolution;
  - source-level to protective-stop conversion;
  - tick-size/digit normalization;
  - structural BUY/SELL side validation;
  - structural risk geometry;
  - ATR fallback geometry.
- Structural-stop candidate selection now keeps the exact stop produced during
  candidate evaluation instead of recalculating it later.
- Removed the obsolete StructuralStopFinalizer.cs duplicate authority.
- Added StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(...) so the
  configured maximum remains independent from the spread-derived minimum.
- Plan, preview and parallel planning paths consume the canonical fallback and
  the same bounded risk envelope.
- Pending-fill, startup-recovery and orphan-protection fallbacks use the same
  fallback geometry and fail closed when fallback geometry/risk is invalid.
- Early prediction fallback construction also consumes the same geometry owner.
- Added CI-12 static architecture audit and accumulated it after CI-11.

## Deterministic contracts

The Runtime Acceptance suite covers:

- M5 buffer versus HTF buffer;
- BUY/SELL mirrored structural-stop geometry;
- tick-size normalization symmetry;
- invalid-side candidate rejection;
- fallback geometry symmetry;
- configured maximum stop-risk enforcement;
- explicit proof that spread pressure cannot increase the configured maximum
  stop-risk ceiling.

## Safety and compatibility

- No public parameter name, type or DefaultValue was changed.
- No RR, confidence, entry, SL, TP or execution threshold was tuned.
- Existing MinimumSlAtr, MaximumSlAtr, MaximumStructuralStopAtr,
  StopBufferAtr, HtfStopBufferAtr and FallbackSlAtr values retain their
  existing meanings.
- Broker-distance validation remains under the existing
  PriceProtectionRule / IsValidStop boundary.
- Existing broker-confirmed live protection remains authoritative; this phase
  does not turn a planning risk ceiling into a live-position exit gate.
- No second decision, plan or broker-mutation authority was introduced.

## Verification boundary

Repository gates required for completion:

- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile / Build.

Manual cTrader acceptance remains required for:

- broker-specific stop-distance behavior;
- startup/reconnect recovery;
- actual order/fill protection;
- terminal chart/panel presentation.

CI-12 is not complete until all three repository gates are green and the final
continuity record is updated with the exact implementation head and merge SHA.

## Next phase

After CI-12 is fully verified and merged, continue with CI-13 — TP source,
target obstacle and TP ladder audit.

Prompt 8 / CR8.4 remains paused until CI-FINAL.
