# CR6.1 / F1 — Opposing FVG/OB target-path direction, mitigation and obstacle caching

Date: 2026-10-01

Status: VERIFIED COMPLETE — PR #127 merged to `main` via merge commit `a7a03a4403a7c6681f95ac0b053344144e933b6e`.

## Required phase confirmation

تأیید می‌کنم — `Trading/Validation/RewardPathZoneObstacleScanner.cs`,
`Core/Math/RewardPathGeometryRule.cs`,
`Planning/TradePlan/TargetCandidateEvaluator.cs` and
`Trading/Validation/HigherTfRewardPathValidator.cs` were checked before the
correction.

## Root causes verified

1. The opposing FVG target-path scan used the trade direction directly while
   the OB scan used the opposite direction. This made the two obstacle paths
   semantically inconsistent.
2. The FVG path scan used raw three-bar geometry instead of the canonical
   managed FVG lifecycle/mitigation owner, so a mitigated FVG could remain a
   path obstacle.
3. Reward-path geometry lived outside Core and was therefore not available as a
   small deterministic platform-neutral contract.
4. Repeated obstacle scans rebuilt the same closed Bars/index/direction zone
   candidates for different target checks.

## Implementation

- Added `Core/Math/RewardPathGeometryRule.cs` as the platform-neutral geometry
  and opposing-direction owner.
- Removed the obsolete Trading-layer reward-path geometry owner.
- `RewardPathZoneObstacleScanner` now derives one
  `opposingDirection = -direction` and uses it consistently for FVG and OB.
- FVG candidates use canonical `FvgRule` geometry and
  `BuildManagedFvgZone`, so mitigation/lifecycle semantics are applied before
  a candidate enters the obstacle cache.
- OB candidates continue through the existing
  `BuildOrderBlockCandidate`/mitigation path and broken OBs are excluded.
- The obstacle candidate list is cached per `Bars` instance + closed index +
  direction. Entry/target-specific reward-path geometry is still evaluated
  after cache retrieval, so changing a target does not reuse a stale boolean.
- Higher-timeframe scans continue through the same owner for M15/M30/H1/H4.
- Added deterministic Runtime Contract coverage for BUY/SELL symmetry,
  canonical FVG orientation, full mitigation exclusion, opposing direction,
  and target-specific path geometry.
- Added `tools/audit_phase_6_1.py` and wired it into the accumulated
  Source/Architecture workflow immediately after E8.

## Behavior changes

**Confirmed behavior correction 1:** A same-direction FVG is no longer eligible
as an opposing target-path obstacle; only a zone whose direction is opposite
to the trade can be considered.

**Confirmed behavior correction 2:** A fully mitigated FVG no longer remains an
obstacle candidate.

**Confirmed semantic alignment:** FVG minimum-gap qualification for this
obstacle path now follows its creation-bar ATR through the canonical FVG
builder rather than the previous raw scan's current-ATR comparison. Existing
parameter values remain unchanged.

**Performance behavior:** repeated target checks reuse the same closed-zone
candidate snapshot for a Bars/index/direction context. This is an optimization,
not a trading-threshold change.

No public [Parameter] name, type or DefaultValue changed. No RR, confidence,
SL/TP or execution threshold was tuned. Decision/execution authority was not
duplicated or moved.

## Deterministic verification

- Source/Architecture: PASS — workflow run #2177; accumulated audits through
  `audit_phase_6_1.py` passed.
- Runtime Acceptance Contracts: PASS — workflow run #1986.
- cTrader Compile: PASS — workflow run #2170.
- `audit_phase_6_1.py`: PASS within Source/Architecture.
- `verify_architecture.py`: PASS with the new Core owner registered in the
  architecture gate.
- Public parameter audits: PASS, 568 parameters and zero unread candidates.

## Project-wide audit / optimization boundary

The accumulated Source/Architecture workflow remained green through the
pre-existing CR4/CR5 audit sequence. The F1-specific change also preserves the
existing bounded-cache pattern: cache invalidation occurs on Bars/index context
change, while target-specific geometry is intentionally not cached.

No unrelated oversized-module or ownership refactor was introduced.

## نیاز به تست دستی در cTrader

- Replay/live chart validation that opposing FVG/OB obstacles are recognized
  on M15/M30/H1/H4 at the expected closed-bar boundary.
- Confirm that fully mitigated zones stop blocking a target path in the actual
  terminal.
- Observe repeated target evaluation under the same closed bar to verify the
  expected warm-cache/runtime behavior.
- Verify final chart/panel rendering remains unchanged by the planning-only
  correction.

## خارج از scope / باگ‌های کشف‌شده

No new out-of-scope production bug was fixed by this phase.

The existing Prompt 6 items F2–F9 and Prompt 7 G1–G6 remain separate roadmap
items and are not treated as implicitly completed by F1.
