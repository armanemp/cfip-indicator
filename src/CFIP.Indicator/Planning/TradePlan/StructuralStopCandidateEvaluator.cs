using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TrySelectBestStructuralStopCandidate(
            List<Level> candidates,
            int closedM5,
            int direction,
            double entry,
            double atr,
            out Level best,
            out string source,
            out int quality)
        {
            best = null;
            source = "NONE";
            quality = 0;

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

            double bestScore =
                double.MinValue;

            string bestSource =
                "NONE";

            int bestQuality = 0;

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
                    ResolveStructuralStopFrameAtr(
                        candidate,
                        closedM5,
                        atr);

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
                    ScoreStructuralStopCandidate(
                        candidate,
                        closedM5,
                        direction,
                        entry,
                        stop,
                        atr,
                        riskAtr);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
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
                bestQuality < minimumQuality)
                return false;

            source = bestSource;
            quality = bestQuality;
            return true;
        }

        private double ResolveStructuralStopFrameAtr(
            Level candidate,
            int closedM5,
            double atr)
        {
            if (candidate == null ||
                candidate.Timeframe == "M5")
                return atr;

            Bars frame =
                ResolveStructuralStopFrame(
                    candidate.Timeframe);

            if (frame == null)
                return atr;

            int idx =
                ClosedIndex(
                    frame,
                    _m5Bars.OpenTimes[
                        closedM5]);

            if (idx < 10)
                return atr;

            double localAtr =
                Atr(
                    frame,
                    idx);

            return localAtr > 0
                ? localAtr
                : atr;
        }

        private Bars ResolveStructuralStopFrame(
            string timeframe)
        {
            switch (timeframe)
            {
                case "M15":
                    return _m15Bars;
                case "M30":
                    return _m30Bars;
                case "H1":
                    return _h1Bars;
                case "H4":
                    return _h4Bars;
                case "D1":
                    return _d1Bars;
                default:
                    return _w1Bars;
            }
        }

        private double ScoreStructuralStopCandidate(
            Level candidate,
            int closedM5,
            int direction,
            double entry,
            double stop,
            double atr,
            double riskAtr)
        {
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

            return score +
                riskBalance *
                Math.Max(
                    0,
                    StopRiskBalanceWeight) /
                20.0;
        }
    }
}
