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
            SignalVisualSnapshot snapshot)
        {
            if (_plan == null ||
                snapshot == null ||
                !snapshot.PlanActive ||
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
                GetCompactPlanLineLeftBar();

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
                    Symbol.PipSize * 3,
                    atr > 0
                        ? atr * 0.055
                        : Symbol.PipSize * 4);

            if (ShowEntry)
            {
                RenderPlanLabel(
                    P + "ENTRY_LABEL",
                    "ENTRY " +
                    Price(snapshot.Entry),
                    snapshot.Entry,
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
                snapshot.IdealEntryVisible;

            RenderPlanLabel(
                P + "IDEAL_ENTRY_LABEL",
                (snapshot.EntryMode ==
                    ExecutionMode.BreakoutMarket
                    ? "ZONE MID "
                    : "IDEAL ") +
                Price(snapshot.IdealEntry),
                snapshot.IdealEntry,
                PanelAccentColor,
                ShowEntry && idealDistinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            bool triggerDistinct =
                IsFinitePositive(snapshot.Trigger) &&
                !SamePrice(
                    snapshot.Trigger,
                    snapshot.Entry) &&
                (!idealDistinct ||
                 !SamePrice(
                     snapshot.Trigger,
                     snapshot.IdealEntry));

            RenderPlanLabel(
                P + "TRIGGER_LABEL",
                "TRIGGER " +
                Price(snapshot.Trigger),
                snapshot.Trigger,
                TriggerLineColor,
                ShowTrigger &&
                triggerDistinct &&
                !snapshot.LivePosition,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            double displayStop =
                snapshot.Stop;

            RenderPlanLabel(
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
                IsFinitePositive(snapshot.Tp1) &&
                !SamePrice(
                    snapshot.Tp1,
                    snapshot.Entry) &&
                !SamePrice(
                    snapshot.Tp1,
                    snapshot.Trigger);

            RenderPlanLabel(
                P + "TP1_LABEL",
                "TP1 " +
                Price(snapshot.Tp1),
                snapshot.Tp1,
                TpLineColor,
                ShowTP1 &&
                tp1Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            bool tp2Distinct =
                IsFinitePositive(snapshot.Tp2) &&
                (!tp1Distinct ||
                 !SamePrice(
                     snapshot.Tp2,
                     snapshot.Tp1)) &&
                !SamePrice(
                    snapshot.Tp2,
                    snapshot.Entry) &&
                !SamePrice(
                    snapshot.Tp2,
                    snapshot.Trigger);

            RenderPlanLabel(
                P + "TP2_LABEL",
                "TP2 " +
                Price(snapshot.Tp2),
                snapshot.Tp2,
                Tp2LineColor,
                ShowTP2 &&
                tp2Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            bool tp3Distinct =
                IsFinitePositive(snapshot.Tp3) &&
                (!tp2Distinct ||
                 !SamePrice(
                     snapshot.Tp3,
                     snapshot.Tp2)) &&
                !SamePrice(
                    snapshot.Tp3,
                    snapshot.Entry) &&
                !SamePrice(
                    snapshot.Tp3,
                    snapshot.Trigger);

            RenderPlanLabel(
                P + "TP3_LABEL",
                "TP3 " +
                Price(snapshot.Tp3),
                snapshot.Tp3,
                Tp3LineColor,
                ShowTP3 &&
                tp3Distinct,
                    lineLeft,
                    labelBar,
                    boxRightBar,
                    boxHalfHeight);

            bool tp4Distinct =
                IsFinitePositive(snapshot.Tp4) &&
                (!tp3Distinct ||
                 !SamePrice(
                     snapshot.Tp4,
                     snapshot.Tp3)) &&
                !SamePrice(
                    snapshot.Tp4,
                    snapshot.Entry) &&
                !SamePrice(
                    snapshot.Tp4,
                    snapshot.Trigger);

            RenderPlanLabel(
                P + "TP4_LABEL",
                "TP4 " +
                Price(snapshot.Tp4),
                snapshot.Tp4,
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

            RenderPlanLabel(
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
