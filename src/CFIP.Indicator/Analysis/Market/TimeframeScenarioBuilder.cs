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
                _m15Frame,
                _h1Frame
            };

            string[] names =
            {
                "M15",
                "H1"
            };

            int[] indices =
            {
                context.M15,
                context.H1
            };

            double[] weights =
            {
                Math.Max(0, M15Weight),
                Math.Max(0, H1Weight)
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

                candidate.LabelPrefix =
                    "PRIMARY-" +
                    names[i];

                candidate.Source =
                    string.IsNullOrWhiteSpace(
                        candidate.Source)
                        ? "TF:" + names[i]
                        : candidate.Source +
                          " • TF:" +
                          names[i];

                candidate.Stage =
                    (candidate.PrimarySignalState ?? "PRIMARY") +
                    " • " +
                    (candidate.ActionableNow
                        ? "M5 ENTRY WINDOW"
                        : candidate.M5TuningAligned
                            ? "M5 TUNING • WAIT"
                            : "M5 TUNING • CONFLICT");

                EnrichScenarioEvidence(
                    candidate,
                    frame,
                    frame.Direction);

                bool m1ConfirmationAvailable =
                    UseM1Trigger &&
                    _m1Bars != null &&
                    context.M1 >= 0;

                bool m1Confirmed =
                    m1ConfirmationAvailable &&
                    M1TriggerReady(
                        _m1Bars,
                        _m5Bars,
                        context.M1,
                        closedM5,
                        reference,
                        frame.Direction);

                PrimaryTimeframeSignalResult primary =
                    PrimaryTimeframeSignalRule.Evaluate(
                        names[i],
                        frame.Direction,
                        frame.Quality,
                        minimumQuality,
                        _m5Frame == null ? 0 : _m5Frame.Direction,
                        m1ConfirmationAvailable,
                        _m1Frame == null ? 0 : _m1Frame.Direction,
                        m1Confirmed);

                if (!primary.Allowed)
                    continue;

                candidate.IsPrimaryTimeframeSignal = true;
                candidate.M5TuningAligned = primary.M5Aligned;
                candidate.M1TuningConfirmed = primary.M1Confirmed;
                candidate.PrimarySignalState = primary.State;

                candidate.ActionabilityReason =
                    string.IsNullOrWhiteSpace(
                        candidate.ActionabilityReason)
                        ? "PRIMARY " +
                          names[i] +
                          " SOURCE"
                        : candidate.ActionabilityReason;

                candidate.Stage =
                    (candidate.PrimarySignalState ?? "PRIMARY") +
                    " • " +
                    (candidate.ActionableNow
                        ? "M5 ENTRY WINDOW"
                        : candidate.M5TuningAligned
                            ? "M5 TUNING • WAIT"
                            : "M5 TUNING • CONFLICT");

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
