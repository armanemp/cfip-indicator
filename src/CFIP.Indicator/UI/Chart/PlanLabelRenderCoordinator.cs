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

            int labelBar =
                GetCompactPlanLabelAnchorBar();

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

                string text;

                if (level.Key == "IDEAL_ENTRY")
                {
                    text =
                        snapshot.EntryMode ==
                            ExecutionMode.BreakoutMarket
                            ? BuildPlanLevelLabel(
                                "ZONE MID",
                                level.Price,
                                state.Entry,
                                false)
                            : BuildPlanLevelLabel(
                                "IDEAL",
                                level.Price,
                                state.Entry,
                                false);
                }
                else if (level.Key == "ACTIVE_TP")
                {
                    text =
                        BuildPlanLevelLabel(
                            "ACTIVE TP",
                            level.Price,
                            state.Entry,
                            true);
                }
                else
                {
                    text =
                        BuildPlanLevelLabel(
                            level.Key,
                            level.Price,
                            state.Entry,
                            level.IncludeDistance);
                }

                RenderCompactPlanLabel(
                    P + level.LabelName,
                    text,
                    level.Price,
                    level.Color,
                    level.Visible,
                    labelBar);
            }
        }
    }
}
