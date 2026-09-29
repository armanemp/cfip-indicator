# CFIP — Phase 6.2 Reaction Intrabar and Chart Observability

## Purpose

Phase 6.2 makes the temporal distinction between confirmed decision state and
live reaction state explicit, while ensuring the operator can see analysis
progress on the chart even when there is no confirmed signal or the panel UI is
not available.

## Reaction contract

`BuildReaction()` evaluates the current open M5 bar. It is therefore an
intrabar feature and is distinct from the closed-bar decision created from the
canonical MTF closed context established in Phase 6.1.

The visual snapshot now records:
- `ReactionIntrabar` — explicit intrabar semantics;
- `ReactionM5Index` — the current live M5 bar used by the reaction;
- `ArrowM5Index` — derived from the reaction identity when reaction is active.

Confirmed decision state remains closed-bar only.

## Chart observability

A persistent `CFIP_ANALYSIS_GUIDE` chart text object reports:
- engine/runtime state;
- data initialization state;
- calculation freshness or `NO CYCLE`;
- decision state;
- live reaction state and intrabar marker;
- visual stage;
- plan state;
- MTF frame direction/quality summary;
- latest evaluated closed M5 index;
- decision block reason and runtime fault state when present.

The guide is intentionally independent of `ShowUnifiedPanel` and panel control
creation. The existing verified `Chart.DrawText` API is used. This means the
chart provides a diagnostic surface even when the normal panel cannot be
created, and it makes the distinction between `NO CYCLE`, `WAIT`, `READY`,
`REACTION`, `BLOCKED` and `ACTIVE` states visible.

## Important runtime finding

The previous chart surface could legitimately appear empty because signal and
plan renderers clear their objects whenever there is no valid direction, plan or
prediction. That visual behavior was not sufficient to distinguish "no signal"
from "no calculation" or "UI failure".

Phase 6.2 addresses that observability gap without manufacturing a signal or
changing decision thresholds.

## Scope boundary

No decision thresholds, evidence weights, RR policy, risk sizing, trailing or
broker execution mutation policy is changed by this phase.