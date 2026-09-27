# Capability Traceability

## Analysis

| Capability | Owner |
|---|---|
| Runtime/account/broker snapshot | CFIPClean89RuntimeSnapshot / CFIPClean89RuntimeAuthority |
| Closed-bar MTF | CFIPClean89MtfSnapshotBuilder / CFIPClean89MtfSnapshot |
| EMA/RSI/ADX/ATR/regime | CFIPClean89MarketModelBuilder / CFIPClean89MarketModel |
| BOS/MSS/CHOCH/displacement | CFIPClean89StructureLedgerBuilder / CFIPClean89StructureSnapshot |
| FVG | CFIPClean89StructureLedgerBuilder / CFIPClean89ZoneRecord |
| Order Block | CFIPClean89StructureLedgerBuilder / CFIPClean89ZoneRecord |
| Liquidity and sweeps | CFIPClean89StructureLedgerBuilder / CFIPClean89LiquidityRecord |
| Premium/discount | CFIPClean89PremiumDiscountState |
| Confidence/evidence/confluence | CFIPClean89DecisionEngine / CFIPClean89DecisionSnapshot |

## Trading

| Capability | Owner |
|---|---|
| IdealEntry / EntryZone / Trigger / Invalidation | CFIPClean89EntryTriggerEngine / CFIPClean89EntrySnapshot |
| Trade identity and plan | CFIPClean89TradeIdentity / CFIPClean89TradePlan |
| Structural SL | CFIPClean89TradePlanBuilder |
| TP1-TP4 ladder | CFIPClean89TradePlanBuilder / CFIPClean89TargetLadder |
| Risk sizing and broker constraints | CFIPClean89RiskRequest / CFIPClean89ExecutionEnvelope |
| Unified market/stop/limit execution | CFIPClean89ExecutionPolicy / CFIPClean89ExecutionPlanner |
| Pending order lifecycle | CFIPClean89PendingOrderLifecycleManager |
| Position lifecycle/reconciliation | CFIPClean89PositionLifecycleManager |
| BE / risk-free / structural repricing / dynamic targets | CFIPClean89LivePositionManager |
| Partial TP confirmation/retry | CFIPClean89LivePositionManager / CFIPClean89PositionLifecycleManager |
| Reversal/exhaustion/invalidation/EOD | CFIPClean89LivePositionManager |
| Broker protection | CFIPClean89CTraderBrokerGateway |
| Broker reconciliation | CFIPClean89CTraderBrokerStateReader |
| Outcomes / telemetry boundary | CFIPClean89OutcomeEvent / ICFIPClean89OutcomeRecorder |
| Presentation projection | CFIPClean89PresentationState / ICFIPClean89PresentationProjector |

## Product constraints

- No manual BUY/SELL/STOP/LIMIT entry controls.
- Analysis has no broker side effects.
- UI never becomes trading authority.
- Broker acceptance is not broker confirmation.
- Partial TP consumes state only after broker confirmation.
- SL moves only protectively.
- BUY/SELL paths must remain symmetric.
- Closed-bar data is the decision reference.
