# CFIP — Master Continuation Prompt
## CFIP-PROMPT.md
### Canonical prompt for continuation in any chat/account
### Edition: 2026-10-04

> This file is the reusable bootstrap prompt for CFIP.
> It does not replace the three canonical control files.
> It instructs a new assistant to use them correctly.

---

ادامه پروژه CFIP-indicator را فقط از وضعیت واقعی GitHub شروع کن.

Repository:
armanemp/cfip-indicator

# CONTROL PLANE — سه فایل قطعی پروژه

از این لحظه فقط این سه فایل مرجع canonical پروژه هستند و باید همیشه هر سه بررسی شوند:

1. docs/CFIP-ROADMAP.md
2. docs/CFIP-LIST.md
3. docs/CFIP_GATE.md

نقش هرکدام:

- CFIP-ROADMAP.md = ترتیب، macro phaseها، هدف و معماری کلان
- CFIP-LIST.md = inventory دقیق repository، file-by-file scope و atomic work packages
- CFIP_GATE.md = acceptance، evidence، defect registry، regression و PASS/BLOCKED

هیچ roadmap قدیمی، phase document قدیمی، review، issue، screenshot، memory یا conversation history حق override کردن این سه فایل را ندارد.

docs/WORKFLOW.md فقط workflow اجرایی است و نباید authority جدیدی ایجاد کند.

# مهم‌ترین قانون اجرایی

One Concept → One Owner → One Source of Truth → Many Read-Only Consumers

برای اصلاح هر رفتار موجود:

Trace → Identify Owner → Fix Owner → Migrate Consumers → Remove Duplicate Path → Verify

ممنوع است که برای یک رفتار موجود:
- منطق دوم ایجاد شود؛
- calculation دوم ایجاد شود؛
- state دوم ایجاد شود؛
- renderer دوم ایجاد شود؛
- audio path دوم ایجاد شود؛
- alert path دوم ایجاد شود؛
- execution engine دوم ایجاد شود؛
- threshold دوم ایجاد شود؛
- fallback متناقض ایجاد شود؛
- parameter جدید برای پنهان کردن defect اضافه شود؛
- workaround موقت وارد production شود.

اگر owner فعلی غلط است، همان owner را ریشه‌ای اصلاح کن.

# قانون اجرای هر پیام

هر response دقیقاً یک Atomic Work Package کامل است.

Atomic packageها در docs/CFIP-LIST.md تعریف شده‌اند.

در حال حاضر:

Macro Phase = P2 — Ownership / Single-Source / Dead-Code Closure
Atomic Work Package = WP-05 — Preflight
Status = PASS + TERMINAL PENDING

WP-06 — Contracts is BLOCKED pending real target-terminal evidence.

هر package باید در یک response کامل شود.

به هیچ عنوان این موارد قابل قبول نیستند:
- نصف phase؛
- نصف package؛
- فقط compile؛
- فقط یک فایل؛
- «بعداً تست می‌شود»؛
- «بعداً callerها بررسی می‌شوند»؛
- «باقی مانده در پیام بعد»؛
- workaround موقت.

اگر package بزرگ است، قبل از implementation آن را به packageهای کوچک‌تر تقسیم کن و scope جدید را در canonical files ثبت کن.

# شروع اجباری

قبل از هر تغییر:

1. docs/CFIP-ROADMAP.md را بخوان.
2. docs/CFIP-LIST.md را بخوان.
3. docs/CFIP_GATE.md را بخوان.
4. actual main فعلی GitHub را inspect کن.
5. exact HEAD را ثبت کن.
6. latest CI را بررسی کن.
7. وضعیت فعلی repository را با سه فایل canonical مقایسه کن.
8. اولین executable package با وضعیت NEXT را پیدا کن.
9. کل repository tree را برای orphan/dead/generated/temporary/duplicate artifacts بررسی کن و هر مورد را KEEP/MIGRATE/ARCHIVE/DELETE طبقه‌بندی کن.
10. هیچ DELETE/ARCHIVE تصمیمی را بدون dependency trace و بررسی build/CI/test/audit/runtime/release انجام نده.
11. هیچ فرض قدیمی را صرفاً به خاطر conversation memory معتبر ندان.

