# CFIP — Active Inspection List

## Status

**ACTIVE / MINIMAL**

این فایل فقط اجزای فعال و مسیرهای اصلی مورد بررسی را نگه می‌دارد.

## Active control files

- `docs/CFIP-ROADMAP.md`
- `docs/CFIP-TRADE.md`
- `docs/CFIP_GATE.md`
- `docs/CFIP-LIST.md`
- `docs/CFIP-PROMPT.md`
- `docs/CFIP-PREPROMPT.md`

## Active reference files

- `docs/ARCHITECTURE.md`
- `docs/ENGINEERING-PRINCIPLES.md`
- `docs/WORKFLOW.md`

## Trade-chain source domains

1. Market data / time / price
2. MTF / closed-bar context
3. Analysis / indicators / OSS
4. Structure / liquidity / FVG / OB / regime
5. Evidence / confluence
6. Decision / score / confidence
7. Actionability / trigger / opportunity
8. Plan / Entry / SL / TP / RR
9. Risk / sizing / margin / spread / capacity
10. Signal / Scenario / Identity / Contract
11. Indicator → Contracts → cBot handoff
12. cBot preflight / execution
13. Broker confirmation
14. Protection / management
15. Lifecycle / recovery / restart / reconnect
16. Outcome / history / calibration
17. Performance / resilience
18. End-to-end certification

## Review rule

برای هر work package:
**owner → callers → consumers → state → time/price → side effects → failure paths → verification**

هیچ فایل یا مسیر جدیدی بدون دلیل معماری وارد active scope نمی‌شود.
