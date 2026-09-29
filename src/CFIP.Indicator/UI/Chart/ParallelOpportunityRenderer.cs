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
                !ShowLevelLines ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveParallelOpportunityObjects();
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

                if (ShowTacticalOpportunityLabels &&
                    (ShowLevelPriceLabels ||
                     ShowSignalLabels))
                {
                    int labelBar =
                        Math.Max(
                            left,
                            Math.Min(
                                right,
                                left +
                                Math.Max(
                                    1,
                                    LabelLeftOffsetBars) +
                                i * 8));

                    int boxRight =
                        Math.Max(
                            labelBar,
                            Math.Min(
                                right,
                                labelBar +
                                6));

                    RenderOpportunityLabel(
                        baseName + "_ENTRY_LABEL",
                        LaneLabel(
                            candidate) +
                        " ENTRY " +
                        Price(candidate.Entry),
                        candidate.Entry,
                        EntryLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_SL_LABEL",
                        LaneLabel(
                            candidate) +
                        " SL " +
                        Price(candidate.Stop),
                        candidate.Stop,
                        SlLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP1_LABEL",
                        LaneLabel(
                            candidate) +
                        " TP1 " +
                        Price(candidate.Tp1) +
                        " • " +
                        candidate.Tp1RR.ToString("F2") +
                        "R",
                        candidate.Tp1,
                        TpLineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP2_LABEL",
                        LaneLabel(
                            candidate) +
                        " TP2 " +
                        Price(candidate.Tp2) +
                        " • " +
                        candidate.Tp2RR.ToString("F2") +
                        "R",
                        candidate.Tp2,
                        Tp2LineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP3_LABEL",
                        LaneLabel(
                            candidate) +
                        " TP3 " +
                        Price(candidate.Tp3) +
                        " • " +
                        candidate.Tp3RR.ToString("F2") +
                        "R",
                        candidate.Tp3,
                        Tp3LineColor,
                        left,
                        labelBar,
                        boxRight,
                        boxHalfHeight);

                    RenderOpportunityLabel(
                        baseName + "_TP4_LABEL",
                        LaneLabel(
                            candidate) +
                        " TP4 " +
                        Price(candidate.Tp4) +
                        " • " +
                        candidate.Tp4RR.ToString("F2") +
                        "R",
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
        }
    }
}
