using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareTradeActionability(
            int closedM5,
            int direction,
            OpportunityLane lane,
            string regime,
            ExecutionModel execution,
            TradeSetupPreview preview,
            out TradeActionabilityEvaluationState state,
            out TradeActionabilityResult failure)
        {
            state =
                new TradeActionabilityEvaluationState
                {
                    ClosedM5 = closedM5,
                    Direction = direction,
                    Lane = lane,
                    Regime = regime,
                    Execution = execution,
                    InputPreview = preview,
                    Divergence =
                        DivergenceResult.CreateNoDivergence()
                };

            failure =
                TradeActionabilityResult.Blocked(
                    "SETUP UNAVAILABLE");

            if (_m5Bars == null ||
                execution == null ||
                preview == null ||
                (direction != 1 &&
                 direction != -1))
                return false;

            state.Atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(state.Atr))
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        "ATR UNAVAILABLE");
                return false;
            }

            state.Market =
                NormalizePrice(
                    direction == 1
                        ? Symbol.Ask
                        : Symbol.Bid);

            if (!IsFinitePositive(state.Market))
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        "QUOTE UNAVAILABLE");
                return false;
            }

            bool continuation =
                IsContinuationExecutionContext(direction);

            EntryGeometrySnapshot geometry =
                EntryGeometryRule.Evaluate(
                    direction,
                    ExecutionMode.None,
                    state.Market,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    execution.ZoneTolerance,
                    execution.IdealEntry,
                    execution.Trigger,
                    preview.Entry,
                    state.Atr,
                    Symbol.TickSize,
                    Symbol.PipSize,
                    AllowPrecisionBreakoutEntry,
                    continuation,
                    MaximumEntryExtensionAtr,
                    MaximumEntryDistanceAtr);

            if (!geometry.IsValid)
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        geometry.Reason);
                return false;
            }

            state.InsideZone =
                geometry.InsideZone;
            state.LiveMode =
                geometry.Mode;
            state.EntryDistanceAtr =
                geometry.EntryDistanceAtr;
            state.ZoneDistanceAtr =
                geometry.ZoneDistanceAtr;
            state.ActualEntry =
                geometry.ActualEntry;
            state.Late =
                geometry.IsLate;

            if (!execution.Ready)
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        "EXECUTION MODEL NOT READY");
                return false;
            }

            CanonicalTradePathGeometry canonicalPath;
            string canonicalPathReason;

            if (!TryBuildCanonicalTradePathGeometry(
                    closedM5,
                    direction,
                    execution,
                    lane,
                    out canonicalPath,
                    out canonicalPathReason))
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        string.IsNullOrWhiteSpace(
                            canonicalPathReason)
                            ? "CANONICAL TRADE PATH INVALID"
                            : canonicalPathReason);
                return false;
            }

            double entryConsistencyTolerance =
                Math.Max(
                    Symbol.TickSize * 2,
                    Symbol.PipSize * 0.10);

            if (canonicalPath == null ||
                canonicalPath.EntryMode != state.LiveMode ||
                Math.Abs(
                    canonicalPath.Entry -
                    state.ActualEntry) >
                entryConsistencyTolerance)
            {
                failure =
                    TradeActionabilityResult.Blocked(
                        "EXECUTION GEOMETRY / ACTIONABILITY MISMATCH");
                return false;
            }

            state.CanonicalPath =
                canonicalPath;
            state.EffectivePreview =
                canonicalPath.Preview;

            state.QualityReady =
                !RequirePrecisionEntry ||
                execution.Quality >=
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(
                    MinimumEntryQuality);

            state.RewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    state.ActualEntry,
                    state.EffectivePreview.Stop,
                    state.EffectivePreview.Tp1,
                    state.Atr,
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
                    Math.Max(
                        0,
                        MaximumRewardRR),
                    Symbol.PipSize);

            state.Tp1RR =
                canonicalPath.Tp1RR;

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

            state.Divergence =
                divergence;

            if (!state.RewardRisk.Allowed)
            {
                failure =
                    state.ToBlockedResult(
                        state.RewardRisk.Reason,
                        true);
                return false;
            }

            int primaryM15Index =
                _lastMtfClosedContext == null
                    ? -1
                    : _lastMtfClosedContext.M15;

            if (_m15Frame == null ||
                primaryM15Index < 0 ||
                _m15Frame.Index != primaryM15Index)
            {
                failure =
                    state.ToBlockedResult(
                        "M15 EXECUTION FRAME UNAVAILABLE",
                        true);
                return false;
            }

            if (_m15Frame.Direction != direction)
            {
                failure =
                    state.ToBlockedResult(
                        "M15 EXECUTION DIRECTION CONFLICT",
                        true);
                return false;
            }

            state.LocationQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        execution.Quality +
                        (state.InsideZone ? 10 : 0) -
                        (int)Math.Round(
                            Math.Min(
                                30,
                                state.ZoneDistanceAtr * 30))));

            state.TimingQuality =
                state.InsideZone
                    ? 95
                    : state.LiveMode ==
                      ExecutionMode.BreakoutMarket
                        ? 85 -
                          (int)Math.Round(
                              Math.Min(
                                  35,
                                  state.EntryDistanceAtr * 25))
                        : 55 -
                          (int)Math.Round(
                              Math.Min(
                                  30,
                                  state.ZoneDistanceAtr * 25));

            state.PricePositionQuality =
                100 -
                (int)Math.Round(
                    Math.Min(
                        70,
                        state.EntryDistanceAtr * 45));

            state.RetestTrapContext =
                state.LiveMode == ExecutionMode.RetestMarket &&
                state.InsideZone;

            state.AdverseM5Atr =
                ResolveAdverseM5Atr(
                    _m5Bars,
                    closedM5,
                    direction,
                    state.Atr);

            state.M5AdverseEvidenceKnown =
                closedM5 >= 2 &&
                state.AdverseM5Atr > 0;

            state.M5AdversePreZone =
                state.RetestTrapContext &&
                state.M5AdverseEvidenceKnown &&
                IsAdverseWindowPreZone(
                    _m5Bars,
                    closedM5,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    2);

            state.M1AdverseEvidenceKnown =
                _m1Frame != null &&
                _m1Frame.Index >= 2 &&
                _m1Bars != null &&
                _m1Frame.Index < _m1Bars.Count;

            state.AdverseM1Atr =
                state.M1AdverseEvidenceKnown
                    ? ResolveAdverseM1Atr(
                        _m1Bars,
                        _m1Frame.Index,
                        direction)
                    : 0;

            state.M1DirectionConflict =
                state.M1AdverseEvidenceKnown &&
                IsDirectionConflict(
                    _m1Frame.Direction,
                    direction);

            state.M1AdverseEvidenceKnown =
                state.M1AdverseEvidenceKnown &&
                state.AdverseM1Atr > 0;

            state.M1AdversePreZone =
                state.RetestTrapContext &&
                state.M1AdverseEvidenceKnown &&
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

            state.RangePosition =
                IsFinitePositive(rangeWidth)
                    ? (state.Market - rangeLow) /
                      rangeWidth
                    : 0.50;

            state.OpposingRegularDivergence =
                divergence.Quality >= 70 &&
                ((direction == 1 &&
                  divergence.RegularBear) ||
                 (direction == -1 &&
                  divergence.RegularBull));

            state.SupportiveHiddenDivergence =
                divergence.Quality >= 60 &&
                ((direction == 1 &&
                  divergence.HiddenBull) ||
                 (direction == -1 &&
                  divergence.HiddenBear));

            state.TrapRisk =
                EntryTrapRiskRule.Evaluate(
                    direction,
                    state.RangePosition,
                    state.AdverseM5Atr,
                    state.AdverseM1Atr,
                    state.OpposingRegularDivergence
                        ? divergence.Quality
                        : 0,
                    state.SupportiveHiddenDivergence,
                    state.RetestTrapContext,
                    state.InsideZone,
                    state.M5AdversePreZone,
                    state.M1AdversePreZone,
                    state.M5AdverseEvidenceKnown ||
                    state.M1AdverseEvidenceKnown);

            state.IndicatorGateReason =
                string.Empty;

            if (_m5Frame != null)
            {
                if (_m5Frame.Index != closedM5)
                {
                    state.IndicatorGateReason =
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
                        state.IndicatorGateReason =
                            indicatorGate.Reason;
                }
            }

            state.MicroConflict =
                EntryActionabilityPolicy.IsMicroConflict(
                    state.M1DirectionConflict,
                    state.AdverseM1Atr,
                    state.EntryDistanceAtr);

            int divergenceAdjustment =
                state.OpposingRegularDivergence
                    ? -20
                    : state.SupportiveHiddenDivergence
                        ? 8
                        : 0;

            state.LocationQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        state.LocationQuality +
                        divergenceAdjustment -
                        (int)Math.Round(
                            state.TrapRisk.Risk * 0.22)));

            state.TimingQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        state.TimingQuality +
                        divergenceAdjustment -
                        (int)Math.Round(
                            state.TrapRisk.Risk * 0.18)));

            return true;
        }
    }
}
