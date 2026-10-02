# MTF-P2 — Primary M15/H1 Location Evidence: OB/FVG Provenance

Date: 2026-10-02

## Goal

Strengthen the primary M15/H1 signal representation by preserving the actual
source-timeframe FVG and Order Block evidence all the way into the scenario candidate,
display ordering and panel diagnostics.

## Design

The existing canonical `LocationEvidenceRule` remains the only owner of location
scoring. This phase does not create a competing OB/FVG score.

For each M15/H1 primary candidate:
- source FVG quality is copied from the selected source Frame;
- source Order Block quality is copied from the selected source Frame;
- source OB+FVG confluence is copied from the selected source Frame;
- primary location quality is the canonical LocationEvidenceRule score;
- source confluence and location quality are used for presentation priority.

This makes the evidence attributable to M15/H1 rather than silently collapsing it into
the M5 scenario metadata.

## Panel

The primary candidate summary now shows:
- `OB+FVG <quality>` when both source zones confluence;
- `OB <quality>` when only source OB is present;
- `FVG <quality>` when only source FVG is present;
- existing M5/M1 tuning state remains visible.

## Safety

No public parameter changed. The public parameter count remains 568.

No RR, Entry, SL, TP, confidence, risk, execution or trading threshold changed.

No broker mutation or second decision authority was added.

The primary M15/H1 candidate remains an analytical/presentation candidate and the
non-M5 execution policy remains observe-only.

## Verification

Repository gates:
- accumulated Source/Architecture audit;
- Runtime Acceptance Contracts;
- cTrader Compile/Build.

Manual target-terminal boundary:
- verify that M15/H1 source zones correspond to the correct closed frame;
- verify simultaneous M15/H1 presentation;
- verify panel readability;
- measure empirical signal outcomes before any later threshold adjustment.

## Operator action

After merge to main, run: `git pull --ff-only`.
