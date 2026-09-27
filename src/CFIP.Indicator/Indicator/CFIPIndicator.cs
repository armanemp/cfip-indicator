using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        //    liquidity sweep, FVG confluence, remaining ratio, body/impulse ratio,
        //    age decay) — the composite weighting is sound, no bug found.
        
        //  - Reviewed BuildExecutionModel (M5/M15 FVG+OB confluence zone, ideal
        //    entry, trigger/invalidation, inside-zone vs breakout logic),
        //    PremiumDiscountBias, and TriggerReadyWithoutPrecisionGate in depth —
        //    all structurally sound, no bugs found.
        //  - ENHANCEMENT: added an M30-alignment quality bonus to the precision
        //    entry score, mirroring the existing M15 bonus (M30 direction was
        //    already computed elsewhere in the engine but never checked here) —
        //    a precision entry can no longer score high confluence while fighting
        //    the M30 trend outright.
        
        //  - Reviewed CalculateProtectedStop (break-even, structural swing trail,
        //    smart-exit tightening, final monotonic BetterStop guard),
        //    SpreadAllowed, and CalculateVolume/VolumeForFixedRisk position sizing
        //    (including optional spread-inclusive risk sizing) — all sound, no
        //    bugs found.
        //  - ENHANCEMENT: added a daily loss circuit-breaker. New parameters
        //    "Enable Daily Loss Limit" (default on) and "Maximum Daily Loss
        //    Percent" (default 3.0%). Once today's equity drawdown from the day's
        //    starting equity reaches that percentage, new auto-trade entries
        //    (normal and aggressive) are blocked for the rest of the day and a
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
    }
}
