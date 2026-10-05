# CFIP-CALUDE

> **Historical review & audit register — source: Claude pre-edit review package**
> **Important:** this document preserves the review findings as historical evidence. It does **not** assert that every item remains present in the current `main`; each item must be revalidated against the current architecture/code before remediation.

## 1. Document purpose
این سند یک مرجع متمرکز برای ثبت و بازبینی تمام یافته‌های بستهٔ بازبینی Claude است که پیش از مجموعه ویرایش‌های اخیر پروژه تهیه شده بود. هدف، از دست نرفتن هیچ finding و امکان ردیابی مستقل هر مورد در مراحل بعدی است.

## 2. Source and completeness control
- Source archive: `cfip-review(1).zip`
- Master source: `MASTER-ISSUES.csv`
- Findings in source: **260**
- Unique finding IDs: **260**
- Source SHA-256: `5a698ee649fb3aabd9e6af2c6a84c8ab5e7ad303d82e82cd3277eb8745ce0b44`
- Cross-check: IDs from `REVIEW-PHASE-01.md` through `REVIEW-PHASE-13.md` were reconciled against the CSV; the same 260 unique IDs are covered.
- Method limitation from source package: static review; no compile/run/live validation was performed in that review package.

## 3. Severity distribution
| Severity | Count |
|---|---:|
| بحرانی | 4 |
| بالا | 30 |
| متوسط | 114 |
| پایین | 112 |

## 4. Phase distribution
| Phase | Count |
|---|---:|
| فاز 1 | 35 |
| فاز 2 | 31 |
| فاز 3 | 32 |
| فاز 4 | 24 |
| فاز 5 | 31 |
| فاز 6 | 23 |
| فاز 7 | 18 |
| فاز 8 | 15 |
| فاز 9 | 12 |
| فاز 10 | 12 |
| فاز 11 | 12 |
| فاز 12 | 7 |
| فاز 13 | 8 |

## 5. Critical findings highlighted by source package
The source summary identified these four critical items; the full master register below remains authoritative for all 260 IDs.

| ID | Location | Finding |
|---|---|---|
| C-01 | `CFIPExecutionBot` / `DemoMarketExecutionCoordinator` | اجرای aggressive بدون slippage guard کافی |
| C-02 | `DemoMarketExecutionCoordinator` | SL/TP نسبی بدون post-fill verification |
| C-03 | `CFIPExecutionBot` / `ShadowHostCoordinator` / `CFIPReadOnlyProviderRefresh` | restart indicator باعث stale شدن revision در cBot می‌شود |
| C-05 | `PendingFilledHandler` / `ActivePlanMarketState` / `PlanActivation` | `_peakPrice` پس از fill شدن pending به‌درستی initialize نمی‌شود |

## 6. Cross-cutting root families recorded by source package
کنترل‌های gating که صرفاً بصری/indicator-side هستند، state ناقص یا ناسازگار live/restart/fill، clampهای hard-coded، مسائل culture/time، استفاده از bar index به جای time، cache/event بدون unsubscribe، dead/duplicate code، و تعریف‌های دوگانه برای HTF/RR/reversal از خانواده‌های ریشه‌ای گزارش‌شده هستند.

## 7. Rejected / corrected cases recorded by source package
مواردی که summary منبع صریحاً به‌عنوان rejected/closed/corrected ثبت کرده بود باید در بازبینی مجدد با وضعیت فعلی تطبیق داده شوند: C-04 به M-100 کاهش یافته؛ M-11 درباره spam warning رد شده؛ M-46 درباره news-protection frequency رد شده؛ M-60 درباره non-increasing RR ladder رد شده؛ M-86 درباره InvalidOperationException رد شده؛ L-41 و L-42 بسته شده‌اند.

## 8. Suggested source priorities
اولویت‌های پیشنهادی منبع: C-03/H-28؛ C-01/C-02/H-30؛ C-05/H-11/H-12؛ H-17/H-20/M-01؛ H-26/H-27/H-22؛ M-117. این‌ها «اولویت تاریخی منبع» هستند و جایگزین اولویت‌بندی پس از ممیزی current `main` نیستند.

## 9. Live-validation items called out by source
M-118 (AccessRights.None و file/HTTP روی cTrader نصب‌شده)، M-45 (InstanceId بعد از reattach)، H-17 (API time Kind)، و C-05 (ترتیب event بین PendingFilled و PositionOpened) نیازمند اعتبارسنجی runtime/live معرفی شده‌اند.

## 10. Master Issue Register — all 260 findings
وضعیت/اطمینان در جدول زیر عیناً از `MASTER-ISSUES.csv` حفظ شده است. ستون «شرح ایراد» و «راه‌حل پیشنهادی» نیز برای جلوگیری از از دست رفتن جزئیات به‌صورت مستقیم منتقل شده‌اند.

