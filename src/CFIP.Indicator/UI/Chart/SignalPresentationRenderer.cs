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

            bool showConfirmedSignal =
                ShowSignalArrow &&
                snapshot.DecisionEntryAllowed &&
                snapshot.DecisionDirection != 0;

            bool showDirectionalWatch =
                ShowEarlyArrow &&
                ShowEarlyWatch &&
                !snapshot.PendingOrder &&
                !snapshot.LivePosition &&
                visualDirection != 0 &&
                snapshot.Confidence >=
                    Math.Max(
                        40,
                        MinimumEarlyConfidence);

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
                    snapshot.Confidence >=
                        HighConfidenceThreshold
                        ? "STRONG"
                        : showConfirmedSignal
                            ? "CONFIRMED"
                            : "WATCH");
            }
            else
            {
                RemoveStackedSignalArrows();
            }
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
