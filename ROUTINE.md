# CFIP Indicator — ROUTINE

این فایل حافظه عملیاتی ثابت پروژه است و در شروع هر نوبت باید با وضعیت واقعی ریپو تطبیق داده شود.

## 1. شروع هر نوبت
1. وضعیت main، آخرین commit، شاخه فعال و PRهای باز را بررسی کن.
2. DEVELOPMENT-LOG.md، ROADMAP.md و این فایل را با وضعیت واقعی کد تطبیق بده.
3. مشخص کن کدام فاز بسته، کدام فاز فعال و کدام verification هنوز باز است.
4. در گزارش همان نوبت، پیشرفت مهندسی و کار باقی‌مانده را صریح اعلام کن؛ درصد فقط شاخص checklist مهندسی است و به معنی دقت یا سودآوری نیست.

## 2. روتین تحلیل و سیگنال
1. قرارداد closed-bar و هم‌ترازی MTF را حفظ کن؛ داده mixed-bar وارد تصمیم نشود.
2. زنجیره analysis → consensus → trigger → plan → execution یک مرجع واحد داشته باشد.
3. TFهای مختلف را هم برای consensus و هم برای فرصت مستقل بررسی کن. فرصت مستقل به تنهایی مجوز معامله واقعی نیست.
4. کیفیت OB، FVG و به‌خصوص OB+FVG با ساختار، sweep، displacement، WaveTrend، divergence و شاخص‌های تکمیلی بررسی شود و double-counting کنترل شود.
5. prediction باید با direction، confidence و horizon ثبت و بعد با outcome/replay ارزیابی شود. بدون داده واقعی ادعای accuracy پذیرفته نیست.

## 3. روتین Entry / SL / TP
1. Entry، Ideal Entry و Trigger از یک قرارداد واحد تولید شوند.
2. SL باید از نظر جهت و فاصله معتبر باشد.
3. TPها فقط در جهت معامله و به‌صورت monotonic جلو بروند؛ TP قبلی هرگز به عقب برنگردد.
4. broker minimum distance، RR و finite-number validation قبل از هر update برقرار باشد.
5. بعد از fill وضعیت broker دوباره خوانده و SL/TP reconcile شود.
6. objectهای منقضی و stale باید از چارت پاک شوند.

## 4. روتین چندسناریویی
1. هر scenario باید ScenarioId یکتا، SourceTimeframe مشخص و direction مشخص داشته باشد.
2. سناریوهای TFهای مختلف می‌توانند همزمان حاضر باشند، حتی با جهت‌های متفاوت.
3. ENTRY/SL/TP هر سناریو باید prefix مشترک داشته باشند تا مالکیت خط‌ها روشن باشد.
4. scenario مستقل فقط opportunity است؛ auto-trading فقط با policy و گیت‌های canonical مجاز است.
5. cap کردن سناریوها deterministic باشد و سناریوهای مهم یا با کیفیت بالاتر بدون دلیل حذف نشوند.

## 5. روتین Auto Trading / Auto Orders
1. قبل از order: permission، single-capacity، daily loss، session/spread/volatility/news guards، decision gates، plan integrity، level quality و broker geometry بررسی شوند.
2. One Order Per Signal و duplicate prevention حفظ شود.
3. market و pending بعد از fill/reject/cancel دوباره وضعیت واقعی broker را بخوانند.
4. ambiguity یا recovery ⇒ fail-closed.
5. هیچ scenario مستقل TF نباید مستقیم auto-order شود مگر policy صریح، test و audit برای آن اضافه شده باشد.

## 6. روتین Logging
1. CFIP_RuntimeLog_*.csv مرجع یکپارچه برای decision، scenario و execution است.
2. CFIP_SignalTrace_*.csv جزئیات تصمیم بسته‌شده را نگه می‌دارد.
3. CFIP_History_*.csv outcomeهای واقعی را نگه می‌دارد.
4. در هر log review، reject/fail/recovery، non-finite، stale TP/SL، duplicate، mismatch بین decision/plan/broker و ناسازگاری جهت بررسی شود.
5. log ارسالی کاربر با ابزار analyze_runtime_log.py بررسی و بعد root-cause → fix → regression test → verification ثبت شود.

## 7. روتین Prediction
1. prediction خروجی احتمالاتی/امتیازی است، نه تضمین.
2. confidence به تنهایی کافی نیست؛ regime، structure، distance-to-entry، MTF state و target geometry باید همراه آن ثبت شوند.
3. بعد از جمع شدن outcome کافی، calibration و false-positive/false-negative analysis انجام شود.
4. هر تغییر prediction باید با replay یا outcome data بررسی شود.

## 8. روتین Verification
بعد از هر فاز مهم:
- source / architecture checks
- runtime acceptance contracts
- parameter usage audit
- MTF closed-bar audit
- lifecycle / market / pending execution audit
- exit geometry و monotonic TP audit
- UI object ownership / cleanup audit
- logging schema smoke test
- در صورت امکان historical replay یا log واقعی

## 9. گزارش پایان هر نوبت
در پایان هر چت همیشه این‌ها گزارش شوند:
- فاز فعلی
- کارهای انجام‌شده
- کارهای باقی‌مانده
- verification انجام‌شده
- مواردی که هنوز نیاز به اجرای محلی، replay یا terminal واقعی دارند
- برآورد پیشرفت مهندسی و سهم باقی‌مانده
- commit/PR و وضعیت merge

این فایل جایگزین تست واقعی نیست؛ فقط مانع فراموش شدن روتین پروژه می‌شود.