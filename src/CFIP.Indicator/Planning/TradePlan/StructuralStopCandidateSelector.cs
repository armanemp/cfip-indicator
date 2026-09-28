// CFIP Indicator — StructuralStopPlanner.cs
// Single-responsibility planning module.

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
        private double SelectStructuralStopCandidate(
                                    List<Level> candidates,
                                    int closedM5,
                                    int direction,
                                    double entry,
                                    double atr,
                                    out string source,
                                    out int quality)
                                {
                                    source = "NONE";
                                    quality = 0;

                
                            if (candidates.Count == 0)
                                return 0;
                
                            double minRiskAtr =
                                Math.Max(
                                    0.05,
                                    MinimumSlAtr);
                
                            double maxRiskAtr =
                                Math.Min(
                                    Math.Max(
                                        minRiskAtr,
                                        MaximumSlAtr),
                                    Math.Max(
                                        minRiskAtr,
                                        MaximumStructuralStopAtr));
                
                            double spread =
                                Math.Max(
                                    0,
                                    Symbol.Ask -
                                    Symbol.Bid);
                
                            minRiskAtr =
                                Math.Max(
                                    minRiskAtr,
                                    (spread /
                                     Math.Max(
                                         Symbol.PipSize,
                                         atr)) *
                                    Math.Max(
                                        1.0,
                                        MaximumSpreadToStopRiskRatio));
                
                            Level best = null;
                            double bestScore =
                                double.MinValue;
                            int bestQuality = 0;
                            string bestSource = "NONE";
                
                            int minimumQuality =
                                Math.Max(
                                    Math.Max(
                                        40,
                                        MinimumStructuralStopQuality),
                                    SmartStopQuality);
                
                            for (int i = 0;
                                 i < candidates.Count;
                                 i++)
                            {
                                Level candidate =
                                    candidates[i];
                
                                double frameAtr =
                                    atr;
                
                                if (candidate.Timeframe != "M5")
                                {
                                    Bars frame =
                                        candidate.Timeframe == "M15"
                                            ? _m15Bars
                                            : candidate.Timeframe == "M30"
                                                ? _m30Bars
                                                : candidate.Timeframe == "H1"
                                                    ? _h1Bars
                                                    : candidate.Timeframe == "H4"
                                                        ? _h4Bars
                                                        : candidate.Timeframe == "D1"
                                                            ? _d1Bars
                                                            : _w1Bars;
                
                                    int idx =
                                        ClosedIndex(
                                            frame,
                                            _m5Bars.OpenTimes[
                                                closedM5]);
                
                                    if (idx >= 10)
                                    {
                                        double localAtr =
                                            Atr(
                                                frame,
                                                idx);
                
                                        if (localAtr > 0)
                                            frameAtr =
                                                localAtr;
                                    }
                                }
                
                                double buffer =
                                    frameAtr *
                                    Math.Max(
                                        0.02,
                                        candidate.Timeframe == "M5"
                                            ? StopBufferAtr
                                            : Math.Max(
                                                StopBufferAtr,
                                                HtfStopBufferAtr));
                
                                double stop =
                                    direction == 1
                                        ? candidate.Price - buffer
                                        : candidate.Price + buffer;
                
                                stop =
                                    NormalizePrice(
                                        stop);
                
                                if (!IsValidStop(
                                        direction,
                                        entry,
                                        stop))
                                    continue;
                
                                double risk =
                                    Math.Abs(
                                        entry -
                                        stop);
                
                                double riskAtr =
                                    risk /
                                    Math.Max(
                                        Symbol.PipSize,
                                        atr);
                
                                if (riskAtr < minRiskAtr ||
                                    riskAtr > maxRiskAtr)
                                    continue;
                
                                double score =
                                    candidate.Score;
                
                                if (HasOpposingZonePathObstacle(
                                        _m5Bars,
                                        closedM5,
                                        direction,
                                        entry,
                                        stop,
                                        atr))
                                    score -=
                                        Math.Min(
                                            18,
                                            StopRiskBalanceWeight);
                
                                if (IsHtfTimeframe(
                                        candidate.Timeframe))
                                    score +=
                                        HtfRewardBonus *
                                        0.50;
                
                                if (candidate.Kind.IndexOf(
                                        "FVG",
                                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    candidate.Kind.IndexOf(
                                        "ORDER_BLOCK",
                                        StringComparison.OrdinalIgnoreCase) >= 0)
                                    score +=
                                        SmartStopZoneBonus;
                
                                if (candidate.Kind.IndexOf(
                                        "LIQUIDITY",
                                        StringComparison.OrdinalIgnoreCase) >= 0)
                                    score +=
                                        SmartLiquidityPoolBonus;
                
                                double preferredRisk =
                                    Math.Max(
                                        0.25,
                                        PreferredStopRiskAtr);
                
                                double riskBalance =
                                    Math.Max(
                                        0,
                                        20.0 -
                                        Math.Abs(
                                            riskAtr -
                                            preferredRisk) *
                                        Math.Max(
                                            6.0,
                                            12.0 *
                                            Math.Max(
                                                0.25,
                                                StopRiskBalanceWeight /
                                                18.0)));
                
                                score +=
                                    riskBalance *
                                    Math.Max(
                                        0,
                                        StopRiskBalanceWeight) /
                                    20.0;
                
                                if (score > bestScore)
                                {
                                    bestScore =
                                        score;
                                    best =
                                        candidate;
                                    bestQuality =
                                        ClampInt(
                                            (int)Math.Round(
                                                score),
                                            0,
                                            100);
                                    bestSource =
                                        candidate.Kind +
                                        "@" +
                                        candidate.Timeframe;
                                }
                            }
                
                            if (best == null ||
                                bestQuality <
                                minimumQuality)
                                return 0;
                
                            double finalFrameAtr =
                                atr;
                
                            if (best.Timeframe != "M5")
                            {
                                Bars frame =
                                    best.Timeframe == "M15"
                                        ? _m15Bars
                                        : best.Timeframe == "M30"
                                            ? _m30Bars
                                            : best.Timeframe == "H1"
                                                ? _h1Bars
                                                : best.Timeframe == "H4"
                                                    ? _h4Bars
                                                    : best.Timeframe == "D1"
                                                        ? _d1Bars
                                                        : _w1Bars;
                
                                int idx =
                                    ClosedIndex(
                                        frame,
                                        _m5Bars.OpenTimes[
                                            closedM5]);
                
                                if (idx >= 10)
                                {
                                    double localAtr =
                                        Atr(
                                            frame,
                                            idx);
                
                                    if (localAtr > 0)
                                        finalFrameAtr =
                                            localAtr;
                                }
                            }
                
                            double finalBuffer =
                                finalFrameAtr *
                                Math.Max(
                                    0.02,
                                    best.Timeframe == "M5"
                                        ? StopBufferAtr
                                        : Math.Max(
                                            StopBufferAtr,
                                            HtfStopBufferAtr));
                
                            double selectedStop =
                                direction == 1
                                    ? best.Price - finalBuffer
                                    : best.Price + finalBuffer;
                
                            source = bestSource;
                            quality = bestQuality;
                
                            return NormalizePrice(
                                selectedStop);
                        }
    }
}
