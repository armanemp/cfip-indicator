# CR6.6 / F7 — Independent-timeframe scenario semantics and duplicate-policy owners

## Finding

Repository inspection confirmed that M15/M30/H1/H4/D1/W1 candidates are not independently planned scenarios. They reuse the closed-M5 execution geometry and target ladder while carrying timeframe-specific identity and evidence.

## Decision

These candidates are explicitly classified as timeframe annotations over the canonical M5 plan geometry. Their timeframe remains meaningful for evidence and presentation, but it does not imply independent Entry/SL/TP planning or broker execution authority.

## Implementation

- Added `BasePlanTimeframe` to `TradeOpportunityCandidate` and set it to `M5` for parallel candidates.
- Passed source timeframe into candidate construction before execution-policy evaluation.
- Reused a closed-M5/lane/direction preview cache so same-base timeframe annotations do not repeat full target/preview construction.
- Unified candidate eligibility and execution authorization in Core `ScenarioExecutionPolicyRule`.
- Removed the former Analysis and Trading namespace policy owners.
- Kept independent timeframe candidates structurally evaluable but execution observe-only.
- Preserved distinct scenario identity and coverage for simultaneous H1/H4/etc. observations.
- Moved the former evidence enrichment method out of the deleted policy file into a dedicated analysis partial source file.

## Safety

- No public parameter or default changed.
- No RR, confidence, SL/TP or execution threshold was tuned.
- No second decision or execution authority was introduced.
- No broker mutation path was introduced.

## Verification

- Deterministic runtime contracts cover canonical authorization, observe-only timeframe candidates, direction mismatch, scenario identity and replacement semantics.
- The accumulated Source/Architecture workflow includes `audit_phase_6_6.py`.
- Target-terminal presentation, broker lifecycle, replay and empirical signal-quality validation remain manual boundaries.
