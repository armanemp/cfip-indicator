## 12. روتین اجباری — semantic/visual consistency across consumers

1. هر state یا direction باید یک owner معنایی داشته باشد و UI consumer آن را دوباره به vocabulary دیگری تبدیل نکند.
2. اگر بین bias، trade direction، trigger state، evidence state یا broker state تفاوت واقعی وجود دارد، این تفاوت باید در متن صریح باشد؛ متن و رنگ متناقض یا ambiguous مجاز نیست.
3. رنگ هر سطر/چراغ باید از همان stateای مشتق شود که متن آن را توصیف می‌کند؛ conflict باید صریح نمایش داده شود، نه با متن directional و رنگ neutral پنهان بماند.
4. مقادیر عددی داخلی مانند direction = 1/-1 نباید مستقیم در UI کاربر نمایش داده شوند وقتی owner متنی canonical وجود دارد.
5. این ممیزی باید در هر فاز UI، signal، alert، execution و realtime دوباره اجرا و در گزارش فاز ثبت شود.

# CFIP Indicator — ROUTINE

این فایل حافظه عملیاتی ثابت پروژه است و در شروع هر نوبت باید با وضعیت واقعی ریپو تطبیق داده شود.

## 11. روتین اجباری هر فاز — تمرکز عمیق تحلیل، سیگنال و اجرای خودکار
هر فاز، حتی اگر موضوع اصلی آن UI، startup، performance یا history باشد، باید در همان فاز یک بررسی کامل از زنجیره زیر انجام دهد:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle -> Outcome -> Learning

در هر فاز حتماً:
1. تحلیل MTF و top-down، structure/liquidity، OB/FVG به‌خصوص OB+FVG، WaveTrend، divergence، indicator fusion، regime و evidence attribution دوباره بررسی شوند؛ تغییر threshold بدون measurement/replay مجاز نیست.
2. false-positive، false-negative، missed-actionable، stale decision، trigger/actionability mismatch و duplicate/conflicting signals بررسی شوند.
3. Entry/Ideal Entry/Trigger، SL، TP1..TP4، reward path، RR و BUY/SELL symmetry دوباره بررسی شوند و هیچ target/stop regressions پذیرفته نشود.
4. Auto Trading و Auto Orders هر دو مسیر market/aggressive/pending را از تصمیم تازه، suitability، permission، capacity، spread/risk، volume، geometry، submission gate و broker confirmation تا lifecycle بررسی کنند.
5. قبل از broker mutation، current quote و actionability دوباره refresh شوند؛ safety gate برای رفع missed signal حذف یا دور زده نشود.
6. هر compile warning جدید مانند error تلقی شود؛ warning مربوط به API obsolete، member unused، dead path یا compatibility residue باید در همان فاز رفع یا علت آن مستند و gate شود.
7. panel/visual فقط presentation authority است؛ چراغ heartbeat، pipeline diagnostics و text/line spacing نباید هیچ business rule یا execution authority جدید ایجاد کنند.
8. history/runtime logs، rejection reason، recovery، outcome و calibration برای شواهد بعدی ثبت و با auditهای موجود correlate شوند.

این بخش دائمی است و در هر فاز باید در گزارش پایانی صریحاً اعلام شود که تحلیل، سیگنال، auto-trade، auto-order، SL/TP، history و verification بررسی شده‌اند.

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
9. objectهای منقضی و stale باید از چارت پاک شوند.

## 4. روتین چندسناریویی
1. هر scenario باید ScenarioId یکتا، SourceTimeframe مشخص و direction مشخص داشته باشد.
2. سناریوهای TFهای مختلف می‌توانند همزمان حاضر باشند، حتی با جهت‌های متفاوت.
3. ENTRY/SL/TP هر سناریو باید prefix مشترک داشته باشند تا مالکیت خط‌ها روشن باشد.
4. scenario مستقل فقط opportunity است؛ auto-trading فقط با policy و گیت‌های canonical مجاز است.
5. cap کردن سناریوها deterministic باشد و ابتدا coverage سناریوهای distinct را حفظ کند، سپس بر اساس quality/actionability تکمیل شود.
6. سناریوهای متفاوت نباید فقط به‌دلیل نزدیک‌بودن Entry با هم collapse شوند؛ فقط identity دقیق یکسان مجاز به collapse است.

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
6. file append و LocalStorage.Flush/Reload نباید در Calculate انجام شود؛ archiveها باید از buffer/Timer heartbeat مصرف شوند.
7. broker-state snapshot باید event/mutation-aware و با TTL محدود باشد؛ repeated reconciliation بدون dirty/recovery reason نباید دوباره broker read ایجاد کند.
8. cacheهای تحلیلی FVG/OB فقط روی context بسته و با key deterministic مجازند؛ انتخاب نهایی live می‌تواند quote-sensitive بماند.

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

