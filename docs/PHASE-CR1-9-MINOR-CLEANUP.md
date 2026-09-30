# CR1.9 — Minor Cleanup and Documentation

Date: 2026-09-30

Status: IMPLEMENTED — CI validation pending.

## Scope

CR1.9 closes the remaining A12 cleanup/documentation items without changing public trading thresholds or creating a new execution authority.

### Parameter-count truth

The repository now has a machine-enforced parameter inventory audit:

```
python tools/audit_parameter_count.py
```

The audit counts public `[Parameter(...)]` properties from the authoritative parameter source tree, rejects duplicate public parameter names, reads the documented count from README, and fails when README and source disagree.

Current machine-derived count: **567**.

### MaximumOpenPositions

`MaximumOpenPositions` is intentionally single-valued for the current certified architecture:

- `DefaultValue = 1`;
- `MinValue = 1`;
- `MaxValue = 1`.

It represents the existing single-plan/single-position capacity. It is not a dormant multi-position control and must not be expanded as part of later cBot separation.

### Session minute resolution

The public session parameters are integer UTC hours:

- `SessionStartUtc`;
- `SessionEndUtc`.

Therefore their effective configuration resolution is **60 minutes**.

The canonical `SessionWindowRule` now exposes `SessionResolutionMinutes = 60` and uses that invariant for hour-to-minute conversion. The rule semantics remain unchanged:

- start is inclusive;
- end is exclusive;
- start == end represents the full-day session;
- overnight sessions cross midnight deterministically;
- all comparisons are normalized to UTC.

The existing `TimeWindowParser` can represent arbitrary `HH:MM` values for other time-window strings, but that does not change the public session-parameter resolution.

## Verification

The runtime contract suite explicitly asserts the 60-minute session resolution in addition to the existing same-day, overnight, start==end and EOD boundary cases.

Source/Architecture runs the new parameter-count audit on every push/PR.

No target-terminal behavior is claimed from this source/CI phase.

## Permanent routine audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed for this cleanup.

Performance/code cleanliness:
- parameter counting is bounded to the declaration tree;
- no runtime hot-path I/O was added;
- session time conversion uses one named invariant;
- no Calculate-path persistence/network work was introduced.

## Acceptance

- README count is machine-checked against the source inventory.
- MaximumOpenPositions single-plan semantics are explicit and retained.
- Session minute resolution is explicit in code, documentation and runtime contract.
- No public parameter name/type/default changed.
- No new trading threshold or execution authority was introduced.
