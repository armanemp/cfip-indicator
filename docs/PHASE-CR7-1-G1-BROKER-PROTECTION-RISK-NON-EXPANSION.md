# CR7.1 / G1 — Broker Protection Must Never Increase Live Position Risk

## وضعیت

**تأیید می‌کنم** — ممیزی و اصلاح G1 در مسیر canonical broker protection انجام شد. هدف phase این بود که یک SL موجود و صحیح هرگز فقط به‌دلیل نزدیک بودن به قیمت فعلی، به سطحی دورتر و پرریسک‌تر منتقل نشود.

## Root causes

1. TargetObstacleCacheKey متد Equals را override کرده بود ولی GetHashCode نداشت؛ این همان علت هشدار CS0659 بود.
2. سلامت SL موجود با پذیرش SL جدید از نظر minimum-distance بازار یکی گرفته شده بود.
3. BrokerProtectionCoordinator، BoundPlanProtection و BrokerProtectionStateEvaluator این ابهام را در مسیرهای مختلف مصرف می‌کردند.
4. audit_project_integrity.py duplicate-method detection را بدون درنظرگرفتن containing type انجام می‌داد و دو GetHashCode مستقل را اشتباهاً duplicate گزارش کرد.

## اصلاحات

- TargetObstacleCacheKey.GetHashCode() اضافه شد و تمام فیلدهای مورد استفاده در equality را پوشش می‌دهد.
- در Core، ManagedStopProtectionRule.IsExistingStopHealthy به‌عنوان owner سلامت یک SL موجود اضافه شد:
  - BUY: stop < entry
  - SELL: stop > entry
  - direction/price نامعتبر: fail-closed
- ManagedStopProtectionRule.Validate برای SL جدید بدون تغییر semantics قبلی باقی ماند و همچنان market/minimum-distance را کنترل می‌کند.
- PriceProtectionValidation helper صریح برای existing-stop health دریافت کرد.
- BrokerProtectionCoordinator سلامت SL موجود را از acceptability SL پیشنهادی جدا کرد.
- BoundPlanProtection همین تفکیک را اعمال کرد.
- BrokerProtectionStateEvaluator یک SL موجود و درست‌جهت را صرفاً به‌خاطر نزدیکی به market، missing/invalid گزارش نمی‌کند.
- ProtectionProgressionRule.ShouldAdvanceStop همچنان تنها guard جایگزینی یک SL موجود است؛ در BUY فقط حرکت رو به بالا و در SELL فقط حرکت رو به پایین مجاز است.
- قراردادهای deterministic برای BUY/SELL و hash equality اضافه شد.
- tools/audit_phase_7_1.py اضافه و در زنجیره Source/Architecture بعد از F3 ثبت شد.
- tools/audit_project_integrity.py type-aware شد تا duplicate واقعی را از متدهای هم‌نام در typeهای مستقل تفکیک کند.

## Safety boundary

- هیچ public [Parameter] name/type/DefaultValue تغییر نکرد.
- هیچ RR/confidence/SL/TP/execution threshold tuning انجام نشد.
- هیچ execution authority یا broker mutation owner جدیدی ایجاد نشد.
- wrong-sided و invalid protection همچنان fail-closed هستند.
- یک SL موجود که از نظر جهت نسبت به entry محافظتی است، بدون عبور از progression rule با stop دورتر جایگزین نمی‌شود.

## Verification

Final code HEAD:
2f1cb933a2c2407e1fe33302cf72f538f090ba91

- Source/Architecture: PASS — run 36868297463 / workflow #2262
- Runtime Acceptance Contracts: PASS — run 36868297578 / workflow #2071
- cTrader Compile: PASS — run 36868297556 / workflow #2255

## Local build evidence

User-reported local Release build completed successfully with:
- 0 errors
- 1 warning: CS0659 on TargetObstacleCacheKey
- warning is the issue corrected by this phase

A fresh local build on the user Windows workspace is still the final local confirmation that the warning count is now zero.

## Project-wide routine audit

The accumulated Source/Architecture chain passed on the final code HEAD. No new network/persistence path or allocation-heavy hot path was introduced by G1. The platform-neutral stop-health rule remains isolated in Core/.

## Manual cTrader boundary

Still requires target-terminal validation:
- actual ModifyStopLossPrice broker acceptance/rejection;
- broker-specific minimum-distance behavior;
- restart/reconnect ordering;
- live cTrader panel/runtime behavior;
- empirical signal-quality/profitability.

## Next phase

CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.

## Operator action after merge

Run: git pull --ff-only