### فاز 1 — 35 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| C-01 | بحرانی | قطعی | CFIPExecutionBot.cs:224-280; DemoMarketExecutionCoordinator.cs:225-252 | Aggressive بدون هیچ محافظ اسلیپیج/انحراف قیمت (ExecuteMarketOrder ساده) | گارد انحراف از ورودی برای همهٔ مسیرهای بازار + ExecuteMarketRangeOrder |
| C-02 | بحرانی | قطعی | DemoMarketExecutionCoordinator.cs:155-176 | SL/TP به‌صورت pip نسبی ارسال می‌شود و بعد از fill راستی‌آزمایی نیست | اعمال SL/TP مطلق بعد از fill و بستن پوزیشن در صورت خروج از envelope |
| C-03 | بحرانی | قطعی | CFIPExecutionBot.cs:78,187-193; ShadowHostCoordinator.cs:15-18; CFIPReadOnlyProviderRefresh.cs:90,139 | بعد از ری‌استارت indicator، cBot همهٔ سیگنال‌ها را STALE REVISION می‌بیند (revision با هر کندل M5 بالا می‌رود) | ریست/بازسازی _shadow با Rebind یا تغییر InstanceId |
| H-01 | بالا | قطعی | AutomaticMarket/*, Aggressive/* (۱۰ تابع) | زنجیرهٔ ارسال بازار در indicator بدون هیچ فراخوانی است (~۱٫۹ هزار خط کد مرده) | حذف یا اتصال دوباره؛ هماهنگ‌سازی با audits |
| H-02 | بالا | قطعی | DemoMarketExecutionCoordinator.cs; DemoPendingOrderExecutionCoordinator.cs | cBot سقف ضرر روزانه، ریسک هر معامله، اسپرد، ساعت/نیوز و ایمنی اکانت را چک نمی‌کند | انتقال گاردها به cBot یا gate مرکزی قبل از انتشار |
| H-03 | بالا | محتمل | DemoMarketExecutionCoordinator.cs:281-295; DemoPendingOrderExecutionCoordinator.cs:234-247 | IsSuccessful بدون Position به‌عنوان Rejected گزارش می‌شود (باید RecoveryRequired) | وضعیت Unconfirmed + بررسی Positions |
| H-04 | بالا | قطعی | DemoMarketExecutionCoordinator.cs:166-176 | MarketProfile.StopPips/TargetPips بدون اعتبارسنجی جایگزین فاصله‌های محاسبه‌شده می‌شود | سازگاری‌سنجی با هندسهٔ قیمت‌ها |
| H-05 | بالا | قطعی | ExecutionPlanPreparation.cs:254-300 | RebuildSmartExecutionLevels فیلدهای _plan را قبل از چک نهایی تغییر می‌دهد (بدون rollback) | کار روی کپی و commit بعد از اعتبارسنجی |
| H-06 | بالا | محتمل | ExecutionPlanPreparation.cs:203-267 | ترتیب/تمایز Tp1..Tp4 در بازسازی چک نمی‌شود | (علت اصلی در H-23) |
| H-07 | بالا | محتمل | ExecutionPlanPreparation.cs:297-299 | بازسازی پلن _runtimeTpStageIndex را ریست می‌کند (پیشرفت stage از دست می‌رود) | ریست فقط برای پلن جدید |
| H-08 | بالا | محتمل | BrokerProtectionCoordinator.cs:245-264 vs BoundPlanProtection.cs:277-284 | بازگشت از RecoveryRequired بعد از محافظت موفق در دو مسیر ناهمخوان است | یکسان‌سازی بازگشت وضعیت |
| H-09 | بالا | محتمل | ServerSideTakeProfitLadder.cs:220-290 | Adopt ladder پرچم‌ها را false می‌کند؛ با استثنا وضعیت «ladder غیرفعال» می‌ماند | ریست فقط بعد از موفقیت |
| M-01 | متوسط | قطعی | PriceMath.cs:50-59 | Price() با culture جاری و digits ناهمخوان با NormalizePrice | InvariantCulture |
| M-02 | متوسط | قطعی | SubmissionGateCoordinator.cs:201,205; ServerSideTakeProfitLadder.cs | ToString("F4"/"F0") بدون Invariant | InvariantCulture |
| M-03 | متوسط | محتمل | SubmissionGateCoordinator.cs:14-25 | کلید تلاش بر پایهٔ اندیس بار M5 (با reload history جابه‌جا می‌شود) | زمان باز شدن بار |
| M-04 | متوسط | محتمل | SubmissionGateCoordinator.cs:141-157; SubmissionGate.cs:60-98 | UNCONFIRMED/exception فقط backoff می‌گذارد؛ احتمال سفارش تکراری | قفل قطعی تا تأیید بروکر |
| M-05 | متوسط | محتمل | ExecutionPlanPreparation.cs:82-102,196-249 | requiredRR[0..3] بدون چک طول؛ کپی منطق fallback RR | گارد طول + استخراج تابع |
| M-06 | متوسط | ثبت‌شده | ExecutionPlanPreparation.cs:172-176 | ریسک قبل از SelectTargets چک risk>0 ندارد | گارد |
| M-07 | متوسط | ثبت‌شده | ExecutionPlanPreparation.cs:339-349 | outهای target/stopPips در شکست ریست نمی‌شوند | ریست در شکست |
| M-08 | متوسط | قطعی | Broker*Mutation.cs (۴ فایل) | result.Error هرگز لاگ نمی‌شود | لاگ Error |
| M-09 | متوسط | ثبت‌شده | BrokerLimitOrderPlacement.cs:20-40 | MarkBrokerStateDirty قبل از ثبت سفارش نیست؛ protectionType بدون استفاده | هم‌ترازی |
| M-10 | متوسط | ثبت‌شده | BrokerProtectionCoordinator.cs:56-59 | direction با position.TradeType تطبیق داده نمی‌شود | مشتق از position |
| M-12 | متوسط | ثبت‌شده | OrphanManagedProtection.cs:35-41 | fallback ATR تخمینی PipSize*20 بدون حداقل/حداکثر | گارد |
| M-13 | متوسط | ثبت‌شده | OrphanManagedProtection.cs:160-171 | TP جایگزین بدون ارتباط با ساختار و بدون IsLiveTargetBrokerSafe | چک |
| M-14 | متوسط | ثبت‌شده | CFIPExecutionBot.cs:100-107; ShadowHostCoordinator.cs:93-96 | چک Live فقط در OnStart؛ recheck بروکر تا ۵۰۰ms کهنه | چک دوره‌ای |
| L-01 | پایین | ثبت‌شده | DemoMarketExecutionCoordinator.cs:146-152 | Ask<Bid در مسیر بازار چک نمی‌شود | گارد |
| L-02 | پایین | ثبت‌شده | DemoPendingOrderExecutionCoordinator.cs:83-88 | (TradeType)(-1) کد مرده | حذف |
| L-03 | پایین | ثبت‌شده | DemoPendingOrderExecutionCoordinator.cs:194-205 | قیمت Stop Order به tick نرمال نمی‌شود | NormalizePrice |
| L-04 | پایین | ثبت‌شده | DemoPendingOrderExecutionCoordinator.cs:128-145 | Math.Abs بدون چک سمت درست (defense-in-depth) | چک سمت |
| L-05 | پایین | ثبت‌شده | CFIPExecutionBot.cs:158-162 | بدون تلورانس اختلاف ساعت | تلورانس |
| L-06 | پایین | ثبت‌شده | CFIPExecutionBot.cs:140-148 | Deserialize در هر تیک | cache |
| L-07 | پایین | ثبت‌شده | BrokerExecutionSafety.cs:75-79 | catch بدون لاگ | لاگ |
| L-08 | پایین | ثبت‌شده | ShadowHostValidator.cs:8-9 | ثابت‌های ManagedLabel بدون استفاده و گمراه‌کننده | حذف |
| L-09 | پایین | ثبت‌شده | ShadowHostValidator.cs:398-403 | شرط زائد (IsFinitePositiveOrZero یا مقدار منفی) | ساده‌سازی |
| L-10 | پایین | ثبت‌شده | Execution/* | توابع بدون مرجع (CanAdoptPosition، AutoTradingPanelLine/Color، SetAutoTradingRuntimeState ...) | حذف |

### فاز 2 — 31 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| C-05 | بحرانی | قطعی | PendingFilledHandler.cs:62-248; ActivePlanMarketState.cs:42-69; PlanActivation.cs:45 | _peakPrice بعد از fill سفارش pending مقداردهی نمی‌شود (peakRR عددی بی‌معنی در BE/trail/exhaustion) | تابع مشترک InitializeLiveRuntimeState(plan) در همهٔ مسیرهای fill |
| H-10 | بالا | قطعی | LifecycleTransitionPolicy.cs; LifecycleStateStore.cs:13-41 | LifecycleTransitionPolicy هیچ مرجعی ندارد؛ انتقال‌ها اعمال نمی‌شوند و جدول با انتقال‌های واقعی ناسازگار است | اعمال سیاست در SetLifecycleState یا حذف |
| H-11 | بالا | قطعی | ActivePlanFalseSignalGuard.cs:62-109 | بلوک خروج سخت false-signal داخل شرط نرم/پنجرهٔ watch قرار دارد و عملاً غیرفعال است | بیرون آوردن بلوک hard از شرط soft |
| H-12 | بالا | قطعی | PlanRiskRewardRecalculator.cs:15-24; ActivePlanEvaluation.cs:38-40 | Tp1 نامعتبر ⇒ Risk=0 ⇒ کل مدیریت پوزیشن بی‌صدا متوقف می‌شود | fallback به ریسک قبلی + alert |
| H-13 | بالا | محتمل | ActivePlanEvaluation.cs:35-37; ActivePlanIntegrityHandler.cs:52-64; StructuralSetupInvalidationExit.cs:184-206; ReversalCloseGuard.cs:63-75 | رد شدن close بدون backoff در هر تیک تکرار می‌شود | backoff/circuit-breaker برای close |
| H-14 | بالا | محتمل | ManagedLivePlanRecovery.cs:87-89,306-355 | protectionMissing قبل از Adopt ladder محاسبه می‌شود؛ ممکن است RecoveryRequired دائمی شود | محاسبه بعد از Adopt |
| H-15 | بالا | قطعی | PendingFillProtectionCoordinator.cs:86-91 | _activeBrokerStop/Target با مقدار خواسته‌شده پر می‌شود نه مقدار واقعی بروکر | خواندن مقدار از position |
| H-16 | بالا | قطعی | PendingOrderPlanSnapshot.cs:182-221 | بعد از override targetها، تعمیر ladder برای stage 1..3 وجود ندارد (Tp غیر صعودی) | اعتبارسنجی ترتیب بعد از override |
| M-15 | متوسط | قطعی | PendingCancelledHandler.cs:39-53 | لغو pending با پلن غیرزنده ⇒ وضعیت PendingOrder گیر می‌کند | بازگرداندن به Flat |
| M-16 | متوسط | ثبت‌شده | PendingModifiedHandler.cs:26-31 | بدون idempotency/چک وضعیت؛ رویداد دیر می‌تواند وضعیت را عقب ببرد | guard |
| M-17 | متوسط | ثبت‌شده | PendingFilledHandler.cs:23-27 | PendingOrder==null ⇒ handler بی‌اثر و پوزیشن bind نمی‌شود | مسیر جایگزین |
| M-18 | متوسط | ثبت‌شده | PendingFilledHandler.cs:71-87 | _plan قبل از چک null بازنویسی می‌شود | مقدار موقت |
| M-19 | متوسط | قطعی | PendingFillPlanBuilder.cs:111-146; ManagedLivePlanRecovery.cs:186-232 | با stop==0 target = entry±entry×RR محاسبه می‌شود | اعتبارسنجی stop در ابتدا |
| M-20 | متوسط | ثبت‌شده | PositionClosedHandler.cs:82 | _brokerProtectionRecoveryRequired بدون شرط ریست می‌شود | شرط پلن |
| M-21 | متوسط | ثبت‌شده | PositionOpenedHandler.cs:57-63 | RecoverManagedLivePlan روی اولین پوزیشن managed نه args.Position | استفاده از args |
| M-22 | متوسط | ثبت‌شده | BrokerStateSnapshot.cs:116-132 | بازگشت سلامت محافظت RecoveryRequired را اصلاح نمی‌کند | ریست |
| M-23 | متوسط | قطعی | ActivePlanLiveManagement.cs:135-152 | شرط تغییر SL همیشه صفر ⇒ alert PLANUPDATE هرگز نمی‌رود | مقایسه با کاندید |
| M-24 | متوسط | ثبت‌شده | ProtectionManager.cs:41-58,94-242 | ترکیب کاندیدها قبل از اعتبارسنجی؛ دو بلوک swing تکراری | اعتبارسنجی جدا |
| M-25 | متوسط | قطعی | ActivePlanLevelExitHandler.cs:45-66 | hitTp3 با Owned و بقیه با Active سنجیده می‌شود | یکسان‌سازی |
| M-26 | متوسط | قطعی | ActivePlanLevelExitHandler.cs:111-133; PartialTakeProfitExecutor.cs:25-29 | ExecutePartialClose در OriginalVolume<=0 true برمی‌گرداند (TP1 HIT کاذب) | false + alert |
| M-27 | متوسط | ثبت‌شده | PartialTakeProfitExecutor.cs:61-68 | بستن کامل پوزیشن بدون alert هنگام کم‌بودن باقی‌مانده | alert |
| M-28 | متوسط | ثبت‌شده | StructuralSetupInvalidationExit.cs:63-91 | ارزیابی با قیمت لحظه‌ای؛ کف maxAdverseR=0.30 پارامتر را نادیده می‌گیرد | close بار |
| L-11 | پایین | ثبت‌شده | LiveTargetCandidateEvaluator.cs:35,112-188 | IndexOf/Count بدون چک null؛ peakRR بی‌استفاده | گارد |
| L-12 | پایین | ثبت‌شده | SmartExitPressureCalculator.cs:73-78 | _m5Bars.Count قبل از چک null | ترتیب |
| L-13 | پایین | ثبت‌شده | ActivePlanMarketState.cs:36-40 | _lastMarket قبل از اعتبارسنجی مقدار می‌گیرد | ترتیب |
| L-14 | پایین | ثبت‌شده | PlanActivation.cs:52-83 | ActivatePlan فیلدهای broker/alert/snapshot را ریست نمی‌کند | ریست |
| L-15 | پایین | ثبت‌شده | ActivePlanLevelExitHandler.cs:75-80; StructuralSetupInvalidationExit.cs:172-178 | پوزیشن پیدا نشد: فقط Closed ست می‌شود (_plan پاک نمی‌شود) | پاک‌سازی |
| L-16 | پایین | ثبت‌شده | LiveFillExitReconciler.cs:161-329 | candidateRisk بدون چک >0؛ referencePlan? زائد | گارد |
| L-17 | پایین | ثبت‌شده | ReversalProtection.cs; ReversalCloseGuard.cs; ActivePlanFalseSignalGuard.cs; PartialTakeProfitExecutor.cs | ToString("F0/F2") بدون Invariant | Invariant |
| L-18 | پایین | ثبت‌شده | ReversalProtection.cs; ActivePlanFalseSignalGuard.cs | کف‌های Max(0.8/15/55) پارامتر را بالا می‌برند | M-110 |
| L-19 | پایین | قطعی | PositionCircuitBreaker.cs; PendingOrderCircuitBreaker.cs | CloseAllPositions و CancelAllOrders کد مرده | حذف |

### فاز 3 — 32 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| H-17 | بالا | محتمل | DailyLossPersistence.cs:275-280; DailyLossAccounting.cs; EndOfDayAlert.cs | ToUniversalTime روی DateTime با Kind=Unspecified (جابه‌جایی مرز روز بر اساس ساعت محلی ماشین) | SpecifyKind(Utc) برای مقادیر API |
| H-18 | بالا | قطعی | BrokerProtectionExecution.cs:70-81 vs ManagedIdentityRule.cs | حلقهٔ orphan فقط برچسب پایه را مقایسه می‌کند ⇒ محافظت پوزیشن بدون پلن هرگز اجرا نمی‌شود | استفاده از IsManagedPosition |
| H-19 | بالا | محتمل | BrokerIdentity.cs:131-150 | برچسب pending خالی ⇒ سفارش دستی با Label="" «مدیریت‌شده» حساب می‌شود | false صریح برای برچسب خالی |
| M-29 | متوسط | قطعی | RiskPercentPolicy.cs:11-28 | درصد ریسک بی‌صدا در [0.05,5] قفل می‌شود (کم‌ها افزایش می‌یابد) | لاگ/UI |
| M-30 | متوسط | قطعی | MarginSafetyCalculator.cs:62-78 | حلقهٔ کاهش حجم بدون چک step<=0 و سقف تکرار | گارد |
| M-31 | متوسط | ثبت‌شده | MarginSafetyCalculator.cs:20-22 | با UseAutoMarginGuard=false حجم نرمال‌نشده برمی‌گردد | نرمال‌سازی |
| M-32 | متوسط | قطعی | AlertEngine.cs:262-302 | صدای هشدارهای حیاتی بد نگاشت می‌شود (PROTECTION-REJECTED و ...) | نگاشت منفی |
| M-33 | متوسط | محتمل | EndOfDayAlert.cs:74-165 | close تکراری در هر فراخوانی؛ نتیجه نادیده؛ بدون backoff | backoff |
| M-34 | متوسط | ثبت‌شده | AlertEngine.cs:152-171 | SendEmail همزمان و برای همهٔ هشدارها | async/فقط critical |
| M-35 | متوسط | قطعی | AlertEngine.cs:58-80 | با SuppressDuplicateAlerts=false هیچ cooldown نیست | حداقل cooldown |
| M-36 | متوسط | ثبت‌شده | PlanCreationEligibility.cs:37-46 | cooldown خروج با _lastSignalM5 سنجیده می‌شود نه _lastExitM5 | اصلاح مبنا |
| M-37 | متوسط | ثبت‌شده | SignalPlanCoordinator.cs:19-37 | _lastAutoPlanAttemptM5 فقط بعد از موفقیت ست می‌شود | ثبت تلاش |
| M-38 | متوسط | ثبت‌شده | PreTradePlanSynchronizer.cs:19-21; PlanActivation.cs:28-35 | بعد از لغو پلن وضعیت PlanReady و snapshot پاک نمی‌شود | پاک‌سازی |
| M-39 | متوسط | قطعی | TargetObstacleScanCache.cs:196-207 | اشتراک رویداد بدون لغو | unsubscribe |
| M-40 | متوسط | ثبت‌شده | DailyLossPersistence.cs:81-146 | Parse به‌جای TryParse؛ کلید بدون بروکر | TryParse + بروکر |
| M-41 | متوسط | ثبت‌شده | BrokerIdentity.cs:42-66 | TradingPermission.Request هر ۳ ثانیه تکرار | backoff |
| M-42 | متوسط | ثبت‌شده | PriceProtectionValidation.cs:187-190 | IsValidManagedStop مرجع بدون جهت (Bid) | نسخهٔ جهت‌دار |
| M-43 | متوسط | ثبت‌شده | ContextAlertEmitter.cs:20-25,79-92 | اعتبار frame و اندیس bar چک نمی‌شود | گارد |
| M-44 | متوسط | ثبت‌شده | TradeActionabilityEvaluator.cs:238-312 | حلقه بدون چک closedM5<Count؛ شواهد «ناشناخته» مبهم | گارد |
| M-45 | متوسط | محتمل | BrokerIdentity.cs:120-129 | برچسب شامل InstanceId؛ با تغییر آن پوزیشن‌های قبلی unmanaged می‌شوند | InstanceId پایدار |
| L-20 | پایین | ثبت‌شده | AlertEngine.cs:33,46 | key بدون چک null | گارد |
| L-21 | پایین | ثبت‌شده | AutoTradeSafetyGuard.cs:33-105 | newsReason دور ریخته؛ فرمول سقف margin سه‌بار تکرار؛ F0 | یکی‌سازی |
| L-22 | پایین | ثبت‌شده | MarketSuitabilityGuard.cs:45 | "NEWS BLACKOUT" با reason "NEWS" نمی‌خواند | رشته |
| L-23 | پایین | ثبت‌شده | BrokerIdentity.cs:27; PriceProtectionValidation.cs:61,135 | catch خالی | لاگ |
| L-24 | پایین | ثبت‌شده | HtfTargetPresenceValidator.cs:19-25; HigherTfRewardPathValidator.cs:32-38 | عنصر null و index>=Count چک نمی‌شود | گارد |
| L-25 | پایین | ثبت‌شده | LiveProtectionDistanceResolver.cs:52-58 | پارامتر 1.0 بدون توضیح | ثابت نام‌دار |
| L-26 | پایین | ثبت‌شده | TradeActionabilityEvaluator.cs:11,468 | lane بی‌استفاده؛ چک tp1RR تکراری | حذف |
| L-27 | پایین | ثبت‌شده | SuitabilityRiskMultiplierCalculator.cs; SuitabilityCalculator.cs | اعداد ثابت 70/60/0.82 | پارامتر |
| L-28 | پایین | ثبت‌شده | VolumeSizer.cs:39; AggressiveVolumeSizer.cs:26 | حجم بر پایهٔ Equity (P/L شناور اثر دارد) | Balance/گزینه |
| L-29 | پایین | ثبت‌شده | DailyLossAccounting.cs:143-157 | فقط Deposit/Withdrawal؛ ریست ۰۰:۰۰ UTC | Credit/Bonus |
| L-30 | پایین | ثبت‌شده | ContextAlertEmitter.cs:126-149 | هر دو liquidity true ⇒ فقط BUY؛ AbsoluteStrength با آستانهٔ confidence | اصلاح |
| L-31 | پایین | ثبت‌شده | MarketSuitabilityGuard.cs:33-47 | اتصال شکنندهٔ رشته‌ای با SuitabilityCalculator | enum |

### فاز 4 — 24 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| H-20 | بالا | محتمل | OutcomeMemoryStore.cs:206-240; RuntimeLogPersistence.cs:165-199; SignalEvaluationTraceArchivePersistence.cs | نوشتن int با culture جاری و خواندن با Invariant؛ سابقهٔ SELL (Direction=-1) هنگام restore حذف می‌شود | InvariantCulture در همهٔ Append(int/long) |
| H-21 | بالا | محتمل | OutcomeTelemetryEngine.cs:118-152; PlanRiskRewardRecalculator.cs | مخرج RealizedR ریسک جاری است نه ریسک اولیه؛ وارد AdaptiveOutcomeRiskPolicy می‌شود | ذخیرهٔ InitialRisk/InitialStop در Plan |
| M-47 | متوسط | ثبت‌شده | EconomicNewsRiskEvaluator.cs:172-180,277 | NewsRiskPanelLine (رندر) _economicNewsBlockingEvent را بازنویسی می‌کند | جداسازی |
| M-48 | متوسط | قطعی | EconomicNewsCalendarClient.cs:120-125 | ResolveCurrencies با پارامتر اشتباه ⇒ فیلتر خبر شاخص/فلز کور است | پاس symbolCurrencyMap |
| M-49 | متوسط | ثبت‌شده | EconomicNewsCalendarClient.cs:252-262 | بعد از شکست دریافت، تلاش بعدی NewsRefreshMinutes عقب می‌افتد | backoff کوتاه |
| M-50 | متوسط | ثبت‌شده | BufferedArchivePersistence.cs:215-265 | صف pending بدون سقف و PendingKeys پاک نمی‌شود | سقف |
| M-51 | متوسط | ثبت‌شده | BufferedArchivePersistence.cs:445-494; OutcomeHistoryArchiveStore.cs:358-440 | خواندن همزمان فایل‌های بزرگ روی رشتهٔ اصلی | غیرهمزمان/سقف |
| M-52 | متوسط | ثبت‌شده | PortableMemorySnapshotStore.cs:504-512 | نوشتن درجا (نه temp+replace) | temp+replace |
| M-53 | متوسط | ثبت‌شده | BufferedArchivePersistence.cs:432-440 | catch بدون لاگ در Flush | لاگ |
| M-54 | متوسط | ثبت‌شده | EarlyPredictionEngine.cs:164-275 | Prediction بدون Entry/SL/TP با Direction معتبر برمی‌گردد | گارد |
| M-55 | متوسط | ثبت‌شده | LiveReversalAnalyzer.cs:250-265 | دو موتور reversal موازی؛ IsManagedPosition چک نمی‌شود | یکی‌سازی |
| M-56 | متوسط | ثبت‌شده | OutcomeTelemetryEngine.cs:154-175; AdaptiveOutcomeRiskPolicy.cs | معاملهٔ orphan با RealizedR=0 در سیاست ریسک می‌آید | فیلتر CalibrationEligible |
| M-57 | متوسط | ثبت‌شده | OutcomeHistoryArchiveStore.cs:358-440 | Import شمارنده‌ها را صفر و دوباره می‌سازد (از دست‌رفتن صف buffer) | flush قبل از import |
| M-58 | متوسط | ثبت‌شده | OutcomeMemoryStore.cs:505-526 | دو نمونه روی یک نماد: آخرین نوشتن برنده است | merge |
| M-59 | متوسط | ثبت‌شده | OutcomeMemoryAccountSwitch.cs:7-52 | تعویض حساب _wins/_losses و traceها را ریست نمی‌کند | ریست |
| L-32 | پایین | ثبت‌شده | TradePlanRegistry.cs:25-72 | تلورانس جایگزینی عدد جادویی؛ رفتار Upsert/UpsertScenario متفاوت | یکسان |
| L-33 | پایین | ثبت‌شده | EconomicNewsProtection.cs:177-183 | utc بی‌استفاده؛ ArchiveRuntimeEvent با ۲۰ آرگومان | DTO |
| L-34 | پایین | ثبت‌شده | SignalEvaluationTraceRecorder.cs:303-309 | MatchesClosedBar همان ticks را دوبار می‌گیرد (همیشه true) | اصلاح |
| L-35 | پایین | ثبت‌شده | OutcomePanelTelemetry.cs; OutcomeTelemetryEngine.cs:353; LiveReversalAnalyzer.cs:243 | ToString F بدون Invariant | Invariant |
| L-36 | پایین | ثبت‌شده | EarlyPredictionEngine.cs:222-285 | ZoneLow/High نرمال نمی‌شود؛ BuildExecutionModel هر کندل | cache |
| L-37 | پایین | ثبت‌شده | RuntimeLogPersistence.cs; OutcomeHistoryArchiveStore.cs | Base64 بدون decoder | ابزار decode |
| L-38 | پایین | ثبت‌شده | RuntimeLogPersistence.cs:332-333 | Confidence به‌جای smartQuality | اصلاح |
| L-39 | پایین | ثبت‌شده | RuntimeLogPersistence.cs:140; BufferedPersistenceCoordinator.cs:39-41 | ToUniversalTime روی Unspecified | H-17 |
| L-40 | پایین | ثبت‌شده | HistoricalOutcomeReader.cs:13-22 | fallback به NetProfit شیء Position بسته | History |

### فاز 5 — 31 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| H-22 | بالا | قطعی | DailyPivotTargetSource.cs:26-37 | پیوت روزانه از پریروز ساخته می‌شود (previous=index-1 در حالی که ClosedIndex=دیروز) | previous=index |
| H-23 | بالا | قطعی | TargetStageSelector.cs:25-67; PlanTargetPreparation.cs:55-93 | target مصنوعی مراحل خالی با target قبلی مقایسه نمی‌شود؛ چک ترتیب فقط با RequirePlanIntegrity | محدود کردن به previous + اعتبارسنجی اجباری |
| H-24 | بالا | محتمل | TargetLevelBuilder.cs:158-173 | Take(N) قبل از فیلتر سمت اشتباه و merge سطوح سالم را بیرون می‌کند | فیلتر سمت → merge → Take |
| M-61 | متوسط | قطعی | HtfSourceClassifier.cs; TargetLevelCandidateMerger.cs; HtfTargetPresenceValidator | چهار تعریف متفاوت از HTF | تعریف واحد |
| M-62 | متوسط | قطعی | MinimumRequiredRiskRewardCalculator.cs:30-47 | کف‌های 2.10 و 1.75 پارامتر Tp1MinimumRR را جایگزین می‌کنند | گزارش مقدار مؤثر |
| M-63 | متوسط | قطعی | PlanInputPreparation.cs; PlanProtectionIntegrityValidator.cs; StructuralStopCandidateEvaluator.cs | کف‌های ثابت Max(40,...) روی MinimumEntryQuality | هم‌خوانی با UI |
| M-64 | متوسط | قطعی | PlanMarketConstraintValidator.cs:72-76 | Tp1Quality=0 (نامشخص) از فیلتر رد نمی‌شود | رد/هشدار |
| M-65 | متوسط | ثبت‌شده | StructuralStopCandidateEvaluator.cs:311-397 | ارزیابی ضرب‌شونده + آلودگی telemetry + requireHtf=false | حذف ثبت stage=0 |
| M-66 | متوسط | قطعی | PlanPreviewBuilder.cs:22-148 | پیش‌نمایش با پلن واقعی فرق دارد؛ کش بدون closedM5 | کلید کش |
| M-67 | متوسط | قطعی | PredictivePendingLevelSelector.cs:136-217 | تغییر candidate.Source حین حلقه ⇒ confluence وابسته به ترتیب | نسخهٔ کپی |
| M-68 | متوسط | ثبت‌شده | ExecutionIntentValidation.cs:64-72 | برای market فاصلهٔ قیمت زنده از RequestedEntry سنجیده نمی‌شود | گارد |
| M-69 | متوسط | قطعی | M1TriggerRuntimeUpdater.cs vs M1TriggerReadyEvaluator.cs | منطق M1 trigger تکراری و ناهمگون؛ warmup TriggerReady را آپدیت نمی‌کند | یکی‌سازی |
| M-70 | متوسط | ثبت‌شده | PlanRewardIntegrityValidator.cs:74-280 | رد پلن فقط-TP1 با RequireHtfRewardForTp2Plus؛ تلورانس drift ریز | اصلاح شرط |
| M-71 | متوسط | ثبت‌شده | TargetLevelBuilder.cs:22-41 | کلید کش ناقص و جهش Level مشترک در MergeLevels | کلید کامل/کپی |
| M-72 | متوسط | ثبت‌شده | TradingSessionFilter.cs:73-115 | VolatilityBlocked هزینهٔ بالا؛ ATR «قبلی» برای i<15 از آینده | اصلاح اندیس |
| M-73 | متوسط | ثبت‌شده | PredictivePendingLevelSelector.cs; PredictivePendingZoneCollector.cs | آستانه‌های سخت‌کد 0.18/2.5/0.12/65 | پارامتر |
| M-74 | متوسط | ثبت‌شده | ExecutionZoneCandidateSelector.cs:95-232 | کیفیت ثابت هر منبع و fallback swing دلخواه | پارامتر |
| M-75 | متوسط | ثبت‌شده | MarketEntryValidation.cs:114-153 | CreatedM5=-1 ⇒ Atr اندیس نامعتبر؛ پیام SLIPPAGE ACCEPTED گمراه‌کننده | گارد |
| M-76 | متوسط | ثبت‌شده | TargetMetadataEnricher.cs:47-105 | هر target ناجور SYNTHETIC_RR با کیفیت ثابت 55 می‌گیرد | برچسب دقیق |
| M-77 | متوسط | ثبت‌شده | Bull/BearTriggerScoreAnalyzer.cs | atr==0 ⇒ امتیاز رایگان | گارد atr |
| L-43 | پایین | ثبت‌شده | TargetLevelCandidateMerger.cs:42-52 | kind.IndexOf بدون چک null | گارد |
| L-44 | پایین | ثبت‌شده | TargetLevelMerger.cs:42-48 | خوشه‌بندی حریصانه و وابسته به ترتیب | مرتب‌سازی |
| L-45 | پایین | ثبت‌شده | SupplyDemandLiquidityTargetSource.cs; SmartExtraTargetSource.cs | SESSION و SESSION_FORECAST دوبار ⇒ Hits باد می‌کند | dedupe |
| L-46 | پایین | ثبت‌شده | DailyPivotTargetSource.cs:64 | PIVOT مرکزی برای هر دو جهت | چک سمت |
| L-47 | پایین | ثبت‌شده | HtfRewardSourcePolicy.cs:31-37 | تلورانس با فایل‌های دیگر هم‌خوان نیست | ثابت مشترک |
| L-48 | پایین | ثبت‌شده | StructuralStopPlanner.cs:28; PlanInputPreparation.cs:28; ExecutionZoneBuilder.cs:33 | حداقل بار 20 و 30 بدون دلیل | ثابت مشترک |
| L-49 | پایین | قطعی | RegimeFilter.cs:176-198 | DetectRegime بدون مرجع | حذف |
| L-50 | پایین | ثبت‌شده | ExecutionZoneQualityEvaluator.cs:19-27 | PremiumDiscountBias دوبار؛ امتیاز M30 جادویی | cache |
| L-51 | پایین | ثبت‌شده | ExecutionIntentBuilder.cs:55 | Volume بدون اعتبارسنجی | گارد |
| L-52 | پایین | ثبت‌شده | PredictivePendingLevelSelector.cs:129-134 | اثر جانبی روی _autoOrdersBlockReason | برگشت مقدار |
| L-53 | پایین | ثبت‌شده | TargetStageRejectionTelemetry.cs; PlanRewardIntegrityValidator.cs:275-301 | علت رد فقط با EnableOutcomeTelemetry ثبت می‌شود | همیشه لاگ |

### فاز 6 — 23 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| H-25 | بالا | قطعی | DecisionOrchestration.cs:242-311; CalculationClosedBar.cs:113 | actionability و پلن با _decision کندل قبل ارزیابی می‌شوند | DecisionContext صریح |
| H-26 | بالا | قطعی | WaveTrendEngine.cs:67-70,207-208 | آستانهٔ oversold (Min(os1,os2)) و overbought (Min(ob1,ob2)) نامتقارن‌اند ⇒ سوگیری به SELL | قرینه‌سازی با Max(os) |
| H-27 | بالا | قطعی | DivergenceAnalyzer.cs:350-354,456-460 | واگرایی مخفی هیچ‌گاه امتیاز oscillatorAgreement نمی‌گیرد (تا ۱۸ امتیاز) | شمارش با علامت معکوس برای hidden |
| M-78 | متوسط | ثبت‌شده | MarketRegimeAnalyzer.cs:29-128 | Stability برای فریم‌های غیر M5 همیشه ۱ | محاسبه |
| M-79 | متوسط | ثبت‌شده | M5RegimeCoreCache.cs:16-94 | cache با ارجاع Bars و index (بدون fingerprint) | fingerprint |
| M-80 | متوسط | ثبت‌شده | WaveTrendEngine.cs:120-157; WaveTrendEvidenceAnalyzer.cs:18-37 | محاسبه فقط رو به جلو؛ اشتراک رویداد بدون لغو | Dispose |
| M-81 | متوسط | قطعی | MarketFrameEvidence.cs; DecisionSmartGates.cs; DecisionConfirmationGates.cs; DecisionStructureGates.cs | کف‌های سخت‌کد (55، 18، 72، 85) پارامتر کاربر را بالا می‌برند | M-110 |
| M-82 | متوسط | ثبت‌شده | TimeframeAgreementAnalyzer.cs:87-114 | فریم‌های خنثی از مخرج حذف می‌شوند ⇒ توافق ۱۰۰٪ کاذب | مخرج کل |
| M-83 | متوسط | ثبت‌شده | EmpiricalConfidenceCalibrator.cs:233-258 | فقط win-rate؛ AverageRealizedR استفاده نمی‌شود | ترکیب R |
| M-84 | متوسط | قطعی | DecisionRestrictionAlertPolicy.cs:16 | case "NEWS BLACKOUT" با reason واقعی "NEWS" نمی‌خواند | اصلاح رشته |
| M-85 | متوسط | قطعی | ParallelScenarioComputation.cs; PlanPreviewBuilder.cs | دو نسخهٔ تقریباً یکسان از هندسهٔ سناریو | یکی‌سازی |
| M-87 | متوسط | ثبت‌شده | DirectionAcceptanceGate.cs:30-34 | _lastConfirmedM5 اندیس بار؛ بعد از reload همهٔ جهت‌ها رد | زمان بار |
| M-88 | متوسط | قطعی | MarketFrameEvidence.cs:21-65; DecisionInputSnapshotFactory.cs:149-167 | فریم بدون داده «خنثی» می‌شود و ValidateRequiredFrame فقط اندیس را چک می‌کند | پرچم Ready |
| L-54 | پایین | ثبت‌شده | DecisionSmartGates.cs; DecisionConfirmationGates.cs | regime با رشتهٔ ثابت | MarketRegimeIdentity |
| L-55 | پایین | ثبت‌شده | DecisionTacticalOpportunityAnalyzer.cs:150 | quality بدون سقف 100 | clamp |
| L-56 | پایین | ثبت‌شده | RangeSignalQualityEvaluator.cs:20,37 | regime==null fail-open ولی نبود bars fail-closed | یکسان |
| L-57 | پایین | ثبت‌شده | TimeframeScenarioBuilder.cs:22-31 | catch بدون لاگ | لاگ |
| L-58 | پایین | ثبت‌شده | DivergenceAnalyzer.cs:456-474 | ثابت‌های 10/5؛ توابع بدون مرجع؛ کپی جست‌وجوی swing | ساده‌سازی |
| L-59 | پایین | ثبت‌شده | VolumeExpansionAnalyzer.cs:99-101 | تناری با دو شاخهٔ یکسان | ساده‌سازی |
| L-60 | پایین | ثبت‌شده | VwapBiasAnalyzer.cs:27-80 | «VWAP» غلتان است نه جلسه‌ای | تغییر نام |
| L-61 | پایین | ثبت‌شده | HigherTimeframePenaltyCalculator.cs:13-33 | فقط H1/H4/D1؛ تعداد فریم مخالف بی‌اثر | گسترش |
| L-62 | پایین | ثبت‌شده | DecisionQualityCalculator.cs:31 | retestQuality<=0 ⇒ 50 (مبهم) | nullable |
| L-63 | پایین | ثبت‌شده | ParallelOpportunityBuilder.cs:227-406 | Max(PipSize*2,0) زائد؛ upsert با _decision کهنه | H-25 |

### فاز 7 — 18 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-89 | متوسط | قطعی | ZoneLookupHotCache.cs:27-45; FvgDetectionAnalyzer.cs; OrderBlockAnalyzer.cs | cache تک‌جایگاهی (thrash) و کلید بدون atr | کلید کامل |
| M-90 | متوسط | ثبت‌شده | FvgZoneQualityCalculator.cs:95-114 | هم‌راستایی HTF از آخرین frame صرف‌نظر از currentIndex | اندیس‌محور |
| M-91 | متوسط | ثبت‌شده | OrderBlockConfluenceAnalyzer.cs:16-65 | هزینهٔ ضرب‌شوندهٔ sweep برای هر OB | cache |
| M-92 | متوسط | ثبت‌شده | EqualLevelAnalyzer.cs:53-90 | قدیمی‌ترین خوشه نه نزدیک‌ترین؛ بدون چک شکسته‌نشده | اصلاح |
| M-93 | متوسط | ثبت‌شده | SwingPointAnalyzer.cs:278-379 | swing شکسته‌شده هنوز target شمرده می‌شود | IsActiveUnbrokenLevel |
| M-94 | متوسط | محتمل | StructureAnalyzer.cs:95-165 | MSS عملاً همان Structure (شمارش دوگانه) | تأیید در فاز ۱۴ |
| M-95 | متوسط | ثبت‌شده | StructureAnalyzer.cs:173-245 | BullChoch فقط برگشت یک‌کندلی را می‌گیرد | بازنگری |
| M-96 | متوسط | ثبت‌شده | NativeIndicatorRegistry.cs:72-91; wrappers | Native نیمه‌ساخته ذخیره می‌شود؛ مقدار خنثی ≈ «آماده نیست» | پرچم Ready |
| M-97 | متوسط | قطعی | SkenderStoch.cs:38-43 | null در warm-up با ?? 0 صفر می‌شود ⇒ رأی کاذب | رد |
| M-98 | متوسط | ثبت‌شده | OssQuoteSeriesCache.cs:44-362 | رویدادها بدون لغو؛ لیست داخلی بیرون داده می‌شود؛ cast decimal NaN | کپی/گارد |
| M-99 | متوسط | ثبت‌شده | ReactionAnalyzer.cs:85-223 | Confidence بعد از Clamp تنظیم می‌شود (>100 یا <0) | clamp نهایی |
| L-64 | پایین | ثبت‌شده | ReactionAnalyzer.cs:19-80 | TriggerReady/Confidence روی کندل زنده نوسان دارد (عمدی) | مستندسازی |
| L-65 | پایین | ثبت‌شده | FvgDetectionAnalyzer.cs:54-61; OrderBlockAnalyzer.cs:58-65 | Lookback بی‌صدا به MaximumZoneAgeBars محدود می‌شود | مستندسازی |
| L-66 | پایین | ثبت‌شده | OrderBlockAnalyzer.cs:83,157-181 | کف Max(50) و ثابت‌ها در امتیاز انتخاب | پارامتر |
| L-67 | پایین | ثبت‌شده | StructureAnalyzer.cs:287-388 | آستانه‌های سخت‌کد 0.15/15/RSI50 | پارامتر |
| L-68 | پایین | ثبت‌شده | OssIndicatorParameters.cs:7-18 | پنجره‌ها و دوره‌های OSS ثابت | پارامتر |
| L-69 | پایین | ثبت‌شده | Structure/* | کپی آینه‌ای Bull*/Bear* در همه‌جا | الگوی واحد |
| L-70 | پایین | ثبت‌شده | OrderBlockConfluenceAnalyzer.cs:126,163 | حاشیهٔ atr*0.05 ثابت | پارامتر |

