# Static Verification

The verifier is intentionally independent from cTrader and the proprietary/local API DLL.

Checks currently enforced:
- repository source tree is present;
- the migrated host retains exactly 512 public cTrader parameters;
- all host partials exist;
- a manual BUY/SELL entry surface is not introduced;
- the verifier itself runs in CI.

This is a structural gate only. It does not replace real cTrader compilation or broker/runtime acceptance.