## 10. روتین بررسی اندیکاتورهای مرجع
1. ZIPهای مرجع قبلی را دوباره بررسی کن و قبل از کپی‌کردن هر کد، parity و همپوشانی با موتورهای داخلی CFIP را مشخص کن.
2. WaveTrend و FVG مرجع فقط وقتی وارد runtime شوند که parity ریاضی، closed-bar semantics و double-counting audit آن‌ها روشن باشد.
3. Economic News feed باید فقط از مسیر cache/Timer به decision و execution برسد؛ هیچ تصمیمی نباید HTTP را شروع یا منتظر نتیجه‌ی شبکه بماند. Refresh باید async، single-flight و generation-aware باشد و feed stateهای NEVER_LOADED/HEALTHY/STALE/BLOCKING_EVENT/DISABLED قابل تشخیص باشند.
4. قبل از هر automatic market/pending entry، news guard نهایی دوباره ارزیابی شود.
5. در صورت خبر مهم نزدیک، pending قابل لغو باشد و در صورت فعال‌بودن policy، active position با telemetry ثبت و مدیریت شود.
## 12. روتین اختصاصی Phase 11.4 — Plan Quality و Multi-Scenario Execution

در هر فاز بعدی که روی کیفیت یا execution کار می‌کند، این موارد باید صریحاً دوباره بررسی شوند:

1. برای هر candidate، Entry/SL/TP1 و ریسک ATR محاسبه و nominal RR و effective RR بعد از spread هر دو بررسی شوند.
2. stop candidate فقط بر اساس نزدیک‌بودن به قیمت انتخاب نشود؛ reward-path feasibility و stop-width نیز در انتخاب structural stop بررسی شوند.
3. Tactical، Counter-HTF و Micro نباید از کف RR canonical plan پایین‌تر بروند.
4. scenario identity باید از source timeframe + lane + direction مستقل باشد و نزدیکی قیمت باعث حذف scenarioهای متفاوت نشود.
5. پیش از broker mutation در market، aggressive و pending دوباره reward-risk geometry و spread بررسی شود.
6. auto-trading و auto-order اگر scenarioهای موازی را مصرف می‌کنند باید execution authority، capacity، duplicate prevention، broker confirmation و lifecycle را به‌صورت deterministic حفظ کنند.
7. تغییر threshold به‌عنوان tuning محسوب می‌شود و بدون runtime log/replay/outcome evidence مجاز نیست؛ اصلاح contract mismatch از tuning جدا گزارش شود.

8. signal trace باید Risk ATR، Effective TP1 RR و Required TP1 RR را برای مسیرهای قابل محاسبه ثبت کند؛ schema قدیمی باید همچنان قابل خواندن باشد.
9. بهینه‌سازی performance فقط از طریق cacheهای deterministic با key کامل و بدون اشتراک mutable business state مجاز است.

## 13. گزارش اجباری کیفیت سیگنال

در گزارش هر فاز باید مشخص شود:
- چند مسیر candidate به‌علت RR یا stop-width رد شدند؛
- چند scenario مستقل حفظ شدند و چند مورد duplicate دقیقاً collapse شدند؛
- market/aggressive/pending هر کدام چه reward-risk gate نهایی دارند؛
- آیا mismatch بین signal، plan و execution authority باقی مانده است؛
- کدام موارد هنوز فقط با target-terminal replay قابل اثبات هستند.

    
## 14. روتین اختصاصی Phase 11.5 — Scenario-Aware Execution Materialization

در هر فاز بعدی که سناریوهای موازی یا auto-execution را توسعه می‌دهد:

