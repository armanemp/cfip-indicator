# Trading Safety Matrix

- Duplicate ticks/events must not create duplicate broker actions.
- Rejections must not create synthetic fills.
- Accepted close requests remain pending until broker confirmation.
- Accepted partial closes remain pending until broker volume confirmation.
- Missing protection is explicit recovery state.
- Broker mutation passes through one gateway.
- Symbol, label and strategy identity are checked at mutation boundaries.
- Reconnect requires reconciliation before new actions.
- Structural invalidation requests lifecycle exit; it does not declare closure.
- Risk, daily-loss and exposure gates apply before execution.
- Live SL/TP validation uses current market-side anchors.
