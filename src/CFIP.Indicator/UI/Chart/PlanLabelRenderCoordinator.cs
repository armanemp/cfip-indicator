using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelRenderCoordinator.cs
// Single-responsibility plan-label renderer.

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPlanLabels(
            SignalVisualSnapshot snapshot,
            bool preview)
        {
            if ((!preview && _plan == null) ||
                snapshot == null ||
                (preview
                    ? !snapshot.SetupPreviewActive
                    : !snapshot.PlanActive) ||
                snapshot.PendingOrder ||
                !ShowLevelLines ||
                (!ShowLevelPriceLabels &&
                 !ShowSignalLabels) ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemovePlanLabels();
                return;
            }

            int lineLeft =
                GetPlanLineLeftBar();

            int labelBar =
                GetCompactPlanLabelAnchorBar(
                    lineLeft);

            double entry =
                preview
                    ? snapshot.SetupEntry
                    : snapshot.Entry;

            double idealEntry =
                preview
                    ? snapshot.SetupIdealEntry
                    : snapshot.IdealEntry;

            double trigger =
                preview
                    ? snapshot.SetupTrigger
                    : snapshot.Trigger;

            double stop =
                preview
                    ? snapshot.SetupStop
                    : snapshot.Stop;

            double tp1 =
                preview
                    ? snapshot.SetupTp1
                    : snapshot.Tp1;

            double tp2 =
                preview
                    ? snapshot.SetupTp2
                    : snapshot.Tp2;

            double tp3 =
                preview
                    ? snapshot.SetupTp3
                    : snapshot.Tp3;

            double tp4 =
                preview
                    ? snapshot.SetupTp4
                    : snapshot.Tp4;

            if (ShowEntry)
            {
                RenderCompactPlanLabel(
                    P + "ENTRY_LABEL",
                    BuildPlanLevelLabel(
                        "ENTRY",
                        entry,
                        entry,
                        false),
                    entry,
                    EntryLineColor,
                    true,
                    lineLeft,
                    labelBar);
            }
            else
            {
                RemovePlanLabel(P + "ENTRY_LABEL");
            }

            bool idealDistinct =
                (preview ? IsFinitePositive(idealEntry) && !SamePrice(idealEntry, entry) : snapshot.IdealEntryVisible);

            RenderCompactPlanLabel(
                P + "IDEAL_ENTRY_LABEL",
                snapshot.EntryMode ==
                    ExecutionMode.BreakoutMarket
                    ? BuildPlanLevelLabel(
                        "ZONE MID",
                        idealEntry,
                        entry,
                        false)
                    : BuildPlanLevelLabel(
                        "IDEAL",
                        idealEntry,
                        entry,
                        false),
                idealEntry,
                PanelAccentColor,
                ShowEntry && idealDistinct,
                    lineLeft,
                    labelBar);

            bool triggerDistinct =
                IsFinitePositive(trigger) &&
                !SamePrice(
                    trigger,
                    entry) &&
                (!idealDistinct ||
                 !SamePrice(
                     trigger,
                     idealEntry));

            RenderCompactPlanLabel(
                P + "TRIGGER_LABEL",
                BuildPlanLevelLabel(
                    "TRIGGER",
                    trigger,
                    entry,
                    false),
                trigger,
                TriggerLineColor,
                ShowTrigger &&
                triggerDistinct &&
                !snapshot.LivePosition,
                    lineLeft,
                    labelBar);

            double displayStop = stop;

            RenderCompactPlanLabel(
                P + "SL_LABEL",
                BuildPlanLevelLabel(
                "SL",
                displayStop,
                entry,
                true),
                displayStop,
                SlLineColor,
                ShowSL,
                    lineLeft,
                    labelBar);

            bool tp1Distinct =
                IsFinitePositive(tp1) &&
                !SamePrice(
                    tp1,
                    entry) &&
                !SamePrice(
                    tp1,
                    trigger);

            RenderCompactPlanLabel(
                P + "TP1_LABEL",
                BuildPlanLevelLabel(
                "TP1",
                tp1,
                entry,
                true),
                tp1,
                TpLineColor,
                ShowTP1 &&
                tp1Distinct,
                    lineLeft,
                    labelBar);

            bool tp2Distinct =
                IsFinitePositive(tp2) &&
                (!tp1Distinct ||
                 !SamePrice(
                     tp2,
                     tp1)) &&
                !SamePrice(
                    tp2,
                    entry) &&
                !SamePrice(
                    tp2,
                    trigger);

            RenderCompactPlanLabel(
                P + "TP2_LABEL",
                BuildPlanLevelLabel(
                "TP2",
                tp2,
                entry,
                true),
                tp2,
                Tp2LineColor,
                ShowTP2 &&
                tp2Distinct,
                    lineLeft,
                    labelBar);

            bool tp3Distinct =
                IsFinitePositive(tp3) &&
                (!tp2Distinct ||
                 !SamePrice(
                     tp3,
                     tp2)) &&
                !SamePrice(
                    tp3,
                    entry) &&
                !SamePrice(
                    tp3,
                    trigger);

            RenderCompactPlanLabel(
                P + "TP3_LABEL",
                BuildPlanLevelLabel(
                "TP3",
                tp3,
                entry,
                true),
                tp3,
                Tp3LineColor,
                ShowTP3 &&
                tp3Distinct,
                    lineLeft,
                    labelBar);

            bool tp4Distinct =
                IsFinitePositive(tp4) &&
                (!tp3Distinct ||
                 !SamePrice(
                     tp4,
                     tp3)) &&
                !SamePrice(
                    tp4,
                    entry) &&
                !SamePrice(
                    tp4,
                    trigger);

            RenderCompactPlanLabel(
                P + "TP4_LABEL",
                BuildPlanLevelLabel(
                "TP4",
                tp4,
                entry,
                true),
                tp4,
                Tp4LineColor,
                ShowTP4 &&
                tp4Distinct,
                    lineLeft,
                    labelBar);

            double activeBrokerTarget =
                snapshot.BrokerTarget;

            bool activeBrokerTargetDistinct =
                snapshot.ActiveBrokerTargetVisible &&
                !SamePrice(
                    activeBrokerTarget,
                    snapshot.Entry) &&
                !SamePrice(
                    activeBrokerTarget,
                    snapshot.Tp1) &&
                !SamePrice(
                    activeBrokerTarget,
                    snapshot.Tp2) &&
                !SamePrice(
                    activeBrokerTarget,
                    snapshot.Tp3) &&
                !SamePrice(
                    activeBrokerTarget,
                    snapshot.Tp4);

            RenderCompactPlanLabel(
                P + "ACTIVE_TP_LABEL",
                BuildPlanLevelLabel(
                "ACTIVE TP",
                activeBrokerTarget,
                snapshot.Entry,
                true),
                activeBrokerTarget,
                PanelAccentColor,
                (ShowTP1 ||
                 ShowTP2 ||
                 ShowTP3 ||
                 ShowTP4) &&
                activeBrokerTargetDistinct,
                    lineLeft,
                    labelBar);
        }
    }
}
