# F2 — Single-Owner / No-Duality / Dead-Code Closure — 2026-10-04

Status: **IMPLEMENTATION COMPLETE — repository verification pending.**

## Goal
Close a concrete F2 ownership/dead-code finding without creating a parallel implementation.

## Root cause
ParallelOpportunityRenderer contained two private helpers that had no production callers: LaneLabel(...) was a legacy label-construction path superseded by the canonical ScenarioLabelPrefix / BuildScenarioLevelLabel flow; HashSetCurrentOpportunityVisuals() was a no-op whose only call had no effect. Keeping either helper created dead surface that could later be mistaken for a presentation owner.

## Canonical correction
Removed both dead helpers from the existing ParallelOpportunityRenderer owner. No replacement renderer, formatter, state store, or fallback was added.

Existing canonical owners remain unchanged: PlanLineRenderer for line geometry; PlanLabelRenderer for native plan labels; PlanLabelFormatting for label semantics; PlanLabelAnchorCalculator for label anchor geometry; MtfTrendStrengthRule for arrow direction/strength; AlertDeliveryProcessor for Indicator sound delivery; CbotLifecycleAudioService for cBot lifecycle audio.

## Regression
Extended audit_phase_single_owner_duality_2026_10_03.py so the removed dead/no-op helper surface cannot silently return.

## Interaction audit
No decision, signal, Entry/SL/TP/RR, MTF, cBot execution, broker lifecycle, alert semantics, or chart geometry contract changed. Parallel opportunities still render through the existing canonical line/label owners.

## Performance
Removed one dead method and one no-op call from the structural render path. No new allocation, calculation, chart object, or timer was introduced.

## Verification boundary
Required before merge: Source / Architecture; Runtime Acceptance; cTrader Compile/Build; accumulated single-owner/dead-code audit. Target-terminal visual acceptance remains a separate manual boundary and is not claimed by repository CI.

## F2 verification correction — Runtime Contract false negative

The F2 verification log exposed a pre-existing CI-17A contract defect: the Runtime Contract and accumulated CI-17A audit matched the panel restore timestamp assignment using an exact whitespace/newline string. The production owner correctly resets the canonical panel-content timestamp to DateTime.MinValue, but harmless formatting changes caused the contract to fail.

Correction: both checks now validate the semantic presence and ordering of the canonical state reset rather than source indentation. No production panel behavior, renderer, refresh cadence, or fallback path was changed.

The 152 CS0649 warnings observed when compiling the Runtime Contracts harness are recorded as a separate warning-quality finding; they were not suppressed or relabeled as errors in this correction.
