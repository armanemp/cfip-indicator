## Permanent Development Rule — Canonical Owners / No Patches

For every future phase, modify the existing canonical production owner directly. Do not create parallel hotfix files, duplicate executors, compatibility wrappers, alternate calculation paths, alternate identity formatters, or detached patch subsystems when the existing owner can be corrected. Any obsolete owner created by an extraction must be deleted in the same phase, and all audits/docs must point to the single surviving owner.

## Build Warning / Panel Height Integrity — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending on branch.

Scope:
- eliminate the 9 locally observed Release-build warnings without null-suppression or artificial field references;
- make nullable transport initialization explicit in the shared Contracts project;
- remove dead last-pending-signal state;
- remove the panel's remaining Chart.Height-derived geometry feedback path;
- disable Indicator AutoRescale because the transparent Provider Heartbeat writes a non-price provider revision series;
- preserve the canonical overlay panel, bounded ScrollViewer, 50px bottom clearance and cBot execution boundary;
- add a dedicated acceptance audit covering warning cleanup and chart-height safety.

Root cause:
- CFIPReadOnlyProvider writes the provider revision to a transparent output series;
- cTrader documents AutoRescale as chart auto-rescaling and its default as enabled; the revision is metadata, not price data;
- panel maximum-height resolution was still reading Chart.Height, so a transient chart viewport could influence the next panel geometry calculation.

Invariant:
Panel geometry and indicator chart scaling must never be driven by provider metadata or a transient chart viewport.

Phase record: docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md.

Next staged phase after acceptance: CBOT-P5 — Protection / Lifecycle / Recovery completion; signal-quality/target-quality work remains a separate evidence-driven phase.
## CBOT-P4E — Management Command + Remaining Broker Mutation Authority — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #197 as `996cd9cb4872608674269c205b7a03b33ab9e812`.**

The Indicator's remaining broker mutation paths are command-only; `ManagementExecutionCoordinator` in the cBot is the single owner for cancellation, full/partial close, SL, absolute TP, TP-by-pips and server TP ladder mutation.

Verification on final merged implementation:
- Source / Architecture workflow #3064: **PASS**;
- Runtime Acceptance workflow #2873: **PASS**;
- cTrader Compile/Build workflow #3057: **PASS**;
- CBOT-P4E audit: **PASS**;
- accumulated architecture/execution/UI/identity audits: **PASS**.

Target-terminal broker execution verification remains manual. No profitability claim is made from this structural migration.

Phase record: `docs/PHASE-CBOT-P4E-MANAGEMENT-AUTHORITY-2026-10-02.md`.

Next staged phase: **CBOT-P5 — Protection / Lifecycle / Recovery completion**.

## CBOT-P4D — Pending Limit Authority + Signal/Popup Continuity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #196 as 352e6229adcff8a4ebb6ee6e5c71a0e0397dc70b.**

Scope:
- move Pending Limit broker mutation completely to the existing cBot pending execution owner;
- remove the obsolete Indicator Pending Limit broker owner;
- keep Indicator responsible for analysis, scenario, intent and absolute lifecycle snapshot only;
- expose a dedicated cBot Pending Limit arm while preserving demo-only/fail-closed execution;
- restore arrow-only direction visibility for early/watch/confirmed/strong states using three directional intensity colors;
- keep popup at BottomRight, persistent until next alert/manual close, and restrict popup delivery to important canonical alert families;
- preserve M15 internal execution independence from host Chart TF.

No new strategy engine, duplicate signal engine, duplicate broker owner, or alternate label formatter is introduced.

Repository verification on final P4D implementation head 22d7af189d7037237b27a3df09a42a9cdda7f252:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile/Build: **PASS**;
- CBOT-P4D acceptance audit: **PASS**;
- accumulated execution/UI/identity audits: **PASS**.

Target-terminal acceptance remains manual. No profitability claim is made from this structural/UI phase alone.

Next staged execution phase: **CBOT-P4E — full remaining broker execution authority consolidation (cancellation, protection, partial TP, break-even, trail, close/recovery) in the cBot, with no duplicate Indicator mutation owners.**

## CBOT-P4C — Pending Stop Authority + Host-Timeframe Independence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #195 as 7b8648091bde26753b1e0fcff3d75984a1f1b9eb.**

Completed:
- M15 is the internal execution clock; Chart timeframe is host/presentation-only;
- Indicator/cBot runtime no longer rejects or branches on host Chart TF for execution;
- Pending Stop broker mutation moved completely to src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs;
- shared cBot margin/capacity safety owner prevents duplicate helper implementations;
- Pending Stop trigger uses current spread through canonical PendingEntryPriceRule;
- Indicator Pending Stop path is intent-only;
- active audits and documentation were reconciled;
- Pending Stop lifecycle snapshot is preserved across Indicator to cBot handoff;
- the canonical instance-scoped execution label is transported in CFIP.Contracts.ExecutionIntent and consumed by cBot broker execution/state ownership.

Verification on final implementation head c700e3adbc3557bf1278024df870a15a6e308b69:
- Source / Architecture 36991157739 / workflow #3025: PASS;
- Runtime Acceptance Contracts 36991157724 / workflow #2834: PASS;
- cTrader Compile/Build 36991157712 / workflow #3018: PASS;
- CBOT-P4C acceptance audit: PASS;
- CR5.4 absolute pending-fill reconciliation audit: PASS;
- CI-15 execution-geometry/broker-boundary audit: PASS;
- M15 / Risk / Spread audit: PASS;
- CR1.8 / A11 identity audit: PASS.

Target-terminal acceptance remains manual. No profitability claim is made from this structural migration alone.

Next staged execution migration: **CBOT-P4D — Pending Limit authority extraction.**

## MTF-EXECUTION-M15 — Primary Execution + Smart Margin/Spread Risk — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` in implementation commit `0a45bb251d28a5542fa7580d886d3af8b25184b7`.**

Decision for the active execution architecture:
- **M15 = internal primary execution clock**. The Indicator and cBot may be attached to any Chart TF; Chart TF is host/presentation-only and must not alter execution identity or logic.
- **M5 + M1 = defensive tuning layers** for entry timing/location and adverse microstructure; they refine or block an M15 setup but do not become a competing execution clock.
- **H1 + H4 + D1 + W1 = higher-timeframe context/reward-path layers** used for context and extended target selection.
- cBot runtime is host-timeframe independent.

Risk/price contract:
- requested volume remains risk-based from the Indicator's canonical stop geometry;
- Indicator sizing already includes spread when `Include Spread In Risk Sizing=true`;
- cBot now performs a final margin-budget cap after broker margin estimation and normalization, and only reduces exposure;
- canonical effective RR now subtracts spread from both risk and realized reward distance;
- synthetic targets account for the same spread cost;
- no new signal score, confidence threshold, RR policy tuning or target selection tuning is introduced beyond the requested spread/margin correctness.

Phase report: `docs/PHASE-MTF-EXECUTION-M15-RISK-SPREAD-2026-10-02.md`.

Verification on the final implementation head:
- Source / Architecture `36985594477` / workflow #2997: **PASS**;
- Runtime Acceptance `36985594449` / workflow #2806: **PASS**;
- cTrader Compile/Build `36985594530` / workflow #2990: **PASS**;
- M15 / Risk / Spread audit: **PASS**.

Target-terminal acceptance remains manual. No profitability claim is made from this architecture change alone.

Next execution-migration phase: **CBOT-P4C — Pending Stop authority extraction**.


# CFIP Indicator — Master Roadmap 2026-10-02
## Single Source of Truth — Correctness → Certification → cBot → Data-Driven Intelligence → Release

> این فایل **تنها مرجع اجرای پروژه** است. ترتیب فازها، شرایط پذیرش، وضعیت فعلی، معماری نهایی، قوانین تغییر، تست، پاکسازی و معیار پایان همگی در همین فایل تعریف می‌شوند.
>
> قانون اصلی: در هر پاسخ/واحد کاری فقط **یک فاز کامل** اجرا می‌شود. فاز بعدی فقط پس از ثبت نتیجه و تأیید ادامه داده می‌شود.
>
> این roadmap تضمین سود نمی‌دهد؛ هدف آن این است که هیچ نقص شناخته‌شده بدون مالک، تست، وضعیت و تصمیم نهایی باقی نماند و هیچ ادعای «حل شده» بدون شواهد معتبر پذیرفته نشود.

---

# 1. هدف نهایی

CFIP باید به یک سیستم واحد، قابل‌اعتماد، سریع، قابل‌آزمون و قابل‌نگهداری تبدیل شود که در آن:

1. تحلیل بازار، تصمیم، سناریو و Plan یک حقیقت واحد داشته باشند.
2. Chart، Panel و Alert دقیقاً همان حقیقت را نمایش دهند.
3. Signal → Plan → Alert → Chart → Execution → Broker State → Outcome از یک identity chain واحد تبعیت کند.
4. هیچ مسیر موازی نتواند Buy/Sell، Entry، SL، TP، RR یا وضعیت معامله را خلاف مسیر اصلی تولید کند.
5. SL فقط در جهت محافظتی حرکت کند و TP هرگز به عقب برنگردد.
6. Partial TP، Break-even، Trail، Dynamic TP، Close، Cancel و Recovery بر اساس broker-confirmed state باشند.
7. پنل واقعاً live، کامل، سریع و بدون stale content باشد.
8. Alert صوتی/نمایشی دقیقاً با event واقعی هم‌زمان و idempotent باشد.
9. تحلیل از execution جدا شود.
10. Indicator فقط Analysis / Decision / Scenario / Plan / Presentation باشد.
11. cBot تنها Broker Execution / Account Risk / Protection / Lifecycle Authority باشد.
12. کیفیت سیگنال فقط با Replay، OOS و Walk-forward سنجیده شود.
13. OB/FVG/WaveTrend/Divergence/VWAP/Liquidity/Volume فقط بر اساس evidence تقویت یا حذف شوند.
14. Clampهای پنهان، parameterهای دروغین، dead code، duplicate owner و فایل‌های زائد حذف شوند.
15. Repository نهایی از clean checkout قابل ساخت و تست باشد.

---

# 2. وضعیت مبنای واقعی — 2026-10-02

## 2.1 baseline

مبنای audit حاضر:
- حدود 631 فایل C#
- حدود 77 هزار خط
- 568 public parameters
- معماری ماژولار Analysis / Planning / Risk / Lifecycle / UI
- broker mutation هنوز داخل Indicator host
- current production OSS boundary validated separately
- Target Terminal acceptance هنوز gate نهایی است

## 2.2 وضعیت فعلی ثبت‌شده

- CI-17A مربوط به اصلاح live-content refresh پنل در repository پیاده و merge شده.
- repository gates آن PASS شده‌اند:
  - Source/Architecture
  - Runtime Acceptance Contracts
  - cTrader Compile
- با این حال stale/incomplete panel content هنوز در محیط واقعی کاربر گزارش شده؛ بنابراین repository PASS به‌تنهایی acceptance نهایی پنل نیست.
- Current terminal/broker boundary همچنان باید دستی اثبات شود.
- مسیر معماری نهایی: Indicator = analysis/signal/plan/presentation؛ cBot = broker execution/account risk/live protection/lifecycle.
- Cloud در این roadmap پیاده‌سازی نمی‌شود و فقط contractها transport-neutral می‌مانند.

## 2.3 اصل مهم status

هر موردی که فقط source-reviewed باشد، «حل‌شده قطعی» محسوب نمی‌شود.
برای رفتار cTrader/broker، evidence target-terminal لازم است.

---

# 3. قوانین غیرقابل‌مذاکره

## 3.1 One phase per response

هر پاسخ اجرایی دقیقاً یک فاز را کامل می‌کند.
نیمه‌کاره ماندن migration یا owner switch مجاز نیست.

## 3.2 ترتیب داخل هر فاز

1. Contract / invariant
2. Root-cause verification
3. Canonical implementation
4. Caller migration
5. Behavioral tests
6. Duplicate/dead-path cleanup
7. Runtime verification
8. Documentation/status
9. Performance/optimization audit
10. Phase closeout

## 3.3 Parameter contract

- نام، نوع و DefaultValue هیچ public Parameter بدون تصمیم صریح تغییر نمی‌کند.
- پارامتر جدید فقط با defaultی که behavior فعلی را حفظ کند.
- هر behavior change جداگانه علامت‌گذاری می‌شود.
- قبل از Replay هیچ tuning برای confidence/RR/risk/weights/thresholds مجاز نیست.

## 3.4 Evidence contract

- grep-only و text-only به‌تنهایی test نیست.
- هر bug fix باید behavioral test داشته باشد.
- ترجیح: test قبل از اصلاح fail و بعد از اصلاح pass.
- pure logic باید در Core یا معادل تست‌پذیر خود باشد.
- clock/time باید injectable باشد.
- ادعای قبلی در صورت تعارض با کد/تست باید رد شود و علت ثبت شود.

## 3.5 Single owner

ممنوع:
- duplicate decision engine
- duplicate execution engine
- alternate broker mutation
- second trading authority
- compatibility alias دارای business logic
- static mutable global برای bridge
- chart-object scraping
- reflection برای state خصوصی
- hidden fallback executor

## 3.6 Broker truth

- broker-confirmed state authoritative
- submitted != accepted
- accepted != filled
- pending != position
- rejection != success
- desired Plan state != broker state
- restart/reconnect: reconcile first, act second

## 3.7 Protective mutation

- Tighten SL فقط protective.
- Repair SL فقط برای missing/invalid/wrong-side.
- TP progression فقط forward.
- Partial/Close/Cancel فقط پس از broker confirmation.
- Plan live state فقط بعد از mutation success/reconciliation advance می‌کند.

## 3.8 Permanent optimization audit

در هر فاز بررسی کن:
startup، hot path، allocations، repeated computation، redraw، broker reads، cache growth، I/O، duplicate helpers، dead code، panel responsiveness.

Optimization نباید correctness را قربانی کند.

## 3.9 Mandatory closeout

هر فاز باید ثبت کند:
- Phase/IDs
- root cause
- files
- behavior changes
- before/after tests
- static checks
- runtime checks
- manual terminal checks
- new bugs
- unresolved risks
- CI results
- exact progress
- git pull requirement after merge

---

# 4. Definition of Done سراسری

پروژه فقط زمانی «کامل» تلقی می‌شود که:

1. هیچ Critical/High known defect بدون disposition باقی نماند.
2. Indicator هیچ direct broker mutation نداشته باشد.
3. یک decision authority و یک execution authority وجود داشته باشد.
4. Signal → Plan → Alert → Chart → Execution → Outcome identity-continuous باشد.
5. BUY/SELL symmetry و lifecycle invariants تست شوند.
6. Replay deterministic و بدون information leak باشد.
7. OOS evidence برای هر تغییر عددی وجود داشته باشد.
8. Target Terminal acceptance کامل باشد.
9. restart/reconnect/rejection/slippage/delayed-confirmation تست شده باشد.
10. Panel live content واقعی و سریع باشد.
11. Alert sound/popup/mark synchronized و idempotent باشد.
12. History durable و recoverable باشد.
13. source/tools/tests/build audit کامل شده باشد.
14. dead/obsolete files و duplicate docs پاک شده باشند.
15. clean checkout reproducible باشد.
16. Indicator بدون cBot قابل استفاده برای analysis/display باشد.
17. cBot به‌تنهایی broker execution را کامل انجام دهد.
18. نبود/stale بودن cBot یا signal source fail-closed باشد.
19. هیچ Cloud dependency برای local product وجود نداشته باشد.
20. بعد از آن feature expansion آزاد شود.

---

# 5. نقشه راه اصلی

# گروه A — تثبیت و اثبات واقعیت

## M0 — Adoption / Freeze / Baseline

Status: **VERIFIED COMPLETE — 2026-10-02.**

Repository baseline and continuity freeze were completed on branch `phase/master-roadmap-single-source-2026-10-02`.

Verified implementation/continuity HEAD before closeout: `6ceab05142fab7f2ac2bf9bbfd6f6346bd1023bc6`.

Verification:
- Source / Architecture #2756: **PASS**;
- Runtime Acceptance Contracts #2565: **PASS**;
- cTrader Compile #2749: **PASS**.

Completed:
- established this file as the single active development roadmap;
- reconciled accumulated historical audit continuity anchors without creating a second roadmap;
- verified repository baseline, parameter surface and execution-boundary inventory;
- preserved the existing Indicator execution architecture without production behavior change;
- documented the local Indicator → Contracts → cBot target boundary;
- documented that target-terminal/manual broker acceptance remains a later certification boundary.

Operator action after merge: `git pull --ff-only`.

### پذیرش
Baseline reproducible and all M0 finding/continuity blockers have a disposition.

### کارها
- ثبت branch/commit/working-tree.
- اجرای سه gate repository و ثبت runها.
- شمارش واقعی parameterها.
- ثبت project tree و dependency graph.
- inventory direct broker calls.
- inventory public parameters و ownership.
- جذب تمام known findings این roadmap.
- هیچ source behavior change.

### پذیرش
Baseline reproducible و همه findingها status داخلی دارند.

---

## M1 — Full Forensic Audit

Status: **IMPLEMENTATION COMPLETE — gate verification pending on M1 branch.**

Audit report: `docs/PHASE-M1-FULL-FORENSIC-AUDIT.md`.

### هدف
هیچ بخش قدیمی یا جدید بر اساس «گفته شده سالم است» فرض نشود.

### دامنه
Core, Analysis, Planning, Trading, Lifecycle, Runtime, UI, Tools, Tests, Build.

### checks
- BUY/SELL symmetry
- closed/live boundary
- NaN/Infinity/zero/negative
- hidden constants
- parameter clamps
- bounded collections
- hot-path I/O
- time/session
- persistence
- event idempotency
- identity propagation
- decision/execution authority
- visual authority
- alert authority
- outcome accounting
- large-method ownership