### فاز 8 — 15 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| H-28 | بالا | قطعی | CFIPReadOnlyProviderRefresh.cs; CFIPReadOnlyProviderPlan.cs:32-41 | کلیدهای ایمنی indicator (EnableAutoTrading، DailyLoss، RuntimeFault، Identity) جلوی انتشار intent به cBot را نمی‌گیرند | دروازهٔ انتشار مرکزی |
| H-29 | بالا | محتمل | CFIPReadOnlyProviderIdentity.cs:122-129 | کلید idempotency شامل revision است؛ همان intent با revision جدید کلید جدید می‌گیرد | کلید بر پایهٔ signalId+planId+action |
| H-30 | بالا | محتمل | CFIPReadOnlyProviderPlan.cs:102,139; CalculationStartupSeed.cs:57-76 | intent بازار بدون ExpiryUtc؛ cBot فقط سن ObservedUtc را می‌سنجد | ExpiryUtc برای market + چک createdUtc |
| M-100 | متوسط | ثبت‌شده | CFIPDeviceSignalPublisher.cs:14-19 | heartbeat در envelope نیست؛ cBot liveness نمی‌بیند | فیلد heartbeat |
| M-101 | متوسط | ثبت‌شده | CalculationCycle.cs:100-108 | finally بعد از خطا هم _lastCalculationCompletedUtc را به‌روز می‌کند | فقط موفق |
| M-102 | متوسط | ثبت‌شده | CalculationStageIsolation.cs:195-460 | ~۲۵ مرحله در هر تیک؛ SynchronizeLiveBrokerState تا ۵× | throttle |
| M-103 | متوسط | ثبت‌شده | CalculationLiveCycle.cs:184-362 | RefreshLiveDecisionActionability از مسیر رندر state تصمیم را تغییر می‌دهد | جداسازی |
| M-104 | متوسط | ثبت‌شده | RuntimeInitialization.cs:224-299; StartupDataHelpers.cs | HookHistoricalBarsEvents یک‌بار؛ Barsهای دیرتر رویداد نمی‌گیرند | hook دوباره |
| M-105 | متوسط | ثبت‌شده | CalculationReadinessStateStore.cs:94-103 | مدیریت پوزیشن در حالت انتظار با closedM5 کهنه | اندیس زمان |
| M-106 | متوسط | قطعی | RuntimeFaultBoundary.cs:108-127; CalculationStageIsolation.cs:462-504 | هر استثنا (حتی رندر پنل) BlockAutomaticEntry می‌کند؛ stacktrace در هر تیک | طبقه‌بندی مراحل |
| L-71 | پایین | ثبت‌شده | CalculationCycle.cs:86; CalculationStageIsolation.cs; CalculationStartupSeed.cs:97 | catch(StackOverflowException) کد مرده | حذف |
| L-72 | پایین | ثبت‌شده | CFIPDeviceSignalPublisher.cs:23-34; RuntimeInitialization.cs:503-546 | envelope یتیم در LocalStorage بعد از OnDestroy | حذف کلید |
| L-73 | پایین | ثبت‌شده | PanelHeartbeatLiveState.cs:45,69 | ToString F بدون Invariant | Invariant |
| L-74 | پایین | ثبت‌شده | MtfContextBuilder.cs:71-78 | HasEnoughData با آستانه‌های ثابت و بدون M1/D1/W1 | یکسان‌سازی |
| L-75 | پایین | ثبت‌شده | CFIPReadOnlyProviderRefresh.cs:90 | closedM5 در fingerprint ⇒ revision و Flush هر کندل | حذف از fingerprint |

