using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanRenderCoordinator.cs
// Single-responsibility chart plan renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                                private void RenderPlan(
            SignalVisualSnapshot snapshot)
        {
            if (_plan == null ||
                snapshot == null ||
                !snapshot.PlanActive ||
                snapshot.PendingOrder)
            {
                RemovePlanObjects();
                return;
            }

            RemovePredictionObjects();
            ClearWatchObjects();
            RenderLevelLines(snapshot, false);

            if (ShowLevelPriceLabels ||
                ShowSignalLabels)
                RenderPlanLabels(snapshot, false);
            else
                RemovePlanLabels();

            if (!ShowSignalArrow ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveStackedSignalArrows();
            }
            else
            {
            // The active-plan direction arrow is live guidance: keep it on
            // the current chart bar so it continuously follows the active path.
            int hostBar =
                Bars.Count - 1;
            double atr =
                Atr(
                    Bars,
                    Math.Max(
                        1,
                        Math.Min(
                            Bars.Count - 1,
                            hostBar)));
            double offset =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr * 0.18);
            RenderStackedSignalArrows(
                snapshot,
                snapshot.PlanDirection,
                hostBar,
                offset);
            }

        }

        private void RenderSetupPreview(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null ||
                !snapshot.SetupPreviewActive ||
                snapshot.PendingOrder)
            {
                RemovePlanObjects();
                return;
            }

            RemovePredictionObjects();
            RenderLevelLines(snapshot, true);

            if (ShowLevelPriceLabels ||
                ShowSignalLabels)
                RenderPlanLabels(snapshot, true);
            else
                RemovePlanLabels();
            RemoveStackedSignalArrows();

        }

        private void RenderLevelLines(
            SignalVisualSnapshot snapshot,
            bool preview)
        {
            if (snapshot == null)
            {
                RemovePlanLine(P + "ENTRY");
                RemovePlanLine(P + "IDEAL_ENTRY");
                RemovePlanLine(P + "TRIGGER");
                RemovePlanLine(P + "SL");
                RemovePlanLine(P + "TP1");
                RemovePlanLine(P + "TP2");
                RemovePlanLine(P + "TP3");
                RemovePlanLine(P + "TP4");
                RemovePlanLine(P + "ACTIVE_TP");
                return;
            }

            bool active =
                preview
                    ? snapshot.SetupPreviewActive
                    : snapshot.PlanActive;
            if (!active ||
                snapshot.PendingOrder ||
                !ShowLevelLines)
            {
                RemovePlanLine(P + "ENTRY");
                RemovePlanLine(P + "IDEAL_ENTRY");
                RemovePlanLine(P + "TRIGGER");
                RemovePlanLine(P + "SL");
                RemovePlanLine(P + "TP1");
                RemovePlanLine(P + "TP2");
                RemovePlanLine(P + "TP3");
                RemovePlanLine(P + "TP4");
                RemovePlanLine(P + "ACTIVE_TP");
                return;
            }

            double entry = preview ? snapshot.SetupEntry : snapshot.Entry;
            double idealEntry = preview ? snapshot.SetupIdealEntry : snapshot.IdealEntry;
            double trigger = preview ? snapshot.SetupTrigger : snapshot.Trigger;
            double stop = preview ? snapshot.SetupStop : snapshot.Stop;
            double tp1 = preview ? snapshot.SetupTp1 : snapshot.Tp1;
            double tp2 = preview ? snapshot.SetupTp2 : snapshot.Tp2;
            double tp3 = preview ? snapshot.SetupTp3 : snapshot.Tp3;
            double tp4 = preview ? snapshot.SetupTp4 : snapshot.Tp4;
            DrawPlanLine(P + "ENTRY", entry, EntryLineColor, ShowEntry);

            bool idealDistinct =
                IsFinitePositive(idealEntry) &&
                !SamePrice(idealEntry, entry);
            DrawPlanLine(
                P + "IDEAL_ENTRY",
                idealEntry,
                PanelAccentColor,
                ShowEntry && idealDistinct);

            bool triggerDistinct =
                IsFinitePositive(trigger) &&
                !SamePrice(trigger, entry) &&
                (!idealDistinct || !SamePrice(trigger, idealEntry));
            bool triggerVisible =
                preview
                    ? triggerDistinct
                    : snapshot.TriggerVisible && triggerDistinct;
            DrawPlanLine(
                P + "TRIGGER",
                trigger,
                TriggerLineColor,
                ShowTrigger && triggerVisible);

            DrawPlanLine(
                P + "SL",
                stop,
                SlLineColor,
                ShowSL);

            bool tp1Distinct =
                IsFinitePositive(tp1) &&
                !SamePrice(tp1, entry) &&
                !SamePrice(tp1, trigger);
            DrawPlanLine(
                P + "TP1",
                tp1,
                TpLineColor,
                ShowTP1 && tp1Distinct);

            bool tp2Distinct =
                IsFinitePositive(tp2) &&
                (!tp1Distinct || !SamePrice(tp2, tp1)) &&
                !SamePrice(tp2, entry) &&
                !SamePrice(tp2, trigger);
            DrawPlanLine(
                P + "TP2",
                tp2,
                Tp2LineColor,
                ShowTP2 && tp2Distinct);

            bool tp3Distinct =
                IsFinitePositive(tp3) &&
                (!tp2Distinct || !SamePrice(tp3, tp2)) &&
                !SamePrice(tp3, entry) &&
                !SamePrice(tp3, trigger);
            DrawPlanLine(
                P + "TP3",
                tp3,
                Tp3LineColor,
                ShowTP3 && tp3Distinct);

            bool tp4Distinct =
                IsFinitePositive(tp4) &&
                (!tp3Distinct || !SamePrice(tp4, tp3)) &&
                !SamePrice(tp4, entry) &&
                !SamePrice(tp4, trigger);
            DrawPlanLine(
                P + "TP4",
                tp4,
                Tp4LineColor,
                ShowTP4 && tp4Distinct);

            double activeBrokerTarget = snapshot.BrokerTarget;
            bool activeBrokerTargetDistinct =
                !preview &&
                snapshot.ActiveBrokerTargetVisible &&
                !SamePrice(activeBrokerTarget, entry) &&
                !SamePrice(activeBrokerTarget, tp1) &&
                !SamePrice(activeBrokerTarget, tp2) &&
                !SamePrice(activeBrokerTarget, tp3) &&
                !SamePrice(activeBrokerTarget, tp4);
            DrawPlanLine(
                P + "ACTIVE_TP",
                activeBrokerTarget,
                PanelAccentColor,
                !preview &&
                (ShowTP1 || ShowTP2 || ShowTP3 || ShowTP4) &&
                activeBrokerTargetDistinct);
        }
    }
}