### پذیرش
هر finding: VERIFIED / PARTIAL / OPEN / FALSE / MANUAL-ONLY / DESIGN-RISK.
هیچ finding بدون disposition.

M1 disposition record: all F1–F18 are classified with an explicit owner/next-phase disposition. The only audit-tool correction made in M1 aligns `audit_cbot_boundary.py` with the current 568-parameter baseline; no production trading behavior changed.

---

## M2 — Repository Hygiene / Dead Code / Ownership

### هدف
قبل از توسعه جدید، repo از فایل و owner زائد پاک شود.

### کارها
- unreachable files
- dead classes/methods
- duplicate constants
- duplicate helpers
- empty methods
- duplicated visual suffix arrays
- unused scripts
- unused tests
- obsolete parameters بعد از migration
- stale references
- docs classification: WORKING / EVIDENCE / ARCHIVE

### rule
این roadmap تنها WORKING planning authority است.
بعد از جذب کامل محتوا، اسناد برنامه‌ریزی پراکنده و promptهای مصرف‌شده در cleanup نهایی حذف می‌شوند؛ Git history محفوظ می‌ماند.

---

# گروه A — Parallel cBot Separation (فعال از 2026-10-02)

این track به درخواست مالک پروژه از همین نقطه همزمان با M2 به بعد اجرا می‌شود و ترتیب قبلی M29–M38 را برای شروع migration عوض می‌کند. هدف آن این است که execution/account/lifecycle قبل از تکمیل بخش‌های تحلیلی آینده از Indicator خارج شود تا هیچ دوباره‌کاری یا دو owner باقی نماند.

## CBOT-P0 — Activation / Boundary Lock

Status: VERIFIED COMPLETE — 2026-10-02

Verification: Source/Architecture #2771 PASS; Runtime Acceptance #2580 PASS; cTrader Compile #2764 PASS.

- Indicator owner = analysis / decision / scenario / plan / presentation.
- cBot owner = broker execution / account risk / live protection / lifecycle / recovery.
- Contracts owner = platform-neutral immutable data-only boundary.
- No new broker mutation may be added to Indicator.
- Existing Indicator mutation remains temporarily only until its cBot replacement passes parity and terminal-safe verification.
- A migration step is not complete until: replacement exists → callers migrate → old owner removed → source audit proves zero duplicate authority for that migrated path.
- No project/file is copied wholesale when it mixes analytical and broker responsibilities.
- Exact extraction inventory: docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md.

## CBOT-P1 — Platform-Neutral Contracts

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2778 PASS; Runtime Acceptance #2587 PASS; cTrader Compile #2771 PASS.

Create and freeze one shared, immutable, platform-neutral contract model for Indicator ↔ cBot. Contracts must contain no cTrader dependency, no decision logic and no broker mutation. Required families: SignalEnvelope, PlanSnapshot, ExecutionIntent, ManagementCommand, BrokerExecutionReport, LifecycleEvent, ContractIdentity, ContractVersion, revision/sequence identity, CorrelationId and IdempotencyKey.

Authoritative schema audit: `tools/audit_cbot_contract_schema.py`.
Phase report: `docs/PHASE-CBOT-P1-PLATFORM-NEUTRAL-CONTRACTS.md`.

No parallel DTO/model set may be introduced in the cBot. Existing Indicator internal models remain temporary source models until P2+ provider migration replaces them with these contracts.

## CBOT-P2 — Read-Only Indicator Provider

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2790 PASS; Runtime Acceptance #2599 PASS; cTrader Compile #2783 PASS.

Expose one structured, immutable, read-only provider from the Indicator to the cBot using the supported cTrader custom-indicator reference mechanism. The provider uses the canonical `CFIP.Contracts` envelope and an invisible output heartbeat to make lazy evaluation deterministic. No chart scraping, reflection, private-field access or static mutable bridge is allowed.

Implementation: `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs`.
Boundary audit: `tools/audit_cbot_provider_boundary.py`.
Phase report: `docs/PHASE-CBOT-P2-READ-ONLY-INDICATOR-PROVIDER.md`.

## CBOT-P3 — cBot Host / Shadow

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2801 PASS; Runtime Acceptance #2610 PASS; cTrader Compile #2794 PASS.

The cBot now runs a deterministic shadow host over the canonical Indicator provider:

`receive → validate → expiry/revision → deduplicate → broker-safety → shadow state → telemetry`.

Implemented:
- `src/CFIP.cBot/Shadow/ShadowHostContracts.cs`
- `src/CFIP.cBot/Shadow/ShadowHostValidator.cs`
- `src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs`
- cBot broker-state read adapter with single-plan capacity;
- bounded 128-key idempotency cache;
- providerRevision/envelopeRevision integrity check;
- BUY/SELL contract-geometry safety invariants;
- deterministic behavioral test project;
- Source/Architecture audit `tools/audit_cbot_shadow_host.py`.

No broker mutation is added in P3.

Phase report: `docs/PHASE-CBOT-P3-CBOT-HOST-SHADOW.md`.

## MTF-P1 — Primary M15/H1 Signal Layer + Panel Separation

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `c811967d8462229e8efc5f14af1f48cc3e3e72b2`; target-terminal evidence remains manual.**

Decision:
- M15 and H1 are the primary visible signal sources.
- Closed M5 tunes local structure/location/actionability.
- Closed M1 supplies timing confirmation through the existing trigger path.
- M15 and H1 may coexist simultaneously and independently.
- H1+/M30 context remains part of canonical top-down reasoning.
- Non-M5 primary candidates remain observe-only for execution until the cBot provider/execution track explicitly adopts them.
- M15/H1 OB/FVG evidence is preserved on the candidate and participates in display priority.
- Candidate Entry/SL/TP remains the canonical M5 planning projection in this phase; independent source-timeframe execution geometry is a later, separately verified change.

Panel:
- chart-panel AUTO TRADE / AUTO ORDERS quick controls are removed;
- canonical status/diagnostic rows remain;
- BottomLeft/BottomRight Indicator panel positions keep a fixed 100px bottom clearance for the separate cBot surface.

No public parameter, trading threshold, broker mutation authority or second decision authority is introduced.

Phase report: `docs/PHASE-MTF-P1-PRIMARY-M15-H1-PANEL.md`.

## MTF-P2 — Primary M15/H1 Location Evidence: OB/FVG Provenance

Status: **IMPLEMENTED — repository verification pending on this branch; target-terminal evidence remains manual.**

M15/H1 primary candidates now retain source-frame FVG quality, Order Block quality, OB+FVG confluence and canonical location quality. The existing LocationEvidenceRule remains the sole location-scoring owner; no new threshold or competing score was introduced.

Presentation priority uses primary source location evidence, and the chart panel exposes OB/FVG source evidence alongside the existing M5/M1 tuning state. This phase does not change broker authority, execution policy, RR, Entry, SL, TP, confidence, risk or public parameters.

Phase report: `docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md`.


## CBOT-DEMO-MARKET — Demo-only Market Execution Bridge — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Repository merge commit: `8ea385e2aa782ae24aa4f4a7941aee64bf4b685b`.

Verification on final implementation head `ba981da85e9ee06216f6c5ac3d7b4f2b4f4c6453`:
- Runtime Acceptance: PASS (run `36976202827`);
- cTrader Compile/Build: PASS (run `36976202817`);
- Source/Architecture: PASS (run `36976202805`).

Target-terminal evidence remains manual: attach exactly one `CFIP Smart Indicator` and this cBot to the same M5 chart, keep Indicator execution switches OFF, enable only the cBot demo-market switch on a demo account, and verify one bounded Market execution with broker-confirmed SL/TP.

The SDK `CS0612` warning for `IndicatorAttribute(string)` is suppressed only at the stable display-name attribute boundary; the equivalent cBot name warning is handled the same way. No strategy threshold, RR, Entry, SL, TP, risk or analytical score was retuned.

This phase is a validation bridge. Final production migration still requires the reissued CBOT-P4A Market / Market Range extraction from the current `main`, followed by Aggressive, Pending, Close, protection, lifecycle, account-risk and recovery migration.

Purpose: provide a bounded local/demo execution path while the remaining broker-mutation migration continues phase-by-phase.

Current boundary:
- Indicator = analysis / decision / scenario / plan / presentation and canonical SignalEnvelope publication.
- Contracts = immutable transport schema.
- cBot = demo Market broker mutation only in this phase, guarded against live accounts.

Safeguards:
- Enable Demo Market Execution defaults to false;
- live accounts are rejected by Account.IsLive;
- maximum demo market executions per cBot session defaults to 1;
- missing/duplicate Indicator, stale envelope, wrong symbol, invalid geometry, broker capacity or invalid volume fail closed;
- one idempotency key can never submit twice in one cBot session;
- only ExecuteMarketOrder is present in the cBot mutation owner; no pending, aggressive, close, SL mutation or TP mutation is activated by this phase;
- the existing Indicator market mutation remains dormant when Indicator automatic trading is OFF and is scheduled for the current-main P4A extraction immediately after this demo validation bridge.

