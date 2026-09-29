# CFIP Runtime Log — Operator Guide

## Generated files
`History/CFIP_RuntimeLog_<symbol>_<chart-timeframe>_<configuration-fingerprint>_<90-day-start>_<90-day-end>.csv`

The runtime log is append-only within its rolling 90-day bucket. The configuration fingerprint keeps materially different indicator configurations separated.

## Events
- `DECISION`: canonical closed-bar decision trace.
- `PREDICTION`: early forecast output, confidence and target geometry.
- `SCENARIO`: independent timeframe opportunity with ScenarioId and SourceTimeframe.
- `EXECUTION`: execution/lifecycle telemetry and current broker-managed identifiers.
- `AUTO_STATE`: meaningful automatic-trading state transitions.

## Review workflow
1. Copy the generated CSV from the cTrader data location where the indicator's `History` folder is stored.
2. Send the CSV back with the exact file name unchanged.
3. The project routine first runs `tools/analyze_runtime_log.py` and then performs deeper correlation against signal traces and outcome history when those files are available.

## What the review can detect
- invalid BUY/SELL Entry-Stop-TP geometry;
- non-finite numeric fields;
- repeated or conflicting scenario identities;
- excessive blocks/rejections and their reasons;
- mismatches between decision, plan and execution state;
- prediction/decision divergence and later realized outcome when outcome data exists;
- stale or regressing level behavior when consecutive records are available.

## Important limitation
A runtime log can expose internal calculation and lifecycle failures, but it cannot by itself prove profitability or prediction accuracy. Empirical claims require enough historical outcomes or replay data and should be reported with the relevant sample size and measurement window.