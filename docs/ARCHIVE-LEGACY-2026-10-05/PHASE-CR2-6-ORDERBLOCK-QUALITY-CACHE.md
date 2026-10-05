# CR2.6 — OrderBlock Quality and Cache Discipline

Status: VERIFIED COMPLETE — PR #91 merged 2026-09-30, merge commit 8f89d75e9fed70a53b51a902b52c539d9c514f0b.

## Scope

Covers Claude review item B7:
- exact Order Block quality base/clamp semantics;
- directional side-of-market validity;
- explicit fresh/mitigated/broken lifecycle;
- closed-bar/frame/direction candidate caching;
- independent evidence attribution without accidental double-counting.

## Implementation decisions

1. OrderBlockQualityRule is now the single mathematical quality owner. The base score remains 54 and all existing component weights/clamps remain explicit and testable.
2. The impulse ratio normalization is unit-consistent: strongest directional body is divided by max(PipSize, creation ATR), rather than multiplying the pip size by ATR.
3. Bullish OB zones must be on/below the executable market side; bearish OB zones must be on/above it, with one tick of tolerance. This validation is evaluated after candidate caching so live quote selection remains dynamic.
4. Active OBs are classified as Fresh or Mitigated; a full fill or retained-width <= 5% is Broken and cannot be returned as an active candidate.
5. The existing hot cache remains intentionally bounded to the current closed Bars reference + closed index. Direction and historical-retest mode form the per-context key. Selection price is never part of the cache identity.
6. Displacement, structure break, liquidity sweep and FVG confluence remain separate boolean quality components. No HTF/structure component is merged into another evidence bucket.
7. No public parameter, trade capacity, execution authority or public threshold was added or changed.

## Verification contract

Deterministic contracts cover:
- base score and upper clamp;
- independent evidence contributions;
- body/impulse/age/mitigation scoring;
- non-finite fail-closed behavior;
- fresh/mitigated/broken lifecycle;
- side-of-market validity;
- bullish/bearish mitigation symmetry.

Verification completed on head `1c5dbf17140f3757ea4e3289183fb73ab8e65b9b`: Source/Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS.

Source/Architecture also requires the CR2.6 static audit and cache ownership checks.

## Routine audit

The phase rechecked the project-wide chain:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning.

The primary CR2.6 risk is analytical candidate quality and repeat evaluation cost; no execution mutation path or safety gate was bypassed. OB/FVG confluence remains attributable, and live selection remains quote-sensitive only after cached closed-bar candidate construction.

## Remaining manual boundary

Target-terminal replay remains required to measure real cTrader quote timing, zone interaction and empirical signal-quality effects. CI does not establish win rate, false-positive rate, or profitability.
