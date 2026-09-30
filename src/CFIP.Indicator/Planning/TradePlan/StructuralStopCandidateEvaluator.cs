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
                         atr)) /
                    Math.Max(
                        0.02,
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

                if (!IsFinitePositive(frameAtr))
                    continue;

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

                double bestTp1RR =
                    EstimateBestTp1RRForStop(
                        closedM5,
                        direction,
                        entry,
                        stop,
                        atr);

                if (!IsFinitePositive(bestTp1RR))
                    continue;

                PlanRewardRiskQualityResult rewardRisk =
                    PlanRewardRiskQualityRule.Evaluate(
                        direction,
                        entry,
                        stop,
                        direction == 1
                            ? entry + risk * bestTp1RR
                            : entry - risk * bestTp1RR,
                        atr,
                        Math.Max(
                            0,
                            Symbol.Ask - Symbol.Bid),
                        Math.Max(
                            Tp1MinimumRR,
                            MinimumRequiredRRForRegime(
                                _decision == null
                                    ? "UNKNOWN"
                                    : _decision.Regime)),
                        PreferredStopRiskAtr,
                        maxRiskAtr);

                if (!rewardRisk.Allowed)
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

                // Prefer structurally valid stops that leave a larger
                // reward path after accounting for stop width.
                score +=
                    StructuralStopScoringRule.CalculateRewardPathBonus(
                        bestTp1RR,
                        rewardRisk.RequiredRR);

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
            if (candidate == null)
                return 0;

            if (candidate.Timeframe == "M5")
                return atr;

            if (!StructuralTimeframeRule.IsSupported(
                    candidate.Timeframe))
                return 0;

            Bars frame =
                ResolveStructuralStopFrame(
                    candidate.Timeframe);

            if (frame == null)
                return 0;

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
                case "W1":
                    return _w1Bars;
                default:
                    // Unknown structural timeframes are invalid. Never
                    // substitute W1 silently because doing so changes the
                    // structural owner of the stop candidate.
                    return null;
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

            double riskBalance =
                StructuralStopScoringRule.CalculateRiskBalance(
                    riskAtr,
                    PreferredStopRiskAtr,
                    StopRiskBalanceWeight);

            return score +
                riskBalance;
        }

        private double EstimateBestTp1RRForStop(
            int closedM5,
            int direction,
            double entry,
            double stop,
            double atr)
        {
            if (_m5Bars == null ||
                closedM5 < 0 ||
                direction != 1 && direction != -1 ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(stop) ||
                !IsFinitePositive(atr))
                return 0;

            double risk =
                Math.Abs(
                    entry - stop);

            if (!IsFinitePositive(risk))
                return 0;

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    entry,
                    atr);

            if (levels == null || levels.Count == 0)
                return 0;

            double requiredRR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRRForRegime(
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime));

            double maximumRR =
                Math.Max(
                    requiredRR,
                    MaximumRewardRR);

            double bestRR = 0;

            for (int i = 0;
                 i < levels.Count;
                 i++)
            {
                Level candidate = levels[i];

                if (candidate == null ||
                    !IsValidTarget(
                        direction,
                        entry,
                        candidate.Price))
                    continue;

                if (!TryScoreTargetCandidate(
                        candidate,
                        new List<Level>(),
                        closedM5,
                        entry,
                        risk,
                        direction,
                        atr,
                        requiredRR,
                        maximumRR,
                        entry,
                        false,
                        0,
                        out _))
                    continue;

                double rr =
                    Math.Abs(
                        candidate.Price -
                        entry) /
                    risk;

                if (rr > bestRR)
                    bestRR = rr;
            }

            return bestRR;
        }

    }
}
