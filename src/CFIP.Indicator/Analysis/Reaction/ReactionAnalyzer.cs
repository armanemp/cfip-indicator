// ============================================================================
// CFIP Indicator — ReactionAnalyzer.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Decision BuildReaction()
        {
            if (!EnableLiveReaction ||
                !EnableFastReversalIntelligence ||
                _m5Bars == null ||
                _m5Bars.Count < 10)
                return new Decision();

            int live = _m5Bars.Count - 1;
            int closedIndex = live - 1;

            if (!ReactionTimingRule.IsSeparatedObservationAndConfirmation(
                    live,
                    closedIndex))
                return new Decision();

            double atr =
                Atr(
                    _m5Bars,
                    Math.Max(
                        1,
                        live - 1));

            if (atr <= 0)
                return new Decision();

            int buyQuality;
            int buyEvidence;
            bool buyContext;
            bool buyCounterMove;
            bool buyQualifyingZone;
            bool buySwingInteraction;
            bool buyZonePresent;

            int sellQuality;
            int sellEvidence;
            bool sellContext;
            bool sellCounterMove;
            bool sellQualifyingZone;
            bool sellSwingInteraction;
            bool sellZonePresent;

            ReversalQuality(
                _m5Bars,
                live,
                1,
                atr,
                out buyQuality,
                out buyEvidence,
                out buyContext,
                out buyCounterMove,
                out buyQualifyingZone,
                out buySwingInteraction,
                out buyZonePresent);

            ReversalQuality(
                _m5Bars,
                live,
                -1,
                atr,
                out sellQuality,
                out sellEvidence,
                out sellContext,
                out sellCounterMove,
                out sellQualifyingZone,
                out sellSwingInteraction,
                out sellZonePresent);

            int watchThreshold =
                Math.Max(
                    50,
                    Math.Max(
                        FastReversalMinimumQuality,
                        LiveReactionWatchThreshold));

            int entryThreshold =
                Math.Max(
                    watchThreshold,
                    LiveReactionThreshold);

            MarketRegimeSnapshot regime =
                GetActiveM5Regime(
                    Math.Max(
                        0,
                        live - 1));

            if (regime != null)
            {
                if (regime.Regime == "RANGE" ||
                    regime.Regime == "COMPRESSION")
                {
                    watchThreshold += 8;
                    entryThreshold += 14;
                }
                else if (regime.Regime == "TRANSITION")
                {
                    watchThreshold += 4;
                    entryThreshold += 8;
                }
                else if (regime.Regime == "HIGH_VOLATILITY")
                {
                    watchThreshold += 6;
                    entryThreshold += 12;
                }
                else if (regime.Regime == "TREND" ||
                         regime.Regime == "EXPANSION")
                {
                    if (regime.Direction == 1)
                    {
                        buyQuality += 4;
                        sellQuality -= 4;
                    }
                    else if (regime.Direction == -1)
                    {
                        buyQuality -= 4;
                        sellQuality += 4;
                    }
                }
            }

            int strongThreshold =
                Math.Max(
                    entryThreshold,
                    LiveReactionStrongThreshold);

            if (buyQuality < watchThreshold &&
                sellQuality < watchThreshold)
                return new Decision();

            Decision d =
                new Decision();

            int direction =
                ReactionQualificationRule.ResolveDirection(
                    buyQuality,
                    sellQuality);

            if (direction == 0)
            {
                d.Direction = 0;
                d.Confidence =
                    Math.Max(
                        buyQuality,
                        sellQuality);
                d.IndependentEvidence =
                    Math.Max(
                        buyEvidence,
                        sellEvidence);
                d.SmartQuality = d.Confidence;
                d.ReactionIntrabarQuality = d.Confidence;
                d.ReactionIntrabarEvidence = d.IndependentEvidence;
                d.TriggerReady = false;
                d.EntryAllowed = false;
                d.BlockReason = "REACTION CONFLICT";
                d.Reason =
                    "REACTION CONFLICT | Q " +
                    d.Confidence +
                    " | B " +
                    buyQuality +
                    " / S " +
                    sellQuality;
                return d;
            }

            d.Direction = direction;
            d.Confidence =
                direction == 1
                    ? buyQuality
                    : sellQuality;
            d.IndependentEvidence =
                direction == 1
                    ? buyEvidence
                    : sellEvidence;
            d.SmartQuality =
                d.Confidence;

            d.ReactionHasContext =
                direction == 1
                    ? buyContext
                    : sellContext;
            d.ReactionHasCounterMove =
                direction == 1
                    ? buyCounterMove
                    : sellCounterMove;
            d.ReactionHasQualifyingZone =
                direction == 1
                    ? buyQualifyingZone
                    : sellQualifyingZone;
            d.ReactionHasSwingInteraction =
                direction == 1
                    ? buySwingInteraction
                    : sellSwingInteraction;
            d.ReactionZonePresent =
                direction == 1
                    ? buyZonePresent
                    : sellZonePresent;
            d.ReactionIntrabarQuality =
                d.Confidence;
            d.ReactionIntrabarEvidence =
                d.IndependentEvidence;

            int requiredReactionEvidence =
                Math.Max(
                    2,
                    LiveReversalMinimumEvidence);

            if (regime != null &&
                (regime.Regime == "TRANSITION" ||
                 regime.Regime == "HIGH_VOLATILITY"))
                requiredReactionEvidence++;

            int confirmedDirection = 0;
            int confirmedQuality = 0;
            int confirmedEvidence = 0;
            bool confirmedContext = false;

            if (closedIndex >= 0)
            {
                double closedAtr =
                    Atr(
                        _m5Bars,
                        closedIndex);

                if (closedAtr > 0)
                {
                    int confirmedBuyQuality;
                    int confirmedBuyEvidence;
                    bool confirmedBuyContext;
                    bool confirmedBuyCounterMove;
                    bool confirmedBuyQualifyingZone;
                    bool confirmedBuySwingInteraction;
                    bool confirmedBuyZonePresent;

                    int confirmedSellQuality;
                    int confirmedSellEvidence;
                    bool confirmedSellContext;
                    bool confirmedSellCounterMove;
                    bool confirmedSellQualifyingZone;
                    bool confirmedSellSwingInteraction;
                    bool confirmedSellZonePresent;

                    ReversalQuality(
                        _m5Bars,
                        closedIndex,
                        1,
                        closedAtr,
                        out confirmedBuyQuality,
                        out confirmedBuyEvidence,
                        out confirmedBuyContext,
                        out confirmedBuyCounterMove,
                        out confirmedBuyQualifyingZone,
                        out confirmedBuySwingInteraction,
                        out confirmedBuyZonePresent);

                    ReversalQuality(
                        _m5Bars,
                        closedIndex,
                        -1,
                        closedAtr,
                        out confirmedSellQuality,
                        out confirmedSellEvidence,
                        out confirmedSellContext,
                        out confirmedSellCounterMove,
                        out confirmedSellQualifyingZone,
                        out confirmedSellSwingInteraction,
                        out confirmedSellZonePresent);

                    confirmedDirection =
                        ReactionQualificationRule.ResolveDirection(
                            confirmedBuyQuality,
                            confirmedSellQuality);

                    if (confirmedDirection == direction)
                    {
                        confirmedQuality =
                            confirmedDirection == 1
                                ? confirmedBuyQuality
                                : confirmedSellQuality;

                        confirmedEvidence =
                            confirmedDirection == 1
                                ? confirmedBuyEvidence
                                : confirmedSellEvidence;

                        confirmedContext =
                            confirmedDirection == 1
                                ? confirmedBuyContext
                                : confirmedSellContext;
                    }
                }
            }

            d.ReactionConfirmedM5 =
                closedIndex;
            d.ReactionConfirmedDirection =
                confirmedDirection;
            d.ReactionConfirmedQuality =
                confirmedQuality;
            d.ReactionConfirmedEvidence =
                confirmedEvidence;
            d.ReactionConfirmedHasContext =
                confirmedContext;
            d.ReactionClosedBarConfirmed =
                ReactionTimingRule.IsSeparatedObservationAndConfirmation(
                    live,
                    closedIndex) &&
                confirmedDirection == direction;

            d.TriggerReady =
                ReactionQualificationRule.IsQualified(
                    direction,
                    d.Confidence,
                    d.IndependentEvidence,
                    d.ReactionHasContext,
                    watchThreshold,
                    Math.Max(
                        1,
                        LiveReversalMinimumEvidence),
                    false,
                    false);

            d.EntryAllowed =
                ReactionQualificationRule.IsQualified(
                    direction,
                    confirmedQuality,
                    confirmedEvidence,
                    confirmedContext,
                    entryThreshold,
                    requiredReactionEvidence,
                    true,
                    d.ReactionClosedBarConfirmed);

            if (!AllowFastM5ReversalBeforeM15 &&
                (_m15Frame == null ||
                 _m15Frame.Direction !=
                 d.Direction))
            {
                d.EntryAllowed = false;
                d.BlockReason = "M15 REVERSAL";
            }

            if (!d.ReactionHasContext)
            {
                d.TriggerReady = false;

                if (!d.EntryAllowed)
                    d.BlockReason =
                        "REACTION CONTEXT";
            }

            d.Reason =
                (d.Confidence >= strongThreshold
                    ? "STRONG "
                    : "") +
                (d.Direction == 1
                    ? "BUY"
                    : "SELL") +
                " REACTION | Q " +
                d.Confidence +
                " | EVID " +
                d.IndependentEvidence +
                " | CONF " +
                d.ReactionConfirmedQuality +
                " / " +
                d.ReactionConfirmedEvidence +
                " | " +
                (d.ReactionHasCounterMove
                    ? "COUNTER"
                    : "NO-COUNTER") +
                " | " +
                (d.ReactionZonePresent
                    ? (d.ReactionHasQualifyingZone
                        ? "ZONE"
                        : "WEAK-ZONE")
                    : "NO-ZONE") +
                " | " +
                (d.ReactionHasSwingInteraction
                    ? "SWING"
                    : "NO-SWING");

            return d;
        }

        private void ReversalQuality(
            Bars bars,
            int index,
            int direction,
            double atr,
            out int quality,
            out int evidence,
            out bool qualifyingContext,
            out bool priorCounterMove,
            out bool qualifyingZone,
            out bool swingInteraction,
            out bool zonePresent)
        {
            quality = 0;
            evidence = 0;
            qualifyingContext = false;
            priorCounterMove = false;
            qualifyingZone = false;
            swingInteraction = false;
            zonePresent = false;

            if (bars == null ||
                index < 6 ||
                atr <= 0)
                return;

            double range =
                Math.Max(
                    Symbol.PipSize,
                    bars.HighPrices[index] -
                    bars.LowPrices[index]);

            double body =
                Math.Abs(
                    bars.ClosePrices[index] -
                    bars.OpenPrices[index]);

            bool directional =
                direction == 1
                    ? bars.ClosePrices[index] >
                      bars.OpenPrices[index]
                    : bars.ClosePrices[index] <
                      bars.OpenPrices[index];

            double closeLocation =
                direction == 1
                    ? (bars.ClosePrices[index] -
                       bars.LowPrices[index]) /
                      range
                    : (bars.HighPrices[index] -
                       bars.ClosePrices[index]) /
                      range;

            bool closeStrong =
                closeLocation >=
                MinimumCloseLocation;

            bool displacement =
                body >=
                atr *
                Math.Max(
                    MinimumTriggerBodyAtr,
                    0.60);

            int lookback =
                Math.Max(
                    2,
                    FastReversalLookbackBars);

            int previous =
                Math.Max(
                    0,
                    index - lookback);

            priorCounterMove =
                ReactionQualificationRule.HasPriorCounterMove(
                    direction,
                    bars.ClosePrices[previous],
                    bars.OpenPrices[
                        Math.Max(
                            0,
                            index - 1)],
                    bars.ClosePrices[
                        Math.Max(
                            0,
                            index - 1)]);

            bool breakMicro =
                direction == 1
                    ? bars.ClosePrices[index] >
                      Highest(
                          bars,
                          previous,
                          index - 1)
                    : bars.ClosePrices[index] <
                      Lowest(
                          bars,
                          previous,
                          index - 1);

            double rsi =
                Rsi(
                    bars,
                    index);

            bool rsiSupport =
                direction == 1
                    ? rsi >= 52
                    : rsi <= 48;

            bool emaSupport =
                direction == 1
                    ? Ema(
                        bars,
                        index,
                        true) >
                      Ema(
                        bars,
                        index,
                        false)
                    : Ema(
                        bars,
                        index,
                        true) <
                      Ema(
                        bars,
                        index,
                        false);

            if (directional)
            {
                quality += 20;
                evidence++;
            }

            if (closeStrong)
            {
                quality += 18;
                evidence++;
            }

            if (displacement)
            {
                quality += 22;
                evidence++;
            }

            if (breakMicro)
            {
                quality += 22;
                evidence++;
            }

            if (rsiSupport)
            {
                quality += 8;
                evidence++;
            }

            if (emaSupport)
            {
                quality += 10;
                evidence++;
            }

            Zone reversalZone =
                FindNearestOpposingZone(
                    bars,
                    index,
                    direction,
                    atr);

            zonePresent =
                reversalZone != null;

            qualifyingZone =
                zonePresent &&
                reversalZone.Quality >=
                FastReversalMinimumZoneQuality;

            if (qualifyingZone)
            {
                quality += 10;
                evidence++;
            }

            int swingStart;
            int swingEnd;
            double swingLevel;

            if (direction == 1)
            {
                swingInteraction =
                    TryFindLatestSwingLow(
                        bars,
                        index,
                        SwingStrength,
                        out swingStart,
                        out swingEnd,
                        out swingLevel) &&
                    ReactionQualificationRule.HasSwingInteraction(
                        direction,
                        bars.LowPrices[index],
                        bars.HighPrices[index],
                        swingLevel);
            }
            else if (direction == -1)
            {
                swingInteraction =
                    TryFindLatestSwingHigh(
                        bars,
                        index,
                        SwingStrength,
                        out swingStart,
                        out swingEnd,
                        out swingLevel) &&
                    ReactionQualificationRule.HasSwingInteraction(
                        direction,
                        bars.LowPrices[index],
                        bars.HighPrices[index],
                        swingLevel);
            }

            qualifyingContext =
                ReactionQualificationRule.HasQualifyingContext(
                    priorCounterMove,
                    zonePresent,
                    qualifyingZone,
                    swingInteraction);

            if (priorCounterMove)
                quality += 4;

            if (swingInteraction)
                quality += 4;

            quality =
                ClampInt(
                    quality,
                    0,
                    100);
        }
    }
}
