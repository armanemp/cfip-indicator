# CR6.9 / F3 — Orphan Managed-Position Protection

## وضعیت

**تأیید می‌کنم** — ممیزی مسیر فعلی نشان داد نقص F3 در مالک protection مربوط به orphan position وجود دارد.

## Root cause

- در `src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs`، متد `ProtectOrphanManagedPosition` (حوالی خطوط 17–88) وقتی stop اصلی و fallback هر دو نامعتبر بودند، به‌اشتباه موفقیت را گزارش می‌کرد.
- caller در `ProtectBrokerExecution.ProtectBrokerPositions` (حوالی خطوط 17–83) نتیجهٔ failure را به `RecoveryRequired` تبدیل نمی‌کرد.

## اصلاح انجام‌شده

- مسیر invalid-stop اکنون fail-closed است و diagnostic صریح `ORPHAN-PROTECTION-FAILED` ثبت می‌کند.
- Core owner جدید `OrphanManagedProtectionRule` موفقیت را فقط برای direction معتبر، stop candidate معتبر و broker-confirmed protection مجاز می‌کند.
- caller در failure، `_brokerProtectionRecoveryRequired` را فعال و `LifecycleState.RecoveryRequired` را ثبت می‌کند.
- `_lastBrokerModifyUtc` در failure به‌روزرسانی نمی‌شود؛ بنابراین failure به‌عنوان موفقیت protection ثبت نشده و فرصت retry/reconciliation حفظ می‌شود.
- هیچ broker mutation owner جدیدی ایجاد نشد.

## Verification contracts

- BUY با stop نامعتبر → failure.
- SELL با stop نامعتبر → failure.
- BUY/SELL با broker rejection → failure.
- BUY/SELL با stop نامعتبر و broker failure → failure.
- BUY/SELL با stop معتبر و broker confirmation → success.
- direction نامعتبر → failure.

## Safety boundary

- هیچ `[Parameter]` جدیدی اضافه نشد.
- هیچ نام/type/`DefaultValue` عمومی تغییر نکرد.
- هیچ RR، confidence، SL/TP یا execution threshold تنظیم نشد.
- تغییر رفتاری صرفاً correction ایمنی F3 است: invalid protection دیگر success نیست.
- broker rejection timing، restart/reconnect ordering و رفتار واقعی target terminal در cTrader همچنان manual acceptance هستند.

## Audit

`tools/audit_phase_6_9.py` بلافاصله بعد از F9 در Source/Architecture workflow اجرا می‌شود و ownership، fail-closed return، RecoveryRequired transition، retry timestamp semantics و Runtime Contracts را بررسی می‌کند.

**Next phase: CR7.1 / G1 — Broker protection must never increase live position risk.**