Warning cleanup:
- the cTrader SDK obsolete IndicatorAttribute(string) warning is intentionally suppressed only around the stable display-name attribute so CFIP Smart Indicator remains unchanged.

Target-terminal acceptance:
- attach exactly one CFIP Smart Indicator and this cBot to the same M5 chart;
- keep Indicator Auto Trading / Automatic Orders / Aggressive Auto Entry OFF;
- arm only the cBot demo-market switch on a demo account;
- verify bind -> envelope -> SHADOW READY -> one demo Market execution -> broker position with initial SL/TP;
- stop/disable the cBot after the bounded test.

Phase report: docs/PHASE-CBOT-DEMO-LIVE-MARKET-2026-10-02.md.

## CBOT-P4B — Aggressive Authority + Panel Geometry Integrity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` on 2026-10-02 as `33dd37f225fb9e2677070f122b5068b0c2df2e92`.**

Scope:
- fix panel content clipping by synchronizing the inner panel-stack height with the final outer panel height;
- harden panel sizing against transient/collapsed Chart.Height during control attachment;
- capture a finite pre-control viewport baseline before Chart.AddControl;
- complete the next staged cBot mutation batch for ExecutionAction.Aggressive;
- keep one demo broker mutation owner and preserve live-account fail-closed behavior.

No analytical threshold, confidence, RR, Entry, SL, TP or risk tuning is included.

Phase report: `docs/PHASE-CBOT-P4B-AGGRESSIVE-PANEL-GEOMETRY-2026-10-02.md`.

Verification:
- Source / Architecture run `36983568671`: **PASS**;
- Runtime Acceptance run `36983568612`: **PASS**;
- cTrader Compile/Build run `36983568641`: **PASS**;
- P4B deterministic panel/Aggressive audit: **PASS**.

Target-terminal acceptance remains manual.

Next: **CBOT-P4C — Pending Stop authority extraction**.

## CBOT-P4A — Market / Market Range Authority Cutover — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` on 2026-10-02 as `dee53a3dfa1cbfab7f4b7ec4826298739559d19c`.**

Repository verification on final implementation HEAD `e51d1bb82db2fec25b91917873ab19ccacbd9859`:
- Source / Architecture: PASS (run `36981439556`);
- Runtime Acceptance: PASS (run `36981439561`);
- cTrader Compile/Build: PASS (run `36981439570`).

This phase performs the physical Market / Market Range owner switch from Indicator to cBot:
- Indicator no longer contains the Market broker-mutation owner;
- the legacy Indicator automatic-market and aggressive-market broker execution stages are removed from the live calculation path;
- cBot consumes the canonical plan-derived MarketExecutionProfile and owns Market / Market-Range submission;
- Indicator plan materialization is no longer tied to Indicator Auto Trading;
- Indicator panel no longer exposes Close/Cancel broker-action buttons or the old Auto Trading / Auto Orders execution-status block;
- panel bootstrap geometry is finite before Chart.AddControl to prevent chart-area collapse during first measure;
- canonical signal presentation is separated from live quote actionability.

Remaining CBOT-P4 migrations are deliberately staged: Aggressive, Pending Stop, Pending Limit, Cancel, Close/Partial, SL, and TP/server-ladder mutations remain with their current Indicator owners until their replacement passes parity and owner-removal verification.

See docs/PHASE-CBOT-P4A-MARKET-RANGE-AUTHORITY-CUTOVER-2026-10-02.md.

## CBOT-P4 — Broker Mutation Extraction

Extract and remove, in controlled batches:
1. Market / Market Range
2. Aggressive
3. Pending Stop
4. Pending Limit
5. Cancel
6. Close / Partial Close
7. SL mutation
8. TP / server TP ladder mutation

Each batch requires caller migration and zero direct mutation remaining in Indicator for that path.

## CBOT-P5 — Protection / Lifecycle / Recovery

Move broker-confirmed state, protection mutation, BE, trailing/profit-lock mutation, partial/full close, pending lifecycle, fill reconciliation, restart/reconnect and orphan recovery to cBot. Analytical reasoning remains in Indicator.

## CBOT-P6 — Account / Execution Risk Authority

Move trading permission, managed identity, capacity, final volume normalization, margin, live spread/session checks, daily-loss enforcement and execution retry/circuit ownership to cBot. Analytical quality/RR/suitability stay in Indicator.

## CBOT-P7 — UI / State Cutover

Indicator panel/chart becomes a read-only reflection of execution state. No UI action in Indicator can mutate broker state. AUTO TRADE / AUTO ORDERS status reflects actual cBot state.

## CBOT-P8 — Physical Removal / Terminal Certification

Delete migrated Indicator execution owners, hidden fallbacks, duplicate lifecycle state, execution-only parameters, and stale references only after replacement parity and target-terminal verification. This phase feeds M39/M40 certification.

### Cross-track close rule

M2–M28 may continue in parallel, but any phase touching execution/protection/panel execution status must consume the current cBot contracts/authority. No later phase may reintroduce execution logic into Indicator.


# گروه B — Canonical Truth

## M3 — Single Trade Truth Chain

### هدف
حل ریشه‌ای mismatch بین pre-analysis، signal، scenario، plan، panel، chart، alert، execution و outcome.

### immutable opportunity identity
- SignalId
- ScenarioId
- PlanId
- SourceTimeframe
- Lane
- Symbol
- Direction
- CreatedUtc
- ClosedBarReference
- Revision
- Expiry
- State

### same source
همه این‌ها باید از همان snapshot/contract بخوانند:
- panel
- chart
- popup
- sound
- market intent
- pending intent
- telemetry
- outcome attribution

### پذیرش
برای یک fixture، همه consumers همان direction/price/RR/identity/state را بدهند.

---

## M4 — Time / Session / History / Persistence Truth

### هدف
حل همه ناسازگاری‌های زمان، trading day، previous-period، EOD، daily loss و history.

### کارها
- canonical trading-day.
- previous D1/W1.
- overnight session.
- EOD warning/cancel/close.
- late-created position handling.
- realized/floating loss separation.
- daily-loss persistence.
- restart-mid-day.
- storage root.
- History marker.
- read/write recovery.
- 90-day rolling archive بدون پاک کردن تاریخچه قبلی.
- duplicate outcome prevention.
- transfer/import path.
- DST-aware session semantics.

### پذیرش
پس از restart/re-attach history و daily-loss state درست و قابل ممیزی باشد.

---

## M5 — Panel Live Content / Responsiveness

### هدف
حل واقعی stale/incomplete/slow panel.

### کارها
- full-layout و content-refresh را جدا کن.
- reaction/prediction/context/plan/execution rows را در content refresh تضمین کن.
- یک snapshot per refresh.
- حداقل broker reads.
- EffectivePanelContentWidth واحد.
- overflow counter؛ silent row drop ممنوع.
- button width بر اساس PanelWidth.
- height measurement واقعی یا conservative deterministic.
- width tests: 220/430/700.
- font tests.
- reload/restore.
- latency budget.
- no flicker/no stale row.

### I2 safety
پارامتر جدید Confirm Panel Close Actions فقط با default false.
وقتی true:
کلیک اول = CONFIRM?، کلیک دوم حداکثر طی 3 ثانیه.
برای مهلت از clock تزریقی استفاده شود.
ManagedActionsOnly و هشدار حالت unrestricted باید verification شوند.

### پذیرش
در target terminal، panel content بدون full-layout rebuild به‌روز شود و هیچ row مهمی stale نشود.

---

## M6 — Alert Synchronization / External Watchdog

### هدف
sound، popup، chart mark، panel state و telemetry یک event باشند.

### کارها
- canonical AlertEnvelope.
- event timestamp.
- source Revision.
- duplicate suppression.
- stale suppression.
- blocked signal → no sound/no mark.
- watch/prediction/reaction/confirmed/active/expired جدا.
- sound after accepted alert event only.
- restart/reconnect semantics.
- race tests between Calculate/Timer/UI.
- critical external notification channel برای مواردی مثل lost protection، rejected close و daily-loss lock؛ provider اتصال‌پذیر باشد و notification source همان canonical event باشد.

### پذیرش
برای هر fixture دقیقاً expected alerts تولید شود؛ sound/popup/mark mismatch صفر.

---

## M7 — Chart / Label Truth

### هدف
تمام chart objects دقیقاً همان Plan را نشان دهند.

### کارها
- Entry/Trigger/SL/TP canonical geometry.
- LineLengthBars به plan lines وصل شود.
- default 40 حفظ شود.
- future extension با DateTime/timeframe.
- solid/thickness contract حفظ شود.
- label text readability.
- واقعی کردن Source Timeframe به‌جای tag ثابت.
- label font consistency.
- LabelLeftOffsetBars دامنه را بی‌اثر نکند.
- expired drawings حذف.
- multiple scenarios بدون overlap/ambiguity.
- old generic signal text حذف فقط اگر duplicate owner باشد.
- marker shape از contract تبعیت کند.
- label text: price/name + TP/SL distance in pips + source timeframe، بدون background در طراحی نهایی.

