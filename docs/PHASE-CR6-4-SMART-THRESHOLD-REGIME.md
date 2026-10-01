# CR6.4 / F5 — Smart-threshold regime identity and hidden REVERSAL dead path

تأیید می‌کنم — محدوده اجرای این فاز در MarketRegimeClassifier.cs، FrameRegimeResolutionRule.cs، SmartThresholdPolicy.cs، SmartThresholdPolicyRule.cs، MarketRegimeIdentity.cs و مصرف‌کننده‌های regime بازبینی‌شده است؛ هیچ پارامتر عمومی یا آستانه معاملاتی تغییر نمی‌کند.

Date: 2026-10-01
Status: VERIFIED COMPLETE at repository level.

## Finding reconciliation

MarketRegimeClassifier has six canonical market regimes: TREND, EXPANSION, RANGE, TRANSITION, HIGH_VOLATILITY and COMPRESSION. UNKNOWN is the fail-safe state when classification input is unavailable.

The prior adaptive Smart Threshold code contained a REVERSAL branch, but the canonical classifier never emitted REVERSAL and the frame resolver did not recognize it. The branch was therefore dead in the canonical regime path.

## Implementation

MarketRegimeIdentity centralizes the regime vocabulary and normalization.

MarketRegimeClassifier now returns canonical identity constants while preserving the existing classification predicates and thresholds.

SmartThresholdPolicyRule preserves the existing numerical behavior:
- TREND/EXPANSION reduce quality/share/edge by the existing buffer formulas;
- RANGE increases quality/share/edge by the existing buffer formulas;
- COMPRESSION increases quality/share/edge by the existing buffer formulas;
- HIGH_VOLATILITY, TRANSITION, UNKNOWN and future values remain on the base threshold path;
- AdaptiveSmartThresholds=false returns the configured base thresholds.

SmartThresholdPolicy.cs is now only the indicator-facing adapter to the Core rule.

Reviewed regime consumers reference the same canonical identity constants, including frame normalization, evidence fusion, no-trade regime filtering and market-regime snapshot initialization.

## Deterministic evidence

Runtime Contracts verify every canonical regime, normalization behavior, REVERSAL-to-UNKNOWN safety, future-regime fallback, legacy numeric adjustment values, disabled adaptation and direction-neutral symmetry.

tools/audit_phase_6_4.py verifies the identity owner, classifier output set, dead-branch removal, consumer migration, runtime-contract wiring and Source/Architecture CI order.

## Safety / non-goals

- No public [Parameter] name/type/DefaultValue changed.
- No default Smart Threshold, regime threshold, RR, confidence or execution threshold changed.
- No new decision or execution authority introduced.
- REVERSAL remains valid for execution/reversal semantics elsewhere in the project; it is not a market-regime identifier.
- No empirical signal-quality or profitability claim is made from repository contracts alone.

## Verification boundary

Repository gates: Source/Architecture, Runtime Acceptance Contracts, cTrader Compile.

Manual acceptance: target-terminal regime timing/presentation, restart/reconnect and empirical signal-quality/profitability.

## Performance / cleanliness audit

- indicator-facing Smart Threshold code is a thin adapter;
- regime strings are centralized instead of duplicated;
- the Core rule is pure and introduces no per-bar state, I/O or execution mutation;
- no additional market-history scan was introduced.

Next phase: **CR6.5 / F6 — Trap-risk/trigger exceptions and actionability constant ownership.**
