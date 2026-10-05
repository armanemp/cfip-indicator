# CFIP — Volume Profile Evidence Integration — 2026-10-03

Status: IMPLEMENTATION COMPLETE on the realtime/live hardening branch; repository verification is required before merge; target-terminal validation remains manual.

## Objective

Use the previously supplied cTrader Volume Profile implementation as an analytical evidence source for the CFIP opportunity engine without copying its heavy chart-rendering path into the production hot path.

## Implementation

- Added a platform-neutral VolumeProfileSnapshot carrying POC, VAL, VAH, profile range and volume statistics.
- Added a bounded VolumeProfileAnalyzer using recent closed M15 bars and tick-volume distribution across price bins.
- Added a 70% value-area calculation that expands from the POC toward the higher-volume adjacent side first.
- Added VolumeProfileEvidenceRule for directional location context:
  - BUY near VAL;
  - SELL near VAH;
  - directional acceptance just outside value;
  - proximity to the high-volume POC.
- Added closed-M15 caching so the profile is not recomputed on every tick/candidate.
- Added auditable Volume Profile fields to each TradeOpportunityCandidate.
- Added only a small bounded quality lift plus ranking bonus. Volume Profile never becomes a standalone directional authority and does not bypass RR, risk, MTF, actionability or execution-policy gates.
- The same evidence flows through current actionable candidates and future pending scenarios because both use the canonical opportunity builder.

## Source provenance

The user-supplied Volume Profile source was inspected from the project file volume-profile.txt. Its implementation contains price-binned volume distribution, POC selection and a 70% Value Area calculation. The CFIP integration deliberately uses these analytical concepts without importing the original chart-rendering workload.

## Performance

The profile is:
- M15 based;
- closed-bar based;
- bounded to 96 M15 bars;
- bounded to 48 price bins;
- cached by the closed M15 index.

This keeps it outside the per-tick structural rebuild path while still updating when a new M15 context becomes available.

## Safety

No existing quality/RR/risk threshold is lowered.
Volume Profile is evidence, not an execution bypass.
The cBot remains the only broker mutation owner.
M15 remains the canonical decision/execution reference; M5 remains trigger/tuning; M1 optional.

## Verification

Required:
- Source/Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- dedicated Volume Profile audit;
- target-terminal confirmation that live calculations remain responsive and candidate quality/placement behaves as expected.

Phase audit:
tools/audit_phase_volume_profile_evidence_2026_10_03.py

## VOLUME PROFILE EVIDENCE
This phase remains part of the consolidated realtime/live intelligence architecture and uses the canonical opportunity candidate builder and ranking path.
