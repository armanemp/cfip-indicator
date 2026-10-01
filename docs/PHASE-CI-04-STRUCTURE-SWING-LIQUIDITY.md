# CI-04 — Structure / Swing / Liquidity Semantics

Date: 2026-10-01

Status: **IMPLEMENTED — verification pending CI.**

## Objective

Audit and harden the structural calculation chain without tuning trading
thresholds:

`swing → plateau → confirmation → structure/MSS/CHOCH → liquidity → evidence →
decision/alerts`

This phase also absorbs the cross-cutting alert-delivery synchronization defect
reported during the phase so the same causal event cannot produce unsynchronized
sound/popup behavior or duplicate structural sounds.

## Findings

### 1. Structural break freshness could repeat after a prior break

The previous `StructuralEventRule.IsFreshBreak` checked only the immediate
previous/current close pair. When price had already crossed the same structural
threshold on an earlier closed bar and later retraced, the same source swing
could become eligible again as if it were a fresh break.

The fix consolidates the enhanced contract under the canonical `IsFreshBreak` owner, which requires:

- a confirmed structural source swing;
- no earlier closed bar after source confirmation that already crossed the
  threshold;
- the current closed bar to be the first threshold crossing.

Existing threshold geometry and direction signs are preserved. This is a
freshness/state correction, not a new trading threshold.

### 2. Structure/MSS/CHOCH could produce duplicate structural alert sounds

The analysis/score path already collapsed Structure/MSS/CHOCH as one causal
structural event, while `ContextAlertEmitter` could emit a BOS alert and a
second MSS/CHOCH alert from the same M5 bar when multiple labels were true.

The structural alert path now emits at most one user-facing event per closed M5
direction. The existing alert controls remain respected: a configured BOS
alert is preferred when enabled; otherwise a configured MSS/CHOCH alert can
represent the same event.

### 3. Audio and popup delivery were on different timing paths

`AlertEngine.SendUnifiedAlert` previously played sound immediately during the
calculation call, while popup presentation was queued and drained on the
500-ms runtime timer. This could make the audible cue precede the visible popup.

The delivery path is now unified:

`SendUnifiedAlert → AlertDeliveryQueue → delivery boundary →
popup update → sound`

Calculation cycles drain the queue after calculation/presentation; timer-owned
alerts drain it after timer supervision. Only the delivery processor owns
`Notifications.PlaySound`. A queued alert is not held behind the previous
popup: the current event replaces the popup at the same delivery boundary, then
its sound cue is emitted from that exact event.

### 4. Explicit restriction alerts were accidentally dead

`ProcessRestrictionAlert` created `RESTRICT|` events, but
`SendUnifiedAlert` returned before delivery for every `RESTRICT|` event.
The configured restriction-alert controls and popup option were therefore
effectively unreachable.

The sender now keeps blocked trade/signal side effects suppressed while allowing
explicitly configured restriction diagnostics to reach the normal alert
delivery path.

## Structural semantics covered

- canonical swing plateau grouping;
- fixed-anchor equality semantics;
- confirmation lag;
- closed-index/future-bar protection;
- equal-high/low tolerance ownership;
- active/unbroken liquidity state;
- fresh sweep validation;
- structural direction symmetry;
- structure/MSS/CHOCH evidence de-duplication.

## Alert semantics covered

- one canonical alert delivery queue;
- bounded critical-priority behavior;
- calculation/timer delivery boundary alignment;
- popup-before-sound ordering;
- semantic sound-cue preservation;
- no direct sound calls in `AlertEngine`;
- no direct popup rendering in `AlertEngine`;
- one structural alert event per causal closed-bar break;
- explicit restriction alerts remain configuration-gated.

## Deterministic acceptance contracts

Extended Runtime Acceptance coverage verifies:

- bullish/bearish fresh structural breaks;
- bullish/bearish re-break rejection after an earlier threshold crossing;
- canonical structural event identity;
- swing/plateau and equal-level semantics;
- active/unbroken liquidity symmetry;
- bounded alert queue semantics;
- critical-priority delivery;
- retention of sound/popup intent in the same queued event.

## Static audit

Added:

`tools/audit_phase_ci_04.py`

The accumulated CI workflow executes it immediately after
`audit_phase_ci_03.py`.

The phase audit also checks the full alert path for duplicate sound owners,
stale popup-only queue owners, and calculation/timer delivery boundaries.

## Performance / cleanup

- Alert delivery no longer performs popup creation and sound emission in the
  analytical hot path separately; one bounded event object is queued.
- The queue is fixed-capacity and uses O(1) enqueue/dequeue operations.
- Structural freshness uses only the interval between one confirmed swing and
  the candidate break; it does not scan unrelated future bars.
- No persistent history scan, new I/O path, or public parameter was introduced.
- Old popup-only queue/processor owners were removed rather than retained as
  compatibility aliases.

## Safety / invariants

No change was made to:

- public parameter names/types/defaults;
- confidence/score weights;
- RR/SL/TP numerical thresholds;
- risk limits;
- broker mutation permissions;
- execution ownership.

The indicator remains the analytical/alert authority. This phase does not
reintroduce a second execution engine.

## Verification boundary

Repository-level Source/Architecture, Runtime Acceptance, and cTrader Compile/
Build gates must pass before merge.

Target-terminal validation remains required for:

- actual sound latency against rendered chart state;
- popup rendering timing under heavy terminal load;
- multiple simultaneous alert bursts;
- broker/event callbacks that occur outside Calculate;
- replay-level structural event frequency and missed/repeated break rates.

