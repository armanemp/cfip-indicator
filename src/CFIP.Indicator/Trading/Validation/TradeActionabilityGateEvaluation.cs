using cAlgo.API;

using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeActionabilityResult EvaluatePreparedTradeActionability(
            TradeActionabilityEvaluationState state)
        {
            if (state == null)
                return TradeActionabilityResult.Blocked(
                    "ACTIONABILITY STATE UNAVAILABLE");

            if (!string.IsNullOrEmpty(
                    state.IndicatorGateReason))
                return state.ToBlockedResult(
                    state.IndicatorGateReason,
                    true);

            if (!state.QualityReady)
                return state.ToBlockedResult(
                    "EXECUTION ZONE QUALITY",
                    true);

            if (!IsActionabilityTriggerReady(
                    state.ClosedM5,
                    state.Direction,
                    state.LiveMode))
                return state.ToBlockedResult(
                    "M5 TRIGGER",
                    true);

            if (state.LiveMode ==
                ExecutionMode.WaitingForTrigger)
                return state.ToBlockedResult(
                    "WAITING FOR TRIGGER",
                    true);

            if (state.LiveMode ==
                ExecutionMode.None)
                return state.ToBlockedResult(
                    "OUTSIDE EXECUTION WINDOW",
                    true);

            if (state.Tp1RR <
                state.RewardRisk.RequiredRR)
                return state.ToBlockedResult(
                    "RR BELOW ACTIONABLE FLOOR",
                    true);

            if (state.Late)
                return state.ToBlockedResult(
                    "LATE / PRICE EXTENDED",
                    true);

            if (state.MicroConflict)
                return state.ToBlockedResult(
                    "ENTRY MOMENTUM CONFLICT",
                    true);

            if (state.OpposingRegularDivergence &&
                state.Divergence.Quality >= 78)
                return state.ToBlockedResult(
                    "OPPOSING REGULAR DIVERGENCE",
                    true);

            if (state.TrapRisk.Block &&
                EntryActionabilityPolicy.ShouldBlockTrapRisk(
                    state.LiveMode))
                return state.ToBlockedResult(
                    EntryTrapRiskPolicy.FormatPresentationReason(
                        state.TrapRisk.Reason,
                        state.TrapRisk.Context),
                    true);

            if (state.LocationQuality <
                ActionabilityThresholdPolicy.EffectiveUpstreamEntryLocationQuality(
                    MinimumEntryQuality))
                return state.ToBlockedResult(
                    "ENTRY LOCATION QUALITY",
                    true);

            if (state.TimingQuality <
                ActionabilityThresholdPolicy.EffectiveUpstreamEntryTimingQuality())
                return state.ToBlockedResult(
                    "ENTRY TIMING",
                    true);

            return new TradeActionabilityResult(
                true,
                state.LocationQuality,
                state.TimingQuality,
                state.PricePositionQuality,
                state.EntryDistanceAtr,
                state.Tp1RR,
                state.Divergence.Quality,
                state.Divergence.Direction,
                state.Divergence.Type,
                state.SupportiveHiddenDivergence
                    ? "ACTIONABLE • HIDDEN DIVERGENCE CONFIRM"
                    : "ACTIONABLE");
        }
    }
}
