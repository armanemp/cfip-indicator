# M4 — Time / Session / History / Persistence Truth — 2026-10-02

## Goal

Remove duplicated or machine-local date/time semantics from the trading-day, session, EOD, daily-loss and history/persistence paths while preserving the existing strategy thresholds and broker authority.

## Canonical time owner

`Core/Math/CanonicalTimeRule.cs` is now the single pure owner for:
- UTC normalization;
- UTC trading-day start/next-day start;
- same-UTC-day comparison;
- stable `yyyyMMdd` day identity;
- deterministic 90-day rolling period boundaries;
- half-open UTC interval membership.

The Indicator and cBot are explicitly configured with `TimeZone = TimeZones.UTC`. cTrader documents that an algo's configured TimeZone controls its `Server.Time` and `Bars.OpenTime` references; the project uses UTC as the algorithm time zone. citeturn767245search0turn767245search3

## Implemented corrections

### Session / EOD

Session membership and EOD boundary calculation continue to support same-day, overnight and start==end/full-day semantics, but their day arithmetic now flows through the canonical UTC owner.

Late-created positions remain protected from previous-session cleanup: EOD only schedules positions whose entry time is strictly before the resolved boundary.

### Daily Loss

Daily-loss accounting now uses one UTC-day identity for:
- baseline validity;
- realized-trade filtering;
- cash-flow filtering;
- persisted state;
- lock/alert identity.

The persistent key is now account-scope aware using broker/account-type/live-vs-demo identity. A read of the former account-number-only key remains available solely for one-time safe migration, after which canonical storage is used.

Realized P/L and floating P/L remain separate inputs to the existing DailyLossRule; no threshold change was introduced.

### History / 90-day archives

Outcome archives and runtime logs use the same deterministic 90-day period owner. Existing files are not deleted by this phase.

The existing buffered writer remains the only archive mutation path; the History location marker remains startup-only.

### Time normalization

Relevant signal timing, broker refresh, calculation-readiness, news-state, news-calendar, news-protection and buffered-persistence owners now use CanonicalTimeRule instead of machine-local `DateTime.ToUniversalTime()` conversion.

This prevents a machine's local Windows timezone from changing trading-day identity when a timestamp arrives with `DateTimeKind.Unspecified` while the algo itself is UTC.

## Deterministic runtime contracts

`M4TimeHistoryContracts` verifies:
- exact UTC midnight transitions;
- start-inclusive/end-exclusive interval semantics;
- same-day and overnight sessions;
- EOD boundary/pre/post windows;
- deterministic 90-day archive period rotation;
- UTC behavior across DST calendar transition dates;
- stable broker/account/live-state persistence scope identity.

## Full-chain audit

Every M4 verification cycle rechecks:

Market/MTF preparation → Decision → Prediction → Plan → Actionability → Signal presentation → Popup/Sound → Indicator provider → cBot → broker lifecycle → outcome/history attribution.

M4 also explicitly checks that time/session changes do not introduce a second execution path or change cBot broker-mutation ownership.

## Verification boundary

Required repository gates:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build.

Target-terminal evidence remains required for exact broker session-time behavior, restart-mid-day persistence, history-folder creation/readback, EOD cleanup timing and cBot reconnect.

## Operator action

After merge to `main`:

`git pull --ff-only`