# هدف WP-00

WP-00 باید baseline واقعی پروژه را از صفر مشخص کند:

- actual main HEAD؛
- repository tree؛
- تعداد فایل‌ها و دایرکتوری‌ها؛
- solution/project graph؛
- ProjectReference؛
- PackageReference؛
- TargetFramework؛
- source inclusion؛
- orphan/generated artifacts؛
- Release/Debug behavior؛
- latest CI؛
- compiler warnings؛
- current parameter inventory؛
- MTF inventory؛
- M2 scan؛
- current owner map؛
- duplicate/parallel-path map؛
- broker mutation map؛
- Indicator/cBot boundary؛
- visual ownership؛
- alert/audio ownership؛
- persistence/lifecycle ownership؛
- concurrency/re-entrancy risks؛
- active audit infrastructure؛
- historical documentation dependencies؛
- terminal-only validation boundaries؛
- repository-wide orphan/dead/generated/temporary/duplicate artifact inventory؛
- current OPEN/BLOCKED defects.

WP-00 باید evidence واقعی تولید کند و:
CFIP-ROADMAP.md
CFIP-GATE.md
CFIP-LIST.md
را با baseline واقعی sync کند.

# قانون file-by-file

هیچ فایل مرتبطی را از روی اسم آن نادیده نگیر.

برای هر file:

Directory → File → Declaration → Method → Branch → Dependency → Side Effect → Lifecycle → Verification

بررسی کن.

برای هر code-bearing file موارد زیر را بررسی کن:

## Compilation/API
- using;
- namespace;
- nullable;
- accessibility;
- overload;
- generic types;
- async/task;
- platform APIs;
- warnings;
- dead/unreachable code.

## Ownership
- دقیقاً چه مفهومی را owner است؟
- آیا owner دوم وجود دارد؟
- آیا consumer در حال recompute کردن است؟
- آیا helper تبدیل به semantic owner دوم شده؟
- آیا partial class منطق پنهان دارد؟

## State
- fields;
- properties;
- static state;
- collections;
- caches;
- initialization;
- reset;
- invalidation;
- stale state;
- lifetime.

## Inputs
برای هر input:

Source → Validation → Normalization → Use → Output → Lifetime

## Control Flow
همه این‌ها را بررسی کن:
- if/else؛
- switch؛
- early return؛
- fallback؛
- retry؛
- exception؛
- null branch؛
- timeout؛
- state transition؛
- loop bound؛
- ordering assumptions.

فقط happy path کافی نیست.

## Time / Market
هرجا مربوط است:
- bar index؛
- closed/open bar؛
- MTF؛
- UTC؛
- DST؛
- server time؛
- signal time؛
- quote observation time؛
- Bid/Ask؛
- executable side؛
- spread؛
- pip/tick/digits؛
- broker distance؛
- stale data؛
- history replacement؛
- gaps.

## Numerical
- zero؛
- negative؛
- NaN؛
- Infinity؛
- divide-by-zero؛
- overflow/underflow؛
- rounding؛
- normalization؛
- hidden constants؛
- tolerance.

## Communication
برای هر public/cross-file item:

Producer → Transformer → Consumer

و برعکس:

Consumer → Prerequisite → Owner

همه callerها، consumerها، publisher/subscriberها و contract pathها باید trace شوند.

## Identity
- SignalId
- ScenarioId
- ExecutionId
- Broker identity
- Lifecycle identity
- Outcome identity
- Revision/version
- Idempotency

بررسی کن retry یا recovery باعث identity collision نشود.

## Side Effects
تمام این موارد را پیدا کن:
- broker mutation؛
- chart objects؛
- UI mutation؛
- sound؛
- popup؛
- email؛
- file I/O؛
- persistence؛
- queues؛
- network/cloud.

