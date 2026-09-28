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
                
                            int live =
                                _m5Bars.Count - 1;
                
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
                            int sellQuality;
                            int sellEvidence;
                
                            ReversalQuality(
                                _m5Bars,
                                live,
                                1,
                                atr,
                                out buyQuality,
                                out buyEvidence);
                
                            ReversalQuality(
                                _m5Bars,
                                live,
                                -1,
                                atr,
                                out sellQuality,
                                out sellEvidence);
                
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
                
                            if (buyQuality >= sellQuality)
                            {
                                d.Direction = 1;
                                d.Confidence = buyQuality;
                                d.IndependentEvidence = buyEvidence;
                            }
                            else
                            {
                                d.Direction = -1;
                                d.Confidence = sellQuality;
                                d.IndependentEvidence = sellEvidence;
                            }
                
                            d.SmartQuality =
                                d.Confidence;
                
                            int requiredReactionEvidence =
                                Math.Max(
                                    2,
                                    LiveReversalMinimumEvidence);

                            if (regime != null &&
                                (regime.Regime == "TRANSITION" ||
                                 regime.Regime == "HIGH_VOLATILITY"))
                                requiredReactionEvidence++;

                            d.EntryAllowed =
                                d.Confidence >=
                                entryThreshold &&
                                d.IndependentEvidence >=
                                requiredReactionEvidence;
                
                            if (!AllowFastM5ReversalBeforeM15 &&
                                (_m15Frame == null ||
                                 _m15Frame.Direction !=
                                 d.Direction))
                            {
                                d.EntryAllowed = false;
                                d.BlockReason =
                                    "M15 REVERSAL";
                            }
                
                            if (d.EntryAllowed)
                            {
                                Zone reactionZone =
                                    FindNearestOpposingZone(
                                        _m5Bars,
                                        live,
                                        d.Direction,
                                        atr);
                
                                if (reactionZone != null &&
                                    reactionZone.Quality <
                                    FastReversalMinimumZoneQuality)
                                {
                                    d.EntryAllowed = false;
                                    d.BlockReason =
                                        "REACTION ZONE";
                                }
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
                                d.IndependentEvidence;
                
                            return d;
                        }
        
        private void ReversalQuality(
                            Bars bars,
                            int index,
                            int direction,
                            double atr,
                            out int quality,
                            out int evidence)
                        {
                            quality = 0;
                            evidence = 0;
                
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
                
                            if (reversalZone != null &&
                                reversalZone.Quality >=
                                FastReversalMinimumZoneQuality)
                            {
                                quality += 10;
                                evidence++;
                            }
                
                            quality =
                                ClampInt(
                                    quality,
                                    0,
                                    100);
                        }
    }
}
