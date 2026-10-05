# CBOT-P4B — Aggressive Authority + Panel Geometry Integrity — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — repository verification pending on branch.**

## هدف

رفع دو regression گزارش‌شده در پنل و تکمیل batch بعدی جداسازی cBot برای Aggressive بدون tuning استراتژی.

## Root Cause

1. ارتفاع نهایی روی Border اعمال می‌شد ولی `_panelStack` در ارتفاع bootstrap باقی می‌ماند؛ در نتیجه بخش دوم rows داخل layout بریده می‌شد.
2. محاسبه ارتفاع پنل مستقیماً از `Chart.Height` در همان چرخه‌ی measure کنترل overlay استفاده می‌کرد؛ یک مقدار transient/collapsed می‌توانست دوباره به geometry پنل برگردد و فضای plotting chart را جمع کند.

## اصلاحات

- ذخیره‌ی viewport baseline قبل از `Chart.AddControl`؛
- استفاده از baseline وقتی `Chart.Height` موقتاً collapse/invalid است؛
- bounded maximum height با ScrollViewer برای محتوای بلند؛
- همگام‌سازی `_panelStack.Height` با ارتفاع نهایی Border؛
- صریح کردن alignment محتوا؛
- حذف broker mutation از subtree اجرای Aggressive Indicator؛
- پذیرش `ExecutionAction.Aggressive` در تنها demo mutation owner cBot؛
- افزودن `Enable Demo Aggressive Execution` با default=false؛
- حفظ hard live-account guard و idempotency/capacity safeguards؛
- گزارش Aggressive با `BrokerAction.SubmitAggressive`.

## Invariants

- Indicator = analysis / decision / scenario / plan / presentation؛
- Contracts = immutable platform-neutral boundary؛
- cBot = broker mutation authority؛
- هیچ live Aggressive execution در این فاز فعال نمی‌شود؛
- هیچ confidence/RR/Entry/SL/TP/threshold عددی تغییر نمی‌کند؛
- Market و Market Range behavior موجود حفظ می‌شود.

## Verification

Automated acceptance باید شامل Source/Architecture، Runtime Acceptance Contracts، cTrader Compile/Build و `tools/audit_phase_cbot_p4b.py` باشد.

## Target-terminal manual boundary

پس از merge باید در cTrader واقعی بررسی شود: کامل بودن تمام ردیف‌های پنل، عدم collapse فضای chart در attach/reload، responsiveness در hide/show و MTF changes، و demo-only Aggressive execution با live-account fail-closed.

## Next

**CBOT-P4C — Pending Stop authority extraction**.