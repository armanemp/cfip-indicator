using System.Collections.Generic;
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderParallelOpportunityCandidates(
            int closedM5)
        {
            if (!EnableParallelOpportunities ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveParallelOpportunityObjects();
                Chart.RemoveObject(P + "PRIMARY_M15_SIGNAL");
                Chart.RemoveObject(P + "PRIMARY_H1_SIGNAL");
                return;
            }

            int left =
                GetPlanLineLeftBar();
            int right =
                GetPlanLineRightBar();

            double atr =
                Atr(
                    Bars,
                    Math.Max(
                        1,
                        Math.Min(
                            Bars.Count - 2,
                            right)));

            double boxHalfHeight =
                Math.Max(
                    Symbol.PipSize * 3,
                    atr > 0
                        ? atr * 0.055
                        : Symbol.PipSize * 4);

            HashSetCurrentOpportunityVisuals();

            bool primaryM15Rendered = false;
            bool primaryH1Rendered = false;

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    _opportunityCandidates[i];

                if (candidate == null ||
                    IsSameAsLivePlan(candidate))
                    continue;

                string baseName =
                    P +
                    "OPP_" +
                    candidate.Id;

                _opportunityVisualIds.Add(
                    baseName);

                if (candidate.IsPrimaryTimeframeSignal &&
                    (string.Equals(candidate.SourceTimeframe, "M15", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(candidate.SourceTimeframe, "H1", StringComparison.OrdinalIgnoreCase)))
                {
                    int markerBar =
                        MapM5ToClosedChart(
                            candidate.CreatedM5,
                            Math.Max(0, Bars.Count - 2));

                    double markerAtr =
                        markerBar >= 1
                            ? Atr(
                                Bars,
                                markerBar)
                            : 0;

                    double markerOffset =
                        Math.Max(
                            Symbol.PipSize * 3,
                            (markerAtr > 0 ? markerAtr : Symbol.PipSize * 20) *
                            (string.Equals(candidate.SourceTimeframe, "M15", StringComparison.OrdinalIgnoreCase)
                                ? 0.22
                                : 0.38));

                    double markerPrice =
                        candidate.Direction == 1
                            ? Bars.LowPrices[markerBar] - markerOffset
                            : Bars.HighPrices[markerBar] + markerOffset;

                    string markerName =
                        P +
                        "PRIMARY_" +
                        candidate.SourceTimeframe.Trim().ToUpperInvariant() +
                        "_SIGNAL";

                    DrawIcon(
                        markerName,
                        candidate.Direction == 1
                            ? ChartIconType.UpArrow
                            : ChartIconType.DownArrow,
                        markerBar,
                        markerPrice,
                        SignalArrowColorFor(
                            candidate.Direction,
                            "WATCH"));

                    if (string.Equals(candidate.SourceTimeframe, "M15", StringComparison.OrdinalIgnoreCase))
                        primaryM15Rendered = true;
                    else
                        primaryH1Rendered = true;
                }

                RenderOpportunityLine(
                    baseName + "_ENTRY",
                    candidate.Entry,
                    EntryLineColor,
                    ShowEntry);

                RenderOpportunityLine(
                    baseName + "_IDEAL_ENTRY",
                    candidate.IdealEntry,
                    PanelAccentColor,
                    ShowEntry &&
                    IsFinitePositive(candidate.IdealEntry) &&
                    !SamePrice(
                        candidate.IdealEntry,
                        candidate.Entry));

                RenderOpportunityLine(
                    baseName + "_TRIGGER",
                    candidate.Trigger,
                    TriggerLineColor,
                    ShowTrigger &&
                    IsFinitePositive(candidate.Trigger));

                RenderOpportunityLine(
                    baseName + "_SL",
                    candidate.Stop,
                    SlLineColor,
                    ShowSL);

                RenderOpportunityLine(
                    baseName + "_TP1",
                    candidate.Tp1,
                    TpLineColor,
                    ShowTP1);

                RenderOpportunityLine(
                    baseName + "_TP2",
                    candidate.Tp2,
                    Tp2LineColor,
                    ShowTP2);

                RenderOpportunityLine(
                    baseName + "_TP3",
                    candidate.Tp3,
                    Tp3LineColor,
                    ShowTP3);

                RenderOpportunityLine(
                    baseName + "_TP4",
                    candidate.Tp4,
                    Tp4LineColor,
                    ShowTP4);

                if (ShowLevelLines &&
                    ShowTacticalOpportunityLabels &&
                    (ShowLevelPriceLabels ||
                     ShowSignalLabels))
                {
                    int baseLabelBar =
                        GetCompactPlanLabelAnchorBar(
                            left);

                    int labelBar =
                        Math.Max(
                            left,
                            Math.Min(
                                right,
                                baseLabelBar +
                                i * 8));

                    int boxRight =
                        Math.Max(
                            labelBar,
                            Math.Min(
                                right,
                                labelBar +
                                CompactPlanLabelWidthBars));

                    RenderOpportunityLabel(
                        baseName + "_ENTRY_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "ENTRY",
                            candidate.Entry,
                            false,
                            false,
                            0),
                        candidate.Entry,
                        EntryLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_SL_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "SL",
                            candidate.Stop,
                            true,
                            false,
                            0),
                        candidate.Stop,
                        SlLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP1_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP1",
                            candidate.Tp1,
                            true,
                            true,
                            candidate.Tp1RR),
                        candidate.Tp1,
                        TpLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP2_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP2",
                            candidate.Tp2,
                            true,
                            true,
                            candidate.Tp2RR),
                        candidate.Tp2,
                        Tp2LineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP3_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP3",
                            candidate.Tp3,
                            true,
                            true,
                            candidate.Tp3RR),
                        candidate.Tp3,
                        Tp3LineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP4_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP4",
                            candidate.Tp4,
                            true,
                            true,
                            candidate.Tp4RR),
                        candidate.Tp4,
                        Tp4LineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);
                }
                else
                {
                    RemoveOpportunityLabels(
                        baseName);
                }

            }

            if (!primaryM15Rendered)
                Chart.RemoveObject(P + "PRIMARY_M15_SIGNAL");

            if (!primaryH1Rendered)
                Chart.RemoveObject(P + "PRIMARY_H1_SIGNAL");

            RemoveStaleParallelOpportunityObjects();
        }

        private void RenderOpportunityLine(
            string name,
            double price,
            Color color,
            bool visible)
        {
            DrawPlanLine(
                name,
                price,
                color,
                visible);
        }

        private void RenderOpportunityLabel(
            string name,
            string text,
            double price,
            Color color,
            int lineLeft,
            int labelBar,
            int boxRightBar,
            double boxHalfHeight)
        {
            RenderCompactPlanLabel(
                name,
                text,
                price,
                color,
                true,
                lineLeft,
                labelBar,
                boxRightBar,
                boxHalfHeight);
        }

        private string LaneLabel(
            TradeOpportunityCandidate candidate)
        {
            string direction =
                candidate.Direction == 1
                    ? "BUY"
                    : "SELL";

            if (!string.IsNullOrWhiteSpace(
                    candidate.SourceTimeframe))
                return
                    ScenarioLabelPrefix(
                        candidate);

            return
                candidate.LabelPrefix +
                " " +
                direction;
        }

        private bool IsSameAsLivePlan(
            TradeOpportunityCandidate candidate)
        {
            if (_plan == null ||
                !_plan.IsLivePosition ||
                candidate == null)
                return false;

            double tolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    Math.Max(
                        Symbol.PipSize,
                        _plan.Risk) *
                    0.10);

            return
                _plan.Direction ==
                candidate.Direction &&
                Math.Abs(
                    _plan.Entry -
                    candidate.Entry) <=
                tolerance &&
                Math.Abs(
                    _plan.Stop -
                    candidate.Stop) <=
                tolerance &&
                Math.Abs(
                    _plan.Tp1 -
                    candidate.Tp1) <=
                tolerance;
        }

        private void HashSetCurrentOpportunityVisuals()
        {
            // Existing IDs are intentionally kept until the current render has
            // declared ownership; stale IDs are removed in a separate pass.
        }

        private void RemoveOpportunityLabels(
            string baseName)
        {
            string[] suffixes =
            {
                "_ENTRY_LABEL",
                "_SL_LABEL",
                "_TP1_LABEL",
                "_TP2_LABEL",
                "_TP3_LABEL",
                "_TP4_LABEL"
            };

            for (int i = 0;
                 i < suffixes.Length;
                 i++)
            {
                Chart.RemoveObject(
                    baseName +
                    suffixes[i]);
                Chart.RemoveObject(
                    baseName +
                    suffixes[i] +
                    "_BOX");
            }
        }

        private void RemoveStaleParallelOpportunityObjects()
        {
            // Current ownership is represented by the base IDs in the set; all
            // active names are rebuilt from candidate IDs on each structural render.
            HashSet<string> current =
                new HashSet<string>();

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    _opportunityCandidates[i];

                if (candidate == null ||
                    IsSameAsLivePlan(candidate))
                    continue;

                current.Add(
                    P +
                    "OPP_" +
                    candidate.Id);
            }

            List<string> stale =
                new List<string>();

            foreach (string id in _opportunityVisualIds)
            {
                if (!current.Contains(id))
                    stale.Add(id);
            }

            for (int i = 0;
                 i < stale.Count;
                 i++)
            {
                string id = stale[i];

                string[] suffixes =
                {
                    "_ENTRY",
                    "_IDEAL_ENTRY",
                    "_TRIGGER",
                    "_SL",
                    "_TP1",
                    "_TP2",
                    "_TP3",
                    "_TP4",
                    "_ENTRY_LABEL",
                    "_SL_LABEL",
                    "_TP1_LABEL",
                    "_TP2_LABEL",
                    "_TP3_LABEL",
                    "_TP4_LABEL"
                };

                for (int j = 0;
                     j < suffixes.Length;
                     j++)
                {
                    Chart.RemoveObject(
                        id +
                        suffixes[j]);
                    Chart.RemoveObject(
                        id +
                        suffixes[j] +
                        "_BOX");
                }

                _opportunityVisualIds.Remove(id);
            }
        }

        private void RemoveParallelOpportunityObjects()
        {
            List<string> ids =
                new List<string>(
                    _opportunityVisualIds);

            for (int i = 0;
                 i < ids.Count;
                 i++)
            {
                string id = ids[i];

                string[] suffixes =
                {
                    "_ENTRY",
                    "_IDEAL_ENTRY",
                    "_TRIGGER",
                    "_SL",
                    "_TP1",
                    "_TP2",
                    "_TP3",
                    "_TP4",
                    "_ENTRY_LABEL",
                    "_SL_LABEL",
                    "_TP1_LABEL",
                    "_TP2_LABEL",
                    "_TP3_LABEL",
                    "_TP4_LABEL"
                };

                for (int j = 0;
                     j < suffixes.Length;
                     j++)
                {
                    Chart.RemoveObject(
                        id +
                        suffixes[j]);
                    Chart.RemoveObject(
                        id +
                        suffixes[j] +
                        "_BOX");
                }
            }

            _opportunityVisualIds.Clear();
            Chart.RemoveObject(P + "PRIMARY_M15_SIGNAL");
            Chart.RemoveObject(P + "PRIMARY_H1_SIGNAL");
        }
    }
}
