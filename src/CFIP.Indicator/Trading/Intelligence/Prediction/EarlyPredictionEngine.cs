// ============================================================================
// CFIP Indicator — EarlyPredictionEngine.cs
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
        private Prediction BuildEarlyPrediction(
                                    int closedM5)
                                {
                                    Prediction p =
                                        new Prediction();
                        
                                    if (!EnableEarlyPrediction ||
                                        _m5Frame == null ||
                                        _m15Frame == null ||
                                        closedM5 < 10)
                                        return p;
                        
                                    EarlyPredictionScoreResult score =
                                        EarlyPredictionScoreRule.Evaluate(
                                            _m5Frame.BullScore,
                                            _m5Frame.BearScore,
                                            _m15Frame.BullScore,
                                            _m15Frame.BearScore,
                                            UseLiquidityForecast,
                                            _m5Frame.LiquidityBull,
                                            _m5Frame.LiquidityBear,
                                            _m5Frame.VolumeBull,
                                            _m5Frame.VolumeBear,
                                            _m5Frame.VwapBull,
                                            _m5Frame.VwapBear);

                                    p.DirectionalShare =
                                        score.DirectionalShare;

                                    p.Confidence =
                                        p.DirectionalShare;

                                    p.AbsoluteStrength =
                                        score.AbsoluteStrength;

                                    p.BuyStrength =
                                        score.BuyStrength;

                                    p.SellStrength =
                                        score.SellStrength;

                                    p.TotalStrength =
                                        score.TotalStrength;

                                    int candidateDirection =
                                        score.Direction;
                        
                                    MarketRegimeSnapshot regime =
                                        GetActiveM5Regime(
                                            closedM5);

                                    if (regime != null)
                                    {
                                        if (regime.Regime == "RANGE" ||
                                            regime.Regime == "COMPRESSION")
                                        {
                                            p.Direction = 0;
                                            p.Reason =
                                                "EARLY BLOCK • " +
                                                regime.Regime;
                                            return p;
                                        }

                                        if (regime.Regime == "TRANSITION" &&
                                            p.Confidence <
                                            Math.Max(
                                                MinimumEarlyConfidence + 8,
                                                EarlySetupConfidence + 8))
                                        {
                                            p.Direction = 0;
                                            p.Reason =
                                                "EARLY BLOCK • TRANSITION";
                                            return p;
                                        }

                                        if (regime.Regime == "HIGH_VOLATILITY" &&
                                            p.Confidence <
                                            Math.Max(
                                                MinimumEarlyConfidence + 12,
                                                EarlySetupConfidence + 12))
                                        {
                                            p.Direction = 0;
                                            p.Reason =
                                                "EARLY BLOCK • HIGH VOLATILITY";
                                            return p;
                                        }

                                        if ((regime.Regime == "TREND" ||
                                             regime.Regime == "EXPANSION") &&
                                            regime.Direction != 0 &&
                                            regime.Direction != candidateDirection &&
                                            p.Confidence <
                                            Math.Max(
                                                70,
                                                EarlySetupConfidence + 10))
                                        {
                                            p.Direction = 0;
                                            p.Reason =
                                                "EARLY BLOCK • REGIME CONFLICT";
                                            return p;
                                        }
                                    }

                                    int minimumDirectionalShare =
                                        Math.Max(
                                            MinimumEarlyConfidence,
                                            EarlySetupConfidence);

                                    double minimumAbsoluteStrength =
                                        Math.Max(
                                            1,
                                            MinimumEarlyConfidence);

                                    if (candidateDirection == 0)
                                    {
                                        p.Direction = 0;
                                        p.Reason =
                                            "NEUTRAL EARLY";
                                        return p;
                                    }

                                    if (p.AbsoluteStrength <
                                        minimumAbsoluteStrength)
                                    {
                                        p.Direction = 0;
                                        p.Reason =
                                            "EARLY BLOCK • ABSOLUTE STRENGTH " +
                                            p.AbsoluteStrength.ToString(
                                                "F1",
                                                CultureInfo.InvariantCulture) +
                                            " < " +
                                            minimumAbsoluteStrength.ToString(
                                                "F1",
                                                CultureInfo.InvariantCulture);
                                        return p;
                                    }

                                    if (p.DirectionalShare <
                                        minimumDirectionalShare)
                                    {
                                        p.Direction = 0;
                                        p.Reason =
                                            "EARLY WATCH | SHARE " +
                                            p.DirectionalShare;
                                        return p;
                                    }
                        
                                    p.Direction =
                                        candidateDirection;
                        
                                    double atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                        return p;
                        
                                    ExecutionModel predictionExecution =
                                        BuildExecutionModel(
                                            closedM5,
                                            p.Direction);
                        
                                    p.Mode =
                                        predictionExecution == null
                                            ? ExecutionMode.None
                                            : predictionExecution.Mode;
                        
                                    if (predictionExecution != null &&
                                        predictionExecution.ZoneHigh >
                                        predictionExecution.ZoneLow)
                                    {
                                        p.ZoneLow =
                                            NormalizePrice(
                                                predictionExecution.ZoneLow);
                        
                                        p.ZoneHigh =
                                            NormalizePrice(
                                                predictionExecution.ZoneHigh);
                        
                                        p.Trigger =
                                            NormalizePrice(
                                                predictionExecution.Trigger);
                        
                                        p.Entry =
                                            predictionExecution.Ready &&
                                            IsFinitePositive(
                                                predictionExecution.ActualEntry)
                                                ? NormalizePrice(
                                                    predictionExecution.ActualEntry)
                                                : p.Mode ==
                                                  ExecutionMode.WaitingForTrigger
                                                    ? NormalizePrice(
                                                        predictionExecution.Trigger)
                                                    : NormalizePrice(
                                                        _m5Bars.ClosePrices[
                                                            closedM5]);
                                    }
                                    else
                                    {
                                        p.Entry =
                                            NormalizePrice(
                                                _m5Bars.ClosePrices[
                                                    closedM5]);
                        
                                        p.ZoneLow =
                                            p.Entry -
                                            atr * 0.30;
                        
                                        p.ZoneHigh =
                                            p.Entry +
                                            atr * 0.30;
                        
                                        p.Trigger =
                                            p.Direction == 1
                                                ? p.ZoneHigh +
                                                  atr * EntryBufferAtr
                                                : p.ZoneLow -
                                                  atr * EntryBufferAtr;
                                    }
                        
                                    string stopSource;
                                    int stopQuality;
                        
                                    p.StopLoss =
                                        BuildStructuralStop(
                                            closedM5,
                                            p.Direction,
                                            p.Entry,
                                            atr,
                                            out stopSource,
                                            out stopQuality);
                        
                                    if (!IsFinitePositive(
                                            p.StopLoss) &&
                                        AllowExecutionFrameStopFallback)
                                    {
                                        p.StopLoss =
                                            p.Direction == 1
                                                : 0;
                                    }
                        
                                    double risk =
                                        Math.Abs(
                                            p.Entry -
                                            p.StopLoss);
                        
                                    if (risk <= 0)
                                        return p;
                        
                                    List<Level> candidates =
                                        BuildTargetLevels(
                                            closedM5,
                                            p.Direction,
                                            p.Entry,
                                            atr);
                        
                                    OpportunityLane lane =
                                        OpportunityLane.Strategic;

                                    List<Level> selected =
                                        SelectTargets(
                                            candidates,
                                            closedM5,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            atr,
                                            lane);
                        
                                    p.Target1 =
                                        SelectTarget(
                                            selected,
                                            0,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp1MinimumRR,
                                            lane);
                        
                                    p.Target2 =
                                        SelectTarget(
                                            selected,
                                            1,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp2MinimumRR,
                                            lane);
                        
                                    p.Target3 =
                                        SelectTarget(
                                            selected,
                                            2,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp3MinimumRR,
                                            lane);
                        
                                    p.Target4 =
                                        SelectTarget(
                                            selected,
                                            3,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp4MinimumRR,
                                            lane);
                        
                                    p.Target =
                                        p.Target1;
                        
                                    p.Reason =
                                        (p.Direction == 1
                                            ? "BUY"
                                            : "SELL") +
                                        " EARLY | SHARE " +
                                        p.DirectionalShare +
                                        " | STRENGTH " +
                                        p.AbsoluteStrength.ToString(
                                            "F1",
                                            CultureInfo.InvariantCulture);
                        
                                    return p;
                                }
    }
}
