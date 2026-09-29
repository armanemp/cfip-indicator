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

            if (ShowEarlyArrow &&
                IsStrongWatchSnapshot(snapshot) &&
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
                        "WATCH"));
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
            if (snapshot == null ||
                (snapshot.AuthoritativeDirection != 1 &&
                 snapshot.AuthoritativeDirection != -1))
                return false;

            int minimumConfidence =
                Math.Max(
                    60,
                    MinimumConfidence - 4);

            int minimumSmartQuality =
                Math.Max(
                    MinimumSmartQuality,
                    SmartQualityThreshold);

            int minimumTimeframeAgreement =
                Math.Max(
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement);

            return
                snapshot.Confidence >= minimumConfidence &&
                snapshot.SmartQuality >= minimumSmartQuality &&
                snapshot.TimeframeAgreement >=
                    minimumTimeframeAgreement &&
                snapshot.IndependentEvidence >=
                    MinimumIndependentEvidence &&
                snapshot.StructuralConfirmations >=
                    MinimumStructuralConfirmations;
        }
    }
}
