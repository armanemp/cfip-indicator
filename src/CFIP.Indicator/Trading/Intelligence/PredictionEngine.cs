// ============================================================================
// CFIP Indicator — PredictionEngine.cs
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
        
        private void RenderPredictionObjects(
                            Prediction prediction,
                            int closedM5)
                        {
                            RemovePredictionObjects();
                
                            if (!ShowPredictionObjects ||
                                !EnableEarlyPrediction ||
                                prediction == null ||
                                prediction.Direction == 0 ||
                                prediction.Confidence <
                                Math.Max(
                                    MinimumEarlyConfidence,
                                    EarlySetupConfidence))
                                return;
                
                            int start =
                                MapM5ToChart(
                                    Math.Max(
                                        0,
                                        closedM5 -
                                        Math.Max(
                                            2,
                                            PredictionLookaheadBars / 2)),
                                    Bars.Count - 1);
                
                            int end =
                                MapM5ToChart(
                                    Math.Min(
                                        _m5Bars.Count - 1,
                                        closedM5 +
                                        Math.Max(
                                            2,
                                            PredictionLookaheadBars)),
                                    Bars.Count - 1);
                
                            if (end <= start)
                                end =
                                    Math.Min(
                                        Bars.Count - 1,
                                        start + 4);
                
                            if (ShowPredictionZone &&
                                prediction.ZoneHigh >
                                prediction.ZoneLow)
                            {
                                ChartRectangle zone =
                                    Chart.DrawRectangle(
                                        P + "PRED_ZONE",
                                        start,
                                        prediction.ZoneHigh,
                                        end,
                                        prediction.ZoneLow,
                                        PredictionColor,
                                        1,
                                        LineStyle.Solid);
                
                                zone.IsFilled = true;
                
                                zone.Color =
                                    Color.FromArgb(
                                        35,
                                        PredictionColor);
                
                                zone.IsInteractive = false;
                            }
                
                            if (prediction.Entry > 0)
                                DrawPredictionLine(
                                    P + "PRED_ENTRY",
                                    prediction.Entry);
                
                            if (prediction.StopLoss > 0)
                                DrawPredictionLine(
                                    P + "PRED_STOP",
                                    prediction.StopLoss);
                
                            if (prediction.Trigger > 0)
                                DrawPredictionLine(
                                    P + "PRED_TRIGGER",
                                    prediction.Trigger);
                
                            if (ShowPredictionTargets)
                            {
                                if (prediction.Target1 > 0)
                                    DrawPredictionLine(
                                        P + "PRED_TARGET1",
                                        prediction.Target1);
                
                                if (prediction.Target2 > 0)
                                    DrawPredictionLine(
                                        P + "PRED_TARGET2",
                                        prediction.Target2);
                
                                if (prediction.Target3 > 0)
                                    DrawPredictionLine(
                                        P + "PRED_TARGET3",
                                        prediction.Target3);
                
                                if (prediction.Target4 > 0)
                                    DrawPredictionLine(
                                        P + "PRED_TARGET4",
                                        prediction.Target4);
                            }
                
                            RenderPredictionLabels(
                                prediction,
                                closedM5);
                        }
        
        private void RenderPredictionLabels(
                            Prediction prediction,
                            int closedM5)
                        {
                            if (prediction == null ||
                                Bars == null ||
                                Bars.Count < 2)
                                return;
                
                            int bar =
                                MapM5ToChart(
                                    closedM5,
                                    Bars.Count - 1);
                
                            bar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        bar));
                
                            DrawPlanLabel(
                                P + "PRED_ENTRY_LABEL",
                                "ENTRY " +
                                Price(prediction.Entry),
                                bar,
                                prediction.Entry,
                                EntryLineColor);
                
                            if (prediction.StopLoss > 0)
                                DrawPlanLabel(
                                    P + "PRED_STOP_LABEL",
                                    "SL " +
                                    Price(prediction.StopLoss),
                                    bar,
                                    prediction.StopLoss,
                                    SlLineColor);
                
                            if (prediction.Trigger > 0 &&
                                !SamePrice(
                                    prediction.Trigger,
                                    prediction.Entry))
                                DrawPlanLabel(
                                    P + "PRED_TRIGGER_LABEL",
                                    "TRIGGER " +
                                    Price(prediction.Trigger),
                                    bar,
                                    prediction.Trigger,
                                    TriggerLineColor);
                
                            if (ShowPredictionTargets)
                            {
                                if (prediction.Target1 > 0 &&
                                    !SamePrice(
                                        prediction.Target1,
                                        prediction.Entry))
                                    DrawPlanLabel(
                                        P + "PRED_TARGET1_LABEL",
                                        "TP1 " +
                                        Price(prediction.Target1),
                                        bar,
                                        prediction.Target1,
                                        TpLineColor);
                
                                if (prediction.Target2 > 0 &&
                                    !SamePrice(
                                        prediction.Target2,
                                        prediction.Target1))
                                    DrawPlanLabel(
                                        P + "PRED_TARGET2_LABEL",
                                        "TP2 " +
                                        Price(prediction.Target2),
                                        bar,
                                        prediction.Target2,
                                        Tp2LineColor);
                
                                if (prediction.Target3 > 0 &&
                                    !SamePrice(
                                        prediction.Target3,
                                        prediction.Target2))
                                    DrawPlanLabel(
                                        P + "PRED_TARGET3_LABEL",
                                        "TP3 " +
                                        Price(prediction.Target3),
                                        bar,
                                        prediction.Target3,
                                        Tp3LineColor);
                
                                if (prediction.Target4 > 0 &&
                                    !SamePrice(
                                        prediction.Target4,
                                        prediction.Target3))
                                    DrawPlanLabel(
                                        P + "PRED_TARGET4_LABEL",
                                        "TP4 " +
                                        Price(prediction.Target4),
                                        bar,
                                        prediction.Target4,
                                        Tp4LineColor);
                            }
                        }
        
        private Color PredictionLineColor(
                            string name)
                        {
                            if (name.IndexOf(
                                    "PRED_TRIGGER",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return TriggerLineColor;
                
                            if (name.IndexOf(
                                    "PRED_STOP",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return SlLineColor;
                
                            if (name.IndexOf(
                                    "PRED_TARGET1",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return TpLineColor;
                
                            if (name.IndexOf(
                                    "PRED_TARGET2",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return Tp2LineColor;
                
                            if (name.IndexOf(
                                    "PRED_TARGET3",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return Tp3LineColor;
                
                            if (name.IndexOf(
                                    "PRED_TARGET4",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return Tp4LineColor;
                
                            if (name.IndexOf(
                                    "PRED_ENTRY",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return EntryLineColor;
                
                            return PredictionColor;
                        }
        
        private void DrawPredictionLine(
                            string name,
                            double price)
                        {
                            if (!IsFinitePositive(price) ||
                                Bars == null ||
                                Bars.Count < 2)
                            {
                                Chart.RemoveObject(name);
                                return;
                            }
                
                            try
                            {
                                int anchor =
                                    MapM5ToChart(
                                        _m5Bars == null
                                            ? Bars.Count - 1
                                            : Math.Max(
                                                1,
                                                _m5Bars.Count - 1),
                                        Bars.Count - 1);
                
                                anchor =
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            Bars.Count - 1,
                                            anchor));
                
                                int left =
                                    Math.Max(
                                        0,
                                        anchor -
                                        Math.Max(
                                            1,
                                            LineLengthBars));
                
                                int right =
                                    Math.Min(
                                        Bars.Count - 1,
                                        anchor +
                                        Math.Max(
                                            1,
                                            Math.Max(
                                                LineForwardBars,
                                                PredictionLookaheadBars)));
                
                                if (right <= left)
                                {
                                    Chart.RemoveObject(name);
                                    return;
                                }
                
                                double normalized =
                                    NormalizePrice(price);
                
                                if (!IsFinitePositive(normalized))
                                {
                                    Chart.RemoveObject(name);
                                    return;
                                }
                
                                ChartTrendLine line =
                                    Chart.FindObject(name)
                                    as ChartTrendLine;
                
                                LineStyle lineStyle =
                                    LineStyle.Solid;
                
                                if (name.IndexOf(
                                        "PRED_TRIGGER",
                                        StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    lineStyle =
                                        LineStyle.Solid;
                                }
                                else if (name.IndexOf(
                                            "PRED_TARGET",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    lineStyle =
                                        LineStyle.Solid;
                                }
                
                                if (line == null)
                                {
                                    ChartObject existing =
                                        Chart.FindObject(name);
                
                                    if (existing != null)
                                        Chart.RemoveObject(name);
                
                                    line =
                                        Chart.DrawTrendLine(
                                            name,
                                            left,
                                            normalized,
                                            right,
                                            normalized,
                                            PredictionLineColor(name),
                                            Math.Max(
                                                1,
                                                LevelLineThickness),
                                            lineStyle);
                                }
                
                                if (line == null)
                                    return;
                
                                line.Time1 =
                                    Bars.OpenTimes[left];
                                line.Y1 =
                                    normalized;
                                line.Time2 =
                                    Bars.OpenTimes[right];
                                line.Y2 =
                                    normalized;
                                line.Color =
                                    PredictionLineColor(name);
                                line.Thickness =
                                    Math.Max(
                                        1,
                                        LevelLineThickness);
                                line.LineStyle =
                                    lineStyle;
                                line.ExtendToInfinity =
                                    false;
                                line.IsInteractive =
                                    false;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP prediction render failed: {0}",
                                    ex.Message);
                            }
                        }
        
        private void RemovePredictionObjects()
                        {
                            Chart.RemoveObject(
                                P + "PRED_ZONE");
                
                            Chart.RemoveObject(
                                P + "PRED_ENTRY");
                
                            Chart.RemoveObject(
                                P + "PRED_ENTRY_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_STOP");
                
                            Chart.RemoveObject(
                                P + "PRED_STOP_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_TRIGGER");
                
                            Chart.RemoveObject(
                                P + "PRED_TRIGGER_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET1");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET1_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET2");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET2_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET3");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET3_LABEL");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET4");
                
                            Chart.RemoveObject(
                                P + "PRED_TARGET4_LABEL");
                        }
        
        private bool CheckLiveReversalAgainstPlan(
                            int closedM5)
                        {
                            if (_plan == null ||
                                !_plan.IsLivePosition ||
                                _m5Frame == null ||
                                !EnableLiveStructuralReversal ||
                                !EnableFastReversalIntelligence)
                                return false;
                
                            int opposite =
                                _plan.Direction * -1;
                
                            bool structural =
                                opposite == 1
                                    ? (_m5Frame.MssBull ||
                                       _m5Frame.ChochBull)
                                    : (_m5Frame.MssBear ||
                                       _m5Frame.ChochBear);
                
                            bool force =
                                opposite == 1
                                    ? (_m5Frame.DisplacementBull &&
                                       _m5Frame.LiquidityBull)
                                    : (_m5Frame.DisplacementBear &&
                                       _m5Frame.LiquidityBear);
                
                            int reversalConfidence =
                                _reaction != null
                                    ? Math.Max(
                                        _reaction.Confidence,
                                        _m5Frame.Quality)
                                    : _m5Frame.Quality;
                
                            if (reversalConfidence <
                                Math.Max(
                                    50,
                                    LiveReversalMinimumConfidence))
                                return false;
                
                            if (!structural ||
                                _m5Frame.Quality <
                                LiveReversalStructuralScore ||
                                _m5Frame.Evidence <
                                LiveReversalMinimumEvidence)
                                return false;
                
                            if (RequireReversalForce &&
                                !force)
                                return false;
                
                            if (!AllowReversalAgainstStaleHtf &&
                                _m15Frame != null &&
                                _m15Frame.Direction ==
                                _plan.Direction)
                                return false;
                
                            if (RequireM15ReversalForOpposite &&
                                _m15Frame != null &&
                                _m15Frame.Direction !=
                                opposite)
                                return false;
                
                            if (StructuralSequence(
                                    _m5Bars,
                                    closedM5,
                                    opposite) <
                                MinimumOppositeM5Structure)
                                return false;
                
                            if (PreventRapidDirectionFlip &&
                                _lastSignalM5 >= 0 &&
                                closedM5 -
                                _lastSignalM5 <
                                OppositeSignalCooldownM5)
                                return false;
                
                            int flipBars =
                                Math.Max(
                                    1,
                                    SmartFlipConfirmationBars);
                
                            if (flipBars > 1 &&
                                !StableDirection(
                                    _m5Bars,
                                    closedM5,
                                    opposite,
                                    flipBars))
                                return false;
                
                            SendUnifiedAlert(
                                "REVERSAL|" +
                                closedM5 +
                                "|" +
                                opposite,
                                "CFIP ACTIVE PLAN REVERSAL | " +
                                (opposite == 1
                                    ? "BUY"
                                    : "SELL") +
                                " REVERSAL | Q " +
                                _m5Frame.Quality,
                                opposite,
                                true);
                
                            Position livePosition =
                                GetManagedLivePositionForPlan();
                
                            if (livePosition != null)
                            {
                                if (!EnableReversalProtectionClose ||
                                    livePosition.NetProfit <= 0)
                                {
                                    SetAutoTradingState(
                                        "BLOCKED",
                                        "REVERSAL DETECTED • POSITION RETAINED");
                                    return false;
                                }
                
                                double protectedProfit =
                                    livePosition.NetProfit;
                
                                SetLifecycleState(
                                    LifecycleState.ExitRequested,
                                    "ACTIVE PLAN REVERSAL");
                
                                if (!TryClosePosition(
                                        livePosition,
                                        "ACTIVE PLAN REVERSAL"))
                                {
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "ACTIVE PLAN REVERSAL • EXIT REJECTED");
                
                                    SetAutoTradingState(
                                        "ERROR",
                                        "REVERSAL CLOSE REJECTED");
                
                                    return false;
                                }
                
                                _lastExitM5 =
                                    Math.Max(
                                        _lastExitM5,
                                        closedM5);
                
                                SetAutoTradingState(
                                    "EXECUTED",
                                    "REVERSAL EXIT REQUESTED #" +
                                    livePosition.Id);
                
                                SendUnifiedAlert(
                                    "REVERSAL-CLOSE|" +
                                    livePosition.Id,
                                    "CFIP ACTIVE PLAN REVERSAL • EXIT REQUESTED | #" +
                                    livePosition.Id +
                                    " | protected +" +
                                    protectedProfit.ToString("F2"),
                                    _plan.Direction,
                                    true);
                
                                return true;
                            }
                
                            SetLifecycleState(
                                LifecycleState.Closed,
                                "REVERSAL • POSITION ABSENT");
                
                            _plan = null;
                            _executionModel = null;
                            RemovePlanObjects();
                
                            return true;
                        }
        
        private Position GetManagedLivePositionForPlan()
                        {
                            if (_plan == null ||
                                !_plan.IsLivePosition ||
                                _plan.PositionId <= 0)
                                return null;
                
                            foreach (Position position in Positions)
                            {
                                if (position != null &&
                                    position.SymbolName == SymbolName &&
                                    position.Id == _plan.PositionId)
                                    return position;
                            }
                
                            return null;
                        }
    }
}
