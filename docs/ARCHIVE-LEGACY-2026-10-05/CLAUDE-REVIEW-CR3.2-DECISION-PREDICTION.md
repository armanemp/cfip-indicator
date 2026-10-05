# CR3.2 — Decision gate and early prediction semantics

Date: 2026-09-30

Status: implementation complete; verification pending CI.

Covers: Claude review findings C3 and C4.

## C3 — Reward-quality gate semantics

The previous `ProxyExpectedValue` helper was a deterministic heuristic:

```
quality-as-fraction × RR − (1 − quality-as-fraction)
```

The source did not have a calibrated probability input, so the metric was not a true expected-value calculation.

The implementation now owns this calculation in:

`src/CFIP.Indicator/Core/Math/RewardQualityFloorRule.cs`

The executable `Trading/Intelligence/ProxyExpectedValueCalculator.cs` owner was removed.

The deterministic numerical behavior is intentionally preserved. Only the semantic owner and wording changed:

- the smart decision gate now calls `RewardQualityFloorRule.Calculate`;
- the gate reason is `REWARD QUALITY FLOOR`;
- the public properties `UseProxyExpectedValueGate` and `MinimumProxyExpectedValue` remain unchanged for preset/backward compatibility;
- their cTrader labels now describe the actual reward-quality meaning.

No default threshold was changed.

## C4 — Early prediction semantics

Early prediction scoring is now centralized in:

`src/CFIP.Indicator/Core/Math/EarlyPredictionScoreRule.cs`

Named scoring components:

- M5 weight: 0.55
- M15 weight: 0.45
- liquidity forecast bonus: 8
- volume bonus: 2
- VWAP bonus: 1

The prediction retains `Confidence` as a compatibility alias for directional share, while also exposing:

- `DirectionalShare`;
- `AbsoluteStrength`;
- `BuyStrength`;
- `SellStrength`;
- `TotalStrength`.

A prediction direction now requires both:

1. the existing directional-share floor; and
2. an absolute-strength floor based on the existing `MinimumEarlyConfidence` parameter.

This prevents a large ratio between two tiny scores from being presented as a strong early prediction.

Panel/readiness/early-alert wording now presents share and strength explicitly.

## Verification

Deterministic runtime contracts were added for:

- reward-quality numerical preservation and monotonicity;
- non-finite reward-risk handling;
- low-total/high-ratio early prediction rejection;
- BUY/SELL symmetry;
- centralized named early-prediction weights and bonuses.

The phase also adds `tools/audit_phase_3_2.py` and wires it into the source/architecture workflow.

Final acceptance requires Source/Architecture, Runtime Acceptance, cTrader Compile, the CR3.2 static gate, and the accumulated routine/optimization/integrity audits.

No profitability, accuracy, or RR improvement claim is made from source-only changes.
