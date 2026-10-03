## 2026-10-04 — M2 Repository Hygiene / Ownership Started

Branch: `phase/M2-repository-hygiene-ownership`

Status: **IN PROGRESS — not closed.**

M2 begins the first executable full-project audit after the master baseline. It explicitly audits dead code, duplicate semantic ownership, duplicate calculations/renderers/executors, build-graph reachability, conditional compilation, lifecycle duplication, audit-contract drift, documentation drift, dependency direction, hidden mutable state, and async disposal races.

The master audit has been expanded with 20 additional cross-cutting failure classes. See:
`docs/MASTER-FULL-FORENSIC-AUDIT-2026-10-04.md`

Detailed M2 checklist:
`docs/PHASE-M2-REPOSITORY-HYGIENE-OWNERSHIP-2026-10-04.md`

No production strategy tuning or parallel owner has been introduced.

## 2026-10-04 — Master Full Forensic Audit Baseline

Status: **BASELINE CREATED — audit execution pending.**

Created `docs/MASTER-FULL-FORENSIC-AUDIT-2026-10-04.md` as the master restart index.

Baseline inventory:
- 1113 repository files / 82 directories;
- 731 C# files;
- 667 Indicator C# files;
- 22 cBot C# files;
- 22 Contracts C# files;
- 204 Markdown documents;
- 155 Python audit/tool files;
- current machine-enforced public-parameter baseline: 548.

The document contains:
- complete recursive repository file inventory;
- canonical architecture and end-to-end data/execution flow;
- current ownership/source-of-truth map;
- evidenced findings from prior forensic audits;
- full numbered potential-problem/defect checklist;
- line-by-line/code-by-code review method;
- severity and closure evidence rules;
- restart order for one-complete-phase-at-a-time remediation.

This is a documentation/audit baseline only; no strategy threshold or production behavior was changed.

Operator action:
`git pull --ff-only`

## 2026-10-04 — Smart Separated Signal Arrows

Status: VERIFIED COMPLETE — PR #252 merged to main; target-terminal visual acceptance remains the final manual boundary.
Merge: PR #252, commit b095710eb83e7f93017edb1050b20079b27b4371.

Completed for this user-requested item:
- Removed the duplicate HTF arrow-strength owner. The canonical strength source is now MtfTrendStrengthRule.
- The nine-level model is one ladder: 1–3 = Weak 1/2/3, 4–6 = Medium 1/2/3, 7–9 = Strong 1/2/3.
- Strength is derived from the existing multi-timeframe market evidence stack: frame quality, directional score, trend/momentum, ADX, EMA spread/slope, structure, OB/FVG location, independent indicator evidence, live pressure and conflict penalty.
- The canonical directional arrow renderer consumes SignalVisualSnapshot.MtfTrendStrengthLevel only; it no longer recalculates a second strength value.
- The authoritative direction is resolved first and then passed into the strength evaluator, so displayed strength cannot silently belong to another direction.
- Arrow glyphs have deterministic vertical separation using both ATR-relative and minimum-pip clearance.
- M1 trigger is now a Circle precision marker rather than a second directional arrow, eliminating a major overlap/ambiguity path.
- Removed the unused duplicate MtfTrendArrowRenderer production path and its superseded HtfTrendArrowStrengthRule.
- Removed the legacy fallback arrow-state owner from canonical call-sites.
- Added a dedicated deep audit and accumulated it in Source/Architecture CI.
- Routine project audit remains mandatory: Analysis → Decision → Signal → Alert → cBot execution → Broker confirmation → Protection/Lifecycle → Outcome/History, plus performance/code-cleanliness review.

Verification:
- Branch/source consistency: PASS.
- Automated Source/Architecture + Runtime Acceptance + cTrader compile/build: PASS on final PR head.
- Target cTrader terminal visual validation remains the only manual boundary for actual glyph appearance and live chart behavior; code-side spacing/stale-object contracts are verified.

Operator action after merge: git pull --ff-only.

---

## 2026-10-03 — Single-Owner / No-Duality Repair

Status: VERIFIED COMPLETE — merged to `main` via PR #250, merge commit `2e670514e90deeb46a3f140d1383446f0292c64d`.

Root causes closed:
- cBot startup had two audio cues: canonical Start plus an immediate Live-disarmed cue.
- Active Plan and WATCH/Reaction had separate directional-arrow ownership.
- Several chart label paths retained obsolete box/vertical-offset semantics instead of one exact-price presentation.
- Historical audits still encoded superseded line/label contracts.

Corrections:
- One startup cue is owned by `CbotLifecycleAudioService.PlayStarted`; Live DISARMED remains state/panel information, not a second cue.
- Active signal arrows converge on the canonical stacked-arrow renderer.
- Signal/plan lines converge on `PlanLineRenderer`: Solid, fixed 1px, exactly 40 bars from the latest candle.
- Pending/parallel lines delegate to that owner.
- All compact labels converge on one renderer/formatter: exact signal price, white/no-background text, source timeframe once, pip distance where applicable, left-of-line with minimum horizontal gap.
- Added a dedicated Single-Owner / No-Duality source audit and accumulated it in CI.
- Resolved contradictory legacy audits without creating alternate production behavior.

Verification on final implementation head:
Source/Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS.
