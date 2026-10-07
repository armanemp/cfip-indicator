// ============================================================================
// CFIP Indicator — ReactionAnalyzer.ReversalQuality.cs
// Canonical reversal-quality evidence owner.
// ============================================================================

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
