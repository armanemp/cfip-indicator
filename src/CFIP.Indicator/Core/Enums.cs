using System;

namespace cAlgo
{
    //    one-time alert fires — open positions are never touched, consistent
    //    with the alert-only/no-auto-close preference already in place.
    
    //  - Reviewed BuildTargetLevels (swing/opposing-FVG/opposing-OB/supply-
    //    demand/liquidity/HTF/previous-period/smart-extra target aggregation)
    //    and the TP-hit alert/telemetry path — all sound.
    //  - Found the real limiter on reward: SyncBrokerTakeProfit always kept the
    //    live broker take-profit fixed at one configured stage (Auto TP Stage,
    //    TP2 by default) for the entire life of the trade, so a move with real
    //    structural room for TP3/TP4 always closed early at TP2.
    //  - ENHANCEMENT: added "Enable Dynamic TP Advance" (default off — opt-in
    //    since it changes live trade-management behavior) + "TP Advance
    //    Proximity Percent" (default 80). When enabled, once price has covered
    //    that percentage of the distance to the current effective stage AND a
    //    further stage has a genuinely farther valid price, the live target
    //    ratchets forward to that stage instead of closing there. Only moves
    //    forward, resets to the base stage on every new trade, and never
    //    touches position size — no partial closes were added.
    
    //  - FIX (CFIP-BUG-ALERT-DEDUP): SendUnifiedAlert's duplicate-suppression
    //    compared every incoming key against a single shared "last alert key",
    //    so any two DIFFERENT alert kinds firing back-to-back (e.g. a TP1 alert
    //    then a smart/reaction alert) reset that tracker and silently defeated
    //    the per-key cooldown — the same alert could then re-fire immediately
    //    instead of waiting out its cooldown. Cooldowns are now tracked per key
    //    in a dictionary, with light housekeeping (entries older than 24h are
    //    pruned once the table passes 500 keys) so it can't grow unbounded over
    //    a long-running session.
    //  - Reviewed PassesAutoTradeSafetyGuards (market hours, margin), and
    //    confirmed TryAggressiveAutoTrade runs through the same safety guards,
    //    IsAutoPlanValid, and (as of ) DailyLossLimitHit as the normal auto-
    //    trade path — no weaker gate found on the aggressive side.
    
    //  changes (auto-trading, heavily extended per request):
    //  - MAJOR ENHANCEMENT: real multi-stage partial take-profit (scale-out).
    //    New parameters "Enable Partial Take Profit" (default off — changes
    //    live trade management), "Partial Close At TP1 Percent" (default 33),
    //    "Partial Close At TP2 Percent" (default 33), and "Move To Break Even
    //    After Partial" (default on). When enabled, the moment price actually
    //    reaches TP1/TP2 on a normal auto-trade, that percentage of the
    //    ORIGINAL fill size is genuinely closed (via ClosePosition's partial
    //    overload) — locking in real, realized profit — while the remainder
    //    keeps running toward the live broker target (TP2/TP3/TP4, and can
    //    still ratchet forward under Dynamic TP Advance from ). After a
    //    partial fires, the stop is moved to break-even (only if that's
    //    actually an improvement — never loosens it). Volume math is rounded
    //    to the broker's step and never leaves an unbrokerable dust remainder
    //    (closes the rest outright instead). This only applies to the normal
}