### فاز 9 — 12 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-107 | متوسط | قطعی | ActionabilityThresholdPolicy.cs:97-228 | کف‌های ثابت 70/75/70/64/50 پارامتر را جایگزین می‌کنند | هم‌خوانی/UI |
| M-108 | متوسط | قطعی | EconomicNewsCurrencyRule.cs:21-142 | فیلتر خبر برای نمادهای غیرفارکس عملاً کور است | (همراه M-48) |
| M-109 | متوسط | ثبت‌شده | LiquidityTargetCandidateRule.cs:49-82 | IsDistinct با ATR نامعتبر همهٔ کاندیدها را رد می‌کند | fallback |
| L-76 | پایین | ثبت‌شده | DivergenceThresholdRule.cs:41-50 | کف مطلق 0.10/0.08 مستقل از نماد (بدون مرجع) | حذف/ATR |
| L-77 | پایین | ثبت‌شده | PlanRewardRiskQualityRule.cs:135-208; FvgRule.cs:195 (۱۸ جا) | ToString F و direction.ToString() بدون Invariant | Invariant |
| L-78 | پایین | ثبت‌شده | BrokerStateRefreshRule.cs; CalculationReadinessRule.cs; EconomicNewsFeedStateRule.cs; EntrySignalTimingRule.cs | الگوی Kind==Utc?x:ToUniversalTime | H-17 |
| L-79 | پایین | ثبت‌شده | ExecutionFillAcceptanceRule.cs:32; ExecutionPlanGeometryRule.cs:67; HealthyVolatilityRule.cs:21 | کف‌های Max(0.10/0.50) روی پارامتر | M-110 |
| L-80 | پایین | ثبت‌شده | ClosedBarReferenceRule.cs:43-49 | آخرین کندل همیشه «زنده» حساب می‌شود (آخر هفته) | مستندسازی |
| L-81 | پایین | ثبت‌شده | LocationEvidenceRule.cs:24-88 | پرچم confluence با حضور واقعی FVG/OB اعتبارسنجی نمی‌شود | چک |
| L-82 | پایین | ثبت‌شده | EconomicNewsCurrencyRule.cs:29-33 | تطابق ارز با Contains روی نام نماد | دقیق‌تر |
| L-83 | پایین | ثبت‌شده | IndicatorEvidenceFusionRule.cs:158-205 | وزن‌ها و آستانه‌های ثابت | پارامتر |
| L-84 | پایین | ثبت‌شده | DailyLossBaselineRule.cs:19-24 | بازسازی baseline با پوزیشن باز ناممکن (وابسته به persistence) | مستندسازی |

