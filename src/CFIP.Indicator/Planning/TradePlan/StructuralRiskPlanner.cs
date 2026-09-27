// ============================================================================
// CFIP Indicator — StructuralRiskPlanner.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private double MinimumRequiredRR()
                        {
                            if (!AdaptiveStructuralRR ||
                                _decision == null)
                                return Tp1MinimumRR;
                
                            double step =
                                Math.Max(
                                    0.05,
                                    StructuralTpRrStep);
                
                            if (_decision.Regime == "EXPANSION")
                                return Math.Max(
                                    2.10,
                                    Tp1MinimumRR + step);
                
                            if (_decision.Regime == "RANGE")
                                return Math.Max(
                                    1.75,
                                    Tp1MinimumRR -
                                    step * 0.50);
                
                            return Tp1MinimumRR;
                        }
        
        private double BuildStructuralStop(
                            int closedM5,
                            int direction,
                            double entry,
                            double atr,
                            out string source,
                            out int quality)
                        {
                            source = "NONE";
                            quality = 0;
                
                            if (_m5Bars == null ||
                                closedM5 < 20 ||
                                atr <= 0 ||
                                !IsFinitePositive(entry))
                                return 0;
                
                            List<Level> candidates =
                                new List<Level>();
                
                            if (UseM5StructureForStop)
                            {
                                double swing =
                                    direction == 1
                                        ? FindSwingLowBelow(
                                            _m5Bars,
                                            closedM5,
                                            entry)
                                        : FindSwingHighAbove(
                                            _m5Bars,
                                            closedM5,
                                            entry);
                
                                AddLevel(
                                    candidates,
                                    swing,
                                    "M5_SWING_STOP",
                                    "M5",
                                    0,
                                    SwingStructureWeight);
                            }
                
                            Zone supportFvg =
                                FindNearestFvg(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    atr);
                
                            if (supportFvg != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? supportFvg.Low
                                        : supportFvg.High,
                                    "FVG_STOP",
                                    "M5",
                                    supportFvg.Age,
                                    FvgWeight +
                                    SmartStopZoneBonus);
                            }
                
                            Zone supportOb =
                                FindNearestOrderBlock(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    atr);
                
                            if (supportOb != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? supportOb.Low
                                        : supportOb.High,
                                    "ORDER_BLOCK_STOP",
                                    "M5",
                                    supportOb.Age,
                                    OrderBlockWeight +
                                    SmartStopZoneBonus);
                            }
                
                            if (UseHtfStructureForStop)
                            {
                                Bars[] frames =
                                {
                                    _m15Bars,
                                    _m30Bars,
                                    _h1Bars,
                                    _h4Bars,
                                    _d1Bars,
                                    _w1Bars
                                };
                
                                string[] names =
                                {
                                    "M15",
                                    "M30",
                                    "H1",
                                    "H4",
                                    "D1",
                                    "W1"
                                };
                
                                int[] weights =
                                {
                                    M15Weight,
                                    M30Weight,
                                    H1Weight,
                                    H4Weight,
                                    D1Weight,
                                    W1Weight
                                };
                
                                DateTime reference =
                                    _m5Bars.OpenTimes[
                                        closedM5];
                
                                for (int i = 0;
                                     i < frames.Length;
                                     i++)
                                {
                                    if (frames[i] == null)
                                        continue;
                
                                    int idx =
                                        ClosedIndex(
                                            frames[i],
                                            reference);
                
                                    if (idx < 10)
                                        continue;
                
                                    double frameAtr =
                                        Atr(
                                            frames[i],
                                            idx);
                
                                    if (frameAtr <= 0)
                                        frameAtr = atr;
                
                                    double swing =
                                        direction == 1
                                            ? FindSwingLowBelow(
                                                frames[i],
                                                idx,
                                                entry)
                                            : FindSwingHighAbove(
                                                frames[i],
                                                idx,
                                                entry);
                
                                    AddLevel(
                                        candidates,
                                        swing,
                                        "HTF_STRUCTURE_STOP",
                                        names[i],
                                        0,
                                        weights[i] +
                                        HtfRewardBonus / 2);
                
                                    Zone fvg =
                                        FindNearestFvg(
                                            frames[i],
                                            idx,
                                            direction,
                                            frameAtr);
                
                                    if (fvg != null)
                                    {
                                        AddLevel(
                                            candidates,
                                            direction == 1
                                                ? fvg.Low
                                                : fvg.High,
                                            "HTF_FVG_STOP",
                                            names[i],
                                            fvg.Age,
                                            FvgWeight +
                                            HtfRewardBonus / 2);
                                    }
                
                                    Zone ob =
                                        FindNearestOrderBlock(
                                            frames[i],
                                            idx,
                                            direction,
                                            frameAtr);
                
                                    if (ob != null)
                                    {
                                        AddLevel(
                                            candidates,
                                            direction == 1
                                                ? ob.Low
                                                : ob.High,
                                            "HTF_ORDER_BLOCK_STOP",
                                            names[i],
                                            ob.Age,
                                            OrderBlockWeight +
                                            HtfRewardBonus / 2);
                                    }
                                }
                            }
                
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
