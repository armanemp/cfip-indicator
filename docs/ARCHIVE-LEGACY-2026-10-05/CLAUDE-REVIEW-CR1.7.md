# CR1.7 — Threshold Truth + Volume Audit

Date: 2026-09-30

## Review confirmations

**تأیید می‌کنم — A9:** `AutomaticMarketPreTradeEligibility.cs` line 188/203 contained fixed IndicatorConfluenceQuality 60 and IndicatorConflict 52; other cited clamps were present at the identified callers.

**تأیید می‌کنم — A10:** `VolumeSizer.cs` line 43 called `VolumeForFixedRisk` without validating positive stop risk first; `AggressiveVolumeSizer.cs` lines 33/39 lacked final broker min/max validation and used a silent catch.

## Implementation

- `ExecutionThresholdPolicy.cs` is the single explicit owner for current execution threshold constants and parameter-boundary guards.
- `VolumeSizingRule.cs` is the platform-neutral owner for finite/positive sizing invariants.
- Broker-specific volume normalization remains in the Indicator; this phase does not move broker authority.

## Behavioral changes

- Aggressive sizing can now fail closed when normalized volume is outside broker bounds instead of returning an invalid value.
- Risk-percent sizing no longer delegates invalid/non-positive stop risk to broker/API failure.
- DirectionShare values above 90 can now use the public parameter range through 95; this corrects the prior hidden ceiling.
- Programmatic risk-percent values above the public 5% ceiling are defensively capped at 5%.

## Verification

Deterministic Runtime Contracts cover threshold constants/bounds, non-finite inputs, stop-risk validation, risk amount calculation, normalized broker volume bounds and risk-percentage ceiling.

Target-terminal verification is still required for broker-specific volume step/normalization behavior and live account/risk conversion.

## Next

CR1.8 — Managed identity boundary.