# CFIP Smart Intelligence

## Scope

This layer improves analytical quality without changing broker execution authority, risk ownership, or the cTrader host contract.

Adaptive Learning remains deferred. The current improvements are deterministic and data-independent.

## Decision fusion

### Quality-weighted timeframe influence

Each timeframe contribution is converted from raw bull/bear score totals into a directional share and then scaled by that frame's existing quality score and configured timeframe weight.

This prevents a timeframe with a large raw score but weak analytical quality from dominating the global consensus.

The transformation is symmetric:

- BUY share and SELL share are derived from the same frame.
- The same quality and weight factor applies to both directions.
- Mirroring bull/bear inputs mirrors the resulting contributions.

### Correlation-aware independent evidence

Independent evidence no longer counts every boolean feature as a separate independent observation.

Evidence is grouped into four families with a maximum contribution of two points per family:

1. Structure: structure, MSS/CHOCH, displacement.
2. Location: liquidity, FVG, order block.
3. Trend/momentum: trend, momentum, MACD, VWAP.
4. Context/participation: volume, volatility, rejection, equal levels.

This preserves broad coverage while applying diminishing returns to strongly related signals.

The resulting independent-evidence range is 0–8, and smart-quality normalization maps that range back to a 0–100 scale.

## Smart quality

Smart Quality now combines:

- consensus strength: 25%;
- timeframe agreement: 20%;
- correlation-aware independent evidence: 20%;
- structural confirmations: 15%;
- regime quality: 10%;
- retest quality: 10%.

A missing retest-quality value is treated as neutral quality rather than an automatic zero.

## Early prediction integrity

Early prediction now has a strict direction boundary:

- exact BUY/SELL ties produce a neutral direction;
- predictions below the configured confidence threshold do not retain a BUY/SELL direction;
- low-confidence predictions remain observations rather than directional signals.

This keeps early prediction separate from confirmed decision authority.

## Accuracy boundary

These changes improve the structure of evidence fusion and reduce known sources of score inflation and false directional state.

They do not, by themselves, prove a higher trading win rate or expectancy. Empirical accuracy must be measured later through deterministic replay and outcome data in the planned outcome/calibration phases.
