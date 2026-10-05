# CR6.5 / F6 — Trap-risk/trigger exceptions and actionability constant ownership

Date: 2026-10-01

Status: **VERIFIED COMPLETE on PR #131**, final implementation head `01db0f46f2a19597be5428a62a230ebf67a7f36c`.

## Phase acknowledgement

تأیید می‌کنم — the current main call chain was audited before implementation, including Breakout, Retest, Pending and Aggressive consumers.

## Findings reconciled

### Breakout
`TradeActionabilityEvaluator` blocks trap-risk for non-Breakout modes only. Breakout remains an explicit policy exception and was not converted into a newly blocked path.

### Retest
The execution-mode resolver can identify a Retest candidate while the price is inside its zone and the trigger has not been reached. This does not create a canonical trade plan by itself: downstream plan creation and decision gating continue to require `Decision.TriggerReady`.

### Entry geometry
The established distinctions are preserved:
- Breakout anchor → trigger;
- Retest/Waiting anchor → ideal entry, with preview fallback;
- Waiting actual entry → preview entry;
- market modes actual entry → market price;
- Breakout late-entry threshold → strict `>` against the larger of configured extension and 0.10 ATR floor;
- Retest late-entry threshold → strict `>` against the larger of configured distance and 0.05 ATR floor;
- trigger tolerance → max(tick size, 10% of pip size).

### Trap/actionability ownership
Hard-coded F6 constants are now centrally named in Core `EntryActionabilityPolicy`, while indicator actionability thresholds remain in `ActionabilityThresholdPolicy`. No numerical tuning was introduced.

## Verification evidence

- Source/Architecture: PASS — `36856702821`
- Runtime Acceptance Contracts: PASS — `36856702812`
- cTrader Compile/Build: PASS — `36856702767`
- Accumulated phase audits through F6: PASS, including `audit_phase_6_5.py`

## CI corrections recorded

The phase required and completed four small verification corrections:
1. unique helper names after architecture duplicate-name detection;
2. M1 strong-adverse contract aligned to the actual 0.40 ATR boundary;
3. Decision.Contracts project explicitly includes `EntryActionabilityPolicy`;
4. resolver/audit ownership and F6 regex assertions corrected.

These were fixed in separate `fix(F6.x)` commits and did not retune trading behavior.

## Safety and manual boundary

No public parameter name/type/default changed. No RR/confidence/SL/TP or execution threshold was retuned. No second decision or execution authority was introduced.

Manual target-terminal acceptance remains necessary for intrabar timing, panel/chart behavior, broker lifecycle ordering, restart/reconnect and empirical signal-quality/profitability.

## Next phase

**CR6.6 / F7 — Independent-timeframe scenario semantics and duplicate-policy owners.**
