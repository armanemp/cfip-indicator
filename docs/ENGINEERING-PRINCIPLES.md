# CFIP Indicator — Engineering Principles

These are persistent project-level engineering rules for all future phases.

## 1. Architecture before patches

Do not add a workaround merely to make a failing path pass. First identify the correct owner and move or redesign the behavior there.

## 2. One semantic owner

Each important concept has one authoritative owner: decision, trigger, plan, execution intent, risk, broker mutation, broker-confirmed state, lifecycle and visual state.

## 3. No duplicate business logic

Do not recreate entry/SL/TP/trigger/validation formulas in renderer, alert, execution or panel code. Consumers read authoritative state.

## 4. No duplicate gates without a semantic reason

Every gate must answer one distinct question. Remove overlapping checks when they represent the same concept; keep independent safety gates explicit.

## 5. Performance is an architectural concern

Prefer asynchronous startup, bounded scans, memoized closed-state calculations, data reuse and low-cost supervision over repeated recomputation.

Hot-path work must be proportional to the new information available, not to the entire historical dataset.

## 6. Precision before complexity

A new indicator or score component is accepted only when it contributes genuinely distinct information, replaces a weaker/correlated component, or improves robustness without creating a second decision authority.

## 7. Broker state is authoritative

Planned levels and broker-confirmed levels are different states. The UI, alerts and live management must label and consume the correct state.

## 8. Closed-bar and intrabar semantics must remain explicit

Confirmed decisions use their defined closed-bar contract. Any intrabar feature must be explicitly modeled and must not silently leak into confirmed semantics.

## 9. Fail closed for automatic entry

Recoverable errors may degrade analysis, but they must not silently permit unsafe automatic entry. Safety-critical broker management must continue independently where possible.

## 10. Prove before claiming

Performance, accuracy or reliability improvements must be distinguished from architectural improvements. Accuracy claims require controlled validation/calibration; performance claims require measurable runtime evidence.

## 11. Every phase leaves continuity artifacts

Major implementation decisions, audit findings, acceptance results and next steps are recorded in the repository documentation so work can continue safely across chat sessions.

## 12. Refactor the owner instead of growing the workaround

When a file or method becomes too large or a concern crosses boundaries, extract a coherent domain owner with a semantic API rather than appending another conditional branch to the existing owner.