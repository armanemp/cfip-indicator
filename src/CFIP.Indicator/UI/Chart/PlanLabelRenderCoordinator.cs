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

            int boxRightBar =
                GetLabelBoxRightBar(
                    lineLeft);

            double atr =
                Bars.Count >= 3
                    ? Atr(
                        Bars,
                        Math.Max(
                            1,
                            Math.Min(
                                Bars.Count - 2,
                                labelBar)))
                    : 0;

            double boxHalfHeight =
                Math.Max(
                    Symbol.PipSize * 4,
                    atr > 0
                        ? atr * 0.065
                        : Symbol.PipSize * 5);

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
                    "ENTRY " +
                    Price(entry),
                    entry,
                    EntryLineColor,
                    true,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);
            }
            else
            {
                RemovePlanLabel(P + "ENTRY_LABEL");
            }

            bool idealDistinct =
                (preview ? IsFinitePositive(idealEntry) && !SamePrice(idealEntry, entry) : snapshot.IdealEntryVisible);

            RenderCompactPlanLabel(
                P + "IDEAL_ENTRY_LABEL",
                (snapshot.EntryMode ==
                    ExecutionMode.BreakoutMarket
                    ? "ZONE MID "
                    : "IDEAL ") +
                Price(idealEntry),
                idealEntry,
                PanelAccentColor,
                ShowEntry && idealDistinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "TRIGGER " +
                Price(trigger),
                trigger,
                TriggerLineColor,
                ShowTrigger &&
                triggerDistinct &&
                !snapshot.LivePosition,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            double displayStop = stop;

            RenderCompactPlanLabel(
                P + "SL_LABEL",
                "SL " +
                Price(displayStop),
                displayStop,
                SlLineColor,
                ShowSL,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "TP1 " +
                Price(tp1),
                tp1,
                TpLineColor,
                ShowTP1 &&
                tp1Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "TP2 " +
                Price(tp2),
                tp2,
                Tp2LineColor,
                ShowTP2 &&
                tp2Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "TP3 " +
                Price(tp3),
                tp3,
                Tp3LineColor,
                ShowTP3 &&
                tp3Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "TP4 " +
                Price(tp4),
                tp4,
                Tp4LineColor,
                ShowTP4 &&
                tp4Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

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
                "ACTIVE TP " +
                Price(activeBrokerTarget),
                activeBrokerTarget,
                PanelAccentColor,
                (ShowTP1 ||
                 ShowTP2 ||
                 ShowTP3 ||
                 ShowTP4) &&
                activeBrokerTargetDistinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);
        }
    }
}