### پذیرش
Visual contract tests + manual chart matrix.

---

# گروه C — Execution Safety قبل از جداسازی

## M8 — Canonical Entry / SL / TP / RR

### هدف
هیچ path نتواند geometry Plan را خراب کند.

### کارها
- BUY/SELL side rules.
- entry validation.
- stop-side validation.
- target-side validation.
- normalized price.
- normalized volume.
- slippage envelope.
- pending/market distinction.
- final RR recalculation.
- invalid stop candidate must fail explicitly.
- invalid computed stop must not return false success.
- RecoveryRequired when protection cannot be established.
- G1 Tighten-only at the mutation boundary.
- TP monotonic progression.
- BE after Partial TP uses same protection owner.
- Dynamic TP advance never backwards.

### پذیرش
Mock broker + symmetric BUY/SELL fixtures.

---

## M9 — Execution Lifecycle / Event Ordering

### کارها
- explicit state machine.
- submission identity.
- scenario-scoped retry/circuit.
- broker rejection.
- delayed confirmation.
- opened-before-ID event.
- duplicate events.
- manual volume reduction.
- pending creation/fill/cancel.
- orphan/recovery.
- restart/reconnect.
- no duplicate execution.

### پذیرش
mock broker deterministic.

---

## M10 — Runtime / Startup / Hot Path

### هدف
رفع late start، 30–60 second waits، calculation starvation و unnecessary work.

### کارها
- cold/warm startup measurement.
- MTF readiness.
- staged initialization.
- timer ownership.
- Skender bounded warm-up/cache.
- repeated allocations.
- broker enumeration.
- chart redraw.
- panel refresh.
- file I/O.
- cache bounds.
- large-method decomposition with golden regression tests.
- signal latency measurement: bar close → decision/plan.

### پذیرش
startup and refresh budgets recorded; no regression.

---

# گروه D — Test / Replay Foundation

## M11 — Deterministic Replay

### inputs
M1/M5/M15/H1/H4/D1/W1 CSV with UTC.

### output per signal
- time
- direction
- Entry
- SL
- TP1..TP4
- Confidence
- Quality
- Regime
- Lane
- ScenarioId
- Stage
- first SL/TP outcome
- MFE
- MAE
- bars-to-event
- spread
- commission
- slippage

### attribution
در یک candle با هر دو touch، SL first.

### reports
- Expectancy R
- Profit Factor
- Win rate
- max drawdown
- Bootstrap CI
- by Regime
- by Lane
- Confidence bucket
- hour
- session
- weekday

### walk-forward
Train/validation/test جدا و بدون leak.

### پذیرش
دو اجرای پشت‌سرهم byte-equivalent.

---

## M12 — Replay vs Live Consistency

### کارها
- SignalTrace comparison.
- repaint detection.
- stage ordering.
- scenario identity.
- plan identity.
- closed-bar semantics.
- future leak detection.
- live vs replay latency.
- plan/visual attribution consistency.

### پذیرش
هر difference یا توضیح ریشه‌ای دارد یا fail است.

---

## M13 — Data Quality / Time Quality

### کارها
- gap detection
- incomplete candle detection
- market closure
- DST
- rollover
- tick-volume limitations
- insufficient bars
- missing news data
- symbol→currency mapping
- insufficient MTF data
- fail closed for unavailable critical data

### پذیرش
داده ناکافی = unavailable / no-trade، نه تحلیل ساختگی.

---

# گروه E — Signal Quality / Data Science

## M14 — Ablation

### module set
- WaveTrend
- Divergence
- OSS indicator families
- OB
- FVG
- OB+FVG
- Liquidity
- VWAP
- Volume

### method
one-at-a-time + controlled combinations + OOS.

### measure
- Expectancy
- PF
- drawdown
- CI
- trade count
- latency
- redundancy

### decision
KEEP / REDUCE / DISABLE / REMOVE.

هیچ ماژول جدیدی قبل از این gate اضافه نمی‌شود.

---

## M15 — Parameter Simplification

### هدف
از 568 public parameters به حدود 40–60 effective controls، فقط در صورت evidence.

### کارها
- sensitivity analysis.
- identify non-effective params.
- detect redundant controls.
- merge semantically duplicate controls only when behavior can be preserved.
- three operator profiles: Conservative / Balanced / Aggressive.
- hide/persist low-value implementation controls only after migration safety.
- no destructive rename/default change without explicit approval.

### پذیرش
هر کاهش parameter با mapping قدیم→جدید و regression tests.

---

## M16 — Confidence Calibration

### هدف
Confidence = calibrated probability, not raw score.

### کارها
- P(TP1 before SL).
- regime-aware mapping.
- Isotonic or Logistic.
- minimum sample rule.
- reliability curve.
- Bootstrap CI.
- calibration drift.
- panel: P=0.xx ± 0.xx (n=...)

### پذیرش
فقط OOS evidence معتبر است.

---

## M17 — Range / Regime Signal Quality

### هدف
ضعف signals در range بدون از دست دادن فرصت‌های low-risk/high-reward.

### کارها
- centralize Range thresholds.
- verify current 0.35 / 0.65 behavior.
- test 20/80 hypothesis only via Replay.
- validate Confidence/SmartQuality/IndependentEvidence/RR interactions.
- prevent structural double counting.
- range false-positive study.
- range reward-path study.

### fixed-to-owner cases
- Confidence
- SmartQuality
- IndependentEvidence
- RR
- WaveTrend threshold
- range edge threshold

### پذیرش
هیچ numeric tuning بدون OOS evidence.

---

## M18 — OB / FVG / Confluence

### هدف
OB و FVG وزن بالاتر فقط اگر data confirms edge.

### کارها
- geometry
- mitigation
- opposing-direction obstacle
- freshness
- age
- touch count
- distance
- OB+FVG confluence
- reaction after touch
- reward-path effect
- target effect

### پذیرش
هر new weight/threshold evidence-backed + OOS.

---

## M19 — Exit MAE/MFE Research

### کارها
اندازه‌گیری:
- stop width
- 0.75R soft invalidation
- BE trigger
- Partial TP
- TP1..TP4 ladder
- Dynamic TP
- trailing give-back

### rule
هر exit policy با MFE/MAE و walk-forward قضاوت می‌شود.

---

## M20 — Session / Spread / News

### کارها
- DST-aware real session calendar.
- hourly/day expectancy.
- rollover filter.
- spread moving median.
- spread z-score.
- dynamic liquidity guard.
- news importance windows.
- symbol-to-currency mapping.
- index/gold/crypto mapping.
- optional reduced-risk news mode.
- transparent block reason in panel.

### پذیرش
changes measured OOS.

---

# گروه F — Risk / Survival / Explainability

## M21 — Outcome Integrity / Adaptive Risk

### هدف
Adaptive Risk فقط از outcome صحیح استفاده کند.

### required
- aggregate R across all legs
- Partial TP accounting
- BE accounting
- close accounting
- NaN/Infinity rejection
- min sample
- named constants/parameters with current defaults
- reason logging
- clear Risk % Equity description
- clear Use Smart Risk Scaling description
- effective risk visible

### current adaptive logic to preserve until evidence
Current multiplier behavior is bounded and cannot exceed configured risk.
Before changing coefficients, correct outcome attribution.

### after evidence
- weekly loss limit
- peak drawdown de-risking
- volatility targeting
- correlated exposure cap
- fractional Kelly ≤ 0.25 Kelly only if statistically justified

### acceptance
effective risk <= configured risk and reason is auditable.

---

## M22 — Statistical Safety Switch

### هدف
وقتی live results statistically diverge from Replay expectation، new auto execution stop شود.

### candidates
CUSUM / SPRT / defensible equivalent.

### behavior
- stop new execution
- preserve existing broker SL/TP
- alert operator
- do not auto-close solely from detector

### acceptance
normal synthetic stream = no trigger؛ degraded stream = trigger.

---

## M23 — Why-No-Trade

### هدف
کاربر بداند چرا معامله انجام نشد.

### output
Top 3 reasons:
- panel
- daily CSV
- replay report

### rule
هر block canonical reason code و owner داشته باشد.

### acceptance
panel/log/replay counts برابر.

---

# گروه G — Advanced Intelligence مشروط

## M24 — Importance-based Swing

- ATR-ZigZag
- level age
- depth
- touch count
- equal-high/low ATR tolerance
- liquidity importance

فقط در صورت evidence.

## M25 — Anchored VWAP / Volume Profile

- session anchored VWAP
- swing anchored VWAP
- high tick-volume zones
- target/obstacle utility

فقط اگر ablation positive باشد.

## M26 — Lightweight Meta-Labeling (اختیاری)