### فاز 10 — 12 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-110 | متوسط | قطعی | Core/Math (۷۵ جا) | ۷۵ کف Math.Max/Min که پارامتر کاربر را بی‌صدا تغییر می‌دهند | لایهٔ «مقدار مؤثر» |
| M-111 | متوسط | قطعی | SmartBreakEvenRule.cs:48-81 | Smart BE وقتی TP1<1.33×trigger کاملاً غیرفعال می‌شود | هشدار/fallback |
| M-112 | متوسط | محتمل | TargetCandidateConstraintRule.cs:75; RiskRewardMathRule.cs; PlanRewardRiskQualityRule.cs | سه تعریف RR (اسمی/اسپرد/مؤثر) ⇒ رد دیرهنگام | یکسان‌سازی |
| L-85 | پایین | ثبت‌شده | SessionWindowRule.cs:10-277 | NormalizeHour 24→23؛ DST نادیده؛ IsInside بدون EnsureUtc | اصلاح |
| L-86 | پایین | ثبت‌شده | PendingFillExitResolutionRule.cs:68-132 | target دورتر انتخاب می‌شود؛ risk=1.0 ثابت | مستندسازی |
| L-87 | پایین | ثبت‌شده | ScenarioExecutionPolicyRule.cs:168 | expectedLane=candidate.Lane (چک همیشه true) | اصلاح |
| L-88 | پایین | ثبت‌شده | EntryTrapRiskRule.cs:175; PendingDecisionArbiterRule.cs:167 | ثابت NearExtremeLocationRisk برای معنای دیگر؛ Max(2,…) | اصلاح |
| L-89 | پایین | ثبت‌شده | OutcomeMemoryIdentityRule.cs:97-175 | fingerprint با culture جاری | Invariant |
| L-90 | پایین | ثبت‌شده | WaveTrendMovingAverageCalculator.BaseKernels.cs:67-113 | هستهٔ حالت‌دار؛ NaN بدون لاگ پخش می‌شود | لاگ |
| L-91 | پایین | ثبت‌شده | StructuralStopRiskRule.cs:57 | مخرج Max(Max(0,pipSize),atr) نامتعارف | بازنگری |
| L-92 | پایین | ثبت‌شده | PartialTakeProfitRetryRule.cs:8-28 | فقط یک retry به‌ازای هر کندل M5 | retry کوتاه‌تر |
| L-93 | پایین | ثبت‌شده | StructuralStopGeometryRule.cs:234-252 | سومین کپی NormalizePrice | استخراج |

