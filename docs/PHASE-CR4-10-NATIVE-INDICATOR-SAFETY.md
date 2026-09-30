# CR4.10 / D10 — Native-indicator defensive safety and registry performance

Status: **VERIFIED COMPLETE — repository acceptance closed 2026-10-01.**

## Scope

Harden native indicator readiness at the analysis boundary and reduce native registry lookup overhead without changing public parameters, default trading thresholds, RR, confidence, stop, target or execution policy.

## Finding reconciliation

The D10 review was reconciled against the current main source.

Inventory result:
- native indicator storage remains a platform-owned `Native` data holder;
- direct native/readiness consumers are confined to the native wrappers/registry plus MarketFrame evidence/scoring, MACD bias, reaction and market-regime analysis;
- ATR and EMA already fail closed for zero/non-finite values;
- ADX/DMI already reject non-finite/zero-denominator input;
- RSI historically returned neutral `50` when unavailable, which is compatibility-preserved but is now prevented from entering frame scoring until the complete native set is ready;
- MACD prior-sample reads are now explicitly warm-up bounded.

## Implementation

### 1. Central readiness owner

Added Core `NativeIndicatorReadinessRule` with:
- indexed warm-up readiness;
- prior-window readiness for stateful comparisons;
- finite-positive checks;
- bounded oscillator checks;
- complete frame readiness across ATR/RSI/ADX/fast EMA/slow EMA.

This keeps numeric/readiness semantics platform-neutral and testable.

### 2. Consumer hardening

The native wrappers now use the centralized readiness boundary.

`MarketFrameEvidence` now marks `NativeIndicatorsReady` only when all core native inputs are present, warmed, finite and mathematically usable. An unready frame returns before additional evidence can be scored.

`MarketFrameScoringService` independently fails closed when the frame is not native-ready.

`MarketRegimeAnalyzer` rejects unusable ATR/baseline/EMA values before normalization/division.

`MacdBiasAnalyzer` requires both current and prior MACD samples to be inside their warm-up windows.

The existing RSI `50` fallback remains for compatibility; it cannot create directional evidence by itself, and incomplete native frames cannot reach the scoring boundary.

### 3. Registry performance and ownership

Replaced the linear `List<Native>` lookup with a `Dictionary<Bars, Native>` using a reference-identity comparer based on `ReferenceEquals` and `RuntimeHelpers.GetHashCode`.

This preserves the existing ownership identity: the exact `Bars` object remains the cache key. No symbol/timeframe inference or secondary ownership rule was introduced.

`Native.cs` remains a storage-only holder; no execution or decision authority was added.

### 4. Deterministic contracts

Added runtime contracts covering:
- warm-up boundary;
- out-of-range/negative index rejection;
- prior-sample window readiness;
- finite-positive and bounded oscillator safety;
- full-frame readiness;
- unusable EMA/volatility rejection;
- proof that neutral RSI `50` alone produces no directional fusion bonus.

Added `tools/audit_phase_4_10.py`, including a repository-wide consumer inventory and accumulated phase-order/documentation checks.

### 5. Performance evidence

Added a platform-neutral benchmark comparing:
- the pre-D10 linear reference scan;
- the D10 reference-identity dictionary lookup.

The benchmark uses deterministic synthetic reference keys and one million lookups per measurement. It is evidence for lookup complexity/performance, not a claim about broker/terminal timing.

## Safety boundary

- No public parameter name/type/`DefaultValue` changed.
- No default RR, confidence, stop, target or execution threshold was tuned.
- No new decision authority or execution authority was introduced.
- No trading policy was changed.
- No target-terminal/broker-runtime result is inferred from source or synthetic benchmark evidence.

## Verification boundary

Repository verification completed on merge commit `3371b9790902c4d35e4e1e28522dee42f93af861`:
- Source/Architecture — PASS, workflow run `36790307897`;
- Runtime Acceptance Contracts — PASS, workflow run `36790307911`;
- cTrader Compile — PASS, workflow run `36790307893`;
- OSS/Registry Benchmark — PASS, workflow run `36790307937`.

Manual acceptance remains required for:
- target-terminal startup/readiness timing;
- cTrader chart/panel responsiveness;
- restart/reconnect behavior;
- broker lifecycle ordering;
- empirical signal-quality/profitability.

## Performance and cleanliness audit

- registry lookup no longer scans all native sets on every access;
- readiness checks are constant-time and do not introduce per-bar I/O;
- no duplicate decision owner was added;
- existing accumulated project-wide routine and optimization audits remain mandatory.

## Transition

CR4.10 / D10 is complete after repository verification passes.

Next phase: **CR-FINAL repository integration gate**, followed by the already-defined target-terminal CBOT-Preflight acceptance boundary before Track 12A can advance.
