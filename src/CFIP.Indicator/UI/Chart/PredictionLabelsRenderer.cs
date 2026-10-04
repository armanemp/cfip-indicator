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
        private void RenderPredictionLabels(
                                    Prediction prediction,
                                    int closedM5)
                                {
                                    if (prediction == null ||
                                        Bars == null ||
                                        Bars.Count < 2)
                                        return;
                        
                                    int lineLeftBar =
                                        GetPredictionLineLeftBar(
                                            closedM5);
                        
                                    DrawPlanLabel(
                                        P + "PRED_ENTRY_LABEL",
                                        "ENTRY " +
                                        Price(prediction.Entry),
                                                lineLeftBar,
                                        prediction.Entry,
                                        EntryLineColor);
                        
                                    if (prediction.StopLoss > 0)
                                        DrawPlanLabel(
                                            P + "PRED_STOP_LABEL",
                                            "SL " +
                                            Price(prediction.StopLoss),
                                                lineLeftBar,
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
                                                lineLeftBar,
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
                                                lineLeftBar,
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
                                                lineLeftBar,
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
                                                lineLeftBar,
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
                                                lineLeftBar,
                                                prediction.Target4,
                                                Tp4LineColor);
                                    }
                                }
    }
}