### فاز 11 — 12 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-113 | متوسط | قطعی | Parameters/03,04,07,11 | بازهٔ UI پارامتر پایین‌تر از کف سخت‌کد (MinimumTriggerBodyAtr و ...) | MinValue واقعی |
| M-114 | متوسط | محتمل | SignalEnvelopeCodec.cs:7-22 | NaN/Infinity ⇒ JsonException ⇒ BlockAutomaticEntry | AllowNamedFloatingPointLiterals |
| L-94 | پایین | قطعی | ManagementCommand.cs; LifecycleEvent.cs; ContractEnums.cs | چهار نوع Contract بدون مرجع | حذف |
| L-95 | پایین | ثبت‌شده | OpportunityLane (Core) vs ContractEnums.cs | enum هم‌نام دوگانه (OpportunityLane، ExecutionAction/Kind) | منبع واحد |
| L-96 | پایین | قطعی | AggressiveEntryPolicy.cs | ۸۴ خط کد مرده | حذف |
| L-97 | پایین | قطعی | TimeWindowParser.cs; TextUtilities.cs | بدون مرجع | حذف |
| L-98 | پایین | ثبت‌شده | SubmissionGate.cs:17-63 | موفقیت کلید را حذف می‌کند؛ identity==null NRE | گارد |
| L-99 | پایین | قطعی | State.cs:115 | _lastAutoTradeAttemptUtc بدون مرجع | حذف |
| L-100 | پایین | ثبت‌شده | State.cs:309-310; UI/* | نام اشیای چارت بدون InstanceId | InstanceId |
| L-101 | پایین | ثبت‌شده | Parameters/13_auto_trading.cs | MaximumOpenPositions با Min=Max=1 | ثابت کردن |
| L-102 | پایین | ثبت‌شده | Parameters | ۴ پارامتر عددی بدون Min/Max | افزودن |
| L-103 | پایین | ثبت‌شده | State.cs:34,50 | _native و _waveTrendEngines پاک نمی‌شوند | Clear |

### فاز 12 — 7 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-115 | متوسط | قطعی | ExecutionControlsFactory.cs; ExecutionControlPresentationRule.cs:5 | کلید AUTO TRADE/ORDERS فقط نمایشی است و روی پرچمی می‌نشیند که gate نیست | اصلاح H-28 یا برچسب |
| M-116 | متوسط | محتمل | ChartObjectCleanup.cs:17-35; RuntimeInitialization.cs:540 | پاک‌سازی ناقص: ParallelOpportunity و PRIMARY_* باقی می‌مانند | اضافه به cleanup |
| L-104 | پایین | ثبت‌شده | PanelVisibility.cs:86 | _lastReactionAlertBar=-1 با toggle پنل ⇒ هشدار تکراری | ریست نشود |
| L-105 | پایین | ثبت‌شده | Panel/Rows/* (۲۲ جا) | ToString F بدون Invariant | Invariant |
| L-106 | پایین | ثبت‌شده | RuntimeInitialization.cs:520-533 | لغو اشتراک‌ها همه در یک try | try جدا |
| L-107 | پایین | ثبت‌شده | SessionPresentation.cs:20-26 | پایان ۲۴ به ۲۳ می‌رود | L-85 |
| L-108 | پایین | ثبت‌شده | AlertDeliveryProcessor.cs:23-28 | هر هشدار popup قبلی را بلافاصله پاک می‌کند | صف نمایش |

### فاز 13 — 8 مورد

| شناسه | شدت | اطمینان | محل | شرح ایراد | راه‌حل پیشنهادی |
|---|---|---|---|---|---|
| M-117 | متوسط | قطعی | tools/audit_*.py; tools/CFIP.*.Contracts | audits فقط حضور رشته؛ تست رفتاری برای ریسک‌های مهم صفر | ۵ تست رفتاری |
| M-118 | متوسط | نیاز به تست | PortableMemorySnapshotStore.cs:17-30; EconomicNewsCalendarClient.cs:304 | رفتار AccessRights.None (فایل و HTTP) فقط از کد دیده شده | تست زنده روی دمو |
| L-109 | پایین | نیاز به تست | global.json; همهٔ csproj | net6.0 خارج از پشتیبانی؛ oss-benchmark با .NET 8 | وابسته به cTrader |
| L-110 | پایین | ثبت‌شده | csproj ها | نسخهٔ cTrader.Automate در ۳-۴ جا تکرار | Directory.Packages.props |
| L-111 | پایین | ثبت‌شده | Directory.Build.props | بدون analyzer؛ TreatWarningsAsErrors=false | CA1305 |
| L-112 | پایین | ثبت‌شده | CFIP.Indicator.sln | preflight و تست‌ها در sln نیستند | افزودن |
| L-113 | پایین | ثبت‌شده | 28_news_guard.cs:10; EconomicNewsCalendarClient.cs:422-440 | منبع خبر یگانه و بدون سقف اندازهٔ بدنه | سقف |
| L-114 | پایین | ثبت‌شده | ci-build.yml | csproj اصلی مستقیم build نمی‌شود (آینهٔ CI) | build مستقیم |

## 11. Revalidation protocol for future remediation
هر finding قبل از اصلاح باید در current `main` با بررسی owner/caller/consumer/contract/test/runtime/downstream revalidated شود. نتیجه باید برای همان ID در نقشه‌راه/گیت پروژه ثبت شود و سپس با build/CI/runtime gate مربوطه تأیید گردد.

## 12. Integrity note
این فایل intentionally به‌عنوان historical register نگهداری می‌شود. حذف finding فقط با دلیل مستند و ثبت وضعیت (resolved/rejected/obsolete/needs revalidation) مجاز است؛ در غیر این صورت ID باید در این رجیستر باقی بماند.