1. candidate با Decision، Lane، Direction، TriggerReady و ActionableNow تطبیق داده شود.
2. execution authorization از candidate eligibility جدا باقی بماند؛ سناریوهای observe-only نباید مستقیم broker mutation را مجاز کنند.
3. سناریوی canonical برای Plan فقط با identity و geometry دقیق Entry/SL/TP1 materialize شود؛ fallback identity هرگز جای safety gate را نمی‌گیرد.
4. SubmissionAttemptIdentity باید ScenarioId داشته باشد تا backoff/circuit یک سناریو، سناریوی مستقل دیگر را سرکوب نکند.
5. market/aggressive/pending قبل از broker mutation باید scenario identity، reward/risk، quote، capacity، permission، news/suitability، submission و broker confirmation را حفظ کنند.
6. چندسناریویی بودن display/registry به معنی چندپوزیشن بودن execution نیست؛ تغییر capacity فقط در یک فاز certification مستقل مجاز است.
7. telemetry باید scenario identity و reason را برای rejection/failure/recovery نگه دارد تا cohort analysis ممکن باشد.
8. هر ادعای کاهش false signal، افزایش accuracy، بهبود realized RR یا افزایش profit capture باید با replay/outcome واقعی پشتیبانی شود.


## 15. روتین اختصاصی CR1.8 — Managed Identity و Reversal Safety

1. در تمام broker mutationها، label باید از یک identity canonical و InstanceId همان instance ساخته شود؛ استفاده مستقیم از base label برای Market/Aggressive ممنوع است.
2. `IsManagedPosition` و `IsManagedPendingOrder` باید exact instance identity را مصرف کنند؛ `ManagedActionsOnly=false` نباید دامنه مالکیت را به manual/foreign positionها باز کند.
3. Recovery، EOD، News، Reversal و protection باید فقط از همان managed identity استفاده کنند و cross-instance adoption نداشته باشند.
4. Reversal Close باید حداقل سود را از یک rule/parameter واحد بگیرد؛ هیچ `NetProfit > 0` hard-code جدیدی مجاز نیست.
5. هشدارهای safety که در scanهای پرتکرار اجرا می‌شوند باید bounded باشند و نباید log I/O تکراری وارد hot path کنند.


## 16. روتین اختصاصی CR2.6 — Order Block Quality و Cache

1. OB quality باید فقط از یک owner ریاضی محاسبه شود و base/clamp و تمام مؤلفه‌ها قابل ردیابی باشند.
2. displacement، structure break، liquidity sweep، FVG و شواهد HTF نباید به‌صورت یک feature تکراری امتیازدهی شوند.
3. bullish OB فقط در سمت حمایتی بازار و bearish OB فقط در سمت مقاومتی بازار انتخاب شود؛ این کنترل باید پس از cache و با quote فعلی انجام شود.
4. lifecycle zone باید Fresh/Mitigated/Broken را از هم جدا کند؛ zone شکسته هرگز به execution/plan برنگردد.
5. cache فقط برای context بسته و key deterministic مجاز است؛ quote/selection price نباید cache identity را تغییر دهد.
6. بهینه‌سازی cache نباید mixed-bar یا stale-zone state ایجاد کند و empirical quality claim فقط با replay/outcome پذیرفته است.


## 17. روتین اختصاصی CR2.7 — WaveTrend Mathematical Correctness

1. تمام MovingAverageTypeهای عمومی باید mapping و implementation مشخص و deterministic داشته باشند؛ هیچ fallback مبهمی جای semantics اعلام‌شده را نگیرد.
2. warm-up باید dependencyهای RSI/MFI/RMI، smoothing، signal و previous-signal stability را پوشش دهد؛ داده‌های ناپایدار باید fail-closed بمانند.
3. DEMA/TEMA باید زنجیره EMA خود را از اولین index معتبر seed کنند؛ صفرهای پیش‌فرض array هرگز نباید وارد محاسبات شوند.
4. HMA باید raw window متوالی را با WMA نهایی درست مصرف کند؛ هر تغییر فرمول باید contract عددی داشته باشد.
5. HistoryLoaded/Reloaded و تغییر prefix/count تاریخچه باید state/cache مشتق WaveTrend را invalidate کنند؛ history extension نباید مقدار stale برگرداند.
6. MFI این موتور با TickVolume محاسبه می‌شود و نباید به‌عنوان real exchange volume گزارش یا تفسیر شود.
7. تغییرات WaveTrend نباید authority تصمیم، execution، broker mutation یا ظرفیت معامله را ایجاد یا تغییر دهد؛ claims مربوط به accuracy/parity/profit فقط با replay و target-terminal evidence مجازند.


## 18. روتین اختصاصی CR2.8 — Historical Rendering Semantics and Cost

