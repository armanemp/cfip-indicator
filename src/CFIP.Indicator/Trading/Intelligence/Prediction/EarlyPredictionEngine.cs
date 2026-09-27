// ============================================================================
// CFIP Indicator — EarlyPredictionEngine.cs
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
                        
                                    double buy =
                                        _m5Frame.BullScore * 0.55 +
                                        _m15Frame.BullScore * 0.45;
                        
                                    double sell =
                                        _m5Frame.BearScore * 0.55 +
                                        _m15Frame.BearScore * 0.45;
                        
                                    if (UseLiquidityForecast)
                                    {
                                        if (_m5Frame.LiquidityBull)
                                            buy += 8;
                        
                                        if (_m5Frame.LiquidityBear)
                                            sell += 8;
                                    }
                        
                                    if (_m5Frame.VolumeBull)
                                        buy += 2;
                        
                                    if (_m5Frame.VolumeBear)
                                        sell += 2;
                        
                                    if (_m5Frame.VwapBull)
                                        buy += 1;
                        
                                    if (_m5Frame.VwapBear)
                                        sell += 1;
                        
                                    p.Direction =
                                        buy >= sell
                                            ? 1
                                            : -1;
                        
                                    double total =
                                        Math.Max(
                                            1,
                                            buy + sell);
                        
                                    p.Confidence =
                                        ClampInt(
                                            (int)Math.Round(
                                                100.0 *
                                                Math.Max(
                                                    buy,
                                                    sell) /
                                                total),
                                            0,
                                            100);
                        
                                    if (p.Confidence <
                                        Math.Max(
                                            MinimumEarlyConfidence,
                                            EarlySetupConfidence))
                                        return p;
                        
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
                                                ? p.Entry -
                                                  atr * FallbackSlAtr
                                                : p.Entry +
                                                  atr * FallbackSlAtr;
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
                        
                                    List<Level> selected =
                                        SelectTargets(
                                            candidates,
                                            closedM5,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            atr);
                        
                                    p.Target1 =
                                        SelectTarget(
                                            selected,
                                            0,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp1MinimumRR);
                        
                                    p.Target2 =
                                        SelectTarget(
                                            selected,
                                            1,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp2MinimumRR);
                        
                                    p.Target3 =
                                        SelectTarget(
                                            selected,
                                            2,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp3MinimumRR);
                        
                                    p.Target4 =
                                        SelectTarget(
                                            selected,
                                            3,
                                            p.Entry,
                                            risk,
                                            p.Direction,
                                            Tp4MinimumRR);
                        
                                    p.Target =
                                        p.Target1;
                        
                                    p.Reason =
                                        (p.Direction == 1
                                            ? "BUY"
                                            : "SELL") +
                                        " EARLY | CONF " +
                                        p.Confidence;
                        
                                    return p;
                                }
    }
}
