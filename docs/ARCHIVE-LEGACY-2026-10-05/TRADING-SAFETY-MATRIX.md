# Trading Safety Matrix

- All automatic execution uses managed symbol/label/identity checks.
- A rejected broker action never becomes a synthetic fill.
- Accepted pending orders are not treated as positions until broker confirmation.
- Accepted closes and partial closes remain pending until broker state confirms them.
- Missing protection enters explicit recovery handling.
- Invalid or missing broker SL/TP protection is treated as recovery state; desired plan values are never used as synthetic broker state.
- Live SL-level exits require a directionally valid, broker-confirmed SL; missing broker SL does not trigger a synthetic level exit.
- Broker protection reconciliation skips materially unchanged mutations and retries only unresolved or invalid protection.
- Risk, margin, spread, session, daily-loss and event guards run before execution.
- Live SL/TP validation uses the current market-side anchor.
- SL updates are protective only.
- Duplicate calculations/events are idempotent.
- Reconnect/restart reconciliation runs before new lifecycle assumptions.
- Manual trade-entry controls are absent.
