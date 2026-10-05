# CFIP Runtime Log — Operator Guide

## Generated files
`History/CFIP_RuntimeLog_v2_<symbol>_<chart-timeframe>_<configuration-fingerprint>_<90-day-start>_<90-day-end>.csv`

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

## Phase 11 additions

Runtime-log schema is now v2. Additional fields include:
- ScenarioEvidence
- LocationQuality
- WaveTrendQuality
- PolicyAllowed
- PolicyReason
- ForecastHorizonBars

Additional event type:
- `NEWS_RISK`: scheduled/blocked economic-event context, event identity and protective action.

For incident analysis, correlate `NEWS_RISK` with the preceding `DECISION`, `SCENARIO` and `EXECUTION` records on the same symbol/configuration and UTC window.

## Exact cTrader storage location

The indicator is explicitly registered as `CFIPIndicator`. Therefore the project's designated filesystem root is the cTrader indicator folder for that name, with the archive directory at:

`Documents/cAlgo/Data/Indicators/CFIPIndicator/History/`

On first startup the indicator creates `History/CFIP_HISTORY_LOCATION.txt`. The marker is useful when the terminal uses a non-obvious Windows Documents location.

The panel also exposes `AUTO TRADE BLOCK` and `AUTO ORDER BLOCK` while the corresponding automatic mode is enabled and blocked. These reasons should be correlated with the runtime log before changing any execution threshold.
