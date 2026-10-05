# CFIP — Gate

## Status

**ACTIVE / MINIMAL**

این فایل فقط معیار پذیرش CFIP را نگه می‌دارد.

## Required gates

هر work package فقط وقتی PASS است که موارد لازم زیر تأیید شده باشند:

1. Root cause مشخص و evidence-based باشد.
2. Canonical owner مشخص باشد.
3. Caller/consumerهای مؤثر بررسی شده باشند.
4. Logic موازی یا duplicate باقی نمانده باشد.
5. State, identity, time و price semantics معتبر باشند.
6. BUY/SELL symmetry بررسی شده باشد.
7. Error, stale, retry و boundary states بررسی شده باشند.
8. Focused tests/audits لازم PASS باشند.
9. Release build PASS باشد.
10. Terminal/Broker evidence هرجا لازم است موجود باشد.
11. تغییر با architecture و contract سازگار باشد.
12. نتیجه در همین gate ثبت شود.

## STOP

در این موارد PASS ممنوع است:

- broker mutation خارج از cBot
- second decision/execution owner
- duplicate execution identity
- stale execution
- M2
- look-ahead
- plan/request به‌جای broker truth
- protection widening
- workaround یا parallel path
- evidence ناقص

## Current

**Trade-chain T1 — IMPLEMENTED / GATE-READY (MARKET DATA TRUTH)**

**Next: T2 — Time / MTF / Closed-Bar Integrity**

### T1 acceptance notes

- Canonical quote/data semantics are owned by CanonicalPriceSnapshot / CalculationMarketContext.
- Live indicator consumers refresh that canonical quote boundary before reaction/actionability/provider use.
- Provider execution intent fails closed when canonical quote metadata/time/quote state is unusable.
- cBot remains the final live broker quote authority.
- T1 does not claim target-terminal execution evidence or current-head CI until those exact artifacts are observed.

### T0 open findings

- Candidate branch is not yet merged to main.
- Multi-opportunity capacity is not complete; current capacity is single-plan.
- cBot capacity checks require owner consolidation.
- Execution-intent construction has two related construction boundaries requiring T9/T10 ownership verification.
- Lifecycle semantics cross Indicator/cBot and require formal ownership closure.
- Current-head CI and target-terminal broker evidence are not claimed as T0 proof.

### Global intelligence gate

Any future smart behavior must reuse the canonical result of the upstream owner, define its causal inputs, remain monotonic with safety, avoid double-counting, and not introduce a parallel state/strategy/execution path. More intelligence is not permission to add contradictory filters or arbitrary hard-coded floors.
