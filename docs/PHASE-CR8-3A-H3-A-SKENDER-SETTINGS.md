# CR8.3a / H3-A — Skender Settings Ownership

Date: 2026-10-01

## Status

VERIFIED COMPLETE — implementation branch `phase/cr8-3a-h3-a-skender-settings`.

The phase centralizes the existing production Skender fixed settings under one
immutable Core owner. The existing public parameter contract and numerical
defaults are preserved.

## Scope completed

- Added `Core/Math/OssIndicatorSettings.cs` as the single immutable
  `OssIndicatorSettings.Default` owner for fixed Skender settings.
- Moved the fixed MACD signal, Bollinger, MFI, Stochastic, SuperTrend, Aroon,
  CCI and Parabolic SAR settings out of `OssIndicatorParameters`.
- Migrated the affected Skender adapters to the central owner.
- Preserved RSI period and MACD fast/slow periods as parameter-driven inputs,
  including the existing safety accessors.
- Kept `OssIndicatorParameters` focused on cache/history and parameter-safety
  requirements such as the bounded rolling window and minimum-history rules.
- Added deterministic Planning Contract coverage for every preserved fixed
  setting.
- Updated Runtime Contract coverage to validate the new owner while retaining
  cache/history ownership in `OssIndicatorParameters`.
- Added `tools/audit_phase_8_3a.py` and accumulated it after the H2 audit in
  Source/Architecture CI.
- Reconciled the accumulated CR4.4 OSS-settings audit with the new canonical
  owner.

## Preserved values

| Setting | Value |
| --- | ---: |
| MACD signal period | 9 |
| Bollinger period | 20 |
| Bollinger standard deviations | 2.0 |
| MFI period | 14 |
| Stochastic lookback | 14 |
| Stochastic signal | 3 |
| Stochastic smoothing | 3 |
| SuperTrend period | 10 |
| SuperTrend multiplier | 3.0 |
| Aroon period | 25 |
| CCI period | 20 |
| Parabolic SAR acceleration | 0.02 |
| Parabolic SAR maximum acceleration | 0.20 |

## Verification

Final H3-A implementation head before documentation closeout:
`b19e3366b0115d79a6e6a61b79af310ac64bbdd7`

Repository gates on that implementation head:

- Source / Architecture: PASS
- Runtime Acceptance Contracts: PASS
- cTrader Compile / Build: PASS

The phase branch contained 17 implementation/test/audit commits before this
documentation closeout.

## Routine and optimization audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation →
Protection/Lifecycle → Outcome → Learning was reviewed for ownership leakage.

Performance/code cleanliness:
- no new trading loop or broker enumeration;
- no unbounded cache;
- no new runtime I/O;
- fixed-setting ownership is centralized without moving cache/history policy;
- H3-B warm-up-window redesign, bounded computation, cache redesign and
  numerical-parity optimization are explicitly out of scope.

## Safety boundary

- No public parameter name, type or DefaultValue changed.
- No RR, confidence, entry, SL, TP, risk or execution threshold was tuned.
- No decision or broker-execution authority changed.
- No second trading engine was introduced.
- No profitability, accuracy or terminal-latency claim is made from repository
  verification alone.

## Next phase

**CR8.3b / H3-B — Skender warm-up, bounded computation, cache design and
numerical-parity optimization.**

Target-terminal timing/parity and empirical signal-quality validation remain
manual acceptance boundaries.
