# CFIP UI / Signal / Alert Quality Hardening — 2026-10-03

## Scope

Integrated correction phase for the user-reported panel, alert, chart drawing and weak-signal issues. The implementation is intentionally cross-cutting: UI state, alert event identity, chart presentation and trade-facing opportunity presentation are corrected at their canonical owners.

## Implemented changes

### Panel footer
- Reduced PanelFooterMinHeight from 40px to 36px.
- Reduced visible alert-row height from 20px to 18px while retaining a 10px minimum font.
- Removed the static bottom margin from the alert rows.
- The relayout path leaves no phantom spacing after the final visible row.

### Timeframe status synchronization
- Added PanelTimeframePresentationState and PanelTimeframePresentationRule as the single presentation owner.
- MTF lamps now consume the same resolved direction/strength state used by panel text.
- The panel presentation key now includes the underlying score/trend/ADX/FVG/OB inputs that can change the displayed timeframe status, preventing a stale text row while lamps update.

### Alert deduplication
- Alert event memory remains queue/lifecycle based.
- The persistent event identity no longer depends on ContractIdentity.SignalId, which can change during provider/plan rebuilds.
- Event identity remains anchored to symbol + scenario + closed M5 + direction + semantic alert key.
- This preserves independent scenario identities while preventing same-event re-delivery caused by producer trace churn.

### Signal arrows
- Confirmed signal arrows now require the canonical ActionableNow state.
- Early/watch arrows require the existing strong-watch evidence contract.
- Early/watch chart arrows additionally require meaningful reward/RR presentation when a concrete setup/plan is available.
- Canonical signal arrows continue to use UpArrow/DownArrow and their dedicated object namespace.

### Signal lines / labels
- The public Level Line Thickness input remains backward compatible, but the canonical rendered line policy is fixed to thickness 1.
- Line style remains Solid and the existing 40-bar compact geometry is preserved.
- Existing left-of-line label anchoring remains canonical; label text is background-free and left-aligned.
- Label content continues to carry level price, distance in pips where applicable, and source timeframe.

### Weak / low-TP opportunity presentation
- Non-presentation opportunity candidates now require a finite reward distance above the canonical regime-aware reward floor.
- They also require a minimum TP1 RR of at least the configured Tp1MinimumRR with a hard floor of 2.0.
- Presentation-only primary fallbacks remain available as diagnostic source context, but no longer become user-facing trade alerts.
- This change does not replace or loosen the deeper TradeActionabilityEvaluator / reward-magnitude / range-quality gates.

### cTrader Local / Cloud boundary
- The source project keeps a stable assembly/algorithm identity (CFIPIndicator / CFIP Smart Indicator) and has no Cloud execution transport.
- Current cTrader documentation states that cloud synchronisation distributes created/installed algos and updates across cTrader apps, while custom indicators execute locally on Windows/Mac and Cloud execution is for cBots. The repeated Local/Cloud reconciliation prompt is therefore a terminal synchronization/algorithm-state concern rather than an indicator source-code transport path.
- No source-code suppression of the terminal prompt was added because that would hide the platform boundary instead of fixing it.

## Verification
- Static integrated audit added: tools/audit_phase_ui_signal_alert_quality_2026_10_03.py.
- Existing footer/alert and accumulated position/UI audits were updated for the 36px/18px compact geometry.
- Full cTrader compile/manual terminal validation remains an external acceptance step and must be recorded separately after the branch CI/build is green.

## Phase decision
The canonical owners remain:
Analysis → Decision/Actionability → Trade Plan → Signal Envelope → cBot → Broker
and
Signal Event → AlertDeliveryQueue → Panel/Sound.

No second execution owner, Cloud transport, or separate signal-analysis engine was introduced.