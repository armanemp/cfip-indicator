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

            // Canonical MTF trend arrows are rendered once by CalculationLiveCycle.
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

        private PlanLevelVisualState BuildPlanLevelVisualState(
            SignalVisualSnapshot snapshot,
            bool preview)
        {
            double entry =
                preview ? snapshot.SetupEntry : snapshot.Entry;
            double idealEntry =
                preview ? snapshot.SetupIdealEntry : snapshot.IdealEntry;
            double trigger =
                preview ? snapshot.SetupTrigger : snapshot.Trigger;
            double stop =
                preview ? snapshot.SetupStop : snapshot.Stop;
            double tp1 =
                preview ? snapshot.SetupTp1 : snapshot.Tp1;
            double tp2 =
                preview ? snapshot.SetupTp2 : snapshot.Tp2;
            double tp3 =
                preview ? snapshot.SetupTp3 : snapshot.Tp3;
            double tp4 =
                preview ? snapshot.SetupTp4 : snapshot.Tp4;
            double activeTarget =
                snapshot.BrokerTarget;

            bool idealDistinct =
                IsFinitePositive(idealEntry) &&
                !SamePrice(idealEntry, entry);

            bool idealAllowed =
                preview
                    ? idealDistinct
                    : snapshot.IdealEntryVisible && idealDistinct;

            bool triggerDistinct =
                IsFinitePositive(trigger) &&
                !SamePrice(trigger, entry) &&
                !SamePrice(trigger, idealEntry);

            bool triggerAllowed =
                preview
                    ? true
                    : snapshot.TriggerVisible && !snapshot.LivePosition;

            bool tp1Distinct =
                IsFinitePositive(tp1) &&
                !SamePrice(tp1, entry) &&
                !SamePrice(tp1, trigger) &&
                !SamePrice(tp1, idealEntry);

            bool tp2Distinct =
                IsFinitePositive(tp2) &&
                !SamePrice(tp2, entry) &&
                !SamePrice(tp2, trigger) &&
                !SamePrice(tp2, idealEntry) &&
                !SamePrice(tp2, tp1);

            bool tp3Distinct =
                IsFinitePositive(tp3) &&
                !SamePrice(tp3, entry) &&
                !SamePrice(tp3, trigger) &&
                !SamePrice(tp3, idealEntry) &&
                !SamePrice(tp3, tp1) &&
                !SamePrice(tp3, tp2);

            bool tp4Distinct =
                IsFinitePositive(tp4) &&
                !SamePrice(tp4, entry) &&
                !SamePrice(tp4, trigger) &&
                !SamePrice(tp4, idealEntry) &&
                !SamePrice(tp4, tp1) &&
                !SamePrice(tp4, tp2) &&
                !SamePrice(tp4, tp3);

            bool activeTargetDistinct =
                !preview &&
                snapshot.ActiveBrokerTargetVisible &&
                IsFinitePositive(activeTarget) &&
                !SamePrice(activeTarget, entry) &&
                !SamePrice(activeTarget, idealEntry) &&
                !SamePrice(activeTarget, trigger) &&
                !SamePrice(activeTarget, stop) &&
                !SamePrice(activeTarget, tp1) &&
                !SamePrice(activeTarget, tp2) &&
                !SamePrice(activeTarget, tp3) &&
                !SamePrice(activeTarget, tp4);

            List<PlanLevelVisual> levels =
                new List<PlanLevelVisual>
                {
                    new PlanLevelVisual(
                        "ENTRY",
                        "ENTRY_LABEL",
                        entry,
                        EntryLineColor,
                        ShowEntry,
                        false),

                    new PlanLevelVisual(
                        "IDEAL_ENTRY",
                        "IDEAL_ENTRY_LABEL",
                        idealEntry,
                        PanelAccentColor,
                        ShowEntry && idealAllowed,
                        false),

                    new PlanLevelVisual(
                        "TRIGGER",
                        "TRIGGER_LABEL",
                        trigger,
                        TriggerLineColor,
                        ShowTrigger &&
                        triggerDistinct &&
                        triggerAllowed,
                        false),

                    new PlanLevelVisual(
                        "SL",
                        "SL_LABEL",
                        stop,
                        SlLineColor,
                        ShowSL,
                        true),

                    new PlanLevelVisual(
                        "TP1",
                        "TP1_LABEL",
                        tp1,
                        TpLineColor,
                        ShowTP1 && tp1Distinct,
                        true),

                    new PlanLevelVisual(
                        "TP2",
                        "TP2_LABEL",
                        tp2,
                        Tp2LineColor,
                        ShowTP2 && tp2Distinct,
                        true),

                    new PlanLevelVisual(
                        "TP3",
                        "TP3_LABEL",
                        tp3,
                        Tp3LineColor,
                        ShowTP3 && tp3Distinct,
                        true),

                    new PlanLevelVisual(
                        "TP4",
                        "TP4_LABEL",
                        tp4,
                        Tp4LineColor,
                        ShowTP4 && tp4Distinct,
                        true),

                    new PlanLevelVisual(
                        "ACTIVE_TP",
                        "ACTIVE_TP_LABEL",
                        activeTarget,
                        PanelAccentColor,
                        (ShowTP1 ||
                         ShowTP2 ||
                         ShowTP3 ||
                         ShowTP4) &&
                        activeTargetDistinct,
                        true)
                };

            return new PlanLevelVisualState(
                entry,
                idealEntry,
                trigger,
                stop,
                tp1,
                tp2,
                tp3,
                tp4,
                activeTarget,
                levels);
        }

        private void RemoveAllPlanLevelLines()
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
        }

        private void RenderLevelLines(
            SignalVisualSnapshot snapshot,
            bool preview)
        {
            if (snapshot == null)
            {
                RemoveAllPlanLevelLines();
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
                RemoveAllPlanLevelLines();
                return;
            }

            PlanLevelVisualState state =
                BuildPlanLevelVisualState(
                    snapshot,
                    preview);

            for (int i = 0;
                 i < state.Levels.Count;
                 i++)
            {
                PlanLevelVisual level =
                    state.Levels[i];

                DrawPlanLine(
                    P + level.Key,
                    level.Price,
                    level.Color,
                    level.Visible);
            }
        }
    }
}
