using System;
using System.Globalization;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private string FormatPlanPips(
            double entry,
            double level)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(level) ||
                Symbol == null ||
                Symbol.PipSize <= 0)
                return "";

            double pips =
                Math.Abs(level - entry) /
                Symbol.PipSize;

            if (double.IsNaN(pips) ||
                double.IsInfinity(pips))
                return "";

            return
                pips.ToString(
                    pips >= 1000
                        ? "F0"
                        : pips >= 100
                            ? "F1"
                            : "F2",
                    CultureInfo.InvariantCulture) +
                " pips";
        }

        private string PlanTimeframeTag()
        {
            // The canonical main plan is a multi-timeframe consensus.
            return "(MTF)";
        }

        private string ScenarioTimeframeTag(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null ||
                string.IsNullOrWhiteSpace(
                    candidate.SourceTimeframe))
                return "(MTF)";

            return
                "(" +
                candidate.SourceTimeframe +
                ")";
        }

        private string ScenarioLabelPrefix(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return "SCENARIO";

            string direction =
                candidate.Direction == 1
                    ? "BUY"
                    : candidate.Direction == -1
                        ? "SELL"
                        : "NEUTRAL";

            string tf =
                string.IsNullOrWhiteSpace(
                    candidate.SourceTimeframe)
                    ? "MTF"
                    : candidate.SourceTimeframe;

            string role =
                candidate.IsPrimaryTimeframeSignal
                    ? "PRIMARY"
                    : "TF";

            return
                role +
                " " +
                tf +
                " " +
                direction +
                " " +
                ScenarioTimeframeTag(candidate);
        }

        private string BuildPlanLevelLabel(
            string levelName,
            double price,
            double entry,
            bool includeDistance)
        {
            string text =
                levelName +
                " " +
                Price(price) +
                " " +
                PlanTimeframeTag();

            if (includeDistance)
            {
                string pips =
                    FormatPlanPips(
                        entry,
                        price);

                if (!string.IsNullOrWhiteSpace(pips))
                {
                    text +=
                        " (" +
                        pips +
                        ")";
                }
            }

            return text;
        }

        private string BuildScenarioLevelLabel(
            TradeOpportunityCandidate candidate,
            string levelName,
            double price,
            bool includeDistance,
            bool includeRr,
            double rr)
        {
            if (candidate == null)
                return
                    levelName +
                    " " +
                    Price(price) +
                    " " +
                    PlanTimeframeTag();

            string text =
                ScenarioLabelPrefix(candidate) +
                " " +
                levelName +
                " " +
                Price(price);

            if (includeDistance)
            {
                string pips =
                    FormatPlanPips(
                        candidate.Entry,
                        price);

                if (!string.IsNullOrWhiteSpace(pips))
                {
                    text +=
                        " (" +
                        pips +
                        ")";
                }
            }

            if (includeRr &&
                rr > 0)
            {
                text +=
                    " • " +
                    rr.ToString(
                        "F2",
                        CultureInfo.InvariantCulture) +
                    "R";
            }

            return text;
        }
    }
}
