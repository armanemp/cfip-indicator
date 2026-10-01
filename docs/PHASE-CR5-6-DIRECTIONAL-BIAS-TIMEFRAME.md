# CR5.6 / E6 — Directional-bias semantics and timeframe consistency

Status: **VERIFIED COMPLETE — PR #122**

Implementation head before documentation closeout: e930e30d9c82ad279316a41c81116d4ae1e19859.

Verification on that implementation head:
- Source/Architecture: PASS — workflow run 36838144439;
- Runtime Acceptance Contracts: PASS — workflow run 36838144403;
- cTrader Compile: PASS — workflow run 36838144416.

## Scope

E6 reconciles the semantic boundary between:
- Premium/Discount context;
- LiveBias / advanced-confluence bias;
- Healthy Volatility evidence;
- canonical closed-M5 decision authority.

No public parameter names, types, defaults, trading thresholds, RR values or execution authority are changed.

## Findings

### Premium / Discount

The existing rule is a midpoint contextual signal:
- close below the selected structure midpoint => BUY context (+1);
- close above the midpoint => SELL context (-1);
- equality/invalid geometry => neutral.

This is a mean-reversion/context contribution and is not itself a regime override. Its established decision contribution remains unchanged at +6 to the applicable direction when enabled.

### LiveBias

The previous implementation read the host chart Bars through a mapped chart index. That made its source depend on the chart timeframe.

E6 changes the source to:
- _m5Bars;
- the canonical closedM5;
- an explicit reference closed-bar check.

The mathematical bias remains unchanged: BUY requires close > fast EMA > slow EMA; SELL requires close < fast EMA < slow EMA.

### Healthy Volatility

The previous implementation mixed ATR health with:
- candle direction;
- MinimumTriggerBodyAtr.

That made a trigger-entry parameter silently redefine volatility health.

E6 separates the owners:
- HealthyVolatilityRule evaluates only the ATR/current-to-baseline ratio envelope;
- MarketFrameEvidence derives VolatilityBull/VolatilityBear from the resulting health flag plus candle direction.

The existing Healthy ATR Minimum Ratio and Healthy ATR Maximum Ratio defaults remain 0.85 and 1.80. Minimum Trigger Body ATR remains 0.12 and continues to belong to trigger/entry semantics.

## Deterministic verification

Added contracts cover:
- Premium/Discount midpoint semantics and invalid-input neutrality;
- BUY/SELL symmetry for closed-M5 live bias;
- invalid-direction/non-finite bias safety;
- ATR-ratio health boundaries;
- M5/M15/H1 fully-closed bar boundaries;
- unfinished M5 rejection.

## Routine architecture / optimization audit

The project-wide ownership chain remains:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning

No second decision or execution authority is introduced.

The E6 refactor removes chart-timeframe coupling from LiveBias and removes trigger-body recalculation from volatility health. It does not add history scans or timer work.

## Verification boundary

Repository verification is provided by:
- Source/Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile.

Target-terminal validation remains required for:
- live M5/M15/H1 timing;
- panel/visual behavior;
- empirical signal quality and profitability.

## Transition

After E6 repository verification, the next phase is CR5.7 / E7 — Decision-owned WATCH/REACTION alerts separated from chart rendering.
