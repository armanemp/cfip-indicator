using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderNonActionableWatchState(
            SignalVisualSnapshot snapshot,
            int visualDirection,
            int hostBar)
        {
            Chart.RemoveObject(
                P + "REACTION_ARROW");

            // The chart arrow is a public signal surface, not a raw
            // direction preview. A confirmed arrow is emitted only when the
            // canonical decision is actionable; early/watch arrows require the
            // same strong-watch evidence contract used by the alert layer.
            bool showConfirmedSignal =
                ShowSignalArrow &&
                snapshot.ActionableNow &&
                snapshot.DecisionDirection != 0;

            bool showDirectionalWatch =
                ShowEarlyArrow &&
                ShowEarlyWatch &&
                !snapshot.PendingOrder &&
                !snapshot.LivePosition &&
                snapshot.DecisionEntryAllowed &&
                visualDirection != 0 &&
                IsStrongWatchSnapshot(snapshot) &&
                IsSignalOpportunityVisuallyMeaningful(snapshot);

            if ((showConfirmedSignal || showDirectionalWatch) &&
                visualDirection != 0)
            {
                double watchAtr =
                    Atr(
                        Bars,
                        Math.Max(
                            1,
                            Math.Min(
                                Bars.Count - 1,
                                hostBar)));

                double watchOffset =
                    Math.Max(
                        Symbol.PipSize *
                        Math.Max(
                            0.5,
                            MinimumArrowOffsetPips),
                        watchAtr *
                        Math.Max(
                            0.02,
                            ArrowOffsetAtr));

                RenderStackedSignalArrows(
                    snapshot,
                    visualDirection,
                    hostBar,
                    watchOffset,
                    ResolveSignalArrowState(
                        snapshot,
                        visualDirection));
            }
            else
            {
                RemoveStackedSignalArrows();
            }
        }

        private bool IsSignalOpportunityVisuallyMeaningful(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            if (snapshot.ActionableNow)
                return true;

            double entry =
                snapshot.SetupPreviewActive
                    ? snapshot.SetupEntry
                    : snapshot.PlanActive
                        ? snapshot.Entry
                        : 0;

            double tp1 =
                snapshot.SetupPreviewActive
                    ? snapshot.SetupTp1
                    : snapshot.PlanActive
                        ? snapshot.Tp1
                        : 0;

            int createdM5 =
                snapshot.SetupPreviewActive
                    ? snapshot.SetupCreatedM5
                    : snapshot.PlanActive
                        ? snapshot.CreatedM5
                        : _lastEvaluatedM5;

            if (IsFinitePositive(entry) &&
                IsFinitePositive(tp1) &&
                _m5Bars != null &&
                createdM5 >= 0 &&
                createdM5 < _m5Bars.Count)
            {
                double atr =
                    Atr(
                        _m5Bars,
                        createdM5);

                double distanceAtr =
                    IsFinitePositive(atr)
                        ? Math.Abs(tp1 - entry) / atr
                        : 0;

                if (!OpportunityMagnitudeRule.IsMeaningful(
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime,
                        distanceAtr))
                    return false;
            }

            if (snapshot.ActionableTp1RR > 0 &&
                snapshot.ActionableTp1RR <
                    Math.Max(
                        2.0,
                        Tp1MinimumRR))
                return false;

            return true;
        }

        private bool IsStrongWatchSnapshot(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            return WatchReactionAlertRule.IsStrongWatch(
                snapshot.AuthoritativeDirection,
                snapshot.Confidence,
                snapshot.SmartQuality,
                snapshot.TimeframeAgreement,
                snapshot.IndependentEvidence,
                snapshot.StructuralConfirmations,
                MinimumConfidence,
                MinimumSmartQuality,
                SmartQualityThreshold,
                MinimumTimeframeAgreement,
                SmartMinimumTimeframeAgreement,
                MinimumIndependentEvidence,
                MinimumStructuralConfirmations);
        }

    }
}
