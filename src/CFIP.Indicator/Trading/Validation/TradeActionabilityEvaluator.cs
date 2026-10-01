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

            bool insideZone =
                market >= execution.ZoneLow &&
                market <= execution.ZoneHigh;

            bool triggerReached =
                IsTriggerReached(
                    direction,
                    market,
                    execution.Trigger);

            bool continuation =
                IsContinuationExecutionContext(direction);

            bool qualityReady =
                !RequirePrecisionEntry ||
                execution.Quality >=
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(
                    MinimumEntryQuality);

            ExecutionMode liveMode;

            if (triggerReached &&
                AllowPrecisionBreakoutEntry)
            {
                liveMode = ExecutionMode.BreakoutMarket;
            }
            else if (continuation)
            {
                liveMode = ExecutionMode.WaitingForTrigger;
            }
            else if (insideZone)
            {
                liveMode = ExecutionMode.RetestMarket;
            }
            else
            {
                liveMode = ExecutionMode.None;
            }

            double anchor =
                EntryActionabilityPolicy.ResolveAnchor(
                    liveMode,
                    execution.Trigger,
                    execution.IdealEntry,
                    preview.Entry);

            double entryDistanceAtr =
                Math.Abs(
                    market - anchor) /
                atr;

            double distanceFromZone =
                insideZone
                    ? 0
                    : direction == 1
                        ? market < execution.ZoneLow
                            ? execution.ZoneLow - market
                            : market - execution.ZoneHigh
                        : market > execution.ZoneHigh
                            ? market - execution.ZoneHigh
                            : execution.ZoneLow - market;

            double zoneDistanceAtr =
                Math.Max(
                    0,
                    distanceFromZone / atr);

            double actualEntry =
                EntryActionabilityPolicy.ResolveActualEntry(
                    liveMode,
                    market,
                    preview.Entry);

            double risk =
                Math.Abs(
                    actualEntry -
                    preview.Stop);

            if (!IsFinitePositive(risk))
                return TradeActionabilityResult.Blocked(
                    "RISK UNAVAILABLE");

            double tp1RR =
                IsFinitePositive(preview.Tp1)
                    ? Math.Abs(
                        preview.Tp1 -
                        actualEntry) /
                      risk
                    : 0;

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
                        MaximumStructuralStopAtr));

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
                liveMode ==
                    ExecutionMode.BreakoutMarket &&
                IsFinitePositive(execution.Trigger) &&
                ((direction == 1 &&
                  market > execution.Trigger) ||
                 (direction == -1 &&
                  market < execution.Trigger))
                    ? Math.Abs(
                        market -
                        execution.Trigger) /
                      atr
                    : 0;

            bool late =
                EntryActionabilityPolicy.IsLate(
                    liveMode,
                    triggerExtensionAtr,
                    entryDistanceAtr,
                    MaximumEntryExtensionAtr,
                    MaximumEntryDistanceAtr);

            bool retestTrapContext =
                liveMode == ExecutionMode.RetestMarket &&
                insideZone;

            double adverseM5Atr = 0;

            if (closedM5 >= 2)
            {
                double m5Move =
                    _m5Bars.ClosePrices[closedM5] -
                    _m5Bars.ClosePrices[closedM5 - 2];

                adverseM5Atr =
                    direction == 1
                        ? Math.Max(0, -m5Move) /
                          Math.Max(Symbol.PipSize, atr)
                        : Math.Max(0, m5Move) /
                          Math.Max(Symbol.PipSize, atr);
            }

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

            double adverseM1Atr = 0;
            bool m1DirectionConflict = false;
            bool m1AdverseEvidenceKnown =
                _m1Frame != null &&
                _m1Frame.Index >= 2 &&
                _m1Bars != null &&
                _m1Frame.Index < _m1Bars.Count;

            if (m1AdverseEvidenceKnown)
            {
                int m1 = _m1Frame.Index;
                double m1Atr =
                    Atr(
                        _m1Bars,
                        m1);

                double m1Move =
                    _m1Bars.ClosePrices[m1] -
                    _m1Bars.ClosePrices[m1 - 2];

                if (IsFinitePositive(m1Atr))
                {
                    adverseM1Atr =
                        direction == 1
                            ? Math.Max(0, -m1Move) /
                              Math.Max(Symbol.PipSize, m1Atr)
                            : Math.Max(0, m1Move) /
                              Math.Max(Symbol.PipSize, m1Atr);
                }

                m1DirectionConflict =
                    (direction == 1 &&
                     _m1Frame.Direction == -1) ||
                    (direction == -1 &&
                     _m1Frame.Direction == 1);
            }

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

        private bool IsAdverseWindowPreZone(
            Bars bars,
            int index,
            double zoneLow,
            double zoneHigh,
            int lookbackBars)
        {
            if (bars == null ||
                index < lookbackBars ||
                index >= bars.Count)
                return false;

            if (double.IsNaN(zoneLow) ||
                double.IsInfinity(zoneLow) ||
                double.IsNaN(zoneHigh) ||
                double.IsInfinity(zoneHigh))
                return false;

            double low =
                Math.Min(
                    zoneLow,
                    zoneHigh);
            double high =
                Math.Max(
                    zoneLow,
                    zoneHigh);

            for (int i = index - lookbackBars;
                 i <= index;
                 i++)
            {
                double barLow = bars.LowPrices[i];
                double barHigh = bars.HighPrices[i];

                if (double.IsNaN(barLow) ||
                    double.IsInfinity(barLow) ||
                    double.IsNaN(barHigh) ||
                    double.IsInfinity(barHigh))
                    return false;

                if (barHigh >= low &&
                    barLow <= high)
                    return false;
            }

            return true;
        }
    }
}
