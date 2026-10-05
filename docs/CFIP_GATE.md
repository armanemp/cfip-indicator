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

**No phase is active until the user specifies the starting work package.**
