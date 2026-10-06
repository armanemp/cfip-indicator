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

            int displayNumber = 0;

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    _opportunityCandidates[i];

                if (candidate == null ||
                    IsSameAsCanonicalPlan(candidate))
                    continue;

                displayNumber++;

                string baseName =
                    P +
                    "OPP_" +
                    candidate.Id;

                _opportunityVisualIds.Add(
                    baseName);

                if (candidate.PresentationOnly)
                    continue;

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
                    DateTime labelTime =
                        GetCompactPlanLabelAnchorTime();

                    RenderOpportunityLabel(
                        baseName + "_ENTRY_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "ENTRY",
                            candidate.Entry,
                            false,
                            false,
                            0,
                            displayNumber),
                        candidate.Entry,
                        EntryLineColor,
                        labelTime);

                    RenderOpportunityLabel(
                        baseName + "_SL_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "SL",
                            candidate.Stop,
                            true,
                            false,
                            0,
                            displayNumber),
                        candidate.Stop,
                        SlLineColor,
                        labelTime);

                    RenderOpportunityLabel(
                        baseName + "_TP1_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP1",
                            candidate.Tp1,
                            true,
                            true,
                            candidate.Tp1RR,
                            displayNumber),
                        candidate.Tp1,
                        TpLineColor,
                        labelTime);

                    RenderOpportunityLabel(
                        baseName + "_TP2_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP2",
                            candidate.Tp2,
                            true,
                            true,
                            candidate.Tp2RR,
                            displayNumber),
                        candidate.Tp2,
                        Tp2LineColor,
                        labelTime);

                    RenderOpportunityLabel(
                        baseName + "_TP3_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP3",
                            candidate.Tp3,
                            true,
                            true,
                            candidate.Tp3RR,
                            displayNumber),
                        candidate.Tp3,
                        Tp3LineColor,
                        labelTime);

                    RenderOpportunityLabel(
                        baseName + "_TP4_LABEL",
                        BuildScenarioLevelLabel(
                            candidate,
                            "TP4",
                            candidate.Tp4,
                            true,
                            true,
                            candidate.Tp4RR,
                            displayNumber),
                        candidate.Tp4,
                        Tp4LineColor,
                        labelTime);
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
            DateTime labelTime)
        {
            RenderCompactPlanLabel(
                name,
                text,
                price,
                color,
                true,
                labelTime);
        }

        private bool IsSameAsCanonicalPlan(
            TradeOpportunityCandidate candidate)
        {
            if (_plan == null ||
                candidate == null ||
                candidate.Direction != _plan.Direction)
                return false;

            double tolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    Math.Max(
                        Symbol.PipSize,
                        _plan.Risk) *
                    0.10);

            return
                SameCanonicalLevel(
                    candidate.Entry,
                    _plan.Entry,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.IdealEntry,
                    _plan.IdealEntry,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Trigger,
                    _plan.EntryTrigger,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Stop,
                    _plan.Stop,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Tp1,
                    _plan.Tp1,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Tp2,
                    _plan.Tp2,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Tp3,
                    _plan.Tp3,
                    tolerance) &&
                SameCanonicalLevel(
                    candidate.Tp4,
                    _plan.Tp4,
                    tolerance);
        }

        // All seven displayed level prices are compared so a visually identical
        // scenario cannot survive as a second line/label family.
        private bool SameCanonicalLevel(
            double candidatePrice,
            double canonicalPrice,
            double tolerance)
        {
            bool candidateFinite =
                IsFinitePositive(candidatePrice);
            bool canonicalFinite =
                IsFinitePositive(canonicalPrice);

            if (!candidateFinite ||
                !canonicalFinite)
                return !candidateFinite &&
                    !canonicalFinite;

            return
                Math.Abs(
                    candidatePrice -
                    canonicalPrice) <=
                tolerance;
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
                    IsSameAsCanonicalPlan(candidate))
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
