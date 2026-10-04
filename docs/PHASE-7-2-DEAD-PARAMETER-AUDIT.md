# Phase 7.2 — Dead/Unused Public Parameter Audit

Date: 2026-09-29

## Objective

Audit every public cTrader parameter and ensure that every retained setting has
an explicit runtime consumer. A parameter is considered dead/unread only when
its declared property is not referenced outside its parameter declaration file.

## Findings

Initial machine audit on 535 parameters found four candidates:

1. FullWidthLevelLines
   - The parameter was declared but the compact 40-bar renderer ignored it.
   - Decision: retain and restore its documented compatibility behavior.
   - `false` is now the default so the compact 40-bar presentation remains the
     product default; `true` explicitly requests full-width level rendering.

2. LabelLeftOffsetBars
   - The parameter existed but the label anchor was hard-coded.
   - Decision: activate it in the canonical label-anchor owner and make the
     label-box width expand with the configured offset.

3. ShowEarlyArrow
   - The parameter existed but the main signal renderer always used
     ShowSignalArrow.
   - Decision: activate it so watch-only / early state arrow visibility is
     independently controlled while confirmed/reaction arrows remain governed
     by ShowSignalArrow.

4. SmartUseClosedBarDecision
   - The parameter was explicitly labeled safety-enforced, but the architecture
     requires confirmed decision logic to remain closed-bar regardless of a
     user toggle.
   - Decision: remove the misleading public setting rather than expose a
     control that cannot safely change the enforced behavior.

## Result

Current public parameter surface:

- 534 total;
- 531 baseline parameters;
- 3 OSS extension parameters.

Machine audit on the final phase head:

- parameter declarations: 534;
- read-by-code candidates: 534;
- unused/unread candidates: 0.

## Regression protection

`tools/audit_parameters.py` now runs in the Source / Architecture workflow.
Future additions of a declared-but-unread public parameter fail CI.

The audit intentionally verifies ownership by source usage; it does not infer
behavioral correctness from a single textual reference. Semantic parameter
duplication remains the dedicated Phase 7.3 task.

## Safety / strategy boundary

No trading authority was added. No broker mutation path was added. No decision
engine was duplicated. No risk, RR or execution threshold was changed as part
of the audit.

The next strategy-quality work remains focused on semantic duplicate cleanup,
then the dedicated analytical correctness sequence, with particular emphasis
on important levels, deeper Order Block quality, signal-quality/confluence
quality and system-wide smart coordination.

## Verification

- Source / Architecture: PASS
- Runtime Acceptance Contracts: PASS
- cTrader Compile: PASS

PR #29 is the implementation PR for this phase.

## Merge record

- PR: #29
- Merge commit: `861f15dda5af4599c92acb64bb6793ed2dfc296e`
- Final pre-merge verification: Source / Architecture PASS; Runtime Acceptance Contracts PASS; cTrader Compile PASS.

## Supersession note — 2026-10-04

The historical Phase 7.2 activation decisions for `LabelLeftOffsetBars` and `ShowEarlyArrow` are superseded by the current canonical UI contracts: compact plan labels use the canonical fixed anchor owner and signal arrows are governed by the current smart signal presentation pipeline. `ShowEarlyWatch` had no production consumer. All three public parameters were removed during F1 rather than retained as dead/no-op settings. This note does not change the historical verification record above.
