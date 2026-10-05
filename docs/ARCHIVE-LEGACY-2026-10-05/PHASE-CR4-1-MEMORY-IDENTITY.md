# CFIP — CR4.1 Learning-Memory Identity and Account Scoping

## Status

COMPLETE — merged via PR #102; no retrievable CI status was available at closeout.

CR4.1 covers Prompt 4 / D1 only.

## Findings reconciled

| ID | Original label | Own verification | Result |
|---|---|---|---|
| D1.1 | presentation parameters contaminate fingerprint | Confirmed | Fixed |
| D1.2 | memory not account-scoped | Confirmed | Fixed |
| D1.3 | PositionId can collide across accounts | Confirmed as key-isolation risk | Fixed with account-scoped identity and conservative legacy ownership validation |
| D1.4 | schema migration missing | Confirmed | Fixed with schema v2 + legacy reader |
| D1.5 | archive/runtime prefixes follow old identity | Confirmed | Fixed |
| D1.6 | portable snapshot identity follows old fingerprint | Confirmed | Fixed |

## Implementation

Canonical owner: src/CFIP.Indicator/Core/Math/OutcomeMemoryIdentityRule.cs

Updated consumers:
- OutcomeMemoryStore.cs
- OutcomeHistoryArchiveStore.cs
- RuntimeLogPersistence.cs
- PortableMemorySnapshotStore.cs
- OutcomeMemoryAccountSwitch.cs
- RuntimeInitialization.cs

Deterministic behavioral contract: tools/CFIP.Runtime.Contracts/Program.cs

Static gate: tools/audit_phase_4_1.py

## Behavioral changes

1. Yes — presentation/alert/display-only parameter changes no longer create a new learning fingerprint.
2. Yes — persistence identity is now account-scoped using broker/account number/account type/live-demo state.
3. Yes — legacy records without account metadata are imported only when current-account broker History confirms the PositionId.
4. No public Parameter name/type/DefaultValue values were changed, and no RR/confidence/threshold tuning was performed.

## Manual cTrader verification

- exact installed cTrader version/build;
- switching between two accounts with the same symbol/timeframe;
- persistence across restart for each account independently;
- legacy migration when current-account History contains the PositionId;
- legacy migration when current-account History does not contain the PositionId;
- actual filesystem/storage location remains CR4.2.

## New bugs observed but not fixed

- relative History path and AccessRights behavior remain CR4.2 scope;
- conservative legacy migration can discard observations no longer represented in broker History.

## Completion boundary

Repository implementation and required verification assets are complete. GitHub did not expose retrievable CI status records for the merge at closeout, so no CI PASS is claimed. Target-terminal account-switch/persistence evidence remains a manual acceptance boundary.

Next phase: CR4.2 — File/archive path and persistence observability (D2).