برای هرکدام owner مشخص کن.

## Error / Fault
هر failure باید مشخص کند:
- recoverable یا fatal؟
- owner کجاست؟
- retry چیست؟
- backoff چیست؟
- آیا entry باید block شود؟
- management باید ادامه دهد؟
- stale state ممکن است؟
- duplicate action ممکن است؟
- evidence/log وجود دارد؟

## Concurrency
بررسی کن:
- Calculate re-entry؛
- timer overlap؛
- UI callback overlap؛
- startup race؛
- reconnect race؛
- duplicate subscription؛
- shared mutable state؛
- duplicate scenario submission؛
- atomicity/idempotency.

## Performance
- repeated loops؛
- full-history scans؛
- allocations؛
- redundant calculations؛
- cache churn؛
- chart object churn؛
- timer frequency؛
- UI update frequency؛
- queue growth؛
- memory retention؛
- synchronous I/O.

## Security/configuration
- secrets؛
- AccessRights؛
- local paths؛
- environment dependencies؛
- config precedence؛
- demo/live behavior؛
- package provenance؛
- license؛
- sensitive logs.

# قوانین CFIP

## Timeframes

فقط:

M1 / M5 / M15 / M30 / H1 / H4 / D1 / W1

نقش:
- M15 = canonical decision/reference
- M5 = trigger/entry precision
- M1 = optional confirmation only
- higher timeframes = context
- M2 = ممنوع مطلق

M2 نباید در provider / Bars request / enum / cache / parameter / panel / contract / signal / fallback / execution clock وجود داشته باشد.

## Execution
- Indicator = analysis
- Contracts = canonical boundary
- cBot = sole broker mutation authority

Indicator نباید broker را mutate کند.

cBot نباید analysis/decision/OB/FVG/structure/regime را دوباره بسازد.

## Broker truth

Requested/Planned/Desired state با Broker-confirmed state یکسان نیست.

بعد از mutation:

Broker-confirmed state = authority

## Protection

Protection فقط باید risk را کاهش دهد.

هیچ mutationای نباید بی‌دلیل SL/risk را بدتر کند.

## Visual

یک signal = یک visual set.

Strength فقط یک owner دارد:
- 1–3 Weak
- 4–6 Medium
- 7–9 Strong

M1 marker جهت‌دار نیست.

Plan lines:
- Solid
- 1px
- finite 40-bar geometry

Labels:
- canonical owner: `PlanLabelRenderer`;
- exact price;
- readable regular-weight native ChartText;
- canonical anchor owner: `PlanLabelAnchorCalculator`;
- visible right edge of the complete right-aligned text exactly one chart bar before the canonical line start;
- label text must use the exact materialized color of its corresponding signal line;
- `Color.White` is forbidden for canonical signal/plan line labels;
- pending, parallel and prediction labels must reuse the same renderer/anchor contract;
- no secondary label geometry or alternate label renderer.

## Alerts

یک causal event = یک delivery lifecycle.

Startup sound باید یک owner داشته باشد.

Retry نباید duplicate causal event بسازد.

Blocked/restricted candidate نباید actionable side effect ایجاد کند.

# Full-chain rule

هر package باید chain مربوط به behavior را بررسی کند:

Market Observation → Time → Evidence → Decision → Actionability → Signal → Alert → Execution Intent → Preflight → Broker Request → Broker Confirmation → Protection → Lifecycle → Outcome → Presentation

اگر یک link شکسته باشد، package PASS نیست.

# Definition of Done

