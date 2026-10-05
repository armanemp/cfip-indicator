# CFIP — Phase 6.2 Reaction Intrabar

## Purpose

Phase 6.2 makes the temporal distinction between confirmed decision state and
live reaction state explicit.

## Reaction contract

`BuildReaction()` evaluates the current open M5 bar. It is therefore an
intrabar feature and is distinct from the closed-bar decision created from the
canonical MTF closed context established in Phase 6.1.

The visual snapshot records:

- `ReactionIntrabar` — explicit intrabar semantics;
- `ReactionM5Index` — the current live M5 bar used by the reaction;
- `ArrowM5Index` — derived from the reaction identity when reaction is active.

Confirmed decision state remains closed-bar only.

## Presentation boundary

The production indicator keeps the existing unified panel as the single
operator-facing status surface. A persistent duplicate chart-status guide was
prototyped during the phase, but live validation showed that it added
unnecessary rendering work and duplicated the panel.

The guide and its timer-driven startup catch-up were subsequently removed in a
runtime regression correction. The reaction intrabar contract remains active.

## Important runtime finding

Signal, plan and prediction renderers can legitimately have no chart objects when
there is no qualifying visual state. That is presentation behavior, not proof
that the calculation engine is idle.

The production UI therefore remains centered on the existing panel, while the
calculation freshness and runtime status continue to be rendered by the panel
heartbeat and the normal calculation path.

## Scope boundary

No decision thresholds, evidence weights, RR policy, risk sizing, trailing or
broker execution mutation policy is changed by Phase 6.2.

The later runtime regression correction changed only presentation/startup
orchestration and did not alter the closed-bar decision or intrabar reaction
semantics.
