# Opportunity Mining / Execution Zone Selection — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

## Problem

Execution zone selection was not globally comparative. FVG/OB sources were considered through a fixed priority order, and FVG selection itself favored proximity before quality. This could make a technically valid but weaker zone become the entry anchor while a stronger OB, same-frame OB+FVG or M5/M15 confluence candidate existed.

## Correction

One canonical selection rule now scores each valid execution-zone candidate with:
- actual zone quality;
- distance from current executable market price normalized by ATR;
- zone age;
- bounded M15 source priority;
- same-timeframe OB+FVG confluence bonus;
- M5/M15 overlap bonus.

The selector compares independent M5 FVG/OB and M15 FVG/OB candidates, same-frame overlap candidates and cross-timeframe overlap candidates, then selects the highest-scoring valid geometry. Execution-only FVG lookup also compares all valid FVG candidates by quality, distance and age instead of returning only the nearest FVG.

## Safety boundary

Selection quality is not execution authorization. The existing Decision, Actionability, Reward/Risk, Regime, M5 trigger, cBot preflight, broker risk and protection gates remain authoritative.

M15 remains the trade-decision/execution reference. M5 remains trigger/tuning/entry precision. M1 remains optional confirmation.

## Verification

Dedicated source audit is accumulated into CI. cTrader compile and Runtime Acceptance must pass. Target-terminal validation should compare observed zone source/quality and actionable entries against the prior implementation.

## Next

The next analytical improvement should be trace-driven candidate-family mining: identify missed good setups from persisted signal traces/outcomes, classify why they were rejected, and improve the canonical candidate families without blind threshold relaxation.

