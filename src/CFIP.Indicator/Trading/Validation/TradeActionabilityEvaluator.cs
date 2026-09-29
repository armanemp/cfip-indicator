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
                Math.Max(
                    40,
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
                liveMode == ExecutionMode.BreakoutMarket
                    ? execution.Trigger
                    : execution.IdealEntry;

            if (!IsFinitePositive(anchor))
                anchor = preview.Entry;

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
                liveMode == ExecutionMode.WaitingForTrigger
                    ? preview.Entry
                    : market;

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

            double minimumRR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRRForRegime(regime));

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
                liveMode ==
                    ExecutionMode.BreakoutMarket
                    ? triggerExtensionAtr >
                      Math.Max(
                          0.10,
                          MaximumEntryExtensionAtr)
                    : entryDistanceAtr >
                      Math.Max(
                          0.05,
                          MaximumEntryDistanceAtr);

            bool microConflict =
                _m1Frame != null &&
                _m1Frame.Index >= 1 &&
                _m1Bars != null &&
                _m1Frame.Index < _m1Bars.Count &&
                ((direction == 1 &&
                  (_m1Frame.Direction == -1 ||
                   _m1Bars.ClosePrices[_m1Frame.Index] <
                   _m1Bars.ClosePrices[
                       Math.Max(
                           0,
                           _m1Frame.Index - 1)] -
                   Math.Max(
                       Symbol.PipSize,
                       Atr(
                           _m1Bars,
                           _m1Frame.Index) *
                       0.15))) ||
                 (direction == -1 &&
                  (_m1Frame.Direction == 1 ||
                   _m1Bars.ClosePrices[_m1Frame.Index] >
                   _m1Bars.ClosePrices[
                       Math.Max(
                           0,
                           _m1Frame.Index - 1)] +
                   Math.Max(
                       Symbol.PipSize,
                       Atr(
                           _m1Bars,
                           _m1Frame.Index) *
                       0.15))));

            DivergenceResult divergence =
                _m5Frame == null
                    ? DivergenceResult.None()
                    : new DivergenceResult(
                        _m5Frame.DivergenceDirection,
                        _m5Frame.DivergenceQuality,
                        _m5Frame.DivergenceType,
                        _m5Frame.RegularDivergenceBull,
                        _m5Frame.RegularDivergenceBear,
                        _m5Frame.HiddenDivergenceBull,
                        _m5Frame.HiddenDivergenceBear);

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
                        divergenceAdjustment));

            timingQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        timingQuality +
                        divergenceAdjustment));

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

            if (tp1RR < minimumRR)
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

            if (microConflict &&
                !insideZone)
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
                (!insideZone ||
                 entryDistanceAtr >
                 Math.Max(
                     0.10,
                     MaximumEntryDistanceAtr * 0.60)))
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

            if (locationQuality <
                Math.Max(
                    MinimumEntryQuality,
                    70))
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

            if (timingQuality < 70)
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