Package فقط وقتی PASS است که:
- root cause evidence-based باشد؛
- owner canonical مشخص باشد؛
- callerها بررسی شده باشند؛
- consumerها بررسی شده باشند؛
- duplicate paths حذف شده باشند؛
- fallback متناقض باقی نمانده باشد؛
- boundary/error cases بررسی شده باشند؛
- BUY/SELL symmetry بررسی شده باشد؛
- time/index semantics بررسی شده باشد؛
- parameters بررسی شده باشند؛
- focused tests PASS باشند؛
- relevant static audits PASS باشند؛
- whole-project audit PASS باشد؛
- Release build PASS باشد؛
- runtime contracts PASS باشند؛
- performance review انجام شده باشد؛
- safety review انجام شده باشد؛
- required terminal acceptance مشخص باشد؛
- CFIP-GATE.md update شده باشد؛
- CFIP-LIST.md scope/status update شده باشد؛
- CFIP-ROADMAP.md next state update شده باشد؛
- یک و فقط یک package بعدی NEXT باشد.

# Regression rule

هر PASS در صورت تغییر owner، consumer، contract، event path، visual path، execution path یا broker behavior باید در صورت نیاز REOPEN شود.

PASS را برای حفظ ظاهری وضعیت با ضعیف‌کردن gate حفظ نکن.

# Historical-document and repository-cleanup rule

فایل‌های قدیمی repository را فقط به‌عنوان evidence/archive ببین، و هر artifact قدیمی/زائد را تا تعیین تکلیف رها نکن.

هر فایل tracked باید یکی از این وضعیت‌ها را داشته باشد:
**KEEP / MIGRATE / ARCHIVE / DELETE**.

برای DELETE/ARCHIVE:
- dependency chain را تا active consumer trace کن؛
- build/CI/audit/test/runtime/release/deployment/continuity را بررسی کن؛
- اگر دانش معتبر دارد، آن را قبل از حذف به canonical owner منتقل کن؛
- artifact obsolete را فقط به خاطر ارجاع دیگر obsolete artifact حفظ نکن.

هدف نهایی repository:
**zero unexplained files + zero duplicate artifacts + zero obsolete active dependencies**.

به‌خصوص:
- old M0–M45;
- old CR/CI sequence;
- old cBot sequencing;
- old hotfix plans;
- superseded visual contracts.

هیچ‌کدام ترتیب فعلی پروژه را تعیین نمی‌کنند.

# Documentation migration

اگر tooling هنوز به docs/CFIP-ROADMAP.md قدیمی وابسته است:

اول dependencyها را کامل trace کن،
سپس owner canonical را به سه فایل جدید migrate کن،
سپس dependency قدیمی را حذف کن،
و فقط بعد از اثبات بی‌نیازی roadmap قدیمی را archive/delete کن.

هرگز فقط برای سبزشدن CI یک workaround ایجاد نکن.

# قانون اجباری گزارش پیشرفت

در پایان هر Atomic Work Package، علاوه بر وضعیت فنی، باید همیشه صریحاً گزارش شود: تعداد کل packageها و تعداد packageهای کاملاً PASS، درصد پیشرفت کل، درصد پیشرفت package جاری، packageهای باقی‌مانده، سناریوهای Terminal/Manual باقی‌مانده، blockerها/defectهای باز و package بعدی قابل اجرا. اگر package از نظر repository کامل ولی از نظر Terminal ناقص است، این دو درصد جداگانه گزارش شوند و package fully PASS شمارش نشود.

# خروجی اجباری هر package

در پایان هر response باید صریحاً گزارش کنی:

- Current HEAD
- Macro Phase
- Atomic Work Package
- Root Cause
- Canonical Owner
- Files inspected
- Lines/regions inspected
- Callers/Consumers audited
- Duplicate paths removed
- Tests
- Static audits
- Runtime verification
- Build
- Performance
- Safety
- Residual risks
- Gate status
- Terminal status
- Commit/PR
- Next atomic package
- Operator pull instruction

و سپس واقعاً state سه فایل canonical را update کن.

# دستور فعلی

از:

P0 → WP-00

شروع کن.

اول baseline واقعی GitHub را بساز.

روی هیچ phase قدیمی کار نکن.

هیچ چیز را با فرضیات گفتگوهای قبلی معتبر ندان.

یک package کامل را از شروع تا closeout انجام بده.

**No partial work. No parallel logic. No second owner. No temporary workaround. No skipped files.**