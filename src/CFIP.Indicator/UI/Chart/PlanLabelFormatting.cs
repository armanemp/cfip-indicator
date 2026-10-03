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
            string snapshotSource =
                _renderSignalVisualSnapshot == null
                    ? ""
                    : _renderSignalVisualSnapshot.SourceTimeframe;

            string source =
                string.IsNullOrWhiteSpace(snapshotSource)
                    ? ResolveCanonicalPlanSourceTimeframe()
                    : snapshotSource.Trim().ToUpperInvariant();

            return
                "(" +
                source +
                ")";
        }

        private string ResolveCanonicalPlanSourceTimeframe()
        {
            int closedM5 =
                _plan != null
                    ? _plan.CreatedM5
                    : _lastEvaluatedM5;

            if (closedM5 < 0 &&
                _m5Bars != null &&
                _m5Bars.Count > 1)
                closedM5 =
                    _m5Bars.Count - 2;

            string signalId =
                ResolveProviderSignalId(
                    closedM5);

            int direction =
                _plan != null
                    ? _plan.Direction
                    : (_decision == null
                        ? 0
                        : _decision.Direction);

            OpportunityLane lane =
                ResolveProviderLane();

            string scenarioId =
                ResolveProviderScenarioId(
                    signalId,
                    lane,
                    direction);

            TradeOpportunityCandidate scenario = null;
            _tradePlanRegistry.TryGetCandidate(
                scenarioId,
                out scenario);

            return
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    scenario,
                    ProviderScenarioIdentityRule.CanonicalM5);
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
                    : candidate.SourceTimeframe.Trim().ToUpperInvariant();

            string role =
                candidate.IsPrimaryTimeframeSignal
                    ? "PRIMARY"
                    : "TF";

            return
                role +
                " " +
                tf +
                " " +
                direction;
        }

        private string BuildCanonicalLevelLabel(
            string prefix,
            string levelName,
            double price,
            double entry,
            bool includeDistance,
            bool includeRr,
            double rr,
            string timeframeTag)
        {
            string text =
                string.IsNullOrWhiteSpace(prefix)
                    ? levelName + " " + Price(price)
                    : prefix +
                      " " +
                      levelName +
                      " " +
                      Price(price);

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

            if (!string.IsNullOrWhiteSpace(timeframeTag))
                text +=
                    " " +
                    timeframeTag;

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

        private string BuildTrendStrengthTag()
        {
            SignalVisualSnapshot snapshot =
                _renderSignalVisualSnapshot;

            if (snapshot == null ||
                snapshot.MtfTrendStrengthLevel <= 0 ||
                string.IsNullOrWhiteSpace(snapshot.MtfTrendStrengthTier))
                return "";

            return
                "TREND " +
                snapshot.MtfTrendStrengthTier.ToUpperInvariant() +
                " L" +
                snapshot.MtfTrendStrengthLevel.ToString(
                    CultureInfo.InvariantCulture);
        }

        private string BuildPlanLevelLabel(
            string levelName,
            double price,
            double entry,
            bool includeDistance)
        {
            string text =
                BuildCanonicalLevelLabel(
                    "",
                    levelName,
                    price,
                    entry,
                    includeDistance,
                    false,
                    0,
                    PlanTimeframeTag());

            if (string.Equals(levelName, "ENTRY", StringComparison.OrdinalIgnoreCase))
            {
                string trendTag = BuildTrendStrengthTag();

                if (!string.IsNullOrWhiteSpace(trendTag))
                    text += " • " + trendTag;
            }

            return text;
        }

        private string BuildScenarioLevelLabel(
            TradeOpportunityCandidate candidate,
            string levelName,
            double price,
            bool includeDistance,
            bool includeRr,
            double rr,
            int displayNumber = 0)
        {
            string prefix =
                ScenarioLabelPrefix(candidate);

            if (displayNumber > 0)
                prefix =
                    "#" +
                    displayNumber.ToString(
                        CultureInfo.InvariantCulture) +
                    " " +
                    prefix;

            return BuildCanonicalLevelLabel(
                prefix,
                levelName,
                price,
                candidate == null
                    ? 0
                    : candidate.Entry,
                includeDistance,
                includeRr,
                rr,
                ScenarioTimeframeTag(candidate));
        }
    }
}