1. اسکن historical فقط روی کندل‌های بسته و با سقف ثابت انجام شود؛ `MaximumScanBars` باید owner مشخص داشته باشد و با HistoricalSignalLimit اشتباه نشود.
2. نتیجه‌ی historical presentation هر closed-bar باید با کلید timestamp/identity پایدار cache شود؛ cache نباید mixed-bar یا quote-sensitive business state را نگه دارد.
3. chart object identity برای historical rendering باید timestamp-based باشد تا prepend/load-more باعث collision یا جابه‌جایی مالکیت نشود.
4. Bars.HistoryLoaded/Reloaded و تغییر first-open-time یا کاهش count باید cache و object state را invalidate کنند.
5. renderer فقط presentation authority است؛ historical arrow هرگز نباید alert، Decision/Plan، broker mutation، lifecycle یا outcome ایجاد کند.
6. روی هر historical refresh فقط objectهای جدید/تغییرکرده ساخته شوند و stale objectهای خارج از ownership حذف شوند؛ حذف و بازسازی کامل مجموعه در هر render ممنوع است مگر history invalidation.
7. هیچ accuracy، win-rate، RR improvement یا profitability claim از historical arrows بدون replay/outcome واقعی پذیرفته نیست.
8. هر تغییر بعدی به historical rendering باید اثر آن بر panel latency، Calculate hot path، object count، history prepend و cleanup بررسی شود.


## 19. روتین اختصاصی CR2.9 — Structural Stop / Divergence / Rejection

1. Structural stop باید فقط از timeframeهای canonical استفاده کند؛ timeframe ناشناخته باید fail-closed باشد و هرگز با W1 جایگزین نشود.
2. امتیاز stop باید reward-path information را نگه دارد، اما reward bonus و preferred-risk contribution باید owner خالص و قابل‌آزمون داشته باشند.
3. هیچ تغییر عددی در stop score، divergence threshold یا rejection threshold بدون fixture/replay/outcome evidence مجاز نیست؛ contract centralization با tuning اشتباه نشود.
4. Divergence thresholdهای quality، conflict، price excursion، RSI، WaveTrend و recency باید یک owner داشته باشند و BUY/SELL symmetry حفظ شود.
5. conflict divergence باید Direction=0 و non-actionable بماند؛ conflict نباید به strength directional تبدیل شود.
6. Doji باید با همان meaningful-body rule سنجیده شود که rejection directional را gate می‌کند؛ wick-only candle نباید rejection evidence بسازد.
7. deterministic contracts باید boundaryهای threshold، invalid numeric input و symmetry را پوشش دهند.
8. در هر تغییر بعدی، اثر stop selection روی risk ATR، RR، reward path و selection bias و نیز اثر divergence/rejection روی false-positive/false-negative فقط با replay/outcome سنجیده شود.

## 20. روتین اختصاصی CR3.1 — Live Invalidation / False-Signal

1. invalidation و false-signal risk باید روی یک canonical closed M5 result ارزیابی شوند؛ همان closed bar نباید چندبار trigger business-state mutation کند.
2. Structural invalidation باید از confirmed swing candidate استفاده کند؛ rolling min/max window جایگزین structural confirmation نیست.
3. broker-close mutation باید نتیجه‌ی explicit داشته باشد؛ rejection باید به RecoveryRequired برسد و هیچ successful-exit bookkeeping را جلو نبرد.
4. _lastExitM5 فقط پس از موفقیت mutation قابل advancement است؛ actual closure همچنان broker-confirmed via OnPositionClosed باقی می‌ماند.
5. FalseSignalAdverseR باید یک semantic owner داشته باشد؛ inputهای NaN/Infinity/out-of-range fail-closed شوند.
6. وقتی broker protective SL در adverse direction شناخته شده است، software adverse-R threshold نباید پشت envelope ریسک محافظت‌شده قرار بگیرد.
7. BUY/SELL threshold geometry باید symmetric باشد و runtime fixture هر دو جهت را پوشش دهد.
8. افزودن safety parameter باید در تمام parameter inventories/audits/README reconcile شود؛ تغییر عددی threshold بدون replay/outcome evidence مجاز نیست.
9. routine audit باید کل زنجیره Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning و همچنین startup/hot-path performance/code cleanliness را دوباره بررسی کند.
10. target-terminal verification برای broker-event timing، rejection/recovery، restart/reconnect و quote/bar synchronization همچنان جدا از CI الزامی است.
