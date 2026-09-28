// ============================================================================
// CFIP Indicator — UI/Chart/PredictionRenderer.cs
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
    }
}
