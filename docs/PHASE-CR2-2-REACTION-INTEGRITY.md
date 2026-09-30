# CFIP — CR2.2 Reaction / Reversal Integrity

## Status
IMPLEMENTED — CI acceptance pending.

## Scope
CR2.2 covers B3 from the Claude review-remediation program:
- separate intrabar reaction observation from closed-bar reversal confirmation;
- require an actual reversal context rather than a strong single-candle score alone;
- define explicit no-zone behavior;
- resolve equal bull/bear reaction scores to neutral direction;
- reuse one canonical qualification policy for Aggressive intrabar arming and Pending reversal qualification.

## Canonical ownership
src/CFIP.Indicator/Core/Math/ReactionQualificationRule.cs is the single semantic owner for:
- reaction direction resolution;
- prior counter-move detection;
- swing interaction;
- no-zone/zone-present context qualification;
- closed-bar confirmation;
- shared quality/evidence/context qualification.

src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs remains responsible for collecting market observations and producing the reaction snapshot.

## Temporal contract
The live M5 bar is an observation surface. It can produce:
- ReactionIntrabarQuality;
- ReactionIntrabarEvidence;
- TriggerReady.

A Pending reversal is not allowed to become actionable from that moving bar alone. It uses:
- ReactionConfirmedQuality;
- ReactionConfirmedEvidence;
- ReactionConfirmedHasContext;
- ReactionClosedBarConfirmed.

Aggressive entry remains controlled intrabar, but its qualification is now evaluated through the same canonical reaction rule before the existing two-sample AggressiveEntryPolicy can arm.

## Reversal-context contract
A reversal must have real context.

When a candidate zone is present, the zone must meet the existing FastReversalMinimumZoneQuality; unrelated counter-move/swing evidence cannot bypass a weak zone.

When no zone is present, the candidate must have at least one non-zone structural context:
- prior counter-move; or
- interaction with a canonical swing.

No new public trading threshold was introduced.

## Direction conflict
Equal bullish/bearish reaction quality resolves to Direction = 0, EntryAllowed = false, and TriggerReady = false. The conflict remains diagnostically visible through the reaction reason.

## Verification
Required:
- CR2.2 static source audit;
- runtime reaction qualification contracts;
- cTrader compile/build.

Target-terminal intrabar behavior and broker semantics remain outside the claims of CI-only verification.

## Routine project audit
The phase was reviewed across:

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning

with the phase-specific change confined to reaction qualification semantics. Performance impact is bounded to the existing M5 reaction calculation and adds only canonical context checks; no new full-history scan or duplicate execution authority is introduced.