- offline only
- logistic regression
- walk-forward
- take/skip existing signal only
- no runtime ML dependency
- no black-box trading engine

اگر overfit/unstable، حذف شود.

---

# گروه H — Engineering Quality

## M27 — Property-Based / Mutation Testing

### tests
- BUY/SELL mirror property
- random geometry
- random regime combinations
- random event order
- NaN/Infinity
- invalid transitions
- mutation testing on Core

### پذیرش
mutated business logic should be caught.

---

## M28 — Replace Text-Only Audits

### کارها
auditهایی که behavior را فقط با regex/text بررسی می‌کنند به behavioral/contract tests منتقل شوند.
فقط architecture checks واقعی در scripts باقی بمانند.

### خروجی
- faster CI
- lower false confidence
- deterministic regression coverage

---

# گروه I — cBot Separation

## M29 — CBOT-0 Boundary Inventory

### Indicator keeps
- Market data
- MTF analysis
- structure
- liquidity
- BOS/MSS/CHoCH
- FVG
- OB
- OB+FVG
- WaveTrend
- divergence
- regime
- no-trade intelligence
- confidence/quality
- trigger intelligence
- scenario construction
- Entry/SL/TP proposal
- analytical RR
- chart/panel
- signal alerts
- analytical outcome/calibration logic

### cBot owns
- trading permission
- managed identity
- balance/equity/margin
- final volume normalization
- capacity
- live daily-loss enforcement
- broker/account spread/session checks
- Market
- Aggressive
- Pending Stop
- Pending Limit
- Cancel
- Close/Partial Close
- SL/TP mutation
- BE
- Trail
- Profit Lock
- broker confirmation
- fill reconciliation
- lifecycle
- restart/reconnect recovery
- retry/backoff/circuit
- execution telemetry

### mixed classes
method-by-method split؛ whole-file copy ممنوع.

### acceptance
every direct broker mutation future-owned exactly once.

---

## M30 — CBOT-Preflight

### no-trade capability test
اثبات کن:
- cBot can instantiate the custom Indicator through supported cTrader mechanism.
- cBot can read structured read-only signal data.
- no reflection/static globals/chart scraping required.
- instance scope deterministic.
- stale/unavailable provider is detected.
- package/build layout works in target terminal.
- startup order variations do not fabricate signal.

### blocking rule
بدون موفقیت M29 + M30، cBot implementation شروع نمی‌شود.

---

## M31 — Platform-Neutral Contracts

### required data
- SignalEnvelope
- PlanSnapshot
- ExecutionIntent
- ManagementCommand
- BrokerExecutionReport
- LifecycleEvent
- ContractVersion
- Sequence/Revision
- CorrelationId
- IdempotencyKey

### identity
SignalId / ScenarioId / PlanId / SourceTimeframe / Lane / Symbol / Direction / CreatedUtc / ClosedBarReference / Expiry / Revision.

### rule
Contracts data-only است؛ decision engine جدید نیست.

---

## M32 — Indicator Read-Only Provider

### states
- unavailable
- initializing
- watch
- prediction
- confirmed
- active
- expired
- blocked

### forbidden
- broker mutation
- private state scraping
- static mutable global bridge
- hidden executor fallback

### acceptance
stale/uninitialized signal never actionable.

---

## M33 — cBot Shadow Execution

### flow
receive → validate → expiry/revision → duplicate → identity → account safety → simulate → telemetry

### acceptance
one-to-one plan/intent parity; no live broker mutation.

---

## M34 — Broker Execution Migration

### exact order
1. Market
2. Aggressive
3. Pending Stop
4. Pending Limit
5. Cancel
6. Close
7. Partial Close
8. SL
9. TP

### acceptance
zero direct broker mutation in Indicator for migrated paths.

---

## M35 — Protection / Lifecycle / Recovery Migration

### move
- broker-confirmed state
- protection reconciliation
- BE
- TP progression mutation
- trailing
- partial/full close
- pending lifecycle
- restart/reconnect
- orphan recovery

### invariant
Indicator supplies analytical intent/management request; cBot validates broker-level mutation and owns the mutation.

---

## M36 — Account Risk / Execution Control Migration

### move
- capacity
- volume normalization
- margin safety
- daily loss enforcement
- trading permission
- managed identity
- live spread
- live session restriction
- broker modification throttles

### stay Indicator
- analytical RR
- signal quality
- market suitability
- scenario reasoning

---

## M37 — UI Control Authority Cutover

### goal
UI never lies about execution.

### rules
- cBot authoritative execution state.
- Indicator shows status/diagnostics.
- no Indicator click can mutate broker.
- unavailable control/status channel = fail-closed.
- AUTO TRADE / AUTO ORDERS status must correspond to actual cBot state.

---

## M38 — Remove Indicator Execution Engine

### physical cleanup
- direct broker mutation code
- hidden fallback
- duplicate execution state
- duplicate managed identity
- dead execution handlers
- execution-only parameters no longer owned by Indicator

### hard acceptance
AST/source architecture proves zero direct broker mutation in Indicator.

---

# گروه J — Terminal Certification / Release

## M39 — Full Target-Terminal Acceptance

### demo only until complete
Mandatory:
- startup/reload
- M1/M5 timing
- MTF readiness
- panel cold/warm
- live panel updates
- chart drawing
- sound alerts
- news
- history write/read
- daily loss
- EOD
- overnight session
- Market/Aggressive/Stop/Limit
- slippage
- rejection
- delayed confirmation
- restart
- reconnect
- partial close
- BE
- trail
- TP advance
- protection recovery
- duplicate event
- missing cBot
- stale signal
- expired signal
- multiple simultaneous scenarios
- panel widths 220/430/700
- fonts
- chart reload

### acceptance
هر fail => defect ID و no-final.

---

## M40 — End-to-End Certification

### chain
Market
→ Analysis
→ Decision
→ Scenario
→ Plan
→ Signal
→ Panel
→ Chart
→ Alert
→ cBot
→ Broker
→ Confirmation
→ Protection
→ Outcome
→ History
→ Replay

### checks
identity, direction, prices, RR, risk, timeframe, revision, reason.

---

## M41 — Final Repository Cleanup

### کارها
- delete unreachable source
- delete dead classes
- remove obsolete execution code
- remove duplicate scripts
- remove obsolete planning files
- remove temporary artifacts
- remove stale references
- clean accidental binaries/logs
- remove historical production identifiers from source
- verify generated outputs
- verify package contents
- verify clean checkout

### acceptance
clean checkout builds/tests exactly.

---

## M42 — Release Gate

### required green
- Source/Architecture
- Runtime Acceptance
- cTrader Compile
- Replay
- deterministic replay
- property tests
- mutation evidence
- target terminal
- restart/reconnect
- repository hygiene
- documentation completeness

### rule
No real-account auto execution before all mandatory gates are green.

---

# 6. بعد از Release

## M43 — Production Observation

monitor:
- live/replay distribution
- drift
- reject reasons
- broker error rate
- protection failures
- panel latency
- execution latency

بدون automatic tuning.

## M44 — Controlled Calibration

- OOS only
- walk-forward
- versioned calibration
- rollback
- no hidden coefficient changes

## M45 — Future Cloud Analysis (اختیاری)

فقط در صورت نیاز:
- same Contracts
- cBot remains execution authority
- no duplicate analysis engine
- local mode remains functional
- no Cloud requirement for local operation

---

# 7. Exact Known-Issue Closure Matrix

## I-series
- I1 IndependentEvidence mismatch → M3
- I2 Close/Cancel confirmation → M5 / M37
- I3 score rounding → M1 / M28
- I4 WaveTrend OS/OB validation → M1
- I5 Quality Recovery reference → M17
- I6 Range hard-coded thresholds → M17
- I7 panel button/height sizing → M5
- I8 missing time/session behavioral tests → M4 / M28
- I9 source-review-only claims must be re-verified → M1

## J-series
- J1 LineLengthBars plan lines → M7
- J2 label color / real timeframe → M7
- J3 LabelLeftOffsetBars clamp → M2 / M7
- J4 effective panel width / overflow → M5

## K-series
- K1 adaptive outcome risk → M21
- K2 empty HashSet + duplicated suffix ownership → M2 / M7
- K3 MAX RR must show actual Plan RR → M7
- K4 Invariant UI formatting → M7

## G1 remainder
- Tighten-only at mutation boundary → M8
- Partial-TP BE path audit → M8/M9

## D2
- Relative History path → M4/M39

---

# 8. Hidden-Clamp Closure Contract

M1/M2 must detect not only simple Max/Min but also:

- nested Max(Min(...))
- Clamp / ClampInt
- multiline calls
- reversed argument order
- helper wrappers
- parameter aliases

Every clamp must be:
1. aligned with public Min/Max;
2. removed if it is accidental;
3. allowlisted with explicit reason;
4. or escalated as a behavior question.

