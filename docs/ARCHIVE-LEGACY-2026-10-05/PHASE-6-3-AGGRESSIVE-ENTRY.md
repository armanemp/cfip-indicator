# CFIP — Phase 6.3 Controlled Intrabar Aggressive Entry Policy

## Purpose

Phase 6.3 removes the previous ambiguous mixed-bar meaning of aggressive automatic entry.

The selected policy is:

**CONTROLLED INTRABAR**

The live M5 reaction is the trigger source, while the closed-bar decision/M5 context remains the structural and execution-planning reference.

## Authoritative policy

`Core/Execution/AggressiveEntryPolicy.cs` is the single qualification owner.

An aggressive reaction can arm only after **two distinct qualifying reaction observations** on the same current open M5 bar.

A repeated observation with the same reaction sample timestamp does not count again.

## Invalidation

Qualification is reset immediately when:

- reaction direction changes;
- reaction is no longer `EntryAllowed`;
- the current open M5 bar changes;
- a confirmed aggressive fill is adopted.

This prevents a stale intrabar reaction from remaining armed after its conditions disappear.

## Execution boundary

The policy does not replace existing execution controls.

After qualification, the existing aggressive execution path still owns:

- automatic-entry safety gates;
- market-hours and capacity guards;
- risk sizing;
- structural stop construction;
- structural target selection;
- execution-intent validation;
- unified submission gate;
- broker-confirmed fill adoption;
- fill-envelope validation;
- broker protection/recovery.

The aggressive market order remains broker-confirmation driven.

## Parameter contract

No new public parameter was introduced.

The existing 535-parameter production contract remains unchanged.

## Scope boundary

This phase does not alter:

- confirmed decision thresholds;
- evidence weights;
- general RR policy;
- risk-policy formulas;
- trailing/profit-lock rules;
- broker mutation ownership;
- managed identity;
- automatic execution authority.

It only makes the already-existing aggressive intrabar path temporally explicit and controlled.
