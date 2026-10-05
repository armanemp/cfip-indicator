# CFIP Market Regime and Signal Synchronization

## Purpose

This design defines the next intelligence layer for CFIP Indicator. The objective is not to add more independent signals blindly. It is to classify the current market state first, then adapt confidence, plan creation and chart presentation to that state.

## Regime model

CFIP uses a deterministic, low-cost regime classifier:

- TREND: directional movement, sufficient ADX, low-to-moderate choppiness, meaningful EMA separation and path efficiency.
- EXPANSION: volatility is expanding while directional structure remains usable.
- RANGE: low directional strength, high choppiness and low path efficiency.
- COMPRESSION: volatility is contracting while price remains trapped/choppy.
- HIGH_VOLATILITY: volatility expands faster than directional structure can justify.
- TRANSITION: metrics disagree or are between the clearer states.

The classifier uses ATR ratio, ADX/DMI, EMA spread and slope, Choppiness Index and price-path efficiency. The design deliberately avoids online machine-learning inference in the realtime indicator at this stage.

## Signal hierarchy

The intended authority order is:

1. Broker-confirmed pending order / live position where applicable.
2. Active executable Plan.
3. Confirmed Decision.
4. Live Reaction.
5. Early Prediction / Watch.

Chart objects must never imply a stronger state than the authoritative state. A Watch/Reaction arrow is not a Plan and therefore does not require Entry/SL/TP plan lines.

## Execution semantics

`Entry` represents the executable entry price of the current Plan.

`IdealEntry` represents the center/optimal location of the selected execution zone.

`Trigger` is an activation threshold, not a second profit/entry level. It is rendered only when the execution mode actually waits for a trigger, such as continuation/stop-style execution. Retest-market and already-triggered market execution do not display a stale trigger line.

## Weak-signal suppression

RANGE and COMPRESSION are treated as non-actionable by the strict regime gate. TRANSITION and HIGH_VOLATILITY require stronger regime quality and directional edge before plan creation. This is a deterministic suppression layer, not a claim that those regimes can never move; it means CFIP does not promote their weak evidence into an actionable trade state.

## Research basis

ADX/DMI are documented as trend-strength / directional-movement measures. ATR measures volatility and is useful for adapting levels to changing market conditions. Choppiness Index is commonly used to distinguish choppy consolidation from directional movement. Regime-switching models such as hidden Markov models are useful research references for state changes, but they are deferred from the realtime indicator until empirical outcome data and resource constraints justify them.

References:
- https://www.fidelity.com/learning-center/trading-investing/technical-analysis/technical-indicator-guide/DMI
- https://www.fidelity.com/learning-center/trading-investing/technical-analysis/technical-indicator-guide/atr
- https://www.quantifiedstrategies.com/choppiness-index/
- https://arxiv.org/abs/2007.14874
- https://arxiv.org/abs/2107.05535

## Development rule

New indicators are added only when they contribute a distinct evidence family or materially improve a measured decision boundary. Redundant oscillators are not added merely to increase indicator count.


## Evidence playbooks

The regime classifier is also used to decide which evidence families deserve promotion:

- TREND: structure, trend alignment, higher-timeframe agreement and stable directional continuation are primary. Oscillator votes are secondary.
- EXPANSION: displacement is required, with volume/structure/liquidity providing supporting confirmation.
- HIGH_VOLATILITY: at least three distinct directional evidence families are required before promotion.
- RANGE/COMPRESSION: directional trend setups are suppressed.
- TRANSITION: weaker evidence is not promoted until the regime becomes stable enough.

Volume is treated as effort and price movement as result. High volume without sufficient directional price result is not counted as a bullish/bearish confirmation.

The execution state follows the analytical state. A pre-trade plan is not allowed to survive a hard thesis invalidation, while a waiting plan may remain during ordinary trigger/location waiting. A broker-confirmed pending order or live position remains authoritative over speculative chart state.