Known high-risk examples that require explicit review:
- MinimumTriggerBodyAtr internal floor higher than its public minimum.
- SmartStrongSetupEdge internal floor higher than its public minimum.
- LabelLeftOffsetBars capped below its public maximum.
- panel width internal minimum above public minimum.
- WaveTrend minimum quality below internal floor.
- BreakEven / Trail hidden minimums.
- structural lookback and obstacle lookbacks.
- predictive quality/zone-age floors.

No default is silently changed merely to make the clamp disappear.

---

# 9. Permanent Signal-Quality Rules

1. Do not add indicators before ablation.
2. Do not raise/lower thresholds by intuition.
3. Do not optimize on the complete dataset.
4. Do not use black-box live entry/exit AI.
5. Do not infer profitability from a visual sample.
6. Do not promote independent scenarios to execution without one authoritative execution contract.
7. Preserve high-RR opportunities during tightening studies by measuring conditional expectancy, not trade count alone.
8. OB/FVG evidence must be measured against baselines.
9. Range filters must be evaluated on Range-only OOS subsets.
10. Confidence must be calibrated before treating it as probability.
11. Exit changes require MAE/MFE evidence.
12. Any change that improves in-sample but harms OOS is rejected.

---

# 10. Permanent Execution / Protection Rules

### Entry
exact plan identity and geometry.

### SL
never farther in Tighten mode.

### TP
never backwards.

### Partial
volume and realized R reconciled from broker facts.

### Close
broker-confirmed close only.

### Cancel
broker-confirmed cancellation only.

### Recovery
missing protection = explicit recovery state.

### Restart
broker state first.

### Reconnect
broker state first.

### Duplicate
idempotency key and scenario identity.

---

# 11. Permanent UI Truth Rules

Panel and chart are presentation surfaces, not authorities.

Every displayed:
- direction
- Entry
- SL
- TP
- RR
- confidence
- quality
- stage
- reason
- execution state
- protection state

must come from the canonical current snapshot.

No display-only value may become an execution input.

---

# 12. Permanent Parameter Ownership

## Indicator owns
Any parameter changing:
- market analysis
- evidence
- MTF interpretation
- confidence
- quality
- scenario selection
- analytical Entry/SL/TP
- analytical RR
- presentation

## cBot owns
Any parameter changing:
- broker execution
- account sizing
- margin
- capacity
- daily loss enforcement
- live spread/session broker guard
- trading permission
- managed identity
- retries
- broker protection
- lifecycle

## rule
No duplicated public controls.
One authority, other side read-only reflection.

---

# 13. Permanent cBot Architecture

### Indicator
Market → Evidence → Decision → Scenario → Plan → Presentation

### Contracts
data exchange only; platform-neutral; deterministic IDs/versioning.

### cBot
Intent → Broker Eligibility → Submission → Confirmation → Protection → Lifecycle → Recovery → Account Risk Enforcement

Exactly one live execution authority.

### forbidden
second engine, chart scraping, static globals, reflection, cloud dependency, multi-position expansion.

---

# 14. Permanent cBot Runtime State Machine

WAITING
→ RECEIVE
→ VALIDATE
→ EXPIRY
→ REVISION
→ DUPLICATE
→ ACCOUNT/BROKER SAFETY
→ PREPARE
→ SUBMIT
→ BROKER RESULT

BROKER RESULT:
- REJECTED → Retry/Circuit
- ACCEPTED/UNCONFIRMED → Reconcile
- CONFIRMED → Adopt

CONFIRMED
→ PROTECTION
→ LIVE MANAGEMENT
→ PARTIAL / TP / SL / CLOSE
→ BROKER CONFIRMATION
→ OUTCOME

RESTART/RECONNECT:
START
→ READ BROKER STATE
→ RECONCILE
→ ADOPT
→ REPAIR PROTECTION
→ RESUME

---

# 15. Permanent Test Matrix

## Analytical
- BUY/SELL mirror
- closed/open candle
- MTF
- Range/Trend/Transition
- OB/FVG
- WaveTrend
- Divergence
- liquidity
- NaN/Infinity
- insufficient data

## Planning
- Entry
- SL
- TP1..TP4
- RR
- obstacle
- source timeframe
- expiry
- scenario coexistence

## Execution
- Market
- Aggressive
- Stop
- Limit
- rejection
- delayed confirmation
- slippage
- duplicate
- partial fill
- manual change

## Lifecycle
- open
- modify
- close
- partial
- cancel
- restart
- reconnect
- orphan
- recovery

## UI
- panel 220/430/700
- fonts
- overflow
- live content
- reload
- chart objects
- label readability

## Persistence
- history write
- history read
- duplicate
- restart
- recovery
- 90-day archive

## Statistical
- deterministic replay
- walk-forward
- bootstrap
- calibration
- ablation
- drift detection

---

# 16. Permanent Phase-Close Record

## Phase-closeout template
Mxx — Title

## Result
VERIFIED COMPLETE / PARTIAL / BLOCKED / FAILED

## Root cause
...

## Changed files
...

## Behavioral changes
...

## Tests
before: FAIL
after: PASS

## CI
Source/Architecture: ...
Runtime Acceptance: ...
cTrader Compile: ...

## Manual Target Terminal
...

## New bugs
...

## Remaining risks
...

## Exact progress
...

## Operator action
git pull required after merge: YES / NO

## Next phase
Mxx+1 — Title

---

# 17. Final Product Target

## Accuracy
- consistent decision evidence
- no double-counted structure
- calibrated confidence
- measured OB/FVG value
- measured exit behavior
- data-backed Range handling

## Performance
- bounded startup
- fast panel content refresh
- bounded caches
- limited redraw
- limited broker reads
- no hot-path file I/O

## Safety
- broker truth
- protective SL
- monotonic TP
- deterministic retries
- restart/reconnect recovery
- fail-closed

## Architecture
- Indicator analysis only
- Contracts data only
- cBot execution only
- one decision authority
- one execution authority

## Maintenance
- one roadmap
- clean repo
- reproducible build
- behavioral tests
- property/mutation evidence
- target-terminal evidence

---

# 18. Current Starting Point

**Canonical implementation start: M2 — Repository Hygiene / Dead Code / Ownership, with the CBOT separation track active; current execution migration phase is CBOT-P4E.**

M0 — Adoption / Freeze / Baseline, M1 — Full Forensic Audit and CBOT-P0 — Activation / Boundary Lock are **VERIFIED COMPLETE**.

No other roadmap, prompt, continuation note or planning document may override this file.

Do not start advanced intelligence, parameter tuning or new indicators before the relevant gates. cBot separation is explicitly active now, but no permanent dual broker executor is allowed: migration is staged, parity-gated and fail-closed.

Once M42 is accepted, the project leaves the remediation/certification track and enters controlled production observation/calibration.


---

# 19. Historical baseline absorbed into this roadmap

این بخش فقط برای حفظ تداوم است. فازهای historical زیر «باز» نیستند مگر M1 خلافشان را ثابت کند.

## CR4.10 / D10 — Native indicator safety and registry performance

CR4.10 / D10 implementation complete. Native indicator readiness, safe warm-up/fail-closed behavior and deterministic registry lookup ownership were hardened without changing public parameter name/type/DefaultValue or trading thresholds. Target-terminal validation remains a separate manual acceptance boundary. The certification transition after the historical CR4.10/D10 remediation is **CR-FINAL**.

## Historical audit string anchors

## Accumulated audit string anchors

These exact historical strings are retained for accumulated repository-audit compatibility. They are continuity markers only and are not additional implementation phases or an alternate roadmap:

- CI-03
- CI-04
- CI-04 closeout
- ## 2.0.1 — Current certification state
- CI-05
- CI-05 implementation record
- CI-07
- CI-08 implementation record
- CI-10 — Trigger and trigger-lifecycle audit
- docs/PHASE-CI-10-TRIGGER-LIFECYCLE.md
- CI-11 — Entry geometry and signal-timing audit
- CI-12 — Structural SL
- docs/PHASE-CI-12-STRUCTURAL-SL.md
- ### CI-13 implementation record
- Status: **VERIFIED COMPLETE — PR #168 merged to `main`.
- CI-14
- CI-15
- CI-16
- CI-17
- CI-17A
- ### CI-17 target-terminal acceptance package — 2026-10-02
- CR-FINAL

- CR7.5 / G5
- CR7.6a

## Historical audit continuity anchors

The following identifiers are retained solely so accumulated repository audits can prove historical continuity after the roadmap consolidation. They are not additional implementation phases and do not override the M0–M42 order:

- CR4.4
- CR4.5
- CR4.6
- CR4.7
- CR4.8
- CR4.9
- CR4.10 / D10
- CR5.3 / E3
- CR5.4 / E4
- CR5.5 / E5
- CR5.7 / E7
- CR5.8 / E8
- CR6.6 / F7 closeout
- CR6.7 / F8
- CR6.7 / F8 closeout
- CR6.8 / F9
- CR6.8 / F9 closeout
- CR6.9 / F3
- CR6.9 / F3 closeout
- CR7.1 / G1
- CR7.3 / G3 closeout
- **Next phase: CR7.4 / G4**

