using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeActionabilityResult EvaluateTradeActionability(
            int closedM5,
            int direction,
            OpportunityLane lane,
            string regime,
            ExecutionModel execution,
            TradeSetupPreview preview)
        {
            if (_m5Bars == null ||
                execution == null ||
                preview == null ||
                (direction != 1 &&
                 direction != -1))
                return TradeActionabilityResult.Blocked(
                    "SETUP UNAVAILABLE");

            double atr = Atr(_m5Bars, closedM5);
            if (!IsFinitePositive(atr))
                return TradeActionabilityResult.Blocked(
                    "ATR UNAVAILABLE");

            double market =
                NormalizePrice(
                    direction == 1
                        ? Symbol.Ask
                        : Symbol.Bid);

            if (!IsFinitePositive(market))
                return TradeActionabilityResult.Blocked(
                    "QUOTE UNAVAILABLE");

            bool continuation =
                IsContinuationExecutionContext(direction);

            EntryGeometrySnapshot geometry =
                EntryGeometryRule.Evaluate(
                    direction,
                    ExecutionMode.None,
                    market,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    execution.ZoneTolerance,
                    execution.IdealEntry,
                    execution.Trigger,
                    preview.Entry,
                    atr,
                    Symbol.TickSize,
                    Symbol.PipSize,
                    AllowPrecisionBreakoutEntry,
                    continuation,
                    MaximumEntryExtensionAtr,
                    MaximumEntryDistanceAtr);

            if (!geometry.IsValid)
                return TradeActionabilityResult.Blocked(
                    geometry.Reason);

            bool insideZone =
                geometry.InsideZone;

            ExecutionMode liveMode =
                geometry.Mode;

            if (!execution.Ready)
                return TradeActionabilityResult.Blocked(
                    "EXECUTION MODEL NOT READY");

            double entryDistanceAtr =
                geometry.EntryDistanceAtr;

            double zoneDistanceAtr =
                geometry.ZoneDistanceAtr;

            double actualEntry =
                geometry.ActualEntry;

            bool qualityReady =
                !RequirePrecisionEntry ||
                execution.Quality >=
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(
                    MinimumEntryQuality);

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    actualEntry,
                    preview.Stop,
                    preview.Tp1,
                    atr,
                    Math.Max(
                        0,
                        Symbol.Ask - Symbol.Bid),
                    Math.Max(
                        Tp1MinimumRR,
                        MinimumRequiredRRForRegime(regime)),
                    PreferredStopRiskAtr,
                    StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                        MinimumSlAtr,
                        MaximumSlAtr,
                        MaximumStructuralStopAtr),
                    Math.Max(0, MaximumRewardRR),
                    Symbol.PipSize);

            double risk =
                rewardRisk.Risk;

            double tp1RR =
                rewardRisk.NominalRR;

            DivergenceResult divergence =
                _m5Frame == null
                    ? DivergenceResult.CreateNoDivergence()
                    : new DivergenceResult(
                        _m5Frame.DivergenceDirection,
                        _m5Frame.DivergenceQuality,
                        _m5Frame.DivergenceType,
                        _m5Frame.RegularDivergenceBull,
                        _m5Frame.RegularDivergenceBear,
                        _m5Frame.HiddenDivergenceBull,
                        _m5Frame.HiddenDivergenceBear);

            if (!rewardRisk.Allowed)
            {
                return new TradeActionabilityResult(
                    false,
                    0,
                    0,
                    0,
                    entryDistanceAtr,
                    Math.Max(
                        0,
                        tp1RR),
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    rewardRisk.Reason);
            }

            // M15 is the canonical execution timeframe. M5/M1 remain
            // defensive tuning inputs and may only refine/block an M15 setup;
            // H1+ remains higher-timeframe context for reward-path selection.
            int primaryM15Index =
                _lastMtfClosedContext == null
                    ? -1
                    : _lastMtfClosedContext.M15;

            if (_m15Frame == null ||
                primaryM15Index < 0 ||
                _m15Frame.Index != primaryM15Index)
            {
                return new TradeActionabilityResult(
                    false,
                    0,
                    0,
                    0,
                    entryDistanceAtr,
                    Math.Max(
                        0,
                        tp1RR),
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "M15 EXECUTION FRAME UNAVAILABLE");
            }

            if (_m15Frame.Direction != direction)
            {
                return new TradeActionabilityResult(
                    false,
                    0,
                    0,
                    0,
                    entryDistanceAtr,
                    Math.Max(
                        0,
                        tp1RR),
                    divergence.Quality,
                    divergence.Direction,
                    divergence.Type,
                    "M15 EXECUTION DIRECTION CONFLICT");
            }

            int locationQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        execution.Quality +
                        (insideZone ? 10 : 0) -
                        (int)Math.Round(
                            Math.Min(
                                30,
                                zoneDistanceAtr * 30))));

            int timingQuality =
                insideZone
                    ? 95
                    : liveMode ==
                      ExecutionMode.BreakoutMarket
                        ? 85 -
                          (int)Math.Round(
                              Math.Min(
                                  35,
                                  entryDistanceAtr * 25))
                        : 55 -
                          (int)Math.Round(
                              Math.Min(
                                  30,
                                  zoneDistanceAtr * 25));

            int pricePositionQuality =
                100 -
                (int)Math.Round(
                    Math.Min(
                        70,
                        entryDistanceAtr * 45));

            double triggerExtensionAtr =
                geometry.TriggerExtensionAtr;

            bool late =
                geometry.IsLate;

            bool retestTrapContext =
                liveMode == ExecutionMode.RetestMarket &&
                insideZone;

            double adverseM5Atr =
                ResolveAdverseM5Atr(
                    _m5Bars,
                    closedM5,
                    direction,
                    atr);

            bool m5AdverseEvidenceKnown =
                closedM5 >= 2 &&
                adverseM5Atr > 0;

            bool m5AdversePreZone =
                retestTrapContext &&
                m5AdverseEvidenceKnown &&
                IsAdverseWindowPreZone(
                    _m5Bars,
                    closedM5,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    2);

            bool m1AdverseEvidenceKnown =
                _m1Frame != null &&
                _m1Frame.Index >= 2 &&
                _m1Bars != null &&
                _m1Frame.Index < _m1Bars.Count;

            double adverseM1Atr =
                m1AdverseEvidenceKnown
                    ? ResolveAdverseM1Atr(
                        _m1Bars,
                        _m1Frame.Index,
                        direction)
                    : 0;

            bool m1DirectionConflict =
                m1AdverseEvidenceKnown &&
                IsDirectionConflict(
                    _m1Frame.Direction,
                    direction);

            m1AdverseEvidenceKnown =
                m1AdverseEvidenceKnown &&
                adverseM1Atr > 0;

            bool m1AdversePreZone =
                retestTrapContext &&
                m1AdverseEvidenceKnown &&
                IsAdverseWindowPreZone(
                    _m1Bars,
                    _m1Frame.Index,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    2);

            double rangeHigh = double.MinValue;
            double rangeLow = double.MaxValue;

            int rangeStart =
                Math.Max(
                    0,
                    closedM5 -
                    Math.Min(
                        40,
                        Math.Max(
                            20,
                            StructureLookback)));

            for (int ri = rangeStart;
                 ri <= closedM5;
                 ri++)
            {
                rangeHigh =
                    Math.Max(
                        rangeHigh,
                        _m5Bars.HighPrices[ri]);

                rangeLow =
                    Math.Min(
                        rangeLow,
                        _m5Bars.LowPrices[ri]);
            }

            double rangeWidth =
                rangeHigh -
                rangeLow;

            double rangePosition =
                IsFinitePositive(rangeWidth)
                    ? (market - rangeLow) /
                      rangeWidth
                    : 0.50;

            bool opposingRegularDivergence =
                divergence.Quality >= 70 &&
                ((direction == 1 &&
                  divergence.RegularBear) ||
                 (direction == -1 &&
                  divergence.RegularBull));

            bool supportiveHiddenDivergence =
                divergence.Quality >= 60 &&
                ((direction == 1 &&
                  divergence.HiddenBull) ||
                 (direction == -1 &&
                  divergence.HiddenBear));

            EntryTrapRiskResult trapRisk =
                EntryTrapRiskRule.Evaluate(
                    direction,
                    rangePosition,
                    adverseM5Atr,
                    adverseM1Atr,
                    opposingRegularDivergence
                        ? divergence.Quality
                        : 0,
                    supportiveHiddenDivergence,
                    retestTrapContext,
                    insideZone,
                    m5AdversePreZone,
                    m1AdversePreZone,
                    m5AdverseEvidenceKnown ||
                    m1AdverseEvidenceKnown);


            // Live actionability must consume the same indicator-fusion quality
            // that guards confirmed Decision/automatic execution. A stale M5
            // fusion snapshot is fail-closed so an old indicator state cannot
            // reopen an otherwise blocked setup.
            string indicatorGateReason = string.Empty;

            if (_m5Frame != null)
            {
                if (_m5Frame.Index != closedM5)
                {
                    indicatorGateReason =
                        "INDICATOR FUSION • STALE";
                }
                else
                {
                    IndicatorActionabilityResult indicatorGate =
                        IndicatorActionabilityRule.Evaluate(
                            regime,
                            _m5Frame.IndicatorConfluenceQuality,
                            _m5Frame.IndicatorConflict);

                    if (!indicatorGate.Allowed)
                        indicatorGateReason =
                            indicatorGate.Reason;
                }
            }

            bool microConflict =
                EntryActionabilityPolicy.IsMicroConflict(
                    m1DirectionConflict,
                    adverseM1Atr,
                    entryDistanceAtr);

            int divergenceAdjustment =
                opposingRegularDivergence
                    ? -20
                    : supportiveHiddenDivergence
                        ? 8
                        : 0;

            locationQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        locationQuality +
                        divergenceAdjustment -
                        (int)Math.Round(
                            trapRisk.Risk * 0.22)));

            timingQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        timingQuality +
                        divergenceAdjustment -
                        (int)Math.Round(
                            trapRisk.Risk * 0.18)));

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

            if (tp1RR < rewardRisk.RequiredRR)
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