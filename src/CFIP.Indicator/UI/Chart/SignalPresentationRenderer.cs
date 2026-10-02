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

            bool showStrongWatch =
                ShowEarlyArrow &&
                ShowEarlyWatch &&
                IsStrongWatchSnapshot(snapshot) &&
                visualDirection != 0;

            if ((showConfirmedSignal || showStrongWatch) &&
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

                DrawIcon(
                    P + "WATCH_ARROW",
                    visualDirection == 1
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    hostBar,
                    visualDirection == 1
                        ? Bars.LowPrices[hostBar] - watchOffset
                        : Bars.HighPrices[hostBar] + watchOffset,
                    SignalArrowColorFor(
                        visualDirection,
                        showConfirmedSignal
                            ? "CONFIRMED"
                            : "WATCH"));
            }
            else
            {
                Chart.RemoveObject(
                    P + "WATCH_ARROW");
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
