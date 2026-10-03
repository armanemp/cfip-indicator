using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeActionabilityResult BuildTradeActionabilityResult(
            int closedM5,
            int direction,
            ExecutionMode liveMode,
            int locationQuality,
            int timingQuality,
            int pricePositionQuality,
            double entryDistanceAtr,
            double tp1RR,
            DivergenceResult divergence,
            string indicatorGateReason,
            bool qualityReady,
            bool late,
            bool microConflict,
            bool opposingRegularDivergence,
            EntryTrapRiskResult trapRisk,
            double requiredRR)
        {
            if (!string.IsNullOrEmpty(indicatorGateReason))
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    indicatorGateReason);

            if (!qualityReady)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "EXECUTION ZONE QUALITY");

            if (!IsActionabilityTriggerReady(
                    closedM5,
                    direction,
                    liveMode))
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "M5 TRIGGER");

            if (liveMode == ExecutionMode.WaitingForTrigger)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "WAITING FOR TRIGGER");

            if (liveMode == ExecutionMode.None)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "OUTSIDE EXECUTION WINDOW");

            if (tp1RR < requiredRR)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "RR BELOW ACTIONABLE FLOOR");

            if (late)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "LATE / PRICE EXTENDED");

            if (microConflict)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "ENTRY MOMENTUM CONFLICT");

            if (opposingRegularDivergence &&
                divergence.Quality >= 78)
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "OPPOSING REGULAR DIVERGENCE");

            if (trapRisk.Block &&
                EntryActionabilityPolicy.ShouldBlockTrapRisk(
                    liveMode))
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    EntryTrapRiskPolicy.FormatPresentationReason(
                        trapRisk.Reason,
                        trapRisk.Context));

            if (locationQuality <
                ActionabilityThresholdPolicy.EffectiveUpstreamEntryLocationQuality(
                    MinimumEntryQuality))
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "ENTRY LOCATION QUALITY");

            if (timingQuality <
                ActionabilityThresholdPolicy.EffectiveUpstreamEntryTimingQuality())
                return new TradeActionabilityResult(
                    false,
                    locationQuality,
                    timingQuality,
                    pricePositionQuality,
                    entryDistanceAtr,
                    tp1RR,
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "ENTRY TIMING");

            return new TradeActionabilityResult(
                true,
                locationQuality,
                timingQuality,
                pricePositionQuality,
                entryDistanceAtr,
                tp1RR,
                divergence.Quality,
                divergence.Direction,
                divergence.Type,
                supportiveHiddenDivergence
                    ? "ACTIONABLE • HIDDEN DIVERGENCE CONFIRM"
                    : "ACTIONABLE");
        }
    }
}
