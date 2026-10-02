using System;
using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void AddTimeframeScenarioCandidates(
            int closedM5)
        {
            if (!EnableParallelOpportunities ||
                _m5Bars == null ||
                closedM5 < 30 ||
                closedM5 >= _m5Bars.Count)
                return;

            DateTime reference =
                _m5Bars.OpenTimes[closedM5];

            MtfClosedContext context;

            try
            {
                context =
                    BuildMtfClosedContext(
                        reference);
            }
            catch
            {
                return;
            }

            Frame[] frames =
            {
                _m5Frame,
                _m15Frame,
                _m30Frame,
                _h1Frame,
                _h4Frame,
                _d1Frame,
                _w1Frame
            };

            string[] names =
            {
                "M5",
                "M15",
                "M30",
                "H1",
                "H4",
                "D1",
                "W1"
            };

            int[] indices =
            {
                context.M5,
                context.M15,
                context.M30,
                context.H1,
                context.H4,
                context.D1,
                context.W1
            };

            double[] weights =
            {
                Math.Max(0, M5Weight),
                Math.Max(0, M15Weight),
                Math.Max(0, M30Weight),
                Math.Max(0, H1Weight),
                Math.Max(0, H4Weight),
                Math.Max(0, D1Weight),
                SmartWeeklyContext
                    ? Math.Max(0, W1Weight)
                    : 0
            };

            for (int i = 0;
                 i < frames.Length;
                 i++)
            {
                Frame frame =
                    frames[i];

                if (frame == null ||
                    weights[i] <= 0 ||
                    indices[i] < 30 ||
                    frame.Index != indices[i] ||
                    frame.Direction == 0 ||
                    frame.Quality <= 0)
                    continue;

                // Independent timeframe scenarios remain opportunities rather
                // than automatic execution authorizations.
                int minimumQuality =
                    Math.Max(
                        60,
                        TacticalOpportunityMinimumQuality - 5);

                if (frame.Quality < minimumQuality)
                    continue;

                TradeOpportunityCandidate candidate =
                    BuildLaneCandidate(
                        closedM5,
                        OpportunityLane.Tactical,
                        frame.Direction,
                        frame.Quality,
                        names[i]);

                if (candidate == null)
                    continue;

                candidate.SourceTimeframe =
                    names[i];

                candidate.ScenarioId =
                    "TF-" +
                    names[i] +
                    "-" +
                    (frame.Direction == 1
                        ? "BUY"
                        : "SELL");

                candidate.Id =
                    candidate.ScenarioId;

                candidate.BasePlanTimeframe = "M5";

                bool primary =
                    PrimarySignalTimeframeRule.IsPrimary(
                        names[i]);

                PrimaryTimeframeTuning tuning =
                    PrimarySignalTimeframeRule.ResolveTuning(
                        frame.Direction,
                        _m5Frame == null ? 0 : _m5Frame.Direction,
                        _m1Frame == null ? 0 : _m1Frame.Direction);

                candidate.PrimarySignal =
                    primary;

                candidate.PrimarySignalReady =
                    primary &&
                    frame.Quality >= minimumQuality;

                candidate.SignalRole =
                    primary ? "PRIMARY SIGNAL" : "CONTEXT";

                candidate.LowerTimeframeTuning =
                    tuning.State;

                candidate.LowerTimeframeTuningAgreement =
                    tuning.Agreement;

                candidate.LowerTimeframeConflict =
                    tuning.Conflict;

                candidate.PrimaryLevelEvidence =
                    PrimarySignalTimeframeRule.ResolveLevelEvidence(
                        frame,
                        frame.Direction);

                candidate.PrimaryLevelEvidenceScore =
                    primary
                        ? Math.Max(
                            0,
                            frame.Direction == 1
                                ? frame.LocationEvidenceBull
                                : frame.LocationEvidenceBear)
                        : 0;

                candidate.PrimaryFvgQuality =
                    primary
                        ? (frame.Direction == 1
                            ? frame.FvgBullQuality
                            : frame.FvgBearQuality)
                        : 0;

                candidate.PrimaryObQuality =
                    primary
                        ? (frame.Direction == 1
                            ? frame.ObBullQuality
                            : frame.ObBearQuality)
                        : 0;

                candidate.PrimaryObFvgConfluence =
                    primary &&
                    (frame.Direction == 1
                        ? frame.FvgObBullConfluence
                        : frame.FvgObBearConfluence);

                candidate.LabelPrefix =
                    "TF-" +
                    names[i] +
                    (primary
                        ? " PRIMARY"
                        : "");

                candidate.Source =
                    string.IsNullOrWhiteSpace(
                        candidate.Source)
                        ? "TF:" + names[i]
                        : candidate.Source +
                          " • TF:" +
                          names[i];

                candidate.Stage =
                    primary
                        ? "PRIMARY " +
                          names[i] +
                          " SIGNAL • " +
                          (candidate.PrimarySignalReady
                              ? tuning.State
                              : "NOT READY")
                        : candidate.ActionableNow &&
                          candidate.ExecutionPolicyAllowed
                            ? "TF SCENARIO • READY"
                            : "TF SCENARIO • WATCH • " +
                              (string.IsNullOrWhiteSpace(
                                  candidate.ExecutionPolicyReason)
                                  ? "POLICY BLOCKED"
                                  : candidate.ExecutionPolicyReason);

                EnrichScenarioEvidence(
                    candidate,
                    frame,
                    frame.Direction);

                candidate.ActionabilityReason =
                    string.IsNullOrWhiteSpace(
                        candidate.ActionabilityReason)
                        ? "INDEPENDENT " +
                          names[i] +
                          " FRAME"
                        : candidate.ActionabilityReason;

                candidate.Stage =
                    primary
                        ? "PRIMARY " +
                          names[i] +
                          " SIGNAL • " +
                          (candidate.PrimarySignalReady
                              ? tuning.State
                              : "NOT READY")
                        : candidate.ActionableNow &&
                          candidate.ExecutionPolicyAllowed
                            ? "TF SCENARIO • READY"
                            : "TF SCENARIO • WATCH • " +
                              (string.IsNullOrWhiteSpace(
                                  candidate.ExecutionPolicyReason)
                                  ? "POLICY BLOCKED"
                                  : candidate.ExecutionPolicyReason);

                AddOpportunityCandidate(
                    candidate);

                ArchiveRuntimeScenario(
                    candidate);
            }
        }

        private void TrimOpportunityCandidates()
        {
            _opportunityCandidates.Clear();

            IReadOnlyList<TradeOpportunityCandidate> selected =
                _tradePlanRegistry.SelectScenariosForDisplay(
                    MaximumVisibleOpportunities);

            _opportunityCandidates.AddRange(
                selected);
        }

    }
}