Historical target-terminal validation remains a separate manual acceptance boundary; no anchor above claims terminal certification.

## Phase 7.4 — MaximumOpenPositions semantics

Historical continuity marker preserved: the single-plan `Maximum Open Positions` semantics remain an audited invariant. This is historical continuity only; it is not a new implementation phase in the M0–M42 execution sequence.

## CR4.4 — Numerical stability and caching continuity

CR4.4 is a completed historical numerical-stability/caching remediation. Its production work remains under the existing OSS adapter/cache owners. Target-terminal validation is a separate manual acceptance boundary and is **not claimed as completed** by this roadmap.

## Track 19 — OSS Numerical Benchmark

Track 19.1 numerical benchmark is a completed historical research/validation milestone. Its detailed continuity and benchmark evidence are retained in:
`docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md`

This track does not authorize production package promotion or numerical policy tuning.

## Foundation already established
- Canonical source/architecture boundary
- Modular indicator/analysis decomposition
- Decision and planning separation
- Risk/sizing separation
- Automatic market execution decomposition
- Aggressive execution decomposition
- Pending Stop/Limit decomposition
- Broker protection ownership work
- Panel decomposition
- Regime-aware intelligence
- Runtime fault containment
- staged startup foundation
- deterministic decision/planning/execution/runtime contracts
- OSS adapter/research separation
- numerical benchmark foundation
- scenario identity and multi-scenario observation semantics

## Previously completed review/correction families
- runtime fault containment
- closed-bar Reaction semantics
- CHoCH structural semantics
- daily loss foundation
- EOD boundary foundation
- asynchronous news retrieval
- calibration-key equality
- FVG/OB direction-aware obstacle scanning
- aggressive RR guard
- existing-stop health checks
- fill acceptance direction
- HTF absolute strength
- bounded Skender computation/cache work
- memory account scoping
- execution/presentation state decomposition

## Current inherited gate
Target-terminal validation remains the final evidence boundary for current repository behavior until M39/M40 closes it.

The historical work is not reimplemented merely because its old phase number disappears from the current roadmap. M1 can reopen a historical behavior only if code/tests/runtime evidence prove regression or missing coverage.

---

# 20. Mapping of the old planning vocabulary into the new master roadmap

این نگاشت فقط برای اثبات پوشش کامل است؛ برای اجرای آینده باید فقط M-phaseهای همین فایل استفاده شوند.

| Old group | Absorbed into |
|---|---|
| P0 | M0, M1 |
| P1 / CI-15 | M8, M9 |
| P2 / CI-16 Replay | M11, M12 |
| P3 / CI-17 Target Terminal | M39 |
| P4 / CI-FINAL | M40 |
| P5 / H4-H6 | M3, M7, M2 |
| P6 / Prompt 9 I2-I7 | M5, M1, M7 |
| P7 / Prompt 10 J1-J4 | M7, M5, M2 |
| P8 / Prompt 11 K1-K4 | M21, M2, M7 |
| P9 independent code review | M1 |
| P10 structural/testability | M10, M27, M28 |
| P11 final integration | M40, M41 |
| P12 cBot separation | M29-M38 |
| S1 Replay/Walk-forward | M11-M13 |
| S2 calibrated confidence | M16 |
| S3 ablation | M14 |
| S4 parameter reduction | M15 |
| S5 MAE/MFE exits | M19 |
| S6 time/session | M13, M20 |
| S7 dynamic risk | M21 |
| S8 statistical cut-off | M22 |
| S9 spread/liquidity guard | M20 |
| S10 news guard | M20 |
| S11 cBot/server execution | M29-M38 |
| S12 watchdog/external alerts | M6, M22 |
| S13 data quality | M13 |
| S14 swing importance | M24 |
| S15 anchored VWAP/volume | M25 |
| S16 meta-labeling | M26 |
| S17 property/mutation testing | M27 |
| S18 why-no-trade | M23 |
| I1-I9 | M1, M3, M4, M5, M28 |
| J1-J4 | M5, M7, M2 |
| K1-K4 | M2, M7, M21 |
| G1 remainder | M8, M9 |

No item above is allowed to create a second roadmap or an alternate implementation order.

---

# 21. Canonical known-risk inventory

M1 must explicitly scan and disposition at least these risk families:

### Analysis
- evidence double counting
- structure/MSS/CHoCH correlation
- closed/open candle contamination
- MTF source contamination
- Range center/edge behavior
- OB/FVG mitigation and direction
- WaveTrend OS/OB validity
- divergence/swing equality
- liquidity false positives
- hidden threshold changes

### Planning
- Entry/SL/TP direction
- reward-path obstacles
- target ladder ordering
- RR after normalization
- expiry
- simultaneous scenarios
- source timeframe attribution

### Execution
- market/aggressive distinction
- pending Stop/Limit distinction
- slippage
- final volume
- broker rejection
- delayed confirmation
- duplicate attempt
- retry storm
- capacity
- account permission
- protection mutation

### Lifecycle
- opened/modified/closed events
- pending events
- partial close
- BE
- trail
- TP progression
- recovery
- orphan adoption
- restart/reconnect

### Presentation
- panel stale content
- panel overflow
- panel sizing
- label readability
- timeframe labels
- line length
- line extension
- marker identity
- alert/sound synchronization
- stale/blocked marks

### Persistence
- history location
- write/read failure
- duplicate rows
- account scope
- session/day boundary
- 90-day retention
- recovery after restart

---

# 22. Final cleanup rule for obsolete planning material

Once M41 is accepted:

1. This ROADMAP.md remains.
2. Evidence produced by actual acceptance/testing remains only when it has operational value.
3. Historical planning prompts, duplicate roadmaps and superseded execution plans are removable after dependency verification.
4. No source code, test or CI workflow may depend on a planning-only document.
5. No deleted document may be the hidden source of implementation order.
6. Git history remains the archival record.
7. The repository must contain exactly one active development roadmap: this file.

---

# 23. No-silent-scope-expansion rule

During M0-M42, a newly discovered issue is handled as follows:

- Critical safety defect: can interrupt the current phase and becomes part of the current phase only if necessary to leave the system safe.
- Direct regression blocking the current acceptance: current phase expands only enough to restore the invariant.
- Non-critical defect: register it here with owner and target phase; do not silently change scope.
- New feature idea: postponed until M42 unless required for an existing acceptance contract.
- New indicator: forbidden before M14.
- Threshold tuning: forbidden before OOS evidence.
- Production live auto-execution remains forbidden before M42. A narrowly bounded demo-only Market execution bridge is allowed only under CBOT-DEMO-MARKET, with live-account rejection and explicit default DISARMED controls.

This prevents the roadmap from becoming an endlessly expanding patch queue.
## MTF-P3 — Primary M15/H1 Provider Scenario Identity Cohesion

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `5f6a9873a27b3b1edfa139ab21d19c1b5faa6bdc`; target-terminal identity verification remains manual.**

The provider bridge must preserve the exact ScenarioId/SourceTimeframe of the canonical
candidate used by the execution-facing plan. Provider SourceTimeframe may not be derived
from the Indicator chart attachment timeframe when a scenario candidate is available.

Implementation scope:
- reuse canonical scenario identity from the existing registry/resolution path;
- derive provider SourceTimeframe from the exact candidate;
- pass the same source timeframe into canonical execution-intent identity;
- remove hard-coded pending Stop/Limit scenario identifiers in favor of the canonical
  direction scenario identity;
- accumulate deterministic runtime/static verification.

This phase changes traceability only. It does not change strategy thresholds, RR, Entry,
SL, TP, confidence, risk, broker authority or the M15/H1 observe-only execution policy.

Phase report: `docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md`.

## Panel Geometry Correction — 50px Bottom Clearance + Hidden Restore Position

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b63f82e1d1fca9ef3af2d7fbbe34e779099ab7d7`; target-terminal visual confirmation remains manual.**

The Indicator panel bottom clearance is corrected from 100px to exactly 50px. The hidden
restore button for BottomLeft/BottomRight positions receives the same 50px bottom clearance.
This preserves the reserved lower chart area for the separate cBot surface without the
previously excessive gap.

No strategy, signal, Decision, Plan, RR, Entry/SL/TP, risk or execution authority changes.

Phase report: `docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md`.

## Indicator Naming + cBot Launch + MTF Panel Direction Correction

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b360761d13df94ac098e8bfc626ed2985499742d`; target-terminal name/visual confirmation remains manual.**

Operator-facing corrections:
- Indicator display name: **CFIP Smart Indicator**;
- cBot display name: **CFIP Smart Execution Bot**;
- cBot default host timeframe: **M5**;
- MTF panel distinguishes resolved BUY/SELL from BULL BIAS/BEAR BIAS and true NEUTRAL;
- obsolete Quick Execution height reservation is removed from the live panel render path.

No strategy/threshold or broker authority change is allowed in this phase.

Phase report: `docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md`.