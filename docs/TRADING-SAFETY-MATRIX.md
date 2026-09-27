# CFIP Indicator — Trading Safety Matrix

## Execution safety

| Scenario | Required behavior |
|---|---|
| Duplicate Calculate/tick | No duplicate order or position |
| Duplicate broker event | Idempotent reconciliation |
| Market order rejected | No fill/state assumption; surface rejection |
| Pending order rejected | No pending state assumption |
| Slippage | Broker-confirmed ActualFill is authoritative |
| Missing SL/TP after fill | Explicit recovery and gateway-owned restoration |
| Broker disconnect | Stop unsafe mutations; retain ownership state |
| Broker reconnect | Reconcile broker reality before new actions |
| Partial close accepted | Remain pending until broker volume confirms |
| Close accepted | Remain ExitRequested until broker confirmation |
| Wrong symbol/label/identity | Gateway rejects mutation |
| Daily loss limit reached | Block new execution and govern managed pending lifecycle |
| Structural invalidation | Request broker exit; never mark closed before confirmation |
| Reversal | Explicit precedence and lifecycle transition |
| Exhaustion | Explicit action, evidence and provenance |
| EOD | Explicit configured policy |
| Multi-position pending fill | Every resulting position gets lifecycle ownership |

## Protection

- Structural SL is the strategy source.
- Broker-held SL is actual broker state.
- Missing protection is recoverable state, not implicit safety.
- BUY SL uses current bid-side validation anchor.
- SELL SL uses current ask-side validation anchor.
- BUY TP uses current ask-side validation anchor.
- SELL TP uses current bid-side validation anchor.
- SL movement is protective only.
- Target progression is monotonic when policy requires it.

## Authority

Strategy: Decision -> Entry -> TradePlan

Broker reality: BrokerState -> Lifecycle

Execution: TradePlan -> ExecutionPolicy -> ExecutionIntent -> BrokerGateway

Presentation: authoritative state -> PresentationState

No reverse authority is permitted.
