# CFIP Indicator — Editing Guide

The repository is organized so a feature is changed at its authoritative owner, not in whichever file happens to call it.

| Change | Primary owner |
|---|---|
| Direction semantics | Core/Direction + DirectionRules |
| MTF/reference-bar logic | Market/MtfSnapshotBuilder |
| EMA/RSI/ADX/ATR/regime | Market/MarketModelBuilder |
| BOS/MSS/CHOCH/displacement | Analysis/StructureLedgerBuilder |
| FVG / Order Block | Analysis/StructureLedgerBuilder + ZoneRecord |
| Liquidity / sweeps / pivots | Analysis/StructureLedgerBuilder + LiquidityRecord |
| Confidence / evidence / confluence | DecisionEngine + DecisionSnapshot |
| IdealEntry / EntryZone / Trigger | EntryTriggerEngine + EntrySnapshot |
| Structural invalidation | Entry/TradePlan |
| Structural SL | TradePlanBuilder |
| TP1-TP4 target ladder | TradePlanBuilder + TargetLadder |
| Risk sizing | RiskRequest + ExecutionEnvelope + host sizing path |
| Market/stop/limit execution choice | ExecutionPolicy + ExecutionPlanner |
| Broker mutation | CTraderBrokerGateway only |
| Broker-state read/reconciliation | CTraderBrokerStateReader |
| Pending order lifecycle | PendingOrderLifecycleManager |
| Position lifecycle | PositionLifecycleManager |
| BE/risk-free | LivePositionManager |
| Dynamic structural SL | LivePositionManager |
| Partial TP | LivePositionManager + PositionLifecycleManager |
| Reversal/exhaustion/invalidation/EOD | LivePositionManager |
| Outcome/telemetry | OutcomeEvent + OutcomeRecorder contract |
| Chart/panel/alerts | Presentation modules |
| Public parameters | CFIPIndicator.Declarations + ConfigSnapshot |

## Rules for edits

1. Change the authoritative owner first.
2. Do not duplicate the rule in a caller.
3. Add or update the corresponding acceptance test.
4. Review BUY and SELL behavior together.
5. Preserve provenance and block reasons.
6. Treat broker state as authoritative after a mutation.
7. Do not add manual trade-entry controls.
8. Do not convert a source/static test into a runtime claim.

## Safe refactoring order

Domain contract → implementation → caller migration → parity tests → old-path removal → runtime validation → documentation.
