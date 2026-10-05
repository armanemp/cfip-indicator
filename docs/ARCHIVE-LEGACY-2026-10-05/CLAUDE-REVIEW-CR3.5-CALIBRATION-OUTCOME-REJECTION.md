# Claude Review Remediation — CR3.5 Calibration, Outcome and Rejection Transparency

Date: 2026-09-30

Status: **VERIFIED COMPLETE — PR #101 merged to main as `2c31232b7d1f16e88e89e693d7d74fd8a5eedab6`.**

## Scope

Covers C8, C9 and B6 from the Claude review remediation roadmap.

## Implemented

### C8 — calibration key correctness
- `ConfidenceCalibrationKey` now implements `IEquatable<ConfidenceCalibrationKey>`.
- Equality normalizes and compares direction, lane, regime and confidence bucket structurally.
- `GetHashCode()` remains consistent with the equality contract.
- Deterministic Decision Contracts cover equal keys, hash equality and directional inequality.

### C9 — outcome/calibration transparency
- Managed outcome registration continues to derive realized net profit/pips from the aggregated broker history when available.
- `OutcomeObservation.RealizedR` remains the canonical realized-risk multiple for the closed managed outcome.
- Recent contextual calibration now exposes `AverageRealizedR` alongside observed win rate and sample count.
- Decision state and the panel expose observed win rate, average realized R and N.
- No probability wording or empirical threshold tuning was introduced.
- Existing lane eligibility semantics remain explicit across Strategic/Tactical/Counter-HTF/Pending-related execution contexts; no lane was silently excluded.

### B6 — rejection transparency
- Plan reward integrity rejection paths now emit explicit `PLAN_REWARD / REJECTED / <reason>` telemetry.
- Repeated identical rejection telemetry is bounded to one record per M5/reason pair to avoid hot-path/archive spam.
- The validator remains a validator/telemetry owner only; it does not become a second decision engine.

## Safety / compatibility

- No public parameter was added, removed, renamed, or default-tuned.
- No RR floor, stop-width threshold, confidence threshold, position capacity, or broker authority was changed.
- No new execution authority was introduced.
- Calibration remains observational and bounded by the existing calibration controls.
- Track 12A remains blocked until CR-FINAL.

## Verification contracts

The Decision Contracts now cover:
- structural equality/hash behavior for calibration keys;
- recent calibration sample selection;
- realized-R evidence propagation;
- deterministic calibration response.

A dedicated `tools/audit_phase_3_5.py` static gate is wired into Source/Architecture CI.

## Final CI verification

- Source/Architecture: PASS on final head;
- Runtime Acceptance: PASS on final head;
- cTrader Compile: PASS on final head;
- CR3.5 static audit: PASS;
- accumulated project-wide audits: PASS.

## Evidence boundary

CI can verify deterministic code contracts, aggregation ownership and telemetry wiring. It cannot prove target-terminal broker-history semantics, restart/reconnect persistence behavior, or empirical signal/profitability improvement.

No empirical improvement claim is made by this phase